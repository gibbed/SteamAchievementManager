/* Copyright (c) 2019 Rick (rick 'at' gibbed 'dot' us)
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
using APITypes = SAM.API.Types;

namespace SAM.Picker
{
    internal partial class GamePicker : Form
    {
        private readonly API.Client _SteamClient;

        private readonly Dictionary<uint, GameInfo> _Games;
        private readonly List<GameInfo> _FilteredGames;
        private int _SelectedGameIndex;

        private readonly List<string> _LogosAttempted;
        private readonly ConcurrentQueue<GameInfo> _LogoQueue;

        private readonly API.Callbacks.AppDataChanged _AppDataChangedCallback;

        public GamePicker(API.Client client)
        {
            _Games = new Dictionary<uint, GameInfo>();
            _FilteredGames = new List<GameInfo>();
            _SelectedGameIndex = -1;
            _LogosAttempted = new List<string>();
            _LogoQueue = new ConcurrentQueue<GameInfo>();

            InitializeComponent();

            var blank = new Bitmap(_LogoImageList.ImageSize.Width, _LogoImageList.ImageSize.Height);
            using (var g = Graphics.FromImage(blank))
            {
                g.Clear(Color.DimGray);
            }

            _LogoImageList.Images.Add("Blank", blank);

            _SteamClient = client;

            _AppDataChangedCallback = client.CreateAndRegisterCallback<API.Callbacks.AppDataChanged>();
            _AppDataChangedCallback.OnRun += OnAppDataChanged;

            AddGames();
        }

        private void OnAppDataChanged(APITypes.AppDataChanged param)
        {
            if (param.Result == true && _Games.ContainsKey(param.Id))
            {
                var game = _Games[param.Id];

                game.Name = _SteamClient.SteamApps001.GetAppData(game.Id, "name");
                AddGameToLogoQueue(game);
                DownloadNextLogo();
            }
        }

        private void DoDownloadList(object sender, DoWorkEventArgs e)
        {
            var pairs = new List<KeyValuePair<uint, string>>();
            byte[] bytes;
            using (var downloader = new WebClient())
            {
                bytes = downloader.DownloadData(new Uri("http://gib.me/sam/games.xml"));
            }
            using (var stream = new MemoryStream(bytes, false))
            {
                var document = new XPathDocument(stream);
                var navigator = document.CreateNavigator();
                var nodes = navigator.Select("/games/game");
                while (nodes.MoveNext())
                {
                    string type = nodes.Current.GetAttribute("type", "");
                    if (string.IsNullOrEmpty(type) == true)
                    {
                        type = "normal";
                    }
                    pairs.Add(new KeyValuePair<uint, string>((uint)nodes.Current.ValueAsLong, type));
                }
            }

            foreach (var kv in pairs)
            {
                AddGame(kv.Key, kv.Value);
            }
        }

        private void OnDownloadList(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null || e.Cancelled)
            {
                AddDefaultGames();
                //MessageBox.Show(e.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            RefreshGames();
            _RefreshGamesButton.Enabled = true;
            DownloadNextLogo();
        }

        private void RefreshGames()
        {
            _SelectedGameIndex = -1;
            _FilteredGames.Clear();
            foreach (var info in _Games.Values.OrderBy(gi => gi.Name))
            {
                if (info.Type == "normal" && _FilterGamesMenuItem.Checked == false)
                {
                    continue;
                }
                if (info.Type == "demo" && _FilterDemosMenuItem.Checked == false)
                {
                    continue;
                }
                if (info.Type == "mod" && _FilterModsMenuItem.Checked == false)
                {
                    continue;
                }
                if (info.Type == "junk" && _FilterJunkMenuItem.Checked == false)
                {
                    continue;
                }
                _FilteredGames.Add(info);
                AddGameToLogoQueue(info);
            }

            _GameListView.VirtualListSize = _FilteredGames.Count;
            _PickerStatusLabel.Text = string.Format(
                CultureInfo.CurrentCulture,
                "Displaying {0} games. Total {1} games.",
                _GameListView.Items.Count,
                _Games.Count);

            if (_GameListView.Items.Count > 0)
            {
                _GameListView.Items[0].Selected = true;
                _GameListView.Select();
            }
        }

        private void OnGameListViewRetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            var info = _FilteredGames[e.ItemIndex];
            e.Item = new ListViewItem()
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

            var count = _FilteredGames.Count;
            if (count < 2)
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
            if (e.StartIndex >= count)
            {
                // starting from the last item in the list
                index = _FilteredGames.FindIndex(0, startIndex - 1, predicate);
            }
            else if (startIndex <= 0)
            {
                // starting from the first item in the list
                index = _FilteredGames.FindIndex(0, count, predicate);
            }
            else
            {
                index = _FilteredGames.FindIndex(startIndex, count - startIndex, predicate);
                if (index < 0)
                {
                    index = _FilteredGames.FindIndex(0, startIndex - 1, predicate);
                }
            }

            e.Index = index < 0 ? -1 : index;
        }

        private void DoDownloadLogo(object sender, DoWorkEventArgs e)
        {
            var info = (GameInfo)e.Argument;
            var logoPath = string.Format(
                CultureInfo.InvariantCulture,
                "https://steamcdn-a.akamaihd.net/steamcommunity/public/images/apps/{0}/{1}.jpg",
                info.Id,
                info.Logo);
            using (var downloader = new WebClient())
            {
                try
                {
                    var data = downloader.DownloadData(new Uri(logoPath));
                    using (var stream = new MemoryStream(data, false))
                    {
                        var bitmap = new Bitmap(stream);
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
                return;
            }

            var logoInfo = (LogoInfo)e.Result;
            if (logoInfo.Bitmap != null && _Games.TryGetValue(logoInfo.Id, out var gameInfo))
            {
                _GameListView.BeginUpdate();
                var imageIndex = _LogoImageList.Images.Count;
                _LogoImageList.Images.Add(gameInfo.Logo, logoInfo.Bitmap);
                gameInfo.ImageIndex = imageIndex;
                _GameListView.EndUpdate();
            }

            DownloadNextLogo();
        }

        private void DownloadNextLogo()
        {
            if (_LogoWorker.IsBusy == true)
            {
                return;
            }

            GameInfo info;
            if (_LogoQueue.TryDequeue(out info) == false)
            {
                _DownloadStatusLabel.Visible = false;
                return;
            }

            _DownloadStatusLabel.Text = string.Format(
                CultureInfo.CurrentCulture,
                "Downloading {0} game icons...",
                _LogoQueue.Count);
            _DownloadStatusLabel.Visible = true;

            _LogoWorker.RunWorkerAsync(info);
        }

        private void AddGameToLogoQueue(GameInfo info)
        {
            string logo = _SteamClient.SteamApps001.GetAppData(info.Id, "logo");

            if (logo == null)
            {
                return;
            }

            info.Logo = logo;

            int imageIndex = _LogoImageList.Images.IndexOfKey(logo);
            if (imageIndex >= 0)
            {
                info.ImageIndex = imageIndex;
            }
            else if (_LogosAttempted.Contains(logo) == false)
            {
                _LogosAttempted.Add(logo);
                _LogoQueue.Enqueue(info);
            }
        }

        private bool OwnsGame(uint id)
        {
            return _SteamClient.SteamApps008.IsSubscribedApp(id);
        }

        private void AddGame(uint id, string type)
        {
            if (_Games.ContainsKey(id))
            {
                return;
            }

            if (OwnsGame(id) == false)
            {
                return;
            }

            var info = new GameInfo(id, type);
            info.Name = _SteamClient.SteamApps001.GetAppData(info.Id, "name");

            _Games.Add(id, info);
        }

        private void AddGames()
        {
            _Games.Clear();
            _RefreshGamesButton.Enabled = false;
            _ListWorker.RunWorkerAsync();
        }

        private void AddDefaultGames()
        {
            AddGame(480, "normal"); // Spacewar
        }

        private void OnTimer(object sender, EventArgs e)
        {
            _CallbackTimer.Enabled = false;
            _SteamClient.RunCallbacks(false);
            _CallbackTimer.Enabled = true;
        }

        private void OnSelectGame(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            _SelectedGameIndex = e.ItemIndex;
        }

        private void OnActivateGame(object sender, EventArgs e)
        {
            var index = _SelectedGameIndex;
            if (index < 0 || index >= _FilteredGames.Count)
            {
                return;
            }

            var info = _FilteredGames[index];
            if (info == null)
            {
                return;
            }

            try
            {
                Process.Start("SAM.Game.exe", info.Id.ToString(CultureInfo.InvariantCulture));
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
            _AddGameTextBox.Text = "";
            AddGames();
        }

        private void OnAddGame(object sender, EventArgs e)
        {
            uint id;

            if (uint.TryParse(_AddGameTextBox.Text, out id) == false)
            {
                MessageBox.Show(
                    this,
                    "Please enter a valid game ID.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            if (OwnsGame(id) == false)
            {
                MessageBox.Show(this, "You don't own that game.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            while (_LogoQueue.TryDequeue(out var logo))
            {
                // clear the download queue because we will be showing only one app
                // TODO: https://github.com/gibbed/SteamAchievementManager/issues/106
                _LogosAttempted.Remove(logo.Logo);
            }

            _AddGameTextBox.Text = "";
            _Games.Clear();
            AddGame(id, "normal");
            _FilterGamesMenuItem.Checked = true;
            RefreshGames();
            DownloadNextLogo();
        }

        private void OnFilterUpdate(object sender, EventArgs e)
        {
            RefreshGames();
        }
    }
}