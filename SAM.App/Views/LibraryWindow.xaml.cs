using System.Diagnostics;
using System.Globalization;
using System.Text.Json.Nodes;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using SAM.App.Columns;
using SAM.App.Helpers;
using SAM.App.Library;
using SAM.App.Lists;
using SAM.App.Localization;
using SAM.App.Steam;
using Windows.ApplicationModel.DataTransfer;

namespace SAM.App.Views;

/// <summary>Game library - the WinUI 3 version of SAM.Picker.</summary>
public sealed partial class LibraryWindow : Window, INotifier, IColumnHost
{
    private const double CardMinWidth = 236;
    private const double CardMargin = 12;

    private readonly WindowChrome _chrome;
    private readonly DispatcherQueueTimer _notifyTimer;
    private readonly List<GameEntry> _games = [];
    private readonly Dictionary<uint, GameEntry> _gamesById = [];
    private readonly HashSet<ProgressState> _collapsed = ReadCollapsedSections();
    private List<GameGroup> _groups = [];
    private string? _steamPath;
    private string _sortKey = ColumnLayouts.Name;
    private bool _sortDescending;
    private readonly Dictionary<string, IReadOnlySet<string>> _columnFilters = [];

    private SteamSession? _session;
    private ListsPage? _listsPage;
    private bool _grouped = AppSettings.ReadString("libraryGrouped") != "false";
    private CancellationTokenSource? _loadCancellation;
    private bool _isLoading;

    public LibraryWindow()
    {
        InitializeComponent();

        Root.FlowDirection = AppLanguage.FlowDirection;
        SearchBox.FlowDirection = AppLanguage.FlowDirection;
        LanguageButtonText.Text = AppLanguage.ShortName;

        // Last chosen view (tiles or list) and sorting, e.g. "Achievements:desc".
        ViewSelector.SelectedItem = AppSettings.ReadString("libraryView") == "List" ? ListModeItem : TileModeItem;

        if (AppSettings.ReadString("librarySort")?.Split(':') is [var key, ..] parts && ColumnLayouts.Library.Find(key) is { Sortable: true })
        {
            _sortKey = key;
            _sortDescending = parts.Length > 1 && parts[1] == "desc";
        }

        UpdateSortMenu();
        GroupSectionsItem.IsChecked = _grouped;

        _chrome = WindowChrome.Apply(this, Root, AppTitleBar, RightInsetColumn, SearchBox, PageSelector);
        _chrome.CenterOnScreen(1320, 880, 820, 560);

        _notifyTimer = DispatcherQueue.CreateTimer();
        _notifyTimer.Interval = TimeSpan.FromSeconds(4);
        _notifyTimer.IsRepeating = false;
        _notifyTimer.Tick += (_, _) => NotifyBar.IsOpen = false;

        Root.Loaded += async (_, _) =>
        {
            if (StartupMessage is not null)
            {
                Notify(StartupMessage, InfoBarSeverity.Warning);
            }

            await ConnectAndLoadAsync();
        };

        Closed += (_, _) =>
        {
            _loadCancellation?.Cancel();
            _session?.Dispose();
        };
    }

    /// <summary>Notice at startup, e.g. for an invalid app ID on the command line.</summary>
    public string? StartupMessage { get; set; }

    // ---------------------------------------------------------------- Steam & game list

    private async Task ConnectAndLoadAsync()
    {
        ShowLoading(Strings.ConnectingToSteam, null);
        SetHeaderEnabled(false);
        RefreshButton.IsEnabled = false;

        // The lists depend on the old session - they are recreated with the new one.
        PageSelector.SelectedItem = LibraryPageItem;
        ListsHost.Children.Clear();
        _listsPage = null;

        _session?.Dispose();
        _session = null;

        try
        {
            _session = SteamSession.Connect(0, DispatcherQueue);
        }
        catch (SteamConnectException e)
        {
            ShowError(e.Title, e.Message, e.CanRetry);
            return;
        }

        var appDataChanged = _session.Client.CreateAndRegisterCallback<API.Callbacks.AppDataChanged>();
        appDataChanged.OnRun += OnAppDataChanged;

        SetHeaderEnabled(true);
        await LoadGamesAsync();
    }

    private void SetHeaderEnabled(bool enabled)
    {
        SearchBox.IsEnabled = enabled;
        CategoryFilterButton.IsEnabled = enabled;
        ListsPageItem.IsEnabled = enabled;
        SortButton.IsEnabled = enabled;
        OpenByIdButton.IsEnabled = enabled;
    }

    private async Task LoadGamesAsync()
    {
        if (_session is not { } session)
        {
            return;
        }

        _loadCancellation?.Cancel();
        var cancellation = _loadCancellation = new CancellationTokenSource();
        var token = cancellation.Token;

        _isLoading = true;
        RefreshButton.IsEnabled = false;
        ListInfoBar.IsOpen = false;
        ShowLoading(Strings.LoadingGameList, null);

        var list = await GameListSource.LoadAsync(token);

        if (token.IsCancellationRequested)
        {
            return;
        }

        if (list.Error is not null)
        {
            ListInfoBar.Title = Strings.GameListUnavailable;
            ListInfoBar.Message = list.FromCache ? Strings.GameListFromCache : Strings.GameListFallback;
            ListInfoBar.IsOpen = true;
        }

        ShowLoading(Strings.MatchingLibrary, 0);
        LoadingDetail.Text = Strings.KnownGames(list.Games.Count.ToString("N0", CultureInfo.CurrentCulture));
        var progress = new Progress<double>(value => LoadingProgress.Value = value);

        List<GameEntry> owned;

        try
        {
            owned = await Task.Run(() => ScanOwnership(session, list.Games, progress, token), token);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        // Before showing: section (Completed / In Progress / Not Started), medal and sets per game.
        await LoadCompletionAsync(session, owned, token);

        if (token.IsCancellationRequested)
        {
            return;
        }

        _games.Clear();
        _gamesById.Clear();

        foreach (var game in owned)
        {
            _games.Add(game);
            _gamesById[game.Id] = game;
        }

        _isLoading = false;
        RefreshButton.IsEnabled = true;
        ApplyFilter();
    }

    /// <summary>Medals and achievement sets for the tiles - without starting a game, from Steam's local files.</summary>
    private static async Task LoadCompletionAsync(SteamSession session, List<GameEntry> games, CancellationToken token)
    {
        Dictionary<uint, CompletionInfo> completion;

        try
        {
            var ids = games.Select(g => g.Id).ToHashSet();
            completion = await Task.Run(() => ReadCompletion(session, ids, token), token);
        }
        catch (Exception e) when (e is OperationCanceledException or IOException or UnauthorizedAccessException or System.Text.Json.JsonException)
        {
            return;
        }

        foreach (var game in games)
        {
            if (completion.TryGetValue(game.Id, out var info))
            {
                game.SetCompletion(info);
            }
        }
    }

    private static Dictionary<uint, CompletionInfo> ReadCompletion(SteamSession session, HashSet<uint> ids, CancellationToken token)
    {
        ulong steamId;
        string language;

        lock (session.Gate)
        {
            if (session.IsDisposed)
            {
                throw new OperationCanceledException();
            }

            steamId = session.Client.SteamUser.GetSteamId();
            language = session.Client.SteamApps008.GetCurrentGameLanguage() ?? "english";
        }

        var account = (uint)(steamId & 0xFFFFFFFF);
        var steamPath = API.Steam.GetInstallPath() ?? "";
        var cachePath = AchievementCache.PathFor(steamPath, account);
        var cache = File.Exists(cachePath) ? AchievementCache.Read(cachePath).GroupBy(e => e.AppId).ToDictionary(g => g.Key, g => g.First()) : [];
        var history = ListState.Load(account).CompletedSets;
        var withSets = SetIndex.GamesWithSetData(steamPath);
        var result = new Dictionary<uint, CompletionInfo>();

        foreach (var id in ids)
        {
            token.ThrowIfCancellationRequested();
            var hasEntry = cache.TryGetValue(id, out var entry);
            SetSummary? sets = null;
            var useSetTotals = false;

            if (withSets.Contains(id) && LocalGameData.ReadStats(steamPath, account, id) is { } stats)
            {
                history.TryGetValue(id, out var before);
                (sets, useSetTotals) = SetEvaluator.ForGame(id, language, stats, hasEntry ? entry.Total : 0, dlc =>
                {
                    lock (session.Gate)
                    {
                        return !session.IsDisposed && session.Client.SteamApps008.IsSubscribedApp(dlc);
                    }
                }, before);
            }

            var complete = hasEntry && SetEvaluator.IsComplete(entry, sets, useSetTotals);
            var medal = complete ? MedalKind.Game : sets?.Medal is MedalKind.Set or MedalKind.SetDouble or MedalKind.DlcSet ? sets.Medal : MedalKind.None;
            var shownSets = sets is { HasSets: true } ? sets : null;
            var state = complete ? ProgressState.Completed
                : hasEntry && entry.Unlocked > 0 ? ProgressState.InProgress
                : ProgressState.NotStarted;

            var (unlocked, total) = sets is not null && useSetTotals ? (sets.Unlocked, sets.Total) : hasEntry ? (entry.Unlocked, entry.Total) : (0, 0);
            var ratio = complete ? 1.0 : total > 0 ? Math.Min(1.0, (double)unlocked / total) : 0.0;

            // Not started games from the cache as well - for "0/57" on the tile and in the list.
            if (hasEntry || medal != MedalKind.None || shownSets is not null)
            {
                result[id] = new CompletionInfo(state, ratio, unlocked, total, medal, shownSets, hasEntry ? entry.Vetted : null);
            }
        }

        return result;
    }

    /// <summary>Like AddGame in the original: only games owned by the account, with name and cover.</summary>
    private static List<GameEntry> ScanOwnership(SteamSession session, IReadOnlyList<(uint Id, string Type)> games, IProgress<double> progress, CancellationToken token)
    {
        var result = new List<GameEntry>();
        var seen = new HashSet<uint>();
        string language;

        lock (session.Gate)
        {
            language = session.Client.SteamApps008.GetCurrentGameLanguage() ?? "english";
        }

        for (var i = 0; i < games.Count; i++)
        {
            if (i % 512 == 0)
            {
                token.ThrowIfCancellationRequested();
                progress.Report((double)i / games.Count);
            }

            var (id, type) = games[i];

            if (!seen.Add(id))
            {
                continue;
            }

            lock (session.Gate)
            {
                if (session.IsDisposed)
                {
                    throw new OperationCanceledException();
                }

                if (!session.Client.SteamApps008.IsSubscribedApp(id))
                {
                    continue;
                }

                var name = session.Client.SteamApps001.GetAppData(id, "name");
                var image = SteamImages.Header(session.Client, id, language);
                result.Add(new GameEntry(id, type, name, image));
            }
        }

        progress.Report(1);
        return result;
    }

    private void OnAppDataChanged(API.Types.AppDataChanged param)
    {
        // Runs in the callback timer, so the Steam lock is already held.
        if (!param.Result || _session is null || !_gamesById.TryGetValue(param.Id, out var game))
        {
            return;
        }

        game.Name = _session.Client.SteamApps001.GetAppData(game.Id, "name") ?? game.Name;
        var language = _session.Client.SteamApps008.GetCurrentGameLanguage() ?? "english";
        var image = SteamImages.Header(_session.Client, game.Id, language);

        if (image != game.ImageUrl)
        {
            game.ImageUrl = image;
            game.ReleaseImage();
            game.EnsureImage();
        }
    }

    // ---------------------------------------------------------------- Filter & view

    /// <summary>Collapse or expand a section (button in the heading); the state is kept across restarts.</summary>
    private void OnSectionToggleClick(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is not GameGroup group)
        {
            return;
        }

        if (!_collapsed.Remove(group.State))
        {
            _collapsed.Add(group.State);
        }

        AppSettings.Write("collapsedSections", new JsonArray([.. _collapsed.Order().Select(state => JsonValue.Create(state.ToString()))]));
        ApplyFilter();

        // The new ItemsSource starts at the top again - jump back to the section heading.
        if (_groups.FirstOrDefault(g => g.State == group.State) is { } shown && _groups.IndexOf(shown) > 0)
        {
            GamesView.UpdateLayout();
            GamesView.MakeVisible(new SemanticZoomLocation { Item = shown });
        }
    }

    private static HashSet<ProgressState> ReadCollapsedSections()
    {
        var result = new HashSet<ProgressState>();

        foreach (var name in AppSettings.ReadStrings("collapsedSections"))
        {
            if (Enum.TryParse<ProgressState>(name, out var state) && Enum.IsDefined(state))
            {
                result.Add(state);
            }
        }

        return result;
    }

    private void ApplyFilter()
    {
        if (_session is null || _isLoading)
        {
            return;
        }

        var wantNormals = FilterGamesItem.IsChecked;
        var wantDemos = FilterDemosItem.IsChecked;
        var wantMods = FilterModsItem.IsChecked;
        var wantJunk = FilterJunkItem.IsChecked;
        var states = new List<ProgressState>();

        foreach (var (item, state) in new[] { (FilterCompletedItem, ProgressState.Completed), (FilterInProgressItem, ProgressState.InProgress), (FilterNotStartedItem, ProgressState.NotStarted) })
        {
            if (item.IsChecked)
            {
                states.Add(state);
            }
        }

        var search = SearchBox.Text.Trim();

        IEnumerable<GameEntry> query = _games.Where(game => states.Contains(game.Progress) && game.Type switch
        {
            "normal" => wantNormals,
            "demo" => wantDemos,
            "mod" => wantMods,
            "junk" => wantJunk,
            _ => true,
        });

        if (search.Length > 0)
        {
            query = query.Where(game =>
                game.Name.Contains(search, StringComparison.CurrentCultureIgnoreCase) ||
                game.Id.ToString(CultureInfo.InvariantCulture) == search);
        }

        _baseGames = query.ToList();

        // Column filters only exist in the list view - that is where the column header shows them.
        var filtered = SortGames(IsListMode ? ApplyColumnFilters(_baseGames, null) : _baseGames, _sortKey, _sortDescending).ToList();

        if (IsListMode)
        {
            // List view: one row per game, the category has its own column.
            _groups = [];
            GamesView.ItemsSource = null;
            GamesList.ItemsSource = filtered;
        }
        else if (!_grouped)
        {
            // Without sections: all tiles in one area.
            _groups = [];
            GamesList.ItemsSource = null;
            GamesView.ItemsSource = filtered;
        }
        else
        {
            // Three sections like in Steam's achievement overview: Completed, In Progress, Not Started - each collapsible.
            _groups = new[] { ProgressState.Completed, ProgressState.InProgress, ProgressState.NotStarted }
                .Select(state => (State: state, Games: filtered.Where(g => g.Progress == state).ToList()))
                .Where(section => section.Games.Count > 0)
                .Select(section => new GameGroup(section.State, section.Games, _collapsed.Contains(section.State)))
                .ToList();
            GamesList.ItemsSource = null;
            GamesView.ItemsSource = new CollectionViewSource { IsSourceGrouped = true, Source = _groups }.View;
        }

        UpdateSortMenu();
        TileOptionsButton.Visibility = IsListMode ? Visibility.Collapsed : Visibility.Visible;
        ColumnsChooserButton.Visibility = IsListMode ? Visibility.Visible : Visibility.Collapsed;

        if (IsListMode)
        {
            ColumnHeaders.Build(ListHeader, this);
        }

        SummaryText.Text = Strings.LibrarySummary(Number(_games.Count))
            + (filtered.Count == _games.Count ? "" : "  ·  " + Strings.LibraryShown(Number(filtered.Count)))
            + (IsListMode && _columnFilters.Count > 0 ? "  ·  " + Strings.ColumnFiltersActive : "");

        if (filtered.Count > 0)
        {
            ShowPanel(IsListMode ? ListPanel : GamesView);
            return;
        }

        if (_games.Count == 0)
        {
            EmptyTitle.Text = Strings.NoGamesWithAchievements;
            EmptyMessage.Text = Strings.NoGamesWithAchievementsHelp;
        }
        else
        {
            EmptyTitle.Text = Strings.NoGamesFound;
            EmptyMessage.Text = Strings.AdjustSearchOrFilter;
        }

        ShowPanel(EmptyPanel);
    }

    private static string Number(int count) => count.ToString("N0", CultureInfo.CurrentCulture);

    private void ShowLoading(string text, double? progress)
    {
        LoadingText.Text = text;
        LoadingDetail.Text = "";
        LoadingProgress.Visibility = progress is null ? Visibility.Collapsed : Visibility.Visible;
        LoadingProgress.Value = progress ?? 0;
        ShowPanel(LoadingPanel);
    }

    private void ShowError(string title, string message, bool canRetry)
    {
        ErrorTitle.Text = title;
        ErrorMessage.Text = message;
        RetryButton.Visibility = canRetry ? Visibility.Visible : Visibility.Collapsed;
        SummaryText.Text = " ";
        ShowPanel(ErrorPanel);
    }

    private void ShowPanel(UIElement panel)
    {
        GamesView.Visibility = panel == GamesView ? Visibility.Visible : Visibility.Collapsed;
        ListPanel.Visibility = panel == ListPanel ? Visibility.Visible : Visibility.Collapsed;
        LoadingPanel.Visibility = panel == LoadingPanel ? Visibility.Visible : Visibility.Collapsed;
        EmptyPanel.Visibility = panel == EmptyPanel ? Visibility.Visible : Visibility.Collapsed;
        ErrorPanel.Visibility = panel == ErrorPanel ? Visibility.Visible : Visibility.Collapsed;
    }

    public void Notify(string message, InfoBarSeverity severity)
    {
        NotifyBar.Severity = severity;
        NotifyBar.Message = message;
        NotifyBar.IsOpen = true;
        _notifyTimer.Stop();
        _notifyTimer.Start();
    }

    // ---------------------------------------------------------------- Tiles

    private void OnGamesViewSizeChanged(object sender, SizeChangedEventArgs e) => UpdateCardSize();

    /// <summary>Size the tiles so the rows fill the window width.</summary>
    private void UpdateCardSize()
    {
        if (GamesView.ItemsPanelRoot is not ItemsWrapGrid panel)
        {
            return;
        }

        var available = GamesView.ActualWidth - GamesView.Padding.Left - GamesView.Padding.Right - 2;

        if (available <= 0)
        {
            return;
        }

        var columns = Math.Max(1, (int)(available / (CardMinWidth + CardMargin)));
        var width = Math.Floor(available / columns);
        panel.ItemWidth = width;
        panel.ItemHeight = Math.Round((width - CardMargin) * 215 / 460) + TileOptions.CaptionHeight + CardMargin;
    }

    private void OnGameContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
    {
        if (args.Item is not GameEntry game)
        {
            return;
        }

        if (args.InRecycleQueue)
        {
            game.ReleaseImage();
        }
        else
        {
            game.EnsureImage();

            if (args.ItemIndex == 0)
            {
                UpdateCardSize();
            }
        }
    }

    // ---------------------------------------------------------------- List view

    private bool IsListMode => ViewSelector.SelectedItem == ListModeItem;

    /// <summary>Games after search and the header filters, before the column filters.</summary>
    private List<GameEntry> _baseGames = [];

    (string Key, bool Descending) IColumnHost.Sort => (_sortKey, _sortDescending);

    void IColumnHost.SortBy(string key, bool? descending)
        => SetSort(key, descending ?? (key == _sortKey && !_sortDescending));

    bool IColumnHost.IsFiltered(string key) => _columnFilters.ContainsKey(key);

    /// <summary>Like in Excel: the values under all other filters, in the order of the column.</summary>
    IReadOnlyList<FilterOption> IColumnHost.FilterOptions(string key)
    {
        _columnFilters.TryGetValue(key, out var chosen);
        var seen = new HashSet<string>();
        var options = new List<FilterOption>();

        foreach (var game in SortGames(ApplyColumnFilters(_baseGames, key), key, false))
        {
            var value = ColumnValue(key, game);

            if (seen.Add(value))
            {
                options.Add(new FilterOption(value, ColumnLabel(key, game), chosen is null || chosen.Contains(value)));
            }
        }

        return options;
    }

    void IColumnHost.SetFilter(string key, IReadOnlySet<string>? values)
    {
        if (values is null)
        {
            _columnFilters.Remove(key);
        }
        else
        {
            _columnFilters[key] = values;
        }

        ApplyFilter();
    }

    private void SetSort(string key, bool descending)
    {
        _sortKey = key;
        _sortDescending = descending;
        AppSettings.Write("librarySort", key + (descending ? ":desc" : ""));
        ApplyFilter();
    }

    private IEnumerable<RadioMenuFlyoutItem> SortItems() =>
        [SortNameItem, SortIdItem, SortProgressItem, SortSetsItem, SortCategoryItem, SortVisibilityItem];

    private void UpdateSortMenu()
    {
        foreach (var item in SortItems())
        {
            item.IsChecked = item.Tag as string == _sortKey;
        }

        SortAscendingItem.IsChecked = !_sortDescending;
        SortDescendingItem.IsChecked = _sortDescending;
        SortButtonText.Text = SortItems().FirstOrDefault(item => item.IsChecked)?.Text ?? Strings.SortName;
        SortDirectionGlyph.Glyph = _sortDescending ? "\uE70D" : "\uE70E";
    }

    private void OnColumnsFlyoutOpening(object sender, object e) => ColumnsFlyout.Content = new ColumnChooserView(ColumnLayouts.Library);

    /// <summary>Turn the sections Completed / In Progress / Not Started on or off ("Categories" menu).</summary>
    private void OnGroupingChanged(object sender, RoutedEventArgs e)
    {
        _grouped = GroupSectionsItem.IsChecked;
        AppSettings.Write("libraryGrouped", _grouped ? "true" : "false");
        ApplyFilter();
    }

    // ---------------------------------------------------------------- Tile options

    private IEnumerable<CheckBox> TileBoxes() => [TileNameBox, TileAppIdBox, TileAchievementsBox, TileSetsBox, TileMedalBox, TileVisibilityBox];

    private void OnTileOptionsOpening(object sender, object e)
    {
        foreach (var box in TileBoxes())
        {
            box.IsChecked = TileOptions.Shows((string)box.Tag);
        }
    }

    private void OnTileOptionChanged(object sender, RoutedEventArgs e)
    {
        TileOptions.Apply(TileBoxes().Where(b => b.IsChecked != true).Select(b => (string)b.Tag));
        RefreshTiles();
    }

    private void OnTileOptionsResetClick(object sender, RoutedEventArgs e)
    {
        TileOptions.Reset();
        OnTileOptionsOpening(sender, e);
        RefreshTiles();
    }

    private void RefreshTiles()
    {
        foreach (var game in _games)
        {
            game.RefreshTile();
        }

        UpdateCardSize();
    }

    private IEnumerable<GameEntry> ApplyColumnFilters(IEnumerable<GameEntry> games, string? except)
    {
        foreach (var (key, values) in _columnFilters)
        {
            if (key != except && ColumnLayouts.Library.IsVisible(key))
            {
                games = games.Where(g => values.Contains(ColumnValue(key, g)));
            }
        }

        return games;
    }

    /// <summary>Value of a cell for the column filter.</summary>
    private static string ColumnValue(string key, GameEntry game) => key switch
    {
        ColumnLayouts.AppId => game.AppIdText,
        ColumnLayouts.Visibility => game.VisibilityText,
        ColumnLayouts.Achievements => game.HasAchievementCount ? game.PercentText : "—",
        ColumnLayouts.Sets => game.SetsCountText,
        ColumnLayouts.Category => game.CategoryText,
        _ => game.Name,
    };

    private static string ColumnLabel(string key, GameEntry game)
        => key == ColumnLayouts.Visibility ? game.VisibilityText + "  " + game.VisibilityTooltip : ColumnValue(key, game);

    /// <summary>Sort by a column; games without a value (no achievement count, no sets) are at the end in both directions.</summary>
    private static IEnumerable<GameEntry> SortGames(IEnumerable<GameEntry> games, string key, bool descending)
    {
        var byName = StringComparer.CurrentCultureIgnoreCase;
        IOrderedEnumerable<GameEntry> ordered = key switch
        {
            ColumnLayouts.AppId => games.OrderBy(g => g.Id, descending),
            ColumnLayouts.Visibility => games.OrderBy(g => g.VisibilityRank, descending),
            ColumnLayouts.Achievements => games.OrderBy(g => g.HasAchievementCount ? 0 : 1).ThenBy(g => g.ProgressRatio, descending).ThenBy(g => g.TotalAchievements, descending),
            ColumnLayouts.Sets => games.OrderBy(g => g.HasSets ? 0 : 1).ThenBy(g => g.SetsRatio, descending).ThenBy(g => g.SetCount, descending),
            ColumnLayouts.Category => games.OrderBy(g => g.Progress switch { ProgressState.Completed => 0, ProgressState.InProgress => 1, _ => 2 }, descending),
            _ => descending ? games.OrderByDescending(g => g.Name, byName) : games.OrderBy(g => g.Name, byName),
        };

        return key is ColumnLayouts.Name or ColumnLayouts.AppId ? ordered : ordered.ThenBy(g => g.Name, byName);
    }

    private void OnViewChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
    {
        AppSettings.Write("libraryView", IsListMode ? "List" : "Tiles");
        ApplyFilter();
    }

    private void OnListContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
    {
        if (!args.InRecycleQueue && args.Item is GameEntry game)
        {
            game.EnsureIcon(ResolveIcon);
        }
    }

    /// <summary>Icon from Steam's library cache, otherwise the address on the CDN (file name from Steam's app data).</summary>
    private (string? Path, string? Url) ResolveIcon(uint appId)
    {
        _steamPath ??= API.Steam.GetInstallPath() ?? "";

        if (LocalGameData.FindIcon(_steamPath, appId) is { } path)
        {
            return (path, null);
        }

        string? hash = null;

        if (_session is { } session)
        {
            lock (session.Gate)
            {
                if (!session.IsDisposed)
                {
                    hash = session.Client.SteamApps001.GetAppData(appId, "icon");
                }
            }
        }

        return (null, LocalGameData.IconUrl(appId, hash));
    }

    private void OnGameClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is GameEntry game)
        {
            OpenGame(game.Id, game.Name);
        }
    }

    /// <summary>Like OnActivateGame in the original: open the game in a process of its own.</summary>
    private void OpenGame(uint appId, string? name)
    {
        if (GameLauncher.OpenGame(appId) is { } error)
        {
            Notify(Strings.GameWindowFailed(error), InfoBarSeverity.Error);
        }
        else
        {
            Notify(Strings.GameOpening(name ?? Strings.AppFallbackName(appId)), InfoBarSeverity.Informational);
        }
    }

    private static uint MenuAppId(object sender) => (sender as FrameworkElement)?.Tag is uint id ? id : 0;

    private void OnOpenGameMenuClick(object sender, RoutedEventArgs e)
    {
        var id = MenuAppId(sender);

        if (id != 0)
        {
            OpenGame(id, _gamesById.GetValueOrDefault(id)?.Name);
        }
    }

    private void OnStorePageMenuClick(object sender, RoutedEventArgs e)
        => OpenUri("https://store.steampowered.com/app/" + MenuAppId(sender).ToString(CultureInfo.InvariantCulture));

    private void OnSteamLibraryMenuClick(object sender, RoutedEventArgs e)
        => OpenUri("steam://nav/games/details/" + MenuAppId(sender).ToString(CultureInfo.InvariantCulture));

    private void OnCopyIdMenuClick(object sender, RoutedEventArgs e)
    {
        var package = new DataPackage();
        package.SetText(MenuAppId(sender).ToString(CultureInfo.InvariantCulture));
        Clipboard.SetContent(package);
        Notify(Strings.AppIdCopied, InfoBarSeverity.Success);
    }

    private void OpenUri(string uri)
    {
        if (GameLauncher.OpenUri(uri) is { } error)
        {
            Notify(Strings.CouldNotOpen(error), InfoBarSeverity.Error);
        }
    }

    // ---------------------------------------------------------------- Area: library or achievement lists

    private bool IsListsPage => PageSelector.SelectedItem == ListsPageItem && _listsPage is not null;

    /// <summary>The lists replace the header and content of the library; they are created on the first switch with the library's Steam session.</summary>
    private void OnPageChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
    {
        var lists = PageSelector.SelectedItem == ListsPageItem && _session is not null;

        if (lists && _listsPage is null)
        {
            _listsPage = new ListsPage(_session!);
            ListsHost.Children.Add(_listsPage);
        }

        ListsHost.Visibility = lists ? Visibility.Visible : Visibility.Collapsed;
        LibraryHeader.Visibility = lists ? Visibility.Collapsed : Visibility.Visible;
        LibraryContent.Visibility = lists ? Visibility.Collapsed : Visibility.Visible;
        ListInfoBar.Visibility = lists ? Visibility.Collapsed : Visibility.Visible;
        SearchBox.Visibility = lists ? Visibility.Collapsed : Visibility.Visible;
    }

    // ---------------------------------------------------------------- Header

    private void OnSearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            ApplyFilter();
        }
    }

    private void OnFilterChanged(object sender, RoutedEventArgs e) => ApplyFilter();

    /// <summary>Sorting from the menu: column and direction.</summary>
    private void OnSortChanged(object sender, RoutedEventArgs e)
    {
        var key = SortItems().FirstOrDefault(item => item.IsChecked)?.Tag as string ?? ColumnLayouts.Name;
        SetSort(key, SortDescendingItem.IsChecked);
    }

    private async void OnRefreshClick(object sender, RoutedEventArgs e) => await LoadGamesAsync();

    private async void OnRetryClick(object sender, RoutedEventArgs e) => await ConnectAndLoadAsync();

    private void OnSearchAccelerator(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        args.Handled = true;

        if (IsListsPage)
        {
            _listsPage!.FocusSearch();
            return;
        }

        SearchBox.Focus(FocusState.Keyboard);
    }

    private async void OnRefreshAccelerator(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        args.Handled = true;

        if (IsListsPage)
        {
            await _listsPage!.RefreshAsync();
        }
        else if (RefreshButton.IsEnabled)
        {
            await LoadGamesAsync();
        }
    }

    private void OnOpenByIdFlyoutOpened(object sender, object e)
    {
        AppIdError.Visibility = Visibility.Collapsed;
        AppIdBox.SelectAll();
        AppIdBox.Focus(FocusState.Programmatic);
    }

    private void OnAppIdKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            e.Handled = true;
            OpenById();
        }
    }

    private void OnOpenByIdClick(object sender, RoutedEventArgs e) => OpenById();

    /// <summary>Successor of OnAddGame: checks ownership and opens the game directly.</summary>
    private void OpenById()
    {
        if (_session is null)
        {
            return;
        }

        if (!uint.TryParse(AppIdBox.Text.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var id) || id == 0)
        {
            ShowAppIdError(Strings.AppIdInvalid);
            return;
        }

        bool owns;
        string? name;

        lock (_session.Gate)
        {
            owns = _session.Client.SteamApps008.IsSubscribedApp(id);
            name = owns ? _session.Client.SteamApps001.GetAppData(id, "name") : null;
        }

        if (!owns)
        {
            ShowAppIdError(Strings.AppIdNotOwned);
            return;
        }

        OpenByIdFlyout.Hide();
        AppIdBox.Text = "";
        OpenGame(id, name);
    }

    private void ShowAppIdError(string message)
    {
        AppIdError.Text = message;
        AppIdError.Visibility = Visibility.Visible;
    }

    // ---------------------------------------------------------------- Language

    private IReadOnlyList<LanguageOption>? _languages;

    private void OnLanguageFlyoutOpened(object sender, object e)
    {
        _languages ??= AppLanguage.Options();
        LanguageSearchBox.Text = "";
        LanguageList.ItemsSource = _languages;
        LanguageList.SelectedItem = _languages.FirstOrDefault(o => o.Locale == AppLanguage.Selected);

        if (LanguageList.SelectedItem is not null)
        {
            LanguageList.ScrollIntoView(LanguageList.SelectedItem, ScrollIntoViewAlignment.Leading);
        }

        LanguageSearchBox.Focus(FocusState.Programmatic);
    }

    private void OnLanguageSearchChanged(object sender, TextChangedEventArgs e)
    {
        if (_languages is null)
        {
            return;
        }

        var search = LanguageSearchBox.Text.Trim();
        LanguageList.ItemsSource = search.Length == 0
            ? _languages
            : _languages.Where(o => o.Title.Contains(search, StringComparison.CurrentCultureIgnoreCase) ||
                                    o.Subtitle.Contains(search, StringComparison.CurrentCultureIgnoreCase)).ToList();
    }

    /// <summary>Save the language and restart the app so all texts and formats switch.</summary>
    private void OnLanguageClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is not LanguageOption option)
        {
            return;
        }

        LanguageFlyout.Hide();

        if (option.Locale == AppLanguage.Selected)
        {
            return;
        }

        AppLanguage.Save(option.Locale);

        try
        {
            Process.Start(new ProcessStartInfo(Environment.ProcessPath!) { UseShellExecute = false, WorkingDirectory = AppContext.BaseDirectory });
            Close();
        }
        catch (Exception ex)
        {
            Notify(Strings.CouldNotOpen(ex.Message), InfoBarSeverity.Error);
        }
    }
}
