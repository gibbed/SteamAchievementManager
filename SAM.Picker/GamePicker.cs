/* Copyright (c) 2024 Rick (rick 'at' gibbed 'dot' us)
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using System.Xml.XPath;
using static SAM.Picker.InvariantShorthand;
using APITypes = SAM.API.Types;

namespace SAM.Picker
{
    internal partial class GamePicker : Form
    {
        private readonly API.Client _SteamClient;

        private readonly Dictionary<uint, GameInfo> _Games;
        private readonly List<GameInfo> _FilteredGames;

        private readonly object _LogoLock;
        private readonly HashSet<string> _LogosAttempting;
        private readonly HashSet<string> _LogosAttempted;
        private readonly ConcurrentQueue<GameInfo> _LogoQueue;

        private readonly API.Callbacks.AppDataChanged _AppDataChangedCallback;

        public GamePicker(API.Client client)
        {
            this._Games = new();
            this._FilteredGames = new();
            this._LogoLock = new();
            this._LogosAttempting = new();
            this._LogosAttempted = new();
            this._LogoQueue = new();

            this.InitializeComponent();

            Bitmap blank = new(this._LogoImageList.ImageSize.Width, this._LogoImageList.ImageSize.Height);
            using (var g = Graphics.FromImage(blank))
            {
                g.Clear(Color.DimGray);
            }

            this._LogoImageList.Images.Add("Blank", blank);

            this._SteamClient = client;

            this._AppDataChangedCallback = client.CreateAndRegisterCallback<API.Callbacks.AppDataChanged>();
            this._AppDataChangedCallback.OnRun += this.OnAppDataChanged;

            this.AddGames();
        }

        private void OnAppDataChanged(APITypes.AppDataChanged param)
        {
            if (param.Result == false)
            {
                return;
            }

            GameInfo game;
            lock (this._Games)
            {
                if (this._Games.TryGetValue(param.Id, out game) == false)
                {
                    return;
                }
            }

            game.Name = this._SteamClient.SteamApps001.GetAppData(game.Id, "name");

            this.AddGameToLogoQueue(game);
            this.DownloadNextLogo();
        }

        private void DoDownloadList(object sender, DoWorkEventArgs e)
        {
            this.SetStatusText("Downloading game list...");

            byte[] bytes;
            using (WebClient downloader = CreateDownloader())
            {
                bytes = downloader.DownloadData(new Uri("https://gib.me/sam/games.xml"));
            }

            List<KeyValuePair<uint, string>> pairs = new();
            using (MemoryStream stream = new(bytes, false))
            {
                XPathDocument document = new(stream);
                var navigator = document.CreateNavigator();
                var nodes = navigator.Select("/games/game");
                while (nodes.MoveNext() == true)
                {
                    string type = nodes.Current.GetAttribute("type", "");
                    if (string.IsNullOrEmpty(type) == true)
                    {
                        type = "normal";
                    }
                    pairs.Add(new((uint)nodes.Current.ValueAsLong, type));
                }
            }

            this.SetStatusText("Checking game ownership...");
            foreach (var kv in pairs)
            {
                this.AddGame(kv.Key, kv.Value);
            }
        }

        private void OnDownloadList(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled == true)
            {
                this.AddDefaultGames();
                MessageBox.Show(
                    this,
                    "Downloading the game list was cancelled.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            else if (e.Error != null)
            {
                this.AddDefaultGames();
                MessageBox.Show(
                    this,
                    "Could not download the game list.\n" +
                    "Check your network connection (and proxy settings, if any).\n\n" +
                    "(" + e.Error.Message + ")",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

            this.RefreshGames();
            this._RefreshGamesButton.Enabled = true;
            this._AddGameButton.Enabled = true;
            this.DownloadNextLogo();

            // Ownership is decided by SteamApps008.IsSubscribedApp; when some
            // other tool fakes licences, Steam claims ownership of the whole
            // catalogue and the list becomes meaningless (#468/#410/#581/#592).
            if (this._Games.Count > 5000)
            {
                this.SetStatusText(
                    $"Steam reports {this._Games.Count} owned games, which no account realistically has: " +
                    "something (a licence faking tool) is telling Steam that you own everything.");
            }
        }

        private static WebClient CreateDownloader()
        {
            // Older machine defaults can still be SSL3/TLS1.0.
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            WebClient downloader = new();
            if (WebRequest.DefaultWebProxy != null)
            {
                // Authenticated proxies otherwise fail with 407 (#466).
                downloader.Proxy = WebRequest.DefaultWebProxy;
                downloader.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials;
            }
            return downloader;
        }

        private void SetStatusText(string text)
        {
            if (this.InvokeRequired == true)
            {
                this.BeginInvoke(new Action<string>(this.SetStatusText), text);
                return;
            }

            this._PickerStatusLabel.Text = text;
        }

        /// <param name="selectFirst">
        /// True to pre-select the first game when nothing is selected. Kept false
        /// while typing in the search box so the selection does not jump back to
        /// the top after every key.
        /// </param>
        private void RefreshGames(bool selectFirst = true)
        {
            var nameSearch = this._SearchGameTextBox.Text.Length > 0
                ? this._SearchGameTextBox.Text
                : null;

            var wantNormals = this._FilterGamesMenuItem.Checked == true;
            var wantDemos = this._FilterDemosMenuItem.Checked == true;
            var wantMods = this._FilterModsMenuItem.Checked == true;
            var wantJunk = this._FilterJunkMenuItem.Checked == true;

            this._FilteredGames.Clear();

            List<GameInfo> games = new();
            lock (this._Games)
            {
                games.AddRange(this._Games.Values);
            }

            foreach (var info in games.OrderBy(gi => gi.Name))
            {
                if (nameSearch != null &&
                    info.Name.IndexOf(nameSearch, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                bool wanted = info.Type switch
                {
                    "normal" => wantNormals,
                    "demo" => wantDemos,
                    "mod" => wantMods,
                    "junk" => wantJunk,
                    _ => true,
                };
                if (wanted == false)
                {
                    continue;
                }

                this._FilteredGames.Add(info);
            }

            this._GameListView.VirtualListSize = this._FilteredGames.Count;
            this._PickerStatusLabel.Text =
                $"Displaying {this._GameListView.Items.Count} games. Total {this._Games.Count} games.";

            if (selectFirst == true && this._GameListView.Items.Count > 0)
            {
                // SelectedItems cannot be queried in virtual mode (it throws);
                // the decision to pre-select is therefore made by the caller.
                this._GameListView.Items[0].Selected = true;
                this._GameListView.Select();
            }
        }

        private void OnGameListViewRetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            var info = this._FilteredGames[e.ItemIndex];
            e.Item = info.Item = new()
            {
                Text = info.Name,
                ImageIndex = info.ImageIndex,
            };
        }

        private void OnGameListViewSearchForVirtualItem(object sender, SearchForVirtualItemEventArgs e)
        {
            if (e.Direction != SearchDirectionHint.Down || e.IsTextSearch == false)
            {
                return;
            }

            var count = this._FilteredGames.Count;
            if (count < 1)
            {
                return;
            }

            var text = e.Text;
            int startIndex = e.StartIndex;

            Predicate<GameInfo> predicate;
            /*if (e.IsPrefixSearch == true)*/
            {
                predicate = gi => gi.Name != null && gi.Name.StartsWith(text, StringComparison.CurrentCultureIgnoreCase);
            }
            /*else
            {
                predicate = gi => gi.Name != null && string.Compare(gi.Name, text, StringComparison.CurrentCultureIgnoreCase) == 0;
            }*/

            int index;
            if (startIndex >= count)
            {
                // starting past the end of the list: search from the first item
                index = this._FilteredGames.FindIndex(0, count, predicate);
            }
            else if (startIndex <= 0)
            {
                // starting from the first item
                index = this._FilteredGames.FindIndex(0, count, predicate);
            }
            else
            {
                index = this._FilteredGames.FindIndex(startIndex, count - startIndex, predicate);
                if (index < 0)
                {
                    // wrap around, including the item just before the start index
                    index = this._FilteredGames.FindIndex(0, startIndex, predicate);
                }
            }

            e.Index = index < 0 ? -1 : index;
        }

        private void DoDownloadLogo(object sender, DoWorkEventArgs e)
        {
            var info = (GameInfo)e.Argument;

            lock (this._LogoLock)
            {
                this._LogosAttempted.Add(info.ImageUrl);
                this._LogosAttempting.Remove(info.ImageUrl);
            }

            using (WebClient downloader = CreateDownloader())
            {
                try
                {
                    var data = downloader.DownloadData(new Uri(info.ImageUrl));
                    using (MemoryStream stream = new(data, false))
                    {
                        Bitmap bitmap = new(stream);
                        e.Result = new LogoInfo(info.Id, bitmap);
                    }
                }
                catch (Exception)
                {
                    e.Result = new LogoInfo(info.Id, null);
                }
            }
        }

        private void OnDownloadLogo(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null || e.Cancelled == true)
            {
                this.DownloadNextLogo();
                return;
            }

            if (e.Result is LogoInfo logoInfo &&
                logoInfo.Bitmap != null)
            {
                GameInfo gameInfo;
                lock (this._Games)
                {
                    if (this._Games.TryGetValue(logoInfo.Id, out gameInfo) == false)
                    {
                        gameInfo = null;
                    }
                }

                if (gameInfo != null)
                {
                    this._GameListView.BeginUpdate();
                    var imageIndex = this._LogoImageList.Images.Count;
                    this._LogoImageList.Images.Add(gameInfo.ImageUrl, logoInfo.Bitmap);
                    gameInfo.ImageIndex = imageIndex;
                    this._GameListView.EndUpdate();
                }
            }

            this.DownloadNextLogo();
        }

        private void DownloadNextLogo()
        {
            lock (this._LogoLock)
            {

                if (this._LogoWorker.IsBusy == true)
                {
                    return;
                }

                GameInfo info;
                while (true)
                {
                    if (this._LogoQueue.TryDequeue(out info) == false)
                    {
                        this._DownloadStatusLabel.Visible = false;
                        return;
                    }

                    if (info.Item == null)
                    {
                        // Never release the URL here, the logo would never be retried.
                        lock (this._LogoLock)
                        {
                            this._LogosAttempting.Remove(info.ImageUrl);
                        }
                        continue;
                    }

                    if (this._FilteredGames.Contains(info) == false ||
                        info.Item.Bounds.IntersectsWith(this._GameListView.ClientRectangle) == false)
                    {
                        this._LogosAttempting.Remove(info.ImageUrl);
                        continue;
                    }

                    break;
                }

                this._DownloadStatusLabel.Text = $"Downloading {1 + this._LogoQueue.Count} game icons...";
                this._DownloadStatusLabel.Visible = true;

                this._LogoWorker.RunWorkerAsync(info);
            }
        }

        private string GetGameImageUrl(uint id)
        {
            string candidate;

            var currentLanguage = this._SteamClient.SteamApps008.GetCurrentGameLanguage();

            candidate = this._SteamClient.SteamApps001.GetAppData(id, _($"small_capsule/{currentLanguage}"));
            if (string.IsNullOrEmpty(candidate) == false)
            {
                return _($"https://shared.cloudflare.steamstatic.com/store_item_assets/steam/apps/{id}/{candidate}");
            }

            if (currentLanguage != "english")
            {
                candidate = this._SteamClient.SteamApps001.GetAppData(id, "small_capsule/english");
                if (string.IsNullOrEmpty(candidate) == false)
                {
                    return _($"https://shared.cloudflare.steamstatic.com/store_item_assets/steam/apps/{id}/{candidate}");
                }
            }

            candidate = this._SteamClient.SteamApps001.GetAppData(id, "logo");
            if (string.IsNullOrEmpty(candidate) == false)
            {
                return _($"https://cdn.steamstatic.com/steamcommunity/public/images/apps/{id}/{candidate}.jpg");
            }

            return null;
        }

        private void AddGameToLogoQueue(GameInfo info)
        {
            if (info.ImageIndex > 0)
            {
                return;
            }

            var imageUrl = GetGameImageUrl(info.Id);
            if (string.IsNullOrEmpty(imageUrl) == true)
            {
                return;
            }

            info.ImageUrl = imageUrl;

            lock (this._LogoLock)
            {
                int imageIndex = this._LogoImageList.Images.IndexOfKey(imageUrl);
                if (imageIndex >= 0)
                {
                    info.ImageIndex = imageIndex;
                }
                else if (
                    this._LogosAttempting.Contains(imageUrl) == false &&
                    this._LogosAttempted.Contains(imageUrl) == false)
                {
                    this._LogosAttempting.Add(imageUrl);
                    this._LogoQueue.Enqueue(info);
                }
            }
        }

        private bool OwnsGame(uint id)
        {
            return this._SteamClient.SteamApps008.IsSubscribedApp(id);
        }

        private void AddGame(uint id, string type)
        {
            // Runs on the download worker while the UI thread may read _Games.
            lock (this._Games)
            {
                if (this._Games.ContainsKey(id) == true)
                {
                    return;
                }

                if (this.OwnsGame(id) == false)
                {
                    return;
                }

                GameInfo info = new(id, type);
                info.Name = this._SteamClient.SteamApps001.GetAppData(info.Id, "name");
                this._Games.Add(id, info);
            }
        }

        private void AddGames()
        {
            lock (this._Games)
            {
                this._Games.Clear();
            }
            this._RefreshGamesButton.Enabled = false;
            this._AddGameButton.Enabled = false;
            this._ListWorker.RunWorkerAsync();
        }

        private void AddDefaultGames()
        {
            this.AddGame(480, "normal"); // Spacewar
        }

        private void OnTimer(object sender, EventArgs e)
        {
            this._CallbackTimer.Enabled = false;
            this._SteamClient.RunCallbacks(false);
            this._CallbackTimer.Enabled = true;
        }

        private void OnActivateGame(object sender, EventArgs e)
        {
            var focusedItem = (sender as MyListView)?.FocusedItem;
            var index = focusedItem != null ? focusedItem.Index : -1;
            if (index < 0 || index >= this._FilteredGames.Count)
            {
                return;
            }

            var info = this._FilteredGames[index];
            if (info == null)
            {
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo()
                {
                    FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SAM.Game.exe"),
                    Arguments = info.Id.ToString(CultureInfo.InvariantCulture),
                    UseShellExecute = true,
                });
            }
            catch (Win32Exception)
            {
                MessageBox.Show(
                    this,
                    "Failed to start SAM.Game.exe.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void OnRefresh(object sender, EventArgs e)
        {
            this._AddGameTextBox.Text = "";
            this.AddGames();
        }

        private void OnAddGame(object sender, EventArgs e)
        {
            if (this._ListWorker.IsBusy == true)
            {
                // _Games is being filled by the worker; adding now would corrupt it.
                return;
            }

            uint id;

            if (uint.TryParse(this._AddGameTextBox.Text, out id) == false)
            {
                MessageBox.Show(
                    this,
                    "Please enter a valid game ID.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            if (this.OwnsGame(id) == false)
            {
                MessageBox.Show(this, "You don't own that game.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            lock (this._LogoLock)
            {
                while (this._LogoQueue.TryDequeue(out var logo) == true)
                {
                    // clear the download queue because we will be showing only one app
                    this._LogosAttempted.Remove(logo.ImageUrl);
                    this._LogosAttempting.Remove(logo.ImageUrl);
                }
            }

            this._AddGameTextBox.Text = "";
            lock (this._Games)
            {
                this._Games.Clear();
            }
            this.AddGame(id, "normal");
            this._FilterGamesMenuItem.Checked = true;
            this.RefreshGames();
            this.DownloadNextLogo();
        }

        private void OnFilterUpdate(object sender, EventArgs e)
        {
            // The search box refreshes on every key: leave the selection alone and
            // keep the focus in the box. A filter change selects the first game,
            // like it always did.
            this.RefreshGames(selectFirst: sender != this._SearchGameTextBox);

            // Compatibility with _GameListView SearchForVirtualItemEventHandler (otherwise _SearchGameTextBox loose focus on KeyUp)
            this._SearchGameTextBox.Focus();
        }

        private void OnGameListViewDrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = true;

            if (e.Item.Bounds.IntersectsWith(this._GameListView.ClientRectangle) == false)
            {
                return;
            }

            var info = this._FilteredGames[e.ItemIndex];
            if (info.ImageIndex <= 0)
            {
                this.AddGameToLogoQueue(info);
                this.DownloadNextLogo();
            }
        }
    }
}
