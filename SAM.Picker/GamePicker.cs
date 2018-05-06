/* Copyright (c) 2017 Rick (rick 'at' gibbed 'dot' us)
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

        private readonly List<GameInfo> _Games;
        private readonly List<GameInfo> _FilteredGames;
        private int _SelectedGameIndex;

        public List<GameInfo> Games
        {
            get { return _Games; }
        }

        private readonly List<string> _LogosAttempted;
        private readonly ConcurrentQueue<GameInfo> _LogoQueue;

        // ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
        private readonly API.Callbacks.AppDataChanged _AppDataChangedCallback;
        // ReSharper restore PrivateFieldCanBeConvertedToLocalVariable

        public GamePicker(API.Client client)
        {
            this._Games = new List<GameInfo>();
            this._FilteredGames = new List<GameInfo>();
            this._SelectedGameIndex = -1;
            this._LogosAttempted = new List<string>();
            this._LogoQueue = new ConcurrentQueue<GameInfo>();

            this.InitializeComponent();

            var blank = new Bitmap(this._LogoImageList.ImageSize.Width, this._LogoImageList.ImageSize.Height);
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
            if (param.Result == true)
            {
                foreach (GameInfo info in this._Games)
                {
                    if (info.Id == param.Id)
                    {
                        info.Name = this._SteamClient.SteamApps001.GetAppData(info.Id, "name");
                        this.AddGameToLogoQueue(info);
                        break;
                    }
                }
            }
        }

        private void DoDownloadList(object sender, DoWorkEventArgs e)
        {
            var pairs = new List<KeyValuePair<uint, string>>();
            byte[] bytes;
            using (var downloader = new WebClient())
            {
                bytes = downloader.DownloadData(new Uri(string.Format("http://gib.me/sam/games.xml")));
            }
            using (var stream = new MemoryStream(bytes, false))
            {
                var document = new XPathDocument(stream);
                var navigator = document.CreateNavigator();
                var nodes = navigator.Select("/games/game");
                while (nodes.MoveNext())
                {
                    string type = nodes.Current.GetAttribute("type", "");
                    if (type == string.Empty)
                    {
                        type = "normal";
                    }
                    pairs.Add(new KeyValuePair<uint, string>((uint)nodes.Current.ValueAsLong, type));
                }
            }
            e.Result = pairs;
        }

        private void OnDownloadList(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null && e.Cancelled == false)
            {
                var pairs = (List<KeyValuePair<uint, string>>)e.Result;
                foreach (var kv in pairs)
                {
                    this.AddGame(kv.Key, kv.Value);
                }
            }
            else
            {
                this.AddDefaultGames();
                //MessageBox.Show(e.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            this.RefreshGames();
            this._RefreshGamesButton.Enabled = true;
            this.unlockAllGames.Enabled = true;
            this.DownloadNextLogo();
        }

        private void RefreshGames()
        {
            this._FilteredGames.Clear();
            foreach (var info in this._Games.OrderBy(gi => gi.Name))
            {
                if (info.Type == "normal" && _FilterGamesMenuItem.Checked == false)
                {
                    continue;
                }
                if (info.Type == "demo" && this._FilterDemosMenuItem.Checked == false)
                {
                    continue;
                }
                if (info.Type == "mod" && this._FilterModsMenuItem.Checked == false)
                {
                    continue;
                }
                if (info.Type == "junk" && this._FilterJunkMenuItem.Checked == false)
                {
                    continue;
                }
                this._FilteredGames.Add(info);
            }

            this._GameListView.BeginUpdate();
            this._GameListView.VirtualListSize = this._FilteredGames.Count;
            if (this._FilteredGames.Count > 0)
            {
                this._GameListView.RedrawItems(0, this._FilteredGames.Count - 1, true);
            }
            this._GameListView.EndUpdate();
            this._PickerStatusLabel.Text = string.Format(
                "Displaying {0} games. Total {1} games.",
                this._GameListView.Items.Count,
                this._Games.Count);
        }

        private void OnGameListViewRetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            var info = this._FilteredGames[e.ItemIndex];
            e.Item = new ListViewItem()
            {
                Text = info.Name,
                ImageIndex = info.ImageIndex,
            };
        }

        private void DoDownloadLogo(object sender, DoWorkEventArgs e)
        {
            var info = (GameInfo)e.Argument;
            var logoPath = string.Format(
                "http://media.steamcommunity.com/steamcommunity/public/images/apps/{0}/{1}.jpg",
                info.Id,
                info.Logo);
            using (var downloader = new WebClient())
            {
                var data = downloader.DownloadData(new Uri(logoPath));

                try
                {
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
            var gameInfo = this._Games.FirstOrDefault(gi => gi.Id == logoInfo.Id);
            if (gameInfo != null && logoInfo.Bitmap != null)
            {
                this._GameListView.BeginUpdate();
                var imageIndex = this._LogoImageList.Images.Count;
                this._LogoImageList.Images.Add(gameInfo.Logo, logoInfo.Bitmap);
                gameInfo.ImageIndex = imageIndex;
                this._GameListView.EndUpdate();
            }

            this.DownloadNextLogo();
        }

        private void DownloadNextLogo()
        {
            if (this._LogoWorker.IsBusy == true)
            {
                return;
            }

            GameInfo info;
            if (this._LogoQueue.TryDequeue(out info) == false)
            {
                this._DownloadStatusLabel.Visible = false;
                return;
            }

            this._DownloadStatusLabel.Text = string.Format(
                "Downloading {0} game icons...",
                this._LogoQueue.Count);
            this._DownloadStatusLabel.Visible = true;

            this._LogoWorker.RunWorkerAsync(info);
        }

        private void AddGameToLogoQueue(GameInfo info)
        {
            string logo = this._SteamClient.SteamApps001.GetAppData(info.Id, "logo");

            if (logo == null)
            {
                return;
            }

            info.Logo = logo;

            int imageIndex = this._LogoImageList.Images.IndexOfKey(logo);
            if (imageIndex >= 0)
            {
                info.ImageIndex = imageIndex;
            }
            else if (this._LogosAttempted.Contains(logo) == false)
            {
                this._LogosAttempted.Add(logo);
                this._LogoQueue.Enqueue(info);
                this.DownloadNextLogo();
            }
        }

        private bool OwnsGame(uint id)
        {
            return this._SteamClient.SteamApps003.IsSubscribedApp(id);
        }

        private void AddGame(uint id, string type)
        {
            if (this._Games.Any(i => i.Id == id) == true)
            {
                return;
            }

            if (this.OwnsGame(id) == false)
            {
                return;
            }

            var info = new GameInfo(id, type);
            info.Name = this._SteamClient.SteamApps001.GetAppData(info.Id, "name");

            this._Games.Add(info);
            this.AddGameToLogoQueue(info);
        }

        private void AddGames()
        {
            this._Games.Clear();
            this._RefreshGamesButton.Enabled = false;
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

        private void OnSelectGame(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (e.IsSelected == true && e.ItemIndex != this._SelectedGameIndex)
            {
                this._SelectedGameIndex = e.ItemIndex;
            }
            else if (e.IsSelected == true && e.ItemIndex == this._SelectedGameIndex)
            {
                this._SelectedGameIndex = -1;
            }
        }

        private void OnActivateGame(object sender, EventArgs e)
        {
            if (this._SelectedGameIndex < 0)
            {
                return;
            }

            var index = this._SelectedGameIndex;
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
            this._AddGameTextBox.Text = "";
            this.AddGames();
        }

        private void OnAddGame(object sender, EventArgs e)
        {
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

            this._AddGameTextBox.Text = "";
            this._Games.Clear();
            this.AddGame(id, "normal");
            this._FilterGamesMenuItem.Checked = true;
            this.RefreshGames();
            this.DownloadNextLogo();
        }

        private void OnFilterUpdate(object sender, EventArgs e)
        {
            this.RefreshGames();
        }

        private void unlockAllGames_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(
                "This will open and close A LOT of windows.\n\nIn your case, it could be " + Games.Count + " windows.\n\nWhile this shouldn't cause a performance drop, it might get annoying if you're trying to do something.\n\nIs this OK?",
                "Warning",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning) != DialogResult.No)
            {
                unlockAllProgress.Visible = true;
                unlockAllProgress.Value = 0;
                unlockAllProgress.Maximum = Games.Count;

                foreach (var Game in Games)
                {
                    unlockAllProgress.Value++;
                    try
                    {
                        var process = Process.Start("SAM.Game.exe", Game.Id.ToString(CultureInfo.InvariantCulture) + " auto");

                        if (process != null && process.HasExited != true)
                        {
                            process.WaitForExit();
                        }
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

                unlockAllProgress.Visible = false;
            }
        }
    }
}
