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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml.XPath;
using static SAM.Picker.InvariantShorthand;
using APITypes = SAM.API.Types;

namespace SAM.Picker
{
    public partial class MainWindow : Window
    {
        private readonly API.Client _SteamClient;
        private readonly ConcurrentDictionary<uint, GameInfo> _Games;
        private readonly List<GameInfo> _FilteredGames;

        private readonly object _LogoLock;
        private readonly HashSet<string> _LogosAttempting;
        private readonly HashSet<string> _LogosAttempted;
        private readonly ConcurrentQueue<GameInfo> _LogoQueue;

        private readonly API.Callbacks.AppDataChanged _AppDataChangedCallback;
        private readonly BackgroundWorker _ListWorker;
        private readonly DispatcherTimer _CallbackTimer;

        public MainWindow(API.Client client)
        {
            this._Games = new();
            this._FilteredGames = new();
            this._LogoLock = new();
            this._LogosAttempting = new();
            this._LogosAttempted = new();
            this._LogoQueue = new();

            this.InitializeComponent();

            try
            {
                var uri = new Uri("pack://application:,,,/SAM.ico", UriKind.Absolute);
                this.Icon = new BitmapImage(uri);
                this.ImgAppIcon.Source = this.Icon;
            }
            catch { }

            this._SteamClient = client;

            this._AppDataChangedCallback = client.CreateAndRegisterCallback<API.Callbacks.AppDataChanged>();
            this._AppDataChangedCallback.OnRun += this.OnAppDataChanged;

            this._ListWorker = new BackgroundWorker();
            this._ListWorker.DoWork += this.DoDownloadList;
            this._ListWorker.RunWorkerCompleted += this.OnDownloadList;

            this._CallbackTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            this._CallbackTimer.Tick += (s, e) =>
            {
                lock (API.Steam.SteamLock)
                {
                    this._SteamClient.RunCallbacks(false);
                }
            };
            this._CallbackTimer.Start();

            this.InitializeLanguageDropdown();
            this.ApplyLocalization();

            this.AddGames();
        }

        private void OnTitleBarMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
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
                    this.RefreshGames();
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
            this.TxtTitle.Text = API.Localization.PickerTitle;
            this.TxtBtnRefresh.Text = API.Localization.RefreshGames;
            this.TxtBtnAddGame.Text = API.Localization.AddGame;
            this.MenuFilter.Header = "📺 " + API.Localization.GameFiltering;
            this.MenuFilterGames.Header = API.Localization.ShowGames.Replace("&", "");
            this.MenuFilterDemos.Header = API.Localization.ShowDemos.Replace("&", "");
            this.MenuFilterMods.Header = API.Localization.ShowMods.Replace("&", "");
            this.MenuFilterJunk.Header = API.Localization.ShowJunk.Replace("&", "");
            this.MenuLanguage.Header = "🌐 " + API.Localization.Language;

            if (this._Games.Count > 0)
            {
                this.TxtStatus.Text = API.Localization.DisplayingGames(this._FilteredGames.Count, this._Games.Count);
            }
        }

        private void OnAppDataChanged(APITypes.AppDataChanged param)
        {
            if (param.Result == false) return;
            if (this._Games.TryGetValue(param.Id, out var game) == false) return;

            lock (API.Steam.SteamLock)
            {
                game.Name = this._SteamClient.SteamApps001.GetAppData(game.Id, "name");
                game.ImageUrl = GetGameImageUrl(game.Id);
            }
            this.RefreshGames();
        }

        private void AddGames()
        {
            if (this._ListWorker.IsBusy) return;
            this._Games.Clear();
            this.RefreshGames();
            this.BtnRefresh.IsEnabled = true;
            this._ListWorker.RunWorkerAsync();
        }

        private void DoDownloadList(object sender, DoWorkEventArgs e)
        {
            try
            {
                this.Dispatcher.Invoke(() => this.TxtStatus.Text = API.Localization.DownloadingGameList);

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
                    while (nodes.MoveNext())
                    {
                        string type = nodes.Current.GetAttribute("type", "");
                        if (string.IsNullOrEmpty(type)) type = "normal";
                        pairs.Add(new((uint)nodes.Current.ValueAsLong, type));
                    }
                }

                this.Dispatcher.Invoke(() => this.TxtStatus.Text = API.Localization.CheckingGameOwnership);
                foreach (var kv in pairs)
                {
                    this.AddGame(kv.Key, kv.Value);
                }
                this.Dispatcher.Invoke(() => this.RefreshGames());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        private void OnDownloadList(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                try
                {
                    if (e.Error != null || e.Cancelled || this._Games.IsEmpty)
                    {
                        this.AddDefaultGames();
                    }

                    this.RefreshGames();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                }
                finally
                {
                    this.BtnRefresh.IsEnabled = true;
                }
            });
        }

        private void AddDefaultGames()
        {
            this.AddGame(480, "normal"); // Spacewar
        }

        private bool OwnsGame(uint id)
        {
            lock (API.Steam.SteamLock)
            {
                return this._SteamClient.SteamApps008.IsSubscribedApp(id);
            }
        }

        private HashSet<uint> _FavoriteAppIds = null;

        private void AddGame(uint id, string type)
        {
            if (this._Games.ContainsKey(id)) return;
            try
            {
                if (this.OwnsGame(id) == false) return;

                if (this._FavoriteAppIds == null)
                {
                    this._FavoriteAppIds = API.SteamLibraryFavourites.GetFavoriteAppIds();
                }

                GameInfo info = new(id, type)
                {
                    IsFavorite = this._FavoriteAppIds.Contains(id)
                };

                lock (API.Steam.SteamLock)
                {
                    info.Name = this._SteamClient.SteamApps001.GetAppData(info.Id, "name");
                    info.ImageUrl = GetGameImageUrl(info.Id);
                }
                this._Games.TryAdd(id, info);
            }
            catch { }
        }

        private void RefreshGames()
        {
            var nameSearch = this.TxtSearchGame.Text.Length > 0 ? this.TxtSearchGame.Text : null;
            bool wantNormals = this.MenuFilterGames.IsChecked;
            bool wantFavoritesOnly = this.MenuFilterFavorites.IsChecked;
            bool wantDemos = this.MenuFilterDemos.IsChecked;
            bool wantMods = this.MenuFilterMods.IsChecked;
            bool wantJunk = this.MenuFilterJunk.IsChecked;

            this._FilteredGames.Clear();
            foreach (var info in this._Games.Values.OrderBy(gi => gi.Name))
            {
                if (nameSearch != null && info.Name.IndexOf(nameSearch, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                if (wantFavoritesOnly && info.IsFavorite == false)
                {
                    continue;
                }

                bool wanted = info.Type switch
                {
                    "normal" => wantNormals,
                    "demo" => wantDemos,
                    "mod" => wantMods,
                    "junk" => wantJunk,
                    _ => true
                };

                if (wanted == false) continue;
                this._FilteredGames.Add(info);
            }

            this.ItemsGameCards.ItemsSource = null;
            this.ItemsGameCards.ItemsSource = this._FilteredGames;
            this.TxtStatus.Text = API.Localization.DisplayingGames(this._FilteredGames.Count, this._Games.Count);
        }

        private void OnSearchFilterChanged(object sender, TextChangedEventArgs e)
        {
            this.TxtSearchPlaceholder.Visibility = string.IsNullOrEmpty(this.TxtSearchGame.Text) ? Visibility.Visible : Visibility.Collapsed;
            this.RefreshGames();
        }

        private void OnFilterMenuClick(object sender, RoutedEventArgs e) => this.RefreshGames();

        private void OnRefreshClick(object sender, RoutedEventArgs e)
        {
            this.TxtSearchGame.Text = "";
            this.TxtAddGameId.Text = "";
            this.AddGames();
        }

        private void OnAddGameClick(object sender, RoutedEventArgs e)
        {
            if (uint.TryParse(this.TxtAddGameId.Text, out uint id) == false)
            {
                MessageBox.Show(this, API.Localization.PleaseEnterValidGameId, API.Localization.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (this.OwnsGame(id) == false)
            {
                MessageBox.Show(this, API.Localization.DontOwnGame, API.Localization.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            this.TxtAddGameId.Text = "";
            this._Games.Clear();
            this.AddGame(id, "normal");
            this.MenuFilterGames.IsChecked = true;
            this.RefreshGames();
        }

        private string GetGameImageUrl(uint id)
        {
            string candidate;
            var activeLanguage = API.LanguageManager.CurrentLanguage;

            candidate = this._SteamClient.SteamApps001.GetAppData(id, _($"small_capsule/{activeLanguage}"));
            if (string.IsNullOrEmpty(candidate) == false)
            {
                return _($"https://shared.cloudflare.steamstatic.com/store_item_assets/steam/apps/{id}/{candidate}");
            }

            if (activeLanguage != "english")
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

        private void OnMenuOpenManagerClick(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is GameInfo info)
            {
                this.LaunchGameManager(info.Id, false);
            }
        }

        private void OnMenuStartIdleClick(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is GameInfo info)
            {
                this.LaunchGameManager(info.Id, true);
            }
        }

        private void OnGameSelectionChanged(object sender, RoutedEventArgs e)
        {
            int selectedCount = this._Games.Values.Count(g => g.IsSelected);
            if (selectedCount > 0)
            {
                this.BtnFarmSelected.Visibility = Visibility.Visible;
                this.TxtBtnFarmSelected.Text = $"Фармить выбранные ({selectedCount})";
            }
            else
            {
                this.BtnFarmSelected.Visibility = Visibility.Collapsed;
            }
        }

        private void OnFarmSelectedClick(object sender, RoutedEventArgs e)
        {
            var selectedGames = this._Games.Values.Where(g => g.IsSelected).ToList();
            if (selectedGames.Count == 0) return;

            foreach (var game in selectedGames)
            {
                this.LaunchGameManager(game.Id, true);
            }
        }

        private void OnGameCardClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is GameInfo info)
            {
                this.LaunchGameManager(info.Id, false);
            }
        }

        private void LaunchGameManager(uint appId, bool idleMode)
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string exePath = Path.Combine(baseDir, "SAM.Game.exe");
                if (File.Exists(exePath) == false)
                {
                    exePath = Path.GetFullPath(Path.Combine(baseDir, "..", "SAM.Game.exe"));
                }

                string args = appId.ToString(CultureInfo.InvariantCulture) + (idleMode ? " -idle" : "");

                ProcessStartInfo psi = new()
                {
                    FileName = File.Exists(exePath) ? exePath : "SAM.Game.exe",
                    Arguments = args,
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(File.Exists(exePath) ? exePath : baseDir)
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, API.Localization.FailedToStartGameExe + "\n\n" + ex.Message, API.Localization.Error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
