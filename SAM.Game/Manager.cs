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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using static SAM.Game.InvariantShorthand;
using APITypes = SAM.API.Types;

namespace SAM.Game
{
    internal partial class Manager : Form
    {
        private readonly long _GameId;
        private readonly API.Client _SteamClient;

        private readonly WebClient _IconDownloader = CreateIconDownloader();

        private readonly List<Stats.AchievementInfo> _IconQueue = new();
        private readonly List<Stats.StatDefinition> _StatDefinitions = new();

        private readonly List<Stats.AchievementDefinition> _AchievementDefinitions = new();

        private readonly BindingList<Stats.StatInfo> _Statistics = new();

        // Achievement states ticked by the user but not stored yet, keyed by achievement id.
        // Survives list rebuilds caused by filtering so pending changes are not lost.
        private readonly Dictionary<string, bool> _PendingAchievementStates = new();

        private int _SkippedAchievements;
        private int _SkippedStats;

        // Set while StoreStats() is waiting for its confirmation callback (1102).
        private bool _StorePending;
        private string _StoreSummary;
        private System.Windows.Forms.Timer _StoreTimeoutTimer;

        private readonly API.Callbacks.UserStatsReceived _UserStatsReceivedCallback;
        private readonly API.Callbacks.UserStatsStored _UserStatsStoredCallback;

        // Display language override (#531). Null = follow the language Steam reports
        // for this game, which is not necessarily the language of the Steam client.
        private string _DisplayLanguage;
        private string _DefaultLanguage;
        private readonly List<string> _SchemaLanguages = new();
        private ToolStripComboBox _LanguageComboBox;

        // Why LoadUserGameStatsSchema failed, so the UI can say something more
        // useful than "Failed to load schema." (#593/#594/#625: half of the
        // games show no achievement because the schema was never downloaded).
        private string _SchemaFailureReason;

        public Manager(long gameId, API.Client client)
        {
            this.InitializeComponent();
            this.SetupLanguageSelector();

            // Sorting is handled on ColumnClick (#495); the ListView otherwise only
            // sorts by text when items are inserted.
            this._AchievementSortColumn = 0;
            this._AchievementSortOrder = SortOrder.Ascending;
            this._AchievementListView.Sorting = SortOrder.None;
            this._AchievementListView.ListViewItemSorter = new AchievementComparer(this);
            this._AchievementListView.ColumnClick += this.OnAchievementColumnClick;

            this._MainTabControl.SelectedTab = this._AchievementsTabPage;
            //this.statisticsList.Enabled = this.checkBox1.Checked;

            this._AchievementImageList.Images.Add("Blank", new Bitmap(64, 64));

            this._StatisticsDataGridView.AutoGenerateColumns = false;

            this._StatisticsDataGridView.Columns.Add("name", "Name");
            this._StatisticsDataGridView.Columns[0].ReadOnly = true;
            this._StatisticsDataGridView.Columns[0].Width = 200;
            this._StatisticsDataGridView.Columns[0].DataPropertyName = "DisplayName";

            this._StatisticsDataGridView.Columns.Add("value", "Value");
            this._StatisticsDataGridView.Columns[1].ReadOnly = this._EnableStatsEditingCheckBox.Checked == false;
            this._StatisticsDataGridView.Columns[1].Width = 90;
            this._StatisticsDataGridView.Columns[1].DataPropertyName = "Value";

            this._StatisticsDataGridView.Columns.Add("extra", "Extra");
            this._StatisticsDataGridView.Columns[2].ReadOnly = true;
            this._StatisticsDataGridView.Columns[2].Width = 200;
            this._StatisticsDataGridView.Columns[2].DataPropertyName = "Extra";

            this._StatisticsDataGridView.DataSource = new BindingSource()
            {
                DataSource = this._Statistics,
            };

            this._GameId = gameId;
            this._SteamClient = client;

            this._IconDownloader.DownloadDataCompleted += this.OnIconDownload;

            string name = this._SteamClient.SteamApps001.GetAppData((uint)this._GameId, "name");
            if (name != null)
            {
                base.Text += " | " + name;
            }
            else
            {
                base.Text += " | " + this._GameId.ToString(CultureInfo.InvariantCulture);
            }

            this._UserStatsReceivedCallback = client.CreateAndRegisterCallback<API.Callbacks.UserStatsReceived>();
            this._UserStatsReceivedCallback.OnRun += this.OnUserStatsReceived;

            this._UserStatsStoredCallback = client.CreateAndRegisterCallback<API.Callbacks.UserStatsStored>();
            this._UserStatsStoredCallback.OnRun += this.OnUserStatsStored;

            this._StoreTimeoutTimer = new System.Windows.Forms.Timer(this.components)
            {
                Interval = 15000,
            };
            this._StoreTimeoutTimer.Tick += this.OnStoreTimeout;

            this.RefreshStats();
        }

        private void AddAchievementIcon(Stats.AchievementInfo info, Image icon)
        {
            if (icon == null)
            {
                info.ImageIndex = 0;
            }
            else
            {
                info.ImageIndex = this._AchievementImageList.Images.Count;
                this._AchievementImageList.Images.Add(info.IsAchieved == true ? info.IconNormal : info.IconLocked, icon);
            }
        }

        private static WebClient CreateIconDownloader()
        {
            // Same treatment as the picker's downloader: older machine defaults
            // can still negotiate SSL3/TLS1.0, and behind an authenticated proxy
            // the icon downloads fail without the default credentials (#466),
            // which used to show up as achievements without any icon.
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            WebClient downloader = new();
            if (WebRequest.DefaultWebProxy != null)
            {
                downloader.Proxy = WebRequest.DefaultWebProxy;
                downloader.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials;
            }

            return downloader;
        }

        private void OnIconDownload(object sender, DownloadDataCompletedEventArgs e)
        {
            if (e.Error == null && e.Cancelled == false)
            {
                var info = (Stats.AchievementInfo)e.UserState;

                Bitmap bitmap;
                try
                {
                    using (MemoryStream stream = new())
                    {
                        stream.Write(e.Result, 0, e.Result.Length);
                        bitmap = new(stream);
                    }
                }
                catch (Exception)
                {
                    bitmap = null;
                }

                this.AddAchievementIcon(info, bitmap);
                this._AchievementListView.Update();
            }

            this.DownloadNextIcon();
        }

        private void DownloadNextIcon()
        {
            if (this._IconQueue.Count == 0)
            {
                this._DownloadStatusLabel.Visible = false;
                return;
            }

            if (this._IconDownloader.IsBusy == true)
            {
                return;
            }

            this._DownloadStatusLabel.Text = $"Downloading {this._IconQueue.Count} icons...";
            this._DownloadStatusLabel.Visible = true;

            var info = this._IconQueue[0];
            this._IconQueue.RemoveAt(0);


            this._IconDownloader.DownloadDataAsync(
                new Uri(_($"https://cdn.steamstatic.com/steamcommunity/public/images/apps/{this._GameId}/{(info.IsAchieved == true ? info.IconNormal : info.IconLocked)}")),
                info);
        }

        internal static string DescribeConstraintsSuffix(Stats.StatInfo stat)
        {
            var constraints = stat.DescribeConstraints();
            return string.IsNullOrEmpty(constraints) == false ? $" [{constraints}]" : "";
        }

        internal static string TranslateError(int id) => id switch
        {
            1 => "ok",
            2 => "generic failure -- this usually means you don't own the game",
            3 => "no connection -- Steam is offline or unreachable",
            5 => "logged out",
            6 => "invalid account information",
            15 => "timed out",
            17 => "account not found",
            19 => "Steam service unavailable",
            20 => "not logged on to Steam",
            _ => _($"error {id}"),
        };

        private static string GetLocalizedString(KeyValue kv, string language, string defaultValue)
        {
            var name = kv[language].AsString("");
            if (string.IsNullOrEmpty(name) == false)
            {
                return name;
            }

            if (language != "english")
            {
                name = kv["english"].AsString("");
                if (string.IsNullOrEmpty(name) == false)
                {
                    return name;
                }
            }

            name = kv.AsString("");
            if (string.IsNullOrEmpty(name) == false)
            {
                return name;
            }

            return defaultValue;
        }

        private bool LoadUserGameStatsSchema()
        {
            this._SchemaFailureReason = null;

            string path;
            string fileName;
            try
            {
                fileName = _($"UserGameStatsSchema_{this._GameId}.bin");
                path = API.Steam.GetInstallPath();
                path = Path.Combine(path, "appcache", "stats", fileName);
                if (File.Exists(path) == false)
                {
                    // Steam only fetches this file once the game has been run.
                    this._SchemaFailureReason =
                        $"{fileName} is not in Steam's appcache (appcache\\stats). " +
                        "Launch the game once from Steam, then reopen this window.";
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }

            var kv = KeyValue.LoadAsBinary(path);
            if (kv == null)
            {
                this._SchemaFailureReason =
                    $"{fileName} could not be read (it may be incomplete or corrupt). " +
                    "Deleting it and running the game again makes Steam rebuild it.";
                return false;
            }

            var currentLanguage = this._SteamClient.SteamApps008.GetCurrentGameLanguage();
            if (string.IsNullOrEmpty(currentLanguage) == true)
            {
                currentLanguage = "english";
            }

            this._DefaultLanguage = currentLanguage;
            this._SchemaLanguages.Clear();

            // Steam's per-game language wins unless the user picked one in the toolbar;
            // the schema may not carry that language, in which case GetLocalizedString
            // falls back to English (#531).
            var displayLanguage = this._DisplayLanguage ?? currentLanguage;

            this._AchievementDefinitions.Clear();
            this._StatDefinitions.Clear();

            var stats = kv[this._GameId.ToString(CultureInfo.InvariantCulture)]["stats"];
            if (stats.Valid == false || stats.Children == null)
            {
                this._SchemaFailureReason =
                    $"{fileName} does not contain the statistics for app {this._GameId}.";
                return false;
            }

            foreach (var stat in stats.Children)
            {
                if (stat.Valid == false)
                {
                    continue;
                }

                APITypes.UserStatType type;

                // schema in the new format?
                var typeNode = stat["type"];
                if (typeNode.Valid == true && typeNode.Type == KeyValueType.String)
                {
                    if (Enum.TryParse((string)typeNode.Value, true, out type) == false)
                    {
                        type = APITypes.UserStatType.Invalid;
                    }
                }
                else
                {
                    type = APITypes.UserStatType.Invalid;
                }

                // schema in the old format?
                if (type == APITypes.UserStatType.Invalid)
                {
                    var typeIntNode = stat["type_int"];
                    var rawType = typeIntNode.Valid == true
                        ? typeIntNode.AsInteger(0)
                        : typeNode.AsInteger(0);
                    type = (APITypes.UserStatType)rawType;
                }

                switch (type)
                {
                    case APITypes.UserStatType.Invalid:
                    {
                        break;
                    }

                    case APITypes.UserStatType.Integer:
                    {
                        var id = stat["name"].AsString("");
                        this.CollectLanguages(stat["display"]["name"]);
                        string name = GetLocalizedString(stat["display"]["name"], displayLanguage, id);

                        this._StatDefinitions.Add(new Stats.IntegerStatDefinition()
                        {
                            Id = stat["name"].AsString(""),
                            DisplayName = name,
                            MinValue = stat["min"].AsInteger(int.MinValue),
                            MaxValue = stat["max"].AsInteger(int.MaxValue),
                            MaxChange = stat["maxchange"].Valid == true
                                ? stat["maxchange"].AsInteger(0)
                                : (int?)null,
                            IncrementOnly = stat["incrementonly"].AsBoolean(false),
                            SetByTrustedGameServer = stat["bSetByTrustedGS"].AsBoolean(false),
                            DefaultValue = stat["default"].AsInteger(0),
                            Permission = stat["permission"].AsInteger(0),
                        });
                        break;
                    }

                    case APITypes.UserStatType.Float:
                    case APITypes.UserStatType.AverageRate:
                    {
                        var id = stat["name"].AsString("");
                        this.CollectLanguages(stat["display"]["name"]);
                        string name = GetLocalizedString(stat["display"]["name"], displayLanguage, id);

                        this._StatDefinitions.Add(new Stats.FloatStatDefinition()
                        {
                            Id = stat["name"].AsString(""),
                            DisplayName = name,
                            MinValue = stat["min"].AsFloat(float.MinValue),
                            MaxValue = stat["max"].AsFloat(float.MaxValue),
                            MaxChange = stat["maxchange"].Valid == true
                                ? stat["maxchange"].AsFloat(0.0f)
                                : (float?)null,
                            IncrementOnly = stat["incrementonly"].AsBoolean(false),
                            AverageRate = type == APITypes.UserStatType.AverageRate,
                            DefaultValue = stat["default"].AsFloat(0.0f),
                            Permission = stat["permission"].AsInteger(0),
                        });
                        break;
                    }

                    case APITypes.UserStatType.Achievements:
                    case APITypes.UserStatType.GroupAchievements:
                    {
                        if (stat.Children != null)
                        {
                            foreach (var bits in stat.Children.Where(
                                b => string.Compare(b.Name, "bits", StringComparison.InvariantCultureIgnoreCase) == 0))
                            {
                                if (bits.Valid == false || bits.Children == null)
                                {
                                    continue;
                                }

                                foreach (var bit in bits.Children)
                                {
                                    string id = bit["name"].AsString("");
                                    this.CollectLanguages(bit["display"]["name"]);
                                    this.CollectLanguages(bit["display"]["desc"]);
                                    string name = GetLocalizedString(bit["display"]["name"], displayLanguage, id);
                                    string desc = GetLocalizedString(bit["display"]["desc"], displayLanguage, "");

                                    this._AchievementDefinitions.Add(new()
                                    {
                                        Id = id,
                                        Name = name,
                                        Description = desc,
                                        IconNormal = bit["display"]["icon"].AsString(""),
                                        IconLocked = bit["display"]["icon_gray"].AsString(""),
                                        IsHidden = bit["display"]["hidden"].AsBoolean(false),
                                        Permission = bit["permission"].AsInteger(0),
                                    });
                                }
                            }
                        }

                        break;
                    }

                    default:
                    {
                        // Unknown stat type: skip it instead of aborting the whole schema.
                        break;
                    }
                }
            }

            this._SchemaLanguages.Sort(StringComparer.CurrentCultureIgnoreCase);
            this.UpdateLanguageSelector();

            return true;
        }

        private static readonly HashSet<string> _KnownLanguages = new(StringComparer.OrdinalIgnoreCase)
        {
            // https://partner.steamgames.com/doc/store/localization/languages
            "brazilian", "bulgarian", "czech", "danish", "dutch", "english", "finnish",
            "french", "german", "greek", "hungarian", "italian", "japanese", "koreana",
            "norwegian", "polish", "portuguese", "romanian", "russian", "schinese",
            "spanish", "swedish", "tchinese", "thai", "turkish", "ukrainian", "vietnamese",
        };

        private void CollectLanguages(KeyValue displayName)
        {
            if (displayName.Valid == false || displayName.Children == null)
            {
                return;
            }

            foreach (var child in displayName.Children)
            {
                if (child.Valid == false || string.IsNullOrEmpty(child.Name) == true)
                {
                    continue;
                }

                // The schema also carries non-language keys under display nodes
                // (e.g. "token"), which must not end up as a language choice.
                if (_KnownLanguages.Contains(child.Name) == false)
                {
                    continue;
                }

                if (this._SchemaLanguages.Contains(child.Name) == false)
                {
                    this._SchemaLanguages.Add(child.Name);
                }
            }
        }

        private void SetupLanguageSelector()
        {
            this._LanguageComboBox = new ToolStripComboBox()
            {
                Name = "_LanguageComboBox",
                DropDownStyle = ComboBoxStyle.DropDownList,
                AutoSize = false,
                Width = 130,
                ToolTipText = "Language used to display achievements and statistics.",
            };
            this._LanguageComboBox.SelectedIndexChanged += this.OnLanguageChanged;

            this._MainToolStrip.Items.Insert(0, new ToolStripLabel("Language:"));
            this._MainToolStrip.Items.Insert(1, this._LanguageComboBox);
        }

        private void UpdateLanguageSelector()
        {
            if (this._LanguageComboBox == null)
            {
                return;
            }

            this._LanguageComboBox.SelectedIndexChanged -= this.OnLanguageChanged;
            try
            {
                this._LanguageComboBox.Items.Clear();
                this._LanguageComboBox.Items.Add(
                    "Automatic (" + (this._DefaultLanguage ?? "english") + ")");
                foreach (var language in this._SchemaLanguages)
                {
                    this._LanguageComboBox.Items.Add(language);
                }

                int index = 0;
                if (this._DisplayLanguage != null)
                {
                    int found = this._SchemaLanguages.IndexOf(this._DisplayLanguage);
                    if (found >= 0)
                    {
                        index = found + 1;
                    }
                    else
                    {
                        // The override is no longer offered by this schema.
                        this._DisplayLanguage = null;
                    }
                }

                this._LanguageComboBox.SelectedIndex = index;
            }
            finally
            {
                this._LanguageComboBox.SelectedIndexChanged += this.OnLanguageChanged;
            }
        }

        private void OnLanguageChanged(object sender, EventArgs e)
        {
            string selected = this._LanguageComboBox.SelectedIndex <= 0
                ? null
                : (string)this._LanguageComboBox.Items[this._LanguageComboBox.SelectedIndex];

            if (selected == this._DisplayLanguage)
            {
                return;
            }

            this._DisplayLanguage = selected;

            bool schemaLoaded;
            try
            {
                schemaLoaded = this.LoadUserGameStatsSchema();
            }
            catch (Exception exception)
            {
                this._GameStatusLabel.Text = "Failed to load schema: " + exception.Message;
                return;
            }

            if (schemaLoaded == false)
            {
                this._GameStatusLabel.Text = this.DescribeSchemaFailure();
                return;
            }

            this.GetAchievements();
            this.GetStatistics();
            this._GameStatusLabel.Text = this.BuildRetrievedStatus();
        }

        private string DescribeSchemaFailure()
        {
            return string.IsNullOrEmpty(this._SchemaFailureReason) == false
                ? "Failed to load schema: " + this._SchemaFailureReason
                : "Failed to load schema.";
        }

        private string BuildRetrievedStatus()
        {
            var unavailable = this._SkippedAchievements + this._SkippedStats;
            return
                $"Retrieved {this._AchievementListView.Items.Count} achievements and {this._StatisticsDataGridView.Rows.Count} statistics" +
                (unavailable > 0
                    ? $" ({this._SkippedAchievements} achievements and {this._SkippedStats} statistics unavailable)"
                    : "") +
                $", language: {this._DisplayLanguage ?? this._DefaultLanguage}.";
        }

        private void OnUserStatsReceived(APITypes.UserStatsReceived param)
        {
            if (param.Result != 1)
            {
                this._GameStatusLabel.Text = $"Error while retrieving stats: {TranslateError(param.Result)}";
                this.EnableInput();
                return;
            }

            bool schemaLoaded;
            try
            {
                schemaLoaded = this.LoadUserGameStatsSchema();
            }
            catch (Exception e)
            {
                this._GameStatusLabel.Text = "Failed to load schema: " + e.Message;
                this.EnableInput();
                MessageBox.Show(
                    "Failed to load schema:\n" + e,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            if (schemaLoaded == false)
            {
                this._GameStatusLabel.Text = this.DescribeSchemaFailure();
                this.EnableInput();
                return;
            }

            try
            {
                this.GetAchievements();
            }
            catch (Exception e)
            {
                this._GameStatusLabel.Text = "Error when handling achievements retrieval.";
                this.EnableInput();
                MessageBox.Show(
                    "Error when handling achievements retrieval:\n" + e,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            try
            {
                this.GetStatistics();
            }
            catch (Exception e)
            {
                this._GameStatusLabel.Text = "Error when handling stats retrieval.";
                this.EnableInput();
                MessageBox.Show(
                    "Error when handling stats retrieval:\n" + e,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            this._GameStatusLabel.Text = this.BuildRetrievedStatus();
            this.EnableInput();
        }

        private void RefreshStats()
        {
            var steamId = this._SteamClient.SteamUser.GetSteamId();

            // This still triggers the UserStatsReceived callback, in addition to the callresult.
            // No need to implement callresults for the time being.
            var callHandle = this._SteamClient.SteamUserStats.RequestUserStats(steamId);
            if (callHandle == API.CallHandle.Invalid)
            {
                MessageBox.Show(
                    this,
                    "Steam refused the request for the stats of this game.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                this.EnableInput();
                return;
            }

            // Only wipe the UI once the request has been accepted, so a failed request
            // does not leave the window empty.
            this._AchievementListView.Items.Clear();
            this._StatisticsDataGridView.Rows.Clear();
            this._PendingAchievementStates.Clear();

            this._GameStatusLabel.Text = "Retrieving stat information...";
            this.DisableInput();
        }

        private bool _IsUpdatingAchievementList;
        private int _AchievementSortColumn;
        private SortOrder _AchievementSortOrder;

        private void OnAchievementColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column == this._AchievementSortColumn)
            {
                this._AchievementSortOrder = this._AchievementSortOrder == SortOrder.Ascending
                    ? SortOrder.Descending
                    : SortOrder.Ascending;
            }
            else
            {
                this._AchievementSortColumn = e.Column;
                this._AchievementSortOrder = SortOrder.Ascending;
            }

            this._AchievementListView.Sort();
        }

        private sealed class AchievementComparer : IComparer
        {
            private readonly Manager _Owner;

            public AchievementComparer(Manager owner)
            {
                this._Owner = owner;
            }

            public int Compare(object x, object y)
            {
                if (x is not ListViewItem left || y is not ListViewItem right)
                {
                    return 0;
                }

                int result;
                if (this._Owner._AchievementSortColumn == 2)
                {
                    // Unlock time: sort chronologically, never-unlocked entries first.
                    var leftTime = left.Tag is Stats.AchievementInfo leftInfo ? leftInfo.UnlockTime : null;
                    var rightTime = right.Tag is Stats.AchievementInfo rightInfo ? rightInfo.UnlockTime : null;
                    result = (leftTime, rightTime) switch
                    {
                        (null, null) => 0,
                        (null, _) => -1,
                        (_, null) => 1,
                        _ => leftTime.Value.CompareTo(rightTime.Value),
                    };
                }
                else
                {
                    var column = this._Owner._AchievementSortColumn;
                    string leftText = left.SubItems.Count > column ? left.SubItems[column].Text : "";
                    string rightText = right.SubItems.Count > column ? right.SubItems[column].Text : "";
                    result = string.Compare(leftText, rightText, StringComparison.CurrentCultureIgnoreCase);
                }

                if (result == 0)
                {
                    result = string.Compare(left.Text, right.Text, StringComparison.CurrentCultureIgnoreCase);
                }

                return this._Owner._AchievementSortOrder == SortOrder.Descending ? -result : result;
            }
        }

        private void GetAchievements()
        {
            var textSearch = this._MatchingStringTextBox.Text.Length > 0
                ? this._MatchingStringTextBox.Text
                : null;

            this._IsUpdatingAchievementList = true;
            this._SkippedAchievements = 0;

            // Drop queued icon downloads from the previous contents of the list; the
            // items they targeted are about to be discarded.
            this._IconQueue.Clear();

            this._AchievementListView.Items.Clear();
            this._AchievementListView.BeginUpdate();
            //this.Achievements.Clear();

            try
            {
                bool wantLocked = this._DisplayLockedOnlyButton.Checked == true;
                bool wantUnlocked = this._DisplayUnlockedOnlyButton.Checked == true;

                foreach (var def in this._AchievementDefinitions)
                {
                    if (string.IsNullOrEmpty(def.Id) == true)
                    {
                        continue;
                    }

                    if (this._SteamClient.SteamUserStats.GetAchievementAndUnlockTime(
                        def.Id,
                        out bool isAchieved,
                        out var unlockTime) == false)
                    {
                        this._SkippedAchievements++;
                        continue;
                    }

                    bool wanted = (wantLocked == false && wantUnlocked == false) || isAchieved switch
                    {
                        true => wantUnlocked,
                        false => wantLocked,
                    };
                    if (wanted == false)
                    {
                        continue;
                    }

                    if (textSearch != null)
                    {
                        if (def.Name.IndexOf(textSearch, StringComparison.OrdinalIgnoreCase) < 0 &&
                            def.Description.IndexOf(textSearch, StringComparison.OrdinalIgnoreCase) < 0)
                        {
                            continue;
                        }
                    }

                    // State ticked by the user but not stored yet wins over the cached state,
                    // so filtering does not discard pending changes.
                    bool checkedState = this._PendingAchievementStates.TryGetValue(def.Id, out var pendingState) == true
                        ? pendingState
                        : isAchieved;

                    Stats.AchievementInfo info = new()
                    {
                        Id = def.Id,
                        IsAchieved = isAchieved,
                        UnlockTime = isAchieved == false
                            ? null
                            : unlockTime > 0
                                ? DateTimeOffset.FromUnixTimeSeconds(unlockTime).LocalDateTime
                                : DateTimeOffset.FromUnixTimeSeconds(0).LocalDateTime,
                        IconNormal = string.IsNullOrEmpty(def.IconNormal) ? null : def.IconNormal,
                        IconLocked = string.IsNullOrEmpty(def.IconLocked) ? def.IconNormal : def.IconLocked,
                        Permission = def.Permission,
                        Name = def.Name,
                        Description = def.Description,
                    };

                    ListViewItem item = new()
                    {
                        Checked = checkedState,
                        Tag = info,
                        Text = info.Name,
                        BackColor = (def.Permission & 3) == 0 ? Color.Black : Color.FromArgb(64, 0, 0),
                    };

                    info.Item = item;

                    if (item.Text.StartsWith("#", StringComparison.InvariantCulture) == true)
                    {
                        item.Text = info.Id;
                        item.SubItems.Add("");
                    }
                    else
                    {
                        item.SubItems.Add(info.Description);
                    }

                    item.SubItems.Add(info.UnlockTime.HasValue == true
                        ? info.UnlockTime.Value.ToString("g")
                        : "-");

                    info.ImageIndex = 0;

                    this.AddAchievementToIconQueue(info, false);
                    this._AchievementListView.Items.Add(item);
                }
            }
            finally
            {
                this._AchievementListView.EndUpdate();
                this._IsUpdatingAchievementList = false;
            }

            this._AchievementListView.Sort();

            this.DownloadNextIcon();
        }

        private void GetStatistics()
        {
            this._Statistics.Clear();
            this._SkippedStats = 0;
            foreach (var stat in this._StatDefinitions)
            {
                if (string.IsNullOrEmpty(stat.Id) == true)
                {
                    continue;
                }

                if (stat is Stats.IntegerStatDefinition intStat)
                {
                    if (this._SteamClient.SteamUserStats.GetStatValue(intStat.Id, out int value) == false)
                    {
                        this._SkippedStats++;
                        continue;
                    }
                    this._Statistics.Add(new Stats.IntStatInfo()
                    {
                        Id = intStat.Id,
                        DisplayName = intStat.DisplayName,
                        IntValue = value,
                        OriginalValue = value,
                        IsIncrementOnly = intStat.IncrementOnly,
                        Permission = intStat.Permission,
                        MinValue = intStat.MinValue,
                        MaxValue = intStat.MaxValue,
                        MaxChange = intStat.MaxChange,
                    });
                }
                else if (stat is Stats.FloatStatDefinition floatStat)
                {
                    if (this._SteamClient.SteamUserStats.GetStatValue(floatStat.Id, out float value) == false)
                    {
                        this._SkippedStats++;
                        continue;
                    }
                    this._Statistics.Add(new Stats.FloatStatInfo()
                    {
                        Id = floatStat.Id,
                        DisplayName = floatStat.DisplayName,
                        FloatValue = value,
                        OriginalValue = value,
                        IsIncrementOnly = floatStat.IncrementOnly,
                        IsAverageRate = floatStat.AverageRate,
                        Permission = floatStat.Permission,
                        MinValue = floatStat.MinValue,
                        MaxValue = floatStat.MaxValue,
                        MaxChange = floatStat.MaxChange,
                    });
                }
            }
        }

        private void AddAchievementToIconQueue(Stats.AchievementInfo info, bool startDownload)
        {
            int imageIndex = this._AchievementImageList.Images.IndexOfKey(
                info.IsAchieved == true ? info.IconNormal : info.IconLocked);

            if (imageIndex >= 0)
            {
                info.ImageIndex = imageIndex;
            }
            else
            {
                this._IconQueue.Add(info);

                if (startDownload == true)
                {
                    this.DownloadNextIcon();
                }
            }
        }

        private int StoreAchievements()
        {
            if (this._AchievementListView.Items.Count == 0)
            {
                return 0;
            }

            List<(Stats.AchievementInfo Info, bool Value)> pending = new();
            foreach (ListViewItem item in this._AchievementListView.Items)
            {
                if (item.Tag is not Stats.AchievementInfo achievementInfo ||
                    achievementInfo.IsAchieved == item.Checked)
                {
                    continue;
                }

                pending.Add((achievementInfo, item.Checked));
            }

            if (pending.Count == 0)
            {
                return 0;
            }

            for (int i = 0; i < pending.Count; i++)
            {
                var (info, value) = pending[i];
                if (this._SteamClient.SteamUserStats.SetAchievement(info.Id, value) == false)
                {
                    MessageBox.Show(
                        this,
                        $"An error occurred while setting the state for {info.Id}, aborting store.\n" +
                        $"{pending.Count - i} change(s) have not been applied.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return -1;
                }

                // Only update the model once Steam accepted the change, so a failure
                // leaves the remaining changes detectable on the next attempt.
                info.IsAchieved = value;
            }

            return pending.Count;
        }

        private int StoreStatistics()
        {
            if (this._Statistics.Count == 0)
            {
                return 0;
            }

            var statistics = this._Statistics.Where(stat => stat.IsModified == true).ToList();
            if (statistics.Count == 0)
            {
                return 0;
            }

            foreach (var stat in statistics)
            {
                if (stat is Stats.IntStatInfo intStat)
                {
                    if (this._SteamClient.SteamUserStats.SetStatValue(
                        intStat.Id,
                        intStat.IntValue) == false)
                    {
                        MessageBox.Show(
                            this,
                            $"An error occurred while setting the value for {stat.Id}{DescribeConstraintsSuffix(stat)}, aborting store.\n" +
                            "The other pending changes are kept, you can fix the value and retry.",
                            "Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        return -1;
                    }
                }
                else if (stat is Stats.FloatStatInfo floatStat)
                {
                    if (this._SteamClient.SteamUserStats.SetStatValue(
                        floatStat.Id,
                        floatStat.FloatValue) == false)
                    {
                        MessageBox.Show(
                            this,
                            $"An error occurred while setting the value for {stat.Id}{DescribeConstraintsSuffix(stat)}, aborting store.\n" +
                            "The other pending changes are kept, you can fix the value and retry." +
                            (stat.IsAverageRate == true
                                ? "\n\nNote: this is an average rate statistic; Steam updates those from " +
                                  "session data (UpdateAvgRateStat) and often rejects SetStat."
                                : ""),
                            "Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        return -1;
                    }
                }
                else
                {
                    MessageBox.Show(
                        this,
                        $"Unsupported statistic type for {stat.Id}, aborting store.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return -1;
                }
            }

            return statistics.Count;
        }

        private void DisableInput()
        {
            this._ReloadButton.Enabled = false;
            this._StoreButton.Enabled = false;
        }

        private void EnableInput()
        {
            this._ReloadButton.Enabled = true;
            this._StoreButton.Enabled = true;
        }

        private void OnTimer(object sender, EventArgs e)
        {
            this._CallbackTimer.Enabled = false;
            try
            {
                this._SteamClient.RunCallbacks(false);
            }
            finally
            {
                this._CallbackTimer.Enabled = true;
            }
        }

        private void OnRefresh(object sender, EventArgs e)
        {
            this.RefreshStats();
        }

        private void OnLockAll(object sender, EventArgs e)
        {
            foreach (ListViewItem item in this._AchievementListView.Items)
            {
                item.Checked = false;
            }
        }

        private void OnInvertAll(object sender, EventArgs e)
        {
            foreach (ListViewItem item in this._AchievementListView.Items)
            {
                item.Checked = !item.Checked;
            }
        }

        private void OnUnlockAll(object sender, EventArgs e)
        {
            foreach (ListViewItem item in this._AchievementListView.Items)
            {
                item.Checked = true;
            }
        }

        private bool Store()
        {
            if (this._SteamClient.SteamUserStats.StoreStats() == false)
            {
                MessageBox.Show(
                    this,
                    "An error occurred while storing, aborting.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        private void OnStore(object sender, EventArgs e)
        {
            // Flush an in-progress cell edit, otherwise its change is invisible to
            // StoreStatistics() and the commit reports "0 statistics".
            if (this._StatisticsDataGridView.IsCurrentCellInEditMode == true)
            {
                this._StatisticsDataGridView.EndEdit();
                this._StatisticsDataGridView.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }

            int achievements = this.StoreAchievements();
            if (achievements < 0)
            {
                // Keep the list as-is: the changes that were not sent are still pending
                // and can be retried after fixing the reported problem.
                return;
            }

            int stats = this.StoreStatistics();
            if (stats < 0)
            {
                return;
            }

            if (achievements == 0 && stats == 0)
            {
                this._GameStatusLabel.Text = "Nothing to store.";
                return;
            }

            if (this.Store() == false)
            {
                this.RefreshStats();
                return;
            }

            this._PendingAchievementStates.Clear();

            // StoreStats() only queues the upload: the outcome arrives through the
            // UserStatsStored (1102) callback. Reporting success and refreshing right
            // away raced the upload and made committed achievements look reverted
            // (#405, #458, #429, #599).
            this._StoreSummary = $"Stored {achievements} achievements and {stats} statistics.";
            this._StorePending = true;
            this.DisableInput();
            this._GameStatusLabel.Text = "Sending changes to Steam...";
            this._StoreTimeoutTimer.Start();
        }

        private void OnUserStatsStored(APITypes.UserStatsStored param)
        {
            if (this._StorePending == false)
            {
                return;
            }

            this._StorePending = false;
            this._StoreTimeoutTimer.Stop();

            string summary = this._StoreSummary;
            this._StoreSummary = null;

            if (param.Result == 1)
            {
                this._GameStatusLabel.Text = summary;
                MessageBox.Show(
                    this,
                    summary,
                    "Information",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else
            {
                var message = "Steam rejected the changes: " + TranslateError(param.Result);
                this._GameStatusLabel.Text = message;
                MessageBox.Show(
                    this,
                    message + "\n\nThe list will be refreshed with the values accepted by Steam.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

            this.RefreshStats();
        }

        private void OnStoreTimeout(object sender, EventArgs e)
        {
            if (this._StorePending == false)
            {
                return;
            }

            this._StorePending = false;
            this._StoreTimeoutTimer.Stop();
            this._StoreSummary = null;

            MessageBox.Show(
                this,
                "Steam did not confirm the changes within 15 seconds.\n" +
                "They may still be applied; the list will be refreshed.",
                "Warning",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            this.RefreshStats();
        }

        private void OnStatDataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            if (e.Context != DataGridViewDataErrorContexts.Commit)
            {
                return;
            }

            var view = (DataGridView)sender;
            if (e.Exception is Stats.StatIsProtectedException)
            {
                e.ThrowException = false;
                e.Cancel = true;
                view.Rows[e.RowIndex].ErrorText = "Stat is protected! -- you can't modify it";
            }
            else if (e.Exception is Stats.StatConstraintException constraint)
            {
                e.ThrowException = false;
                e.Cancel = true;
                view.Rows[e.RowIndex].ErrorText = constraint.Message;
            }
            else
            {
                e.ThrowException = false;
                e.Cancel = true;
                view.Rows[e.RowIndex].ErrorText = "Invalid value";
            }
        }

        private void OnStatAgreementChecked(object sender, EventArgs e)
        {
            this._StatisticsDataGridView.Columns[1].ReadOnly = this._EnableStatsEditingCheckBox.Checked == false;
        }

        private void OnStatCellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            var view = (DataGridView)sender;
            view.Rows[e.RowIndex].ErrorText = "";
        }

        private void OnResetAllStats(object sender, EventArgs e)
        {
            if (MessageBox.Show(
                "Are you absolutely sure you want to reset stats?",
                "Warning",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning) == DialogResult.No)
            {
                return;
            }

            bool achievementsToo = DialogResult.Yes == MessageBox.Show(
                "Do you want to reset achievements too?",
                "Question",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (MessageBox.Show(
                "Really really sure?",
                "Warning",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Error) == DialogResult.No)
            {
                return;
            }

            if (this._SteamClient.SteamUserStats.ResetAllStats(achievementsToo) == false)
            {
                MessageBox.Show(this, "Failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            this.RefreshStats();
        }

        private void OnCheckAchievement(object sender, ItemCheckEventArgs e)
        {
            if (sender != this._AchievementListView)
            {
                return;
            }

            if (this._IsUpdatingAchievementList == true)
            {
                return;
            }

            if (this._AchievementListView.Items[e.Index].Tag is not Stats.AchievementInfo info)
            {
                return;
            }

            if ((info.Permission & 3) != 0)
            {
                MessageBox.Show(
                    this,
                    "Sorry, but this is a protected achievement and cannot be managed with Steam Achievement Manager.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                e.NewValue = e.CurrentValue;
                return;
            }

            // Remember the user's intent so rebuilding the list (filter/display change)
            // does not discard it.
            bool checkedState = e.NewValue == CheckState.Checked;
            if (checkedState != info.IsAchieved)
            {
                this._PendingAchievementStates[info.Id] = checkedState;
            }
            else
            {
                this._PendingAchievementStates.Remove(info.Id);
            }
        }

        private void OnDisplayUncheckedOnly(object sender, EventArgs e)
        {
            if ((sender as ToolStripButton).Checked == true)
            {
                this._DisplayLockedOnlyButton.Checked = false;
            }

            this.GetAchievements();
        }

        private void OnDisplayCheckedOnly(object sender, EventArgs e)
        {
            if ((sender as ToolStripButton).Checked == true)
            {
                this._DisplayUnlockedOnlyButton.Checked = false;
            }

            this.GetAchievements();
        }

        private void OnFilterUpdate(object sender, KeyEventArgs e)
        {
            this.GetAchievements();
        }
    }
}
