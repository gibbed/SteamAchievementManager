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
using System.Threading;
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

        // The picker owns only sessions it started. They intentionally stay
        // alive if the picker is closed, just like the normal SAM.Game window.
        private readonly Dictionary<uint, IdleSession> _IdleSessions;

        private ToolStripStatusLabel _IdleStatusLabel;
        private ToolStripButton _RunningSessionsButton;
        private ToolStripButton _StopIdleSessionsButton;
        private RunningSessionsForm _RunningSessionsForm;

        private readonly API.Callbacks.AppDataChanged _AppDataChangedCallback;

        public GamePicker(API.Client client)
        {
            this._Games = new();
            this._FilteredGames = new();
            this._LogoLock = new();
            this._LogosAttempting = new();
            this._LogosAttempted = new();
            this._LogoQueue = new();
            this._IdleSessions = new();

            this.InitializeComponent();
            this.InitializeBatchControls();

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

        private void InitializeBatchControls()
        {
            var batchButton = new ToolStripSplitButton("Start selected (idle)")
            {
                ToolTipText = "Start the selected games without opening a SAM window for each one.",
            };
            batchButton.ButtonClick += this.OnStartSelectedIdleGames;

            var startIdleItem = new ToolStripMenuItem("Start selected (idle)");
            startIdleItem.Click += this.OnStartSelectedIdleGames;
            var startWindowsItem = new ToolStripMenuItem("Open selected managers");
            startWindowsItem.Click += this.OnOpenSelectedManagers;
            var stopSelectedItem = new ToolStripMenuItem("Stop selected idle sessions");
            stopSelectedItem.Click += this.OnStopSelectedIdleGames;
            var stopAllItem = new ToolStripMenuItem("Stop all idle sessions started here");
            stopAllItem.Click += this.OnStopAllIdleGames;
            batchButton.DropDownItems.AddRange(new ToolStripItem[]
            {
                startIdleItem,
                startWindowsItem,
                new ToolStripSeparator(),
                stopSelectedItem,
                stopAllItem,
            });

            this._PickerToolStrip.Items.Insert(this._PickerToolStrip.Items.Count - 1, new ToolStripSeparator());
            this._PickerToolStrip.Items.Insert(this._PickerToolStrip.Items.Count - 1, batchButton);

            this._RunningSessionsButton = new ToolStripButton("Running sessions")
            {
                ToolTipText = "Show the games currently running in the background and their elapsed time.",
            };
            this._RunningSessionsButton.Click += this.OnShowRunningSessions;
            this._PickerToolStrip.Items.Insert(this._PickerToolStrip.Items.Count - 1, this._RunningSessionsButton);

            this._StopIdleSessionsButton = new ToolStripButton("Stop idle sessions")
            {
                Enabled = false,
                ToolTipText = "Gracefully stop every background game session started by this picker.",
            };
            this._StopIdleSessionsButton.Click += this.OnStopAllIdleGames;
            this._PickerToolStrip.Items.Insert(this._PickerToolStrip.Items.Count - 1, this._StopIdleSessionsButton);

            this._IdleStatusLabel = new ToolStripStatusLabel
            {
                BorderSides = ToolStripStatusLabelBorderSides.Left,
                Padding = new Padding(8, 0, 0, 0),
            };
            this._PickerStatusStrip.Items.Add(this._IdleStatusLabel);
            this.FormClosing += this.OnFormClosing;
            this.UpdateIdleSessionControls();
        }

        private sealed class IdleSession : IDisposable
        {
            public readonly uint GameId;
            public readonly string GameName;
            public readonly DateTimeOffset StartedAt;
            public readonly Process Process;
            public readonly EventWaitHandle StopEvent;

            public IdleSession(uint gameId, string gameName, DateTimeOffset startedAt, Process process, EventWaitHandle stopEvent)
            {
                this.GameId = gameId;
                this.GameName = gameName;
                this.StartedAt = startedAt;
                this.Process = process;
                this.StopEvent = stopEvent;
            }

            public void Dispose()
            {
                this.StopEvent.Dispose();
                this.Process.Dispose();
            }
        }

        private static bool IsRunning(IdleSession session)
        {
            try
            {
                return session.Process.HasExited == false;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        private void UpdateIdleSessionControls()
        {
            var running = this._IdleSessions.Values.Count(IsRunning);
            this._IdleStatusLabel.Text = running > 0
                ? $"● {running} game{(running == 1 ? "" : "s")} running"
                : "● No idle games running";
            this._IdleStatusLabel.ForeColor = running > 0 ? Color.ForestGreen : Color.Firebrick;
            this._RunningSessionsButton.Text = running > 0
                ? $"Running sessions ({running})"
                : "Running sessions";
            this._StopIdleSessionsButton.Text = running > 0
                ? $"Stop {running} idle game{(running == 1 ? "" : "s")}"
                : "Stop idle sessions";
            this._StopIdleSessionsButton.Enabled = running > 0;
        }

        private IReadOnlyList<RunningSessionInfo> GetRunningSessions()
        {
            return this._IdleSessions.Values
                .Where(IsRunning)
                .OrderBy(session => session.StartedAt)
                .Select(session => new RunningSessionInfo(
                    session.GameId,
                    session.GameName,
                    session.StartedAt))
                .ToList();
        }

        private void OnShowRunningSessions(object sender, EventArgs e)
        {
            if (this._RunningSessionsForm == null || this._RunningSessionsForm.IsDisposed == true)
            {
                this._RunningSessionsForm = new RunningSessionsForm(this.GetRunningSessions);
                this._RunningSessionsForm.FormClosed += (formSender, formEventArgs) => this._RunningSessionsForm = null;
                this._RunningSessionsForm.Show(this);
                return;
            }

            this._RunningSessionsForm.BringToFront();
            this._RunningSessionsForm.Activate();
        }

        private List<GameInfo> GetSelectedGames()
        {
            List<GameInfo> games = new();
            foreach (int index in this._GameListView.SelectedIndices)
            {
                if (index >= 0 && index < this._FilteredGames.Count)
                {
                    games.Add(this._FilteredGames[index]);
                }
            }

            return games;
        }

        private void OnStartSelectedIdleGames(object sender, EventArgs e)
        {
            this.StartIdleGames(this.GetSelectedGames(), "Select at least one game first.");
        }

        private void StartIdleGames(IEnumerable<GameInfo> candidates, string noGamesMessage)
        {
            var games = candidates.ToList();
            if (games.Count == 0)
            {
                MessageBox.Show(this, noGamesMessage, "No games selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var started = 0;
            var alreadyRunning = 0;
            foreach (var game in games)
            {
                if (this._IdleSessions.TryGetValue(game.Id, out var existing) && IsRunning(existing) == true)
                {
                    alreadyRunning++;
                    continue;
                }
                else if (existing != null)
                {
                    existing.Dispose();
                    this._IdleSessions.Remove(game.Id);
                }

                EventWaitHandle stopEvent = null;
                try
                {
                    var stopEventName = @"Local\SAM.Idle.Stop." + Guid.NewGuid().ToString("N");
                    stopEvent = new EventWaitHandle(false, EventResetMode.ManualReset, stopEventName);
                    var process = Process.Start(new ProcessStartInfo
                    {
                        FileName = Path.Combine(Application.StartupPath, "SAM.Game.exe"),
                        Arguments = $"--idle {game.Id.ToString(CultureInfo.InvariantCulture)} {stopEventName}",
                        WorkingDirectory = Application.StartupPath,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                    });

                    if (process == null)
                    {
                        stopEvent.Dispose();
                        continue;
                    }

                    process.EnableRaisingEvents = true;
                    process.Exited += this.OnIdleSessionExited;
                    this._IdleSessions[game.Id] = new IdleSession(
                        game.Id,
                        game.Name ?? game.Id.ToString(CultureInfo.InvariantCulture),
                        DateTimeOffset.UtcNow,
                        process,
                        stopEvent);
                    started++;
                }
                catch (Exception ex)
                {
                    stopEvent?.Dispose();
                    MessageBox.Show(
                        this,
                        $"Failed to start SAM.Game.exe for {game.Name}.\n\n{ex.Message}",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }

            this._PickerStatusLabel.Text = $"Started {started} idle game session(s). Already running: {alreadyRunning}.";
            this.UpdateIdleSessionControls();
        }

        private void OnOpenSelectedManagers(object sender, EventArgs e)
        {
            var games = this.GetSelectedGames();
            if (games.Count == 0)
            {
                MessageBox.Show(this, "Select at least one game first.", "No games selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            foreach (var game in games)
            {
                try
                {
                    Process.Start(Path.Combine(Application.StartupPath, "SAM.Game.exe"), game.Id.ToString(CultureInfo.InvariantCulture));
                }
                catch (Win32Exception)
                {
                    MessageBox.Show(this, "Failed to start SAM.Game.exe.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
        }

        private void OnStopSelectedIdleGames(object sender, EventArgs e)
        {
            this.StopIdleSessions(this.GetSelectedGames().Select(game => game.Id));
        }

        private void OnStopAllIdleGames(object sender, EventArgs e)
        {
            this.StopIdleSessions(this._IdleSessions.Keys.ToList());
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            var running = this._IdleSessions.Values.Count(IsRunning);
            if (running > 0 && e.CloseReason == CloseReason.UserClosing)
            {
                var result = MessageBox.Show(
                    this,
                    $"There {(running == 1 ? "is" : "are")} {running} idle game " +
                    $"session{(running == 1 ? "" : "s")} still running.\n\n" +
                    "Closing Steam Achievement Manager will stop all of them. Do you want to close?",
                    "Running idle sessions",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }

            // The picker owns its background sessions. Closing its control
            // window must not leave unseen SAM.Game processes behind.
            this.StopIdleSessions(this._IdleSessions.Keys.ToList());
        }

        private void StopIdleSessions(IEnumerable<uint> gameIds)
        {
            var sessions = gameIds
                .Where(this._IdleSessions.ContainsKey)
                .Select(gameId => this._IdleSessions[gameId])
                .Where(IsRunning)
                .Distinct()
                .ToList();

            foreach (var session in sessions)
            {
                try
                {
                    session.StopEvent.Set();
                }
                catch (ObjectDisposedException)
                {
                    // The process exited and its cleanup already ran.
                }
            }

            var stopped = 0;
            foreach (var session in sessions)
            {
                try
                {
                    if (session.Process.WaitForExit(3000) == false)
                    {
                        session.Process.Kill();
                        session.Process.WaitForExit(3000);
                    }

                    stopped++;
                }
                catch (InvalidOperationException)
                {
                    // The process exited between signalling and waiting.
                }
            }

            this._PickerStatusLabel.Text = $"Stopped {stopped} idle game session(s).";
            this.UpdateIdleSessionControls();
        }

        private void OnIdleSessionExited(object sender, EventArgs e)
        {
            if (sender is not Process exitedProcess || this.IsDisposed == true)
            {
                return;
            }

            this.BeginInvoke((Action)(() =>
            {
                var session = this._IdleSessions.FirstOrDefault(pair => ReferenceEquals(pair.Value.Process, exitedProcess)).Value;
                if (session != null)
                {
                    var gameId = this._IdleSessions.First(pair => ReferenceEquals(pair.Value, session)).Key;
                    this._IdleSessions.Remove(gameId);
                    session.Dispose();
                }

                this.UpdateIdleSessionControls();
            }));
        }

        private void OnAppDataChanged(APITypes.AppDataChanged param)
        {
            if (param.Result == false)
            {
                return;
            }

            if (this._Games.TryGetValue(param.Id, out var game) == false)
            {
                return;
            }

            game.Name = this._SteamClient.SteamApps001.GetAppData(game.Id, "name");

            this.AddGameToLogoQueue(game);
            this.DownloadNextLogo();
        }

        private void DoDownloadList(object sender, DoWorkEventArgs e)
        {
            this._PickerStatusLabel.Text = "Downloading game list...";

            byte[] bytes;
            using (WebClient downloader = new())
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

            this._PickerStatusLabel.Text = "Checking game ownership...";
            foreach (var kv in pairs)
            {
                this.AddGame(kv.Key, kv.Value);
            }
        }

        private void OnDownloadList(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null || e.Cancelled == true)
            {
                this.AddDefaultGames();
                MessageBox.Show(e.Error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            this.RefreshGames();
            this._RefreshGamesButton.Enabled = true;
            this.DownloadNextLogo();
        }

        private void RefreshGames()
        {
            var nameSearch = this._SearchGameTextBox.Text.Length > 0
                ? this._SearchGameTextBox.Text
                : null;

            var wantNormals = this._FilterGamesMenuItem.Checked == true;
            var wantDemos = this._FilterDemosMenuItem.Checked == true;
            var wantMods = this._FilterModsMenuItem.Checked == true;
            var wantJunk = this._FilterJunkMenuItem.Checked == true;

            List<GameInfo> filteredGames = new();
            foreach (var info in this._Games.Values.OrderBy(gi => gi.Name))
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

                filteredGames.Add(info);
            }

            // A virtual ListView can request an item while its data source is
            // being replaced. First detach the old range, then replace the
            // list, and only then expose the new range.
            this._GameListView.BeginUpdate();
            try
            {
                this._GameListView.VirtualListSize = 0;
                this._FilteredGames.Clear();
                this._FilteredGames.AddRange(filteredGames);
                this._GameListView.VirtualListSize = this._FilteredGames.Count;
            }
            finally
            {
                this._GameListView.EndUpdate();
            }

            this._PickerStatusLabel.Text =
                $"Displaying {this._FilteredGames.Count} games. Total {this._Games.Count} games.";
        }

        private void OnGameListViewRetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            // WinForms can request an item from the old virtual range while a
            // filter is replacing the list. Returning an empty item prevents
            // a stale request from bringing down the picker.
            if (e.ItemIndex < 0 || e.ItemIndex >= this._FilteredGames.Count)
            {
                e.Item = new ListViewItem();
                return;
            }

            var info = this._FilteredGames[e.ItemIndex];
            e.Item = info.Item = new()
            {
                Text = info.Name,
                ImageIndex = info.ImageIndex,
            };
        }

        private void DoDownloadLogo(object sender, DoWorkEventArgs e)
        {
            var info = (GameInfo)e.Argument;

            this._LogosAttempted.Add(info.ImageUrl);

            using (WebClient downloader = new())
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
                return;
            }

            if (e.Result is LogoInfo logoInfo &&
                logoInfo.Bitmap != null &&
                this._Games.TryGetValue(logoInfo.Id, out var gameInfo) == true)
            {
                this._GameListView.BeginUpdate();
                var imageIndex = this._LogoImageList.Images.Count;
                this._LogoImageList.Images.Add(gameInfo.ImageUrl, logoInfo.Bitmap);
                gameInfo.ImageIndex = imageIndex;
                this._GameListView.EndUpdate();
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

        private bool OwnsGame(uint id)
        {
            return this._SteamClient.SteamApps008.IsSubscribedApp(id);
        }

        private void AddGame(uint id, string type)
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

            while (this._LogoQueue.TryDequeue(out var logo) == true)
            {
                // clear the download queue because we will be showing only one app
                this._LogosAttempted.Remove(logo.ImageUrl);
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

            // Keep the text box focused so consecutive keystrokes keep filtering.
            this._SearchGameTextBox.Focus();
        }

        private void OnGameListViewDrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = true;

            // The paint message may have been queued before RefreshGames
            // changed VirtualListSize.
            if (e.ItemIndex < 0 || e.ItemIndex >= this._FilteredGames.Count)
            {
                return;
            }

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
