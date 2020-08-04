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

using SAM.API;
using SAM.API.Callbacks;
using SAM.Game.Stats;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using APITypes = SAM.API.Types;

namespace SAM.Game
{
    internal partial class Manager : Form
    {
        private readonly List<AchievementDefinition> _AchievementDefinitions =
            new List<AchievementDefinition>();

        private readonly long _GameId;

        private readonly WebClient _IconDownloader = new WebClient();

        private readonly List<AchievementInfo> _IconQueue = new List<AchievementInfo>();
        private readonly List<StatDefinition> _StatDefinitions = new List<StatDefinition>();

        private readonly BindingList<StatInfo> _Statistics = new BindingList<StatInfo>();
        private readonly Client _SteamClient;

        // ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
        private readonly UserStatsReceived _UserStatsReceivedCallback;

        // ReSharper restore PrivateFieldCanBeConvertedToLocalVariable

        private bool _IsUpdatingAchievementList;

        //private API.Callback<APITypes.UserStatsStored> UserStatsStoredCallback;

        public Manager(long gameId, Client client)
        {
            InitializeComponent();

            _MainTabControl.SelectedTab = _AchievementsTabPage;
            //this.statisticsList.Enabled = this.checkBox1.Checked;

            _AchievementImageList.Images.Add("Blank", new Bitmap(64, 64));

            _StatisticsDataGridView.AutoGenerateColumns = false;

            _StatisticsDataGridView.Columns.Add("name", "Name");
            _StatisticsDataGridView.Columns[0].ReadOnly = true;
            _StatisticsDataGridView.Columns[0].Width = 200;
            _StatisticsDataGridView.Columns[0].DataPropertyName = "DisplayName";

            _StatisticsDataGridView.Columns.Add("value", "Value");
            _StatisticsDataGridView.Columns[1].ReadOnly = _EnableStatsEditingCheckBox.Checked == false;
            _StatisticsDataGridView.Columns[1].Width = 90;
            _StatisticsDataGridView.Columns[1].DataPropertyName = "Value";

            _StatisticsDataGridView.Columns.Add("extra", "Extra");
            _StatisticsDataGridView.Columns[2].ReadOnly = true;
            _StatisticsDataGridView.Columns[2].Width = 200;
            _StatisticsDataGridView.Columns[2].DataPropertyName = "Extra";

            _StatisticsDataGridView.DataSource = new BindingSource
            {
                DataSource = _Statistics
            };

            _GameId = gameId;
            _SteamClient = client;

            _IconDownloader.DownloadDataCompleted += OnIconDownload;

            var name = _SteamClient.SteamApps001.GetAppData((uint)_GameId, "name");
            if (name != null)
                Text += " | " + name;
            else
                Text += " | " + _GameId.ToString(CultureInfo.InvariantCulture);

            _UserStatsReceivedCallback = client.CreateAndRegisterCallback<UserStatsReceived>();
            _UserStatsReceivedCallback.OnRun += OnUserStatsReceived;

            //this.UserStatsStoredCallback = new API.Callback(1102, new API.Callback.CallbackFunction(this.OnUserStatsStored));
            RefreshStats();
        }

        private void AddAchievementIcon(AchievementInfo info, Image icon)
        {
            if (icon == null)
            {
                info.ImageIndex = 0;
            }
            else
            {
                info.ImageIndex = _AchievementImageList.Images.Count;
                _AchievementImageList.Images.Add(info.IsAchieved ? info.IconNormal : info.IconLocked, icon);
            }
        }

        private void OnIconDownload(object sender, DownloadDataCompletedEventArgs e)
        {
            if (e.Error == null && e.Cancelled == false)
            {
                var info = e.UserState as AchievementInfo;

                Bitmap bitmap;

                try
                {
                    using (var stream = new MemoryStream())
                    {
                        stream.Write(e.Result, 0, e.Result.Length);
                        bitmap = new Bitmap(stream);
                    }
                }
                catch (Exception)
                {
                    bitmap = null;
                }

                AddAchievementIcon(info, bitmap);
                _AchievementListView.Update();
            }

            DownloadNextIcon();
        }

        private void DownloadNextIcon()
        {
            if (_IconQueue.Count == 0)
            {
                _DownloadStatusLabel.Visible = false;
                return;
            }

            if (_IconDownloader.IsBusy)
                return;

            _DownloadStatusLabel.Text = string.Format(
                CultureInfo.CurrentCulture,
                "Downloading {0} icons...",
                _IconQueue.Count);
            _DownloadStatusLabel.Visible = true;

            var info = _IconQueue[0];
            _IconQueue.RemoveAt(0);

            _IconDownloader.DownloadDataAsync(
                new Uri(string.Format(
                    CultureInfo.InvariantCulture,
                    "http://steamcdn-a.akamaihd.net/steamcommunity/public/images/apps/{0}/{1}",
                    _GameId,
                    info.IsAchieved ? info.IconNormal : info.IconLocked)),
                info);
        }

        private static string TranslateError(int id)
        {
            switch (id)
            {
                case 2:
                    {
                        return "generic error -- this usually means you don't own the game";
                    }
            }

            return id.ToString(CultureInfo.InvariantCulture);
        }

        private static string GetLocalizedString(KeyValue kv, string language, string defaultValue)
        {
            var name = kv[language].AsString("");
            if (string.IsNullOrEmpty(name) == false)
                return name;

            if (language != "english")
            {
                name = kv["english"].AsString("");
                if (string.IsNullOrEmpty(name) == false)
                    return name;
            }

            name = kv.AsString("");
            if (string.IsNullOrEmpty(name) == false)
                return name;

            return defaultValue;
        }

        private bool LoadUserGameStatsSchema()
        {
            string path;

            try
            {
                path = Steam.GetInstallPath();
                path = Path.Combine(path, "appcache");
                path = Path.Combine(path, "stats");
                path = Path.Combine(path, string.Format(
                    CultureInfo.InvariantCulture,
                    "UserGameStatsSchema_{0}.bin",
                    _GameId));

                if (File.Exists(path) == false)
                    return false;
            }
            catch
            {
                return false;
            }

            var kv = KeyValue.LoadAsBinary(path);

            if (kv == null)
                return false;

            var currentLanguage = _SteamClient.SteamApps008.GetCurrentGameLanguage();
            //var currentLanguage = "german";

            _AchievementDefinitions.Clear();
            _StatDefinitions.Clear();

            var stats = kv[_GameId.ToString(CultureInfo.InvariantCulture)]["stats"];
            if (stats.Valid == false ||
                stats.Children == null)
                return false;

            foreach (var stat in stats.Children)
            {
                if (stat.Valid == false)
                    continue;

                var rawType = stat["type_int"].Valid
                    ? stat["type_int"].AsInteger(0)
                    : stat["type"].AsInteger(0);
                var type = (APITypes.UserStatType)rawType;
                switch (type)
                {
                    case APITypes.UserStatType.Invalid:
                        {
                            break;
                        }

                    case APITypes.UserStatType.Integer:
                        {
                            var id = stat["name"].AsString("");
                            var name = GetLocalizedString(stat["display"]["name"], currentLanguage, id);

                            _StatDefinitions.Add(new IntegerStatDefinition
                            {
                                Id = stat["name"].AsString(""),
                                DisplayName = name,
                                MinValue = stat["min"].AsInteger(int.MinValue),
                                MaxValue = stat["max"].AsInteger(int.MaxValue),
                                MaxChange = stat["maxchange"].AsInteger(0),
                                IncrementOnly = stat["incrementonly"].AsBoolean(false),
                                DefaultValue = stat["default"].AsInteger(0),
                                Permission = stat["permission"].AsInteger(0)
                            });
                            break;
                        }

                    case APITypes.UserStatType.Float:
                    case APITypes.UserStatType.AverageRate:
                        {
                            var id = stat["name"].AsString("");
                            var name = GetLocalizedString(stat["display"]["name"], currentLanguage, id);

                            _StatDefinitions.Add(new FloatStatDefinition
                            {
                                Id = stat["name"].AsString(""),
                                DisplayName = name,
                                MinValue = stat["min"].AsFloat(float.MinValue),
                                MaxValue = stat["max"].AsFloat(float.MaxValue),
                                MaxChange = stat["maxchange"].AsFloat(0.0f),
                                IncrementOnly = stat["incrementonly"].AsBoolean(false),
                                DefaultValue = stat["default"].AsFloat(0.0f),
                                Permission = stat["permission"].AsInteger(0)
                            });
                            break;
                        }

                    case APITypes.UserStatType.Achievements:
                    case APITypes.UserStatType.GroupAchievements:
                        {
                            if (stat.Children != null)
                                foreach (var bits in stat.Children.Where(
                                    b => string.Compare(b.Name, "bits", StringComparison.InvariantCultureIgnoreCase) == 0))
                                {
                                    if (bits.Valid == false ||
                                        bits.Children == null)
                                        continue;

                                    foreach (var bit in bits.Children)
                                    {
                                        var id = bit["name"].AsString("");
                                        var name = GetLocalizedString(bit["display"]["name"], currentLanguage, id);
                                        var desc = GetLocalizedString(bit["display"]["desc"], currentLanguage, "");

                                        _AchievementDefinitions.Add(new AchievementDefinition
                                        {
                                            Id = id,
                                            Name = name,
                                            Description = desc,
                                            IconNormal = bit["display"]["icon"].AsString(""),
                                            IconLocked = bit["display"]["icon_gray"].AsString(""),
                                            IsHidden = bit["display"]["hidden"].AsBoolean(false),
                                            Permission = bit["permission"].AsInteger(0)
                                        });
                                    }
                                }

                            break;
                        }

                    default:
                        {
                            throw new InvalidOperationException("invalid stat type");
                        }
                }
            }

            return true;
        }

        private void OnUserStatsReceived(APITypes.UserStatsReceived param)
        {
            if (param.Result != 1)
            {
                _GameStatusLabel.Text = string.Format(
                    CultureInfo.CurrentCulture,
                    "Error while retrieving stats: {0}",
                    TranslateError(param.Result));
                EnableInput();
                return;
            }

            if (LoadUserGameStatsSchema() == false)
            {
                _GameStatusLabel.Text = "Failed to load schema.";
                EnableInput();
                return;
            }

            try
            {
                GetAchievements();
                GetStatistics();
            }
            catch (Exception e)
            {
                _GameStatusLabel.Text = "Error when handling stats retrieval.";
                EnableInput();
                MessageBox.Show(
                    "Error when handling stats retrieval:\n" + e,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            _GameStatusLabel.Text = string.Format(
                CultureInfo.CurrentCulture,
                "Retrieved {0} achievements and {1} statistics.",
                _AchievementListView.Items.Count,
                _StatisticsDataGridView.Rows.Count);
            EnableInput();
        }

        private void RefreshStats()
        {
            _AchievementListView.Items.Clear();
            _StatisticsDataGridView.Rows.Clear();

            if (_SteamClient.SteamUserStats.RequestCurrentStats() == false)
            {
                MessageBox.Show(this, "Failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _GameStatusLabel.Text = "Retrieving stat information...";
            DisableInput();
        }

        private void GetAchievements()
        {
            _IsUpdatingAchievementList = true;

            _AchievementListView.Items.Clear();
            _AchievementListView.BeginUpdate();
            //this.Achievements.Clear();

            foreach (var def in _AchievementDefinitions)
            {
                if (string.IsNullOrEmpty(def.Id))
                    continue;

                bool isAchieved;
                if (_SteamClient.SteamUserStats.GetAchievementState(def.Id, out isAchieved) == false)
                    continue;

                var info = new AchievementInfo
                {
                    Id = def.Id,
                    IsAchieved = isAchieved,
                    IconNormal = string.IsNullOrEmpty(def.IconNormal) ? null : def.IconNormal,
                    IconLocked = string.IsNullOrEmpty(def.IconLocked) ? def.IconNormal : def.IconLocked,
                    Permission = def.Permission,
                    Name = def.Name,
                    Description = def.Description
                };

                var item = new ListViewItem
                {
                    Checked = isAchieved,
                    Tag = info,
                    Text = info.Name,
                    BackColor = (def.Permission & 3) == 0 ? Color.Black : Color.FromArgb(64, 0, 0)
                };

                info.Item = item;

                if (item.Text.StartsWith("#", StringComparison.InvariantCulture))
                    item.Text = info.Id;
                else
                    item.SubItems.Add(info.Description);

                info.ImageIndex = 0;

                AddAchievementToIconQueue(info, false);
                _AchievementListView.Items.Add(item);
                //this.Achievements.Add(info.Id, info);
            }
            _AchievementListView.EndUpdate();
            _IsUpdatingAchievementList = false;

            DownloadNextIcon();
        }

        private void GetStatistics()
        {
            _Statistics.Clear();
            foreach (var rdef in _StatDefinitions)
            {
                if (string.IsNullOrEmpty(rdef.Id))
                    continue;

                if (rdef is IntegerStatDefinition)
                {
                    var def = (IntegerStatDefinition)rdef;

                    int value;
                    if (_SteamClient.SteamUserStats.GetStatValue(def.Id, out value))
                        _Statistics.Add(new IntStatInfo
                        {
                            Id = def.Id,
                            DisplayName = def.DisplayName,
                            IntValue = value,
                            OriginalValue = value,
                            IsIncrementOnly = def.IncrementOnly,
                            Permission = def.Permission
                        });
                }
                else if (rdef is FloatStatDefinition)
                {
                    var def = (FloatStatDefinition)rdef;

                    float value;
                    if (_SteamClient.SteamUserStats.GetStatValue(def.Id, out value))
                        _Statistics.Add(new FloatStatInfo
                        {
                            Id = def.Id,
                            DisplayName = def.DisplayName,
                            FloatValue = value,
                            OriginalValue = value,
                            IsIncrementOnly = def.IncrementOnly,
                            Permission = def.Permission
                        });
                }
            }
        }

        private void AddAchievementToIconQueue(AchievementInfo info, bool startDownload)
        {
            var imageIndex = _AchievementImageList.Images.IndexOfKey(
                info.IsAchieved ? info.IconNormal : info.IconLocked);

            if (imageIndex >= 0)
            {
                info.ImageIndex = imageIndex;
            }
            else
            {
                _IconQueue.Add(info);

                if (startDownload)
                    DownloadNextIcon();
            }
        }

        private int StoreAchievements()
        {
            if (_AchievementListView.Items.Count == 0)
                return 0;

            var achievements = new List<AchievementInfo>();
            foreach (ListViewItem item in _AchievementListView.Items)
            {
                var achievementInfo = item.Tag as AchievementInfo;
                if (achievementInfo != null &&
                    achievementInfo.IsAchieved != item.Checked)
                {
                    achievementInfo.IsAchieved = item.Checked;
                    achievements.Add(item.Tag as AchievementInfo);
                }
            }

            if (achievements.Count == 0)
                return 0;

            foreach (var info in achievements)
                if (_SteamClient.SteamUserStats.SetAchievement(info.Id, info.IsAchieved) == false)
                {
                    MessageBox.Show(
                        this,
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "An error occurred while setting the state for {0}, aborting store.",
                            info.Id),
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return -1;
                }

            return achievements.Count;
        }

        private int StoreStatistics()
        {
            if (_Statistics.Count == 0)
                return 0;

            var statistics = _Statistics.Where(stat => stat.IsModified).ToList();
            if (statistics.Count == 0)
                return 0;

            foreach (var stat in statistics)
                if (stat is IntStatInfo)
                {
                    var intStat = (IntStatInfo)stat;
                    if (_SteamClient.SteamUserStats.SetStatValue(
                            intStat.Id,
                            intStat.IntValue) == false)
                    {
                        MessageBox.Show(
                            this,
                            string.Format(
                                CultureInfo.CurrentCulture,
                                "An error occurred while setting the value for {0}, aborting store.",
                                stat.Id),
                            "Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        return -1;
                    }
                }
                else if (stat is FloatStatInfo)
                {
                    var floatStat = (FloatStatInfo)stat;
                    if (_SteamClient.SteamUserStats.SetStatValue(
                            floatStat.Id,
                            floatStat.FloatValue) == false)
                    {
                        MessageBox.Show(
                            this,
                            string.Format(
                                CultureInfo.CurrentCulture,
                                "An error occurred while setting the value for {0}, aborting store.",
                                stat.Id),
                            "Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        return -1;
                    }
                }
                else
                {
                    throw new InvalidOperationException("unsupported stat type");
                }

            return statistics.Count;
        }

        private void DisableInput()
        {
            _ReloadButton.Enabled = false;
            _StoreButton.Enabled = false;
        }

        private void EnableInput()
        {
            _ReloadButton.Enabled = true;
            _StoreButton.Enabled = true;
        }

        private void OnTimer(object sender, EventArgs e)
        {
            _CallbackTimer.Enabled = false;
            _SteamClient.RunCallbacks(false);
            _CallbackTimer.Enabled = true;
        }

        private void OnRefresh(object sender, EventArgs e)
        {
            RefreshStats();
        }

        private void OnLockAll(object sender, EventArgs e)
        {
            foreach (ListViewItem item in _AchievementListView.Items)
                item.Checked = false;
        }

        private void OnInvertAll(object sender, EventArgs e)
        {
            foreach (ListViewItem item in _AchievementListView.Items)
                item.Checked = !item.Checked;
        }

        private void OnUnlockAll(object sender, EventArgs e)
        {
            foreach (ListViewItem item in _AchievementListView.Items)
                item.Checked = true;
        }

        private bool Store()
        {
            if (_SteamClient.SteamUserStats.StoreStats() == false)
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
            var achievements = StoreAchievements();
            if (achievements < 0)
            {
                RefreshStats();
                return;
            }

            var stats = StoreStatistics();
            if (stats < 0)
            {
                RefreshStats();
                return;
            }

            if (Store() == false)
            {
                RefreshStats();
                return;
            }

            MessageBox.Show(
                this,
                string.Format(
                    CultureInfo.CurrentCulture,
                    "Stored {0} achievements and {1} statistics.",
                    achievements,
                    stats),
                "Information",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            RefreshStats();
        }

        private void OnStatDataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            if (e.Context == DataGridViewDataErrorContexts.Commit)
            {
                var view = (DataGridView)sender;
                if (e.Exception is StatIsProtectedException)
                {
                    e.ThrowException = false;
                    e.Cancel = true;
                    view.Rows[e.RowIndex].ErrorText = "Stat is protected! -- you can't modify it";
                }
                else
                {
                    e.ThrowException = false;
                    e.Cancel = true;
                    view.Rows[e.RowIndex].ErrorText = "Invalid value";
                }
            }
        }

        private void OnStatAgreementChecked(object sender, EventArgs e)
        {
            _StatisticsDataGridView.Columns[1].ReadOnly = _EnableStatsEditingCheckBox.Checked == false;
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
                return;

            var achievementsToo = DialogResult.Yes == MessageBox.Show(
                                      "Do you want to reset achievements too?",
                                      "Question",
                                      MessageBoxButtons.YesNo,
                                      MessageBoxIcon.Question);

            if (MessageBox.Show(
                    "Really really sure?",
                    "Warning",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Error) == DialogResult.No)
                return;

            if (_SteamClient.SteamUserStats.ResetAllStats(achievementsToo) == false)
            {
                MessageBox.Show(this, "Failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            RefreshStats();
        }

        private void OnCheckAchievement(object sender, ItemCheckEventArgs e)
        {
            if (sender != _AchievementListView)
                return;

            if (_IsUpdatingAchievementList)
                return;

            var info = _AchievementListView.Items[e.Index].Tag
                as AchievementInfo;
            if (info == null)
                return;

            if ((info.Permission & 3) != 0)
            {
                MessageBox.Show(
                    this,
                    "Sorry, but this is a protected achievement and cannot be managed with Steam Achievement Manager.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                e.NewValue = e.CurrentValue;
            }
        }
    }
}