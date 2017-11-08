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

        private readonly WebClient _GameListDownloader = new WebClient();
        private readonly WebClient _LogoDownloader = new WebClient();
        private List<GameInfo> _Games = new List<GameInfo>();

        public List<GameInfo> Games
        {
            get { return _Games; }
        }

        private readonly List<GameInfo> _LogoQueue = new List<GameInfo>();

        // ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
        private readonly API.Callbacks.AppDataChanged _AppDataChangedCallback;
        // ReSharper restore PrivateFieldCanBeConvertedToLocalVariable

        public GamePicker(API.Client client)
        {
            this.InitializeComponent();

            this._GameLogoImageList.Images.Add(
                "Blank",
                new Bitmap(this._GameLogoImageList.ImageSize.Width, this._GameLogoImageList.ImageSize.Height));

            this._GameListDownloader.DownloadDataCompleted += this.OnGameListDownload;
            this._LogoDownloader.DownloadDataCompleted += this.OnLogoDownload;

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
                        info.Name = this._SteamClient.SteamApps001.GetAppData((uint)info.Id, "name");
                        this.AddGameToLogoQueue(info);
                        this._GameListView.Sort();
                        this._GameListView.Update();
                        break;
                    }
                }
            }
        }

        private void OnGameListDownload(object sender, DownloadDataCompletedEventArgs e)
        {
            if (e.Error == null && e.Cancelled == false)
            {
                using (var stream = new MemoryStream())
                {
                    stream.Write(e.Result, 0, e.Result.Length);
                    stream.Seek(0, SeekOrigin.Begin);

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

                        this.AddGame(nodes.Current.ValueAsLong, type);
                    }
                }
            }
            else
            {
                this.AddDefaultGames();
                //MessageBox.Show(e.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            this.RefreshGames();
            this._RefreshGamesButton.Enabled = true;
            this.DownloadNextLogo();
        }

        private void RefreshGames()
        {
            this._GameListView.BeginUpdate();
            this._GameListView.Items.Clear();

            foreach (GameInfo info in this._Games)
            {
                if (info.Type == "normal" &&
                    _FilterGamesMenuItem.Checked == false)
                {
                    continue;
                }

                if (info.Type == "demo" &&
                    this._FilterDemosMenuItem.Checked == false)
                {
                    continue;
                }

                if (info.Type == "mod" &&
                    this._FilterModsMenuItem.Checked == false)
                {
                    continue;
                }

                if (info.Type == "junk" &&
                    this._FilterJunkMenuItem.Checked == false)
                {
                    continue;
                }

                this._GameListView.Items.Add(info.Item);
            }

            this._GameListView.EndUpdate();
            this._PickerStatusLabel.Text = string.Format(
                "Displaying {0} games. Total {1} games.",
                this._GameListView.Items.Count,
                this._Games.Count);
        }

        private void OnLogoDownload(object sender, DownloadDataCompletedEventArgs e)
        {
            if (e.Error == null && e.Cancelled == false)
            {
                var info = e.UserState as GameInfo;
                if (info != null)
                {
                    Image logo;
                    try
                    {
                        using (var stream = new MemoryStream())
                        {
                            stream.Write(e.Result, 0, e.Result.Length);
                            logo = new Bitmap(stream);
                        }
                    }
                    catch
                    {
                        logo = null;
                    }

                    if (logo != null)
                    {
                        info.ImageIndex = this._GameLogoImageList.Images.Count;
                        this._GameLogoImageList.Images.Add(info.Logo, logo);
                        this._GameListView.Update();
                    }
                }
            }

            this.DownloadNextLogo();
        }

        private void DownloadNextLogo()
        {
            if (this._LogoQueue.Count == 0)
            {
                this._DownloadStatusLabel.Visible = false;
                return;
            }

            if (this._LogoDownloader.IsBusy)
            {
                return;
            }

            this._DownloadStatusLabel.Text = string.Format(
                "Downloading {0} game icons...",
                this._LogoQueue.Count);
            this._DownloadStatusLabel.Visible = true;

            GameInfo info = this._LogoQueue[0];
            this._LogoQueue.RemoveAt(0);

            var logoPath = string.Format(
                "http://media.steamcommunity.com/steamcommunity/public/images/apps/{0}/{1}.jpg",
                info.Id,
                info.Logo);
            this._LogoDownloader.DownloadDataAsync(new Uri(logoPath), info);
        }

        private void AddGameToLogoQueue(GameInfo info)
        {
            string logo = this._SteamClient.SteamApps001.GetAppData((uint)info.Id, "logo");

            if (logo == null)
            {
                return;
            }

            info.Logo = logo;

            int imageIndex = this._GameLogoImageList.Images.IndexOfKey(logo);
            if (imageIndex >= 0)
            {
                info.ImageIndex = imageIndex;
            }
            else
            {
                this._LogoQueue.Add(info);
                this.DownloadNextLogo();
            }
        }

        private bool OwnsGame(long id)
        {
            return this._SteamClient.SteamApps003.IsSubscribedApp(id);
        }

        private void AddGame(long id, string type)
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
            info.Name = this._SteamClient.SteamApps001.GetAppData((uint)info.Id, "name");

            this._Games.Add(info);
            this.AddGameToLogoQueue(info);
        }

        private void AddGames()
        {
            this._GameListView.Items.Clear();
            this._Games = new List<GameInfo>();
            this._GameListDownloader.DownloadDataAsync(new Uri(string.Format("http://gib.me/sam/games.xml")));
            this._RefreshGamesButton.Enabled = false;
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

        private void OnSelectGame(object sender, EventArgs e)
        {
            if (this._GameListView.SelectedItems.Count == 0)
            {
                return;
            }

            var info = this._GameListView.SelectedItems[0].Tag as GameInfo;
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
            long id;

            if (long.TryParse(this._AddGameTextBox.Text, out id) == false)
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
            this._GameListView.Items.Clear();
            this._Games = new List<GameInfo>();
            this.AddGame(id, "normal");
            this._FilterGamesMenuItem.Checked = true;
            this.RefreshGames();
            this.DownloadNextLogo();
        }

        private void OnFilterUpdate(object sender, EventArgs e)
        {
            this.RefreshGames();
        }
    }
}
