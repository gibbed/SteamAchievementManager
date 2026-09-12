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
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static SAM.Game.InvariantShorthand;
using APITypes = SAM.API.Types;

namespace SAM.Game
{
    public class AchievementViewModel : INotifyPropertyChanged
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string IconNormal { get; set; }
        public string IconLocked { get; set; }
        public bool IsHidden { get; set; }
        public int Permission { get; set; }
        public Visibility HiddenBadgeVisibility => this.IsHidden ? Visibility.Visible : Visibility.Collapsed;

        private float _globalPercentage = -1.0f;
        public float GlobalPercentage
        {
            get => this._globalPercentage;
            set
            {
                if (Math.Abs(this._globalPercentage - value) > 0.01f)
                {
                    this._globalPercentage = value;
                    OnPropertyChanged(nameof(GlobalPercentage));
                    OnPropertyChanged(nameof(RarityText));
                    OnPropertyChanged(nameof(RarityColor));
                    OnPropertyChanged(nameof(RarityBadgeVisibility));
                }
            }
        }

        public string RarityText => this.GlobalPercentage >= 0.0f ? $"{this.GlobalPercentage:0.0}%" : "";
        public string RarityColor => this.GlobalPercentage switch
        {
            <= 10.0f => "#f2c94c",
            <= 25.0f => "#bb6bd9",
            _ => "#8f98a0"
        };
        public Visibility RarityBadgeVisibility => this.GlobalPercentage >= 0.0f ? Visibility.Visible : Visibility.Collapsed;

        private bool _isAchieved;
        public bool IsAchieved
        {
            get => this._isAchieved;
            set
            {
                if (this._isAchieved != value)
                {
                    this._isAchieved = value;
                    OnPropertyChanged(nameof(IsAchieved));
                    OnPropertyChanged(nameof(DisplayIcon));
                }
            }
        }

        public DateTime? UnlockTime { get; set; }
        public string UnlockTimeText => this.UnlockTime.HasValue ? this.UnlockTime.Value.ToString("g") : "";

        private BitmapImage _normalBitmap;
        public BitmapImage NormalBitmap
        {
            get => this._normalBitmap;
            set
            {
                if (this._normalBitmap != value)
                {
                    this._normalBitmap = value;
                    OnPropertyChanged(nameof(NormalBitmap));
                    OnPropertyChanged(nameof(DisplayIcon));
                }
            }
        }

        private BitmapImage _lockedBitmap;
        public BitmapImage LockedBitmap
        {
            get => this._lockedBitmap;
            set
            {
                if (this._lockedBitmap != value)
                {
                    this._lockedBitmap = value;
                    OnPropertyChanged(nameof(LockedBitmap));
                    OnPropertyChanged(nameof(DisplayIcon));
                }
            }
        }

        public BitmapImage DisplayIcon => this.IsAchieved ? (this.NormalBitmap ?? this.LockedBitmap) : (this.LockedBitmap ?? this.NormalBitmap);

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public partial class ManagerWindow : Window
    {
        private readonly long _GameId;
        private readonly string _GameTitleName;
        private readonly API.Client _SteamClient;
        private readonly List<Stats.AchievementDefinition> _AchievementDefinitions = new();
        private readonly List<Stats.StatDefinition> _StatDefinitions = new();
        private readonly List<Stats.StatInfo> _Statistics = new();
        private readonly List<AchievementViewModel> _AchievementsList = new();
        private readonly List<AchievementViewModel> _FilteredAchievements = new();

        private readonly API.Callbacks.UserStatsReceived _UserStatsReceivedCallback;
        private readonly DispatcherTimer _CallbackTimer;
        private readonly List<AchievementViewModel> _IconQueue = new();
        private readonly WebClient _IconDownloader = new();

        public ManagerWindow(long gameId, API.Client client)
        {
            this._GameId = gameId;
            this._SteamClient = client;

            this.InitializeComponent();

            try
            {
                var uri = new Uri("pack://application:,,,/Blank.ico", UriKind.Absolute);
                this.Icon = new BitmapImage(uri);
                this.ImgAppIcon.Source = this.Icon;
            }
            catch { }

            this._IconDownloader.DownloadDataCompleted += this.OnIconDownloadCompleted;

            string name = this._SteamClient.SteamApps001.GetAppData((uint)this._GameId, "name");
            this._GameTitleName = name ?? this._GameId.ToString(CultureInfo.InvariantCulture);

            this._UserStatsReceivedCallback = client.CreateAndRegisterCallback<API.Callbacks.UserStatsReceived>();
            this._UserStatsReceivedCallback.OnRun += this.OnUserStatsReceived;

            this._CallbackTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            this._CallbackTimer.Tick += (s, e) => this._SteamClient.RunCallbacks(false);
            this._CallbackTimer.Start();

            this.InitializeLanguageDropdown();
            this.ApplyLocalization();

            this.RefreshStats();
        }

        private void OnTitleBarMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
        }

        private void OnMinimizeClick(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;
        private void OnMaximizeClick(object sender, RoutedEventArgs e) =>
            this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        private void OnCloseClick(object sender, RoutedEventArgs e) => this.Close();

        private void InitializeLanguageDropdown()
        {
            this.MenuLanguage.Items.Clear();
            foreach (var lang in API.LanguageManager.SupportedLanguages)
            {
                var item = new MenuItem
                {
                    Header = lang.NativeName,
                    Tag = lang.Code,
                    IsCheckable = true,
                    StaysOpenOnClick = true
                };
                item.Click += (s, e) =>
                {
                    API.LanguageManager.CurrentLanguage = lang.Code;
                    this.UpdateLanguageDropdownState();
                    this.ApplyLocalization();
                    this.RefreshStats();
                };
                this.MenuLanguage.Items.Add(item);
            }
            this.UpdateLanguageDropdownState();
        }

        private void UpdateLanguageDropdownState()
        {
            string current = API.LanguageManager.CurrentLanguage;
            foreach (MenuItem item in this.MenuLanguage.Items)
            {
                item.IsChecked = string.Equals((string)item.Tag, current, StringComparison.OrdinalIgnoreCase);
            }
        }

        private void ApplyLocalization()
        {
            string title = API.Localization.GameTitle(this._GameTitleName);
            this.TxtTitle.Text = title;
            this.Title = title;

            this.TxtBtnSave.Text = API.Localization.CommitChanges;
            this.BtnSave.ToolTip = API.Localization.CommitChangesToolTip;
            this.TxtBtnRefresh.Text = API.Localization.Refresh;
            this.BtnRefresh.ToolTip = API.Localization.RefreshToolTip;
            this.TxtBtnReset.Text = API.Localization.Reset;
            this.BtnReset.ToolTip = API.Localization.ResetToolTip;

            this.TabAchievements.Header = API.Localization.AchievementsTab;
            this.TabStatistics.Header = API.Localization.StatisticsTab;

            this.TxtLockAll.Text = "🔒 " + API.Localization.LockAll;
            this.TxtInvertAll.Text = "🔄 " + API.Localization.InvertAll;
            this.TxtUnlockAll.Text = "🔓 " + API.Localization.UnlockAll;

            this.MenuFilterAchievements.Header = "👁️ " + API.Localization.AchievementView;
            this.MenuFilterUnlocked.Header = API.Localization.ShowUnlocked;
            this.MenuFilterLocked.Header = API.Localization.ShowLocked;
            this.MenuFilterHidden.Header = API.Localization.ShowHidden;

            this.ColStatName.Header = API.Localization.HeaderName;
            this.ColStatValue.Header = API.Localization.HeaderValue;
            this.ColStatExtra.Header = API.Localization.HeaderExtra;

            this.TxtEnableStats.Text = API.Localization.EnableStatsEditing;
            this.MenuLanguage.Header = "🌐 " + API.Localization.Language;
        }

        private bool LoadUserGameStatsSchema()
        {
            try
            {
                string fileName = _($"UserGameStatsSchema_{this._GameId}.bin");
                string path = API.Steam.GetInstallPath();
                path = Path.Combine(path, "appcache", "stats", fileName);
                if (File.Exists(path) == false) return false;

                var kv = KeyValue.LoadAsBinary(path);
                if (kv == null) return false;

                var currentLanguage = API.LanguageManager.CurrentLanguage;

                this._AchievementDefinitions.Clear();
                this._StatDefinitions.Clear();

                var stats = kv[this._GameId.ToString(CultureInfo.InvariantCulture)]["stats"];
                if (stats.Valid == false || stats.Children == null) return false;

                foreach (var stat in stats.Children)
                {
                    if (stat.Valid == false) continue;

                    APITypes.UserStatType type = APITypes.UserStatType.Invalid;
                    var typeNode = stat["type"];
                    if (typeNode.Valid && typeNode.Type == KeyValueType.String)
                    {
                        Enum.TryParse((string)typeNode.Value, true, out type);
                    }

                    if (type == APITypes.UserStatType.Invalid)
                    {
                        var typeIntNode = stat["type_int"];
                        int rawType = typeIntNode.Valid ? typeIntNode.AsInteger(0) : typeNode.AsInteger(0);
                        type = (APITypes.UserStatType)rawType;
                    }

                    switch (type)
                    {
                        case APITypes.UserStatType.Integer:
                            {
                                var id = stat["name"].AsString("");
                                string name = GetLocalizedString(stat["display"]["name"], currentLanguage, id);
                                this._StatDefinitions.Add(new Stats.IntegerStatDefinition
                                {
                                    Id = id,
                                    DisplayName = name,
                                    MinValue = stat["min"].AsInteger(int.MinValue),
                                    MaxValue = stat["max"].AsInteger(int.MaxValue),
                                    MaxChange = stat["maxchange"].AsInteger(0),
                                    IncrementOnly = stat["incrementonly"].AsBoolean(false),
                                    SetByTrustedGameServer = stat["bSetByTrustedGS"].AsBoolean(false),
                                    DefaultValue = stat["default"].AsInteger(0),
                                    Permission = stat["permission"].AsInteger(0)
                                });
                                break;
                            }
                        case APITypes.UserStatType.Float:
                        case APITypes.UserStatType.AverageRate:
                            {
                                var id = stat["name"].AsString("");
                                string name = GetLocalizedString(stat["display"]["name"], currentLanguage, id);
                                this._StatDefinitions.Add(new Stats.FloatStatDefinition
                                {
                                    Id = id,
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
                                {
                                    foreach (var bits in stat.Children.Where(b => string.Compare(b.Name, "bits", StringComparison.InvariantCultureIgnoreCase) == 0))
                                    {
                                        if (bits.Valid == false || bits.Children == null) continue;
                                        foreach (var bit in bits.Children)
                                        {
                                            string id = bit["name"].AsString("");
                                            string name = GetLocalizedString(bit["display"]["name"], currentLanguage, id);
                                            string desc = GetLocalizedString(bit["display"]["desc"], currentLanguage, "");

                                            this._AchievementDefinitions.Add(new()
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
                                }
                                break;
                            }
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string GetLocalizedString(KeyValue kv, string activeLanguage, string defaultValue)
        {
            var name = kv[activeLanguage].AsString("");
            if (string.IsNullOrEmpty(name) == false) return name;

            if (activeLanguage != "english")
            {
                name = kv["english"].AsString("");
                if (string.IsNullOrEmpty(name) == false) return name;
            }

            name = kv.AsString("");
            return string.IsNullOrEmpty(name) == false ? name : defaultValue;
        }

        private void OnUserStatsReceived(APITypes.UserStatsReceived param)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (param.Result != 1)
                {
                    this.TxtStatus.Text = API.Localization.ErrorRetrievingStats(param.Result.ToString());
                    return;
                }

                if (this.LoadUserGameStatsSchema() == false)
                {
                    this.TxtStatus.Text = API.Localization.FailedToLoadSchema;
                    return;
                }

                this.GetAchievements();
                this.GetStatistics();

                this.TxtStatus.Text = API.Localization.RetrievedAchievementsAndStats(this._AchievementsList.Count, this._Statistics.Count);
                this.BtnSave.IsEnabled = true;
            });
        }

        private void RefreshStats()
        {
            this._AchievementsList.Clear();
            this._Statistics.Clear();
            this.ItemsAchievements.ItemsSource = null;
            this.GridStatistics.ItemsSource = null;

            var steamId = this._SteamClient.SteamUser.GetSteamId();
            if (this._SteamClient.SteamUserStats.RequestUserStats(steamId) == API.CallHandle.Invalid)
            {
                MessageBox.Show(this, API.Localization.Error, API.Localization.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            this.TxtStatus.Text = API.Localization.RetrievingStatInfo;
        }

        private void GetAchievements()
        {
            this._AchievementsList.Clear();
            this._IconQueue.Clear();

            foreach (var def in this._AchievementDefinitions)
            {
                if (string.IsNullOrEmpty(def.Id)) continue;
                if (this._SteamClient.SteamUserStats.GetAchievementAndUnlockTime(def.Id, out bool isAchieved, out var unlockTime) == false)
                {
                    continue;
                }

                var vm = new AchievementViewModel
                {
                    Id = def.Id,
                    Name = def.Name,
                    Description = def.Description,
                    IconNormal = string.IsNullOrEmpty(def.IconNormal) ? null : def.IconNormal,
                    IconLocked = string.IsNullOrEmpty(def.IconLocked) ? def.IconNormal : def.IconLocked,
                    IsHidden = def.IsHidden,
                    Permission = def.Permission,
                    IsAchieved = isAchieved,
                    UnlockTime = isAchieved && unlockTime > 0 ? DateTimeOffset.FromUnixTimeSeconds(unlockTime).LocalDateTime : null
                };

                this._AchievementsList.Add(vm);
                this._IconQueue.Add(vm);
            }

            this.UpdateAchievementCounters();
            this.FilterAchievements();
            this.DownloadNextIcon();
            this.FetchGlobalPercentages();
        }

        private void FetchGlobalPercentages()
        {
            try
            {
                using WebClient client = new();
                string url = _($"https://api.steampowered.com/ISteamUserStats/GetGlobalAchievementPercentagesForApp/v0002/?gameid={this._GameId}");
                client.DownloadStringCompleted += (s, e) =>
                {
                    if (e.Error != null || string.IsNullOrEmpty(e.Result)) return;
                    try
                    {
                        var json = e.Result;
                        var dict = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

                        int idx = 0;
                        while ((idx = json.IndexOf("\"name\":", idx, StringComparison.OrdinalIgnoreCase)) > 0)
                        {
                            int q1 = json.IndexOf('\"', idx + 7);
                            int q2 = json.IndexOf('\"', q1 + 1);
                            if (q1 < 0 || q2 < 0) break;

                            string achName = json.Substring(q1 + 1, q2 - q1 - 1);
                            int pctIdx = json.IndexOf("\"percent\":", q2, StringComparison.OrdinalIgnoreCase);
                            if (pctIdx > 0 && pctIdx - q2 < 50)
                            {
                                int commaIdx = json.IndexOfAny(new[] { ',', '}' }, pctIdx + 10);
                                string pctStr = json.Substring(pctIdx + 10, commaIdx - pctIdx - 10).Trim();
                                if (float.TryParse(pctStr, NumberStyles.Any, CultureInfo.InvariantCulture, out float pct))
                                {
                                    dict[achName] = pct;
                                }
                            }
                            idx = q2 + 1;
                        }

                        this.Dispatcher.Invoke(() =>
                        {
                            foreach (var ach in this._AchievementsList)
                            {
                                if (dict.TryGetValue(ach.Id, out float pct))
                                {
                                    ach.GlobalPercentage = pct;
                                }
                            }
                            this.FilterAchievements();
                        });
                    }
                    catch { }
                };
                client.DownloadStringAsync(new Uri(url));
            }
            catch { }
        }

        private void UpdateAchievementCounters()
        {
            int hiddenCount = this._AchievementsList.Count(a => a.IsHidden);
            int normalCount = this._AchievementsList.Count - hiddenCount;

            this.TxtNormalCount.Text = normalCount.ToString(CultureInfo.InvariantCulture);
            this.TxtHiddenCount.Text = hiddenCount.ToString(CultureInfo.InvariantCulture);
        }

        private void GetStatistics()
        {
            this._Statistics.Clear();
            foreach (var def in this._StatDefinitions)
            {
                if (def is Stats.IntegerStatDefinition intDef)
                {
                    if (this._SteamClient.SteamUserStats.GetStatValue(intDef.Id, out int value))
                    {
                        this._Statistics.Add(new Stats.IntStatInfo
                        {
                            Id = intDef.Id,
                            DisplayName = intDef.DisplayName,
                            IntValue = value,
                            OriginalValue = value,
                            Permission = intDef.Permission
                        });
                    }
                }
                else if (def is Stats.FloatStatDefinition floatDef)
                {
                    if (this._SteamClient.SteamUserStats.GetStatValue(floatDef.Id, out float value))
                    {
                        this._Statistics.Add(new Stats.FloatStatInfo
                        {
                            Id = floatDef.Id,
                            DisplayName = floatDef.DisplayName,
                            FloatValue = value,
                            OriginalValue = value,
                            Permission = floatDef.Permission
                        });
                    }
                }
            }
            this.GridStatistics.ItemsSource = null;
            this.GridStatistics.ItemsSource = this._Statistics;
        }

        private void FilterAchievements()
        {
            string search = this.TxtSearchAchievement.Text.Length > 0 ? this.TxtSearchAchievement.Text : null;
            bool wantUnlocked = this.MenuFilterUnlocked.IsChecked;
            bool wantLocked = this.MenuFilterLocked.IsChecked;
            bool wantHidden = this.MenuFilterHidden.IsChecked;

            var list = new List<AchievementViewModel>();
            foreach (var ach in this._AchievementsList)
            {
                if (ach.IsHidden && wantHidden == false) continue;
                if (ach.IsAchieved && wantUnlocked == false) continue;
                if (ach.IsAchieved == false && wantLocked == false) continue;

                if (search != null)
                {
                    if ((ach.Name?.IndexOf(search, StringComparison.OrdinalIgnoreCase) ?? -1) < 0 &&
                        (ach.Description?.IndexOf(search, StringComparison.OrdinalIgnoreCase) ?? -1) < 0)
                    {
                        continue;
                    }
                }
                list.Add(ach);
            }

            if (this.MenuSortRarityAsc != null && this.MenuSortRarityAsc.IsChecked)
            {
                list = list.OrderBy(a => a.GlobalPercentage < 0 ? 100.0f : a.GlobalPercentage).ToList();
            }
            else if (this.MenuSortRarityDesc != null && this.MenuSortRarityDesc.IsChecked)
            {
                list = list.OrderByDescending(a => a.GlobalPercentage).ToList();
            }
            else
            {
                list = list.OrderBy(a => a.Name).ToList();
            }

            this._FilteredAchievements.Clear();
            this._FilteredAchievements.AddRange(list);

            this.ItemsAchievements.ItemsSource = null;
            this.ItemsAchievements.ItemsSource = this._FilteredAchievements;
        }

        private void OnAchievementSearchChanged(object sender, TextChangedEventArgs e)
        {
            this.TxtSearchAchPlaceholder.Visibility = string.IsNullOrEmpty(this.TxtSearchAchievement.Text) ? Visibility.Visible : Visibility.Collapsed;
            this.FilterAchievements();
        }

        private void OnAchievementFilterMenuClick(object sender, RoutedEventArgs e) => this.FilterAchievements();

        private void OnSortMenuClick(object sender, RoutedEventArgs e)
        {
            if (sender == this.MenuSortName)
            {
                this.MenuSortName.IsChecked = true;
                this.MenuSortRarityAsc.IsChecked = false;
                this.MenuSortRarityDesc.IsChecked = false;
            }
            else if (sender == this.MenuSortRarityAsc)
            {
                this.MenuSortName.IsChecked = false;
                this.MenuSortRarityAsc.IsChecked = true;
                this.MenuSortRarityDesc.IsChecked = false;
            }
            else if (sender == this.MenuSortRarityDesc)
            {
                this.MenuSortName.IsChecked = false;
                this.MenuSortRarityAsc.IsChecked = false;
                this.MenuSortRarityDesc.IsChecked = true;
            }
            this.FilterAchievements();
        }

        private void OnLockAllClick(object sender, RoutedEventArgs e)
        {
            foreach (var ach in this._AchievementsList) ach.IsAchieved = false;
        }

        private void OnInvertAllClick(object sender, RoutedEventArgs e)
        {
            foreach (var ach in this._AchievementsList) ach.IsAchieved = !ach.IsAchieved;
        }

        private void OnUnlockAllClick(object sender, RoutedEventArgs e)
        {
            foreach (var ach in this._AchievementsList) ach.IsAchieved = true;
        }

        private void OnAchievementCheckChanged(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox cb && cb.DataContext is AchievementViewModel vm)
            {
                if ((vm.Permission & 3) != 0)
                {
                    MessageBox.Show(this, API.Localization.ProtectedAchievementNotice, API.Localization.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    vm.IsAchieved = !vm.IsAchieved;
                }
            }
        }

        private void DownloadNextIcon()
        {
            if (this._IconQueue.Count == 0)
            {
                this.TxtDownloadStatus.Text = "";
                return;
            }

            var vm = this._IconQueue[0];
            this._IconQueue.RemoveAt(0);

            string iconName = vm.IsAchieved ? vm.IconNormal : vm.IconLocked;
            if (string.IsNullOrEmpty(iconName))
            {
                this.DownloadNextIcon();
                return;
            }

            string url = _($"https://cdn.steamstatic.com/steamcommunity/public/images/apps/{this._GameId}/{iconName}");

            byte[] cachedBytes = API.ImageCache.GetOrDownloadImage(url);
            if (cachedBytes != null && cachedBytes.Length > 0)
            {
                try
                {
                    using MemoryStream ms = new(cachedBytes);
                    BitmapImage bmp = new();
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.StreamSource = ms;
                    bmp.EndInit();
                    bmp.Freeze();

                    if (vm.IsAchieved) vm.NormalBitmap = bmp;
                    else vm.LockedBitmap = bmp;
                }
                catch { }
                this.DownloadNextIcon();
                return;
            }

            this.TxtDownloadStatus.Text = API.Localization.DownloadingIcons(this._IconQueue.Count);
            if (this._IconDownloader.IsBusy == false)
            {
                this._IconDownloader.DownloadDataAsync(new Uri(url), vm);
            }
        }

        private void OnIconDownloadCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            if (e.Error == null && e.Cancelled == false && e.UserState is AchievementViewModel vm)
            {
                try
                {
                    using MemoryStream ms = new(e.Result);
                    BitmapImage bmp = new();
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.StreamSource = ms;
                    bmp.EndInit();
                    bmp.Freeze();

                    this.Dispatcher.Invoke(() =>
                    {
                        if (vm.IsAchieved) vm.NormalBitmap = bmp;
                        else vm.LockedBitmap = bmp;
                    });
                }
                catch { }
            }
            this.DownloadNextIcon();
        }

        private void OnStoreClick(object sender, RoutedEventArgs e)
        {
            int achCount = 0;
            foreach (var ach in this._AchievementsList)
            {
                if (this._SteamClient.SteamUserStats.SetAchievement(ach.Id, ach.IsAchieved) == false)
                {
                    MessageBox.Show(this, API.Localization.ErrorSettingState(ach.Id), API.Localization.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    this.RefreshStats();
                    return;
                }
                achCount++;
            }

            int statCount = 0;
            foreach (var stat in this._Statistics.Where(s => s.IsModified))
            {
                bool success = stat switch
                {
                    Stats.IntStatInfo i => this._SteamClient.SteamUserStats.SetStatValue(i.Id, i.IntValue),
                    Stats.FloatStatInfo f => this._SteamClient.SteamUserStats.SetStatValue(f.Id, f.FloatValue),
                    _ => false
                };

                if (success == false)
                {
                    MessageBox.Show(this, API.Localization.ErrorSettingValue(stat.Id), API.Localization.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    this.RefreshStats();
                    return;
                }
                statCount++;
            }

            if (this._SteamClient.SteamUserStats.StoreStats() == false)
            {
                MessageBox.Show(this, API.Localization.ErrorStoringAborting, API.Localization.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                this.RefreshStats();
                return;
            }

            MessageBox.Show(this, API.Localization.StoredAchievementsAndStats(achCount, statCount), API.Localization.Information, MessageBoxButton.OK, MessageBoxImage.Information);
            this.RefreshStats();
        }

        private void OnRefreshClick(object sender, RoutedEventArgs e) => this.RefreshStats();

        private void OnResetClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(this, API.Localization.ConfirmResetStats, API.Localization.Warning, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
            {
                return;
            }

            bool achievementsToo = MessageBoxResult.Yes == MessageBox.Show(this, API.Localization.ConfirmResetAchievementsToo, API.Localization.Question, MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (MessageBox.Show(this, API.Localization.ConfirmReallySure, API.Localization.Warning, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
            {
                return;
            }

            if (this._SteamClient.SteamUserStats.ResetAllStats(achievementsToo) == false)
            {
                MessageBox.Show(this, API.Localization.Error, API.Localization.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            this.RefreshStats();
        }

        private void OnEnableStatsChecked(object sender, RoutedEventArgs e)
        {
            this.ColStatValue.IsReadOnly = this.ChkEnableStats.IsChecked != true;
        }
    }
}
