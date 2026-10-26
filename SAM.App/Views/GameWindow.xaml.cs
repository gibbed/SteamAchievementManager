using System.Diagnostics;
using System.Globalization;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using SAM.App.Game;
using SAM.App.Helpers;
using SAM.App.Localization;
using SAM.App.Schema;
using SAM.Game;
using SAM.Game.Stats;
using SAM.App.Steam;

namespace SAM.App.Views;

/// <summary>Achievements and statistics of a game - the WinUI 3 version of SAM.Game.</summary>
public sealed partial class GameWindow : Window, INotifier
{
    private readonly uint _appId;
    private readonly WindowChrome _chrome;
    private readonly DispatcherQueueTimer _notifyTimer;
    private readonly DispatcherQueueTimer _statsTimeout;
    private readonly List<AchievementItem> _achievements = [];
    private readonly List<StatItem> _stats = [];

    private SteamSession? _session;
    private List<AchievementItem> _visibleAchievements = [];
    private List<AchievementSet> _sets = [];
    private HashSet<uint> _ownedDlcs = [];
    private List<AchievementGroup> _groups = [];
    private readonly HashSet<int> _collapsedSets = [];
    private string _gameName;
    private bool _awaitingStats;
    private bool _isReady;
    private bool _isBusy;
    private bool _dialogOpen;
    private bool _allowClose;
    private readonly bool _demo;

    public GameWindow(uint appId, bool demo = false)
    {
        _appId = appId;
        _demo = demo;
        _gameName = Strings.AppFallbackName(appId.ToString(CultureInfo.InvariantCulture));

        InitializeComponent();

        Root.FlowDirection = AppLanguage.FlowDirection;
        _chrome = WindowChrome.Apply(this, Root, AppTitleBar, RightInsetColumn);
        _chrome.CenterOnScreen(1180, 920, 940, 660);

        GameTitle.Text = _gameName;
        GameSubtitle.Text = Strings.AppIdValue(appId.ToString(CultureInfo.InvariantCulture));

        _notifyTimer = DispatcherQueue.CreateTimer();
        _notifyTimer.Interval = TimeSpan.FromSeconds(4);
        _notifyTimer.IsRepeating = false;
        _notifyTimer.Tick += (_, _) => NotifyBar.IsOpen = false;

        _statsTimeout = DispatcherQueue.CreateTimer();
        _statsTimeout.Interval = TimeSpan.FromSeconds(30);
        _statsTimeout.IsRepeating = false;
        _statsTimeout.Tick += (_, _) =>
        {
            if (_awaitingStats)
            {
                _awaitingStats = false;
                ShowError(Strings.ErrorTimeoutTitle, Strings.ErrorTimeout);
            }
        };

        UpdateCommands();
        Root.Loaded += (_, _) => Connect();
        AppWindow.Closing += OnWindowClosing;
        Closed += (_, _) => _session?.Dispose();
    }

    private int PendingCount => _achievements.Count(a => a.IsModified) + _stats.Count(s => s.IsModified);

    private bool IsStatsTab => TabBar.SelectedItem == StatsTab;

    // ---------------------------------------------------------------- Steam

    private void Connect()
    {
        _isReady = false;
        ShowLoading(Strings.ConnectingToSteam);

#if DEBUG
        if (_demo)
        {
            LoadDemo();
            return;
        }
#endif

        _session?.Dispose();
        _session = null;

        try
        {
            _session = SteamSession.Connect(_appId, DispatcherQueue);
        }
        catch (SteamConnectException e)
        {
            ShowError(e.Title, e.Message, e.CanRetry);
            return;
        }

        var statsReceived = _session.Client.CreateAndRegisterCallback<API.Callbacks.UserStatsReceived>();
        statsReceived.OnRun += OnUserStatsReceived;

        LoadGameInfo(_session);
        RequestStats();
    }

    private void LoadGameInfo(SteamSession session)
    {
        string? name;
        string? hero;

        lock (session.Gate)
        {
            name = session.Client.SteamApps001.GetAppData(_appId, "name");
            var language = session.Client.SteamApps008.GetCurrentGameLanguage() ?? "english";
            hero = SteamImages.Hero(session.Client, _appId, language);
        }

        if (!string.IsNullOrEmpty(name))
        {
            _gameName = name;
        }

        GameTitle.Text = _gameName;
        Title = _gameName + " – Steam Achievement Manager";
        TitleText.Text = "Steam Achievement Manager  ·  " + _gameName;
        _ = LoadHeroAsync(hero);
    }

    private async Task LoadHeroAsync(string? url)
    {
        var image = await ImageCache.LoadAsync(url, (int)Math.Clamp(Hero.ActualWidth * _chrome.Scale, 1200, 2560));

        if (image is not null)
        {
            HeroImage.Source = image;
            HeroImage.Opacity = 1;
        }
    }

    /// <summary>Like RefreshStats in the original: Steam delivers the values through the UserStatsReceived callback.</summary>
    private void RequestStats()
    {
        if (_session is not { } session)
        {
            return;
        }

        API.CallHandle handle;

        lock (session.Gate)
        {
            var steamId = session.Client.SteamUser.GetSteamId();
            handle = session.Client.SteamUserStats.RequestUserStats(steamId);
        }

        if (handle == API.CallHandle.Invalid)
        {
            ShowError(Strings.ErrorRequestTitle, Strings.ErrorRequest);
            return;
        }

        _isReady = false;
        _awaitingStats = true;
        _statsTimeout.Stop();
        _statsTimeout.Start();
        ShowLoading(Strings.LoadingStats);
    }

    private void OnUserStatsReceived(API.Types.UserStatsReceived param)
    {
        // Runs in the callback timer, so the Steam lock is already held.
        if (!_awaitingStats || _session is not { } session)
        {
            return;
        }

        _awaitingStats = false;
        _statsTimeout.Stop();

        if (param.Result != 1)
        {
            ShowError(Strings.ErrorRetrieveTitle, TranslateError(param.Result));
            return;
        }

        GameSchema? schema;

        try
        {
            schema = SchemaLoader.Load(_appId, session.Client.SteamApps008.GetCurrentGameLanguage() ?? "english");
        }
        catch (Exception e)
        {
            ShowError(Strings.ErrorSchemaReadTitle, e.Message);
            return;
        }

        if (schema is null)
        {
            ShowError(
                Strings.ErrorSchemaTitle,
                Strings.ErrorSchema("appcache\\stats\\UserGameStatsSchema_" + _appId.ToString(CultureInfo.InvariantCulture) + ".bin"));
            return;
        }

        LoadSets(schema, dlc => session.Client.SteamApps008.IsSubscribedApp(dlc));

        try
        {
            LoadAchievements(session, schema);
        }
        catch (Exception e)
        {
            ShowError(Strings.ErrorAchievementsTitle, e.ToString());
            return;
        }

        try
        {
            LoadStatistics(session, schema);
        }
        catch (Exception e)
        {
            ShowError(Strings.ErrorStatisticsTitle, e.ToString());
            return;
        }

        _isReady = true;
        EditStatsCheckBox.IsChecked = false;
        ApplyFilter();
        UpdateSummary();
    }

    /// <summary>Achievement sets from the schema; which DLCs the account owns decides whether their set counts.</summary>
    private void LoadSets(GameSchema schema, Func<uint, bool> ownsDlc)
    {
        _sets = schema.Sets;
        _ownedDlcs = _sets.Where(s => s.IsDlc && ownsDlc(s.DlcAppId)).Select(s => s.DlcAppId).ToHashSet();

        var options = new List<string> { Strings.SetFilterAll };
        options.AddRange(_sets.Select(s => s.Name ?? Strings.SetBaseGame));
        SetFilter.SelectionChanged -= OnSetFilterChanged;
        SetFilter.ItemsSource = options;
        SetFilter.SelectedIndex = 0;
        SetFilter.SelectionChanged += OnSetFilterChanged;
    }

    private bool IsCounted(AchievementItem achievement)
    {
        if (achievement.IsArchived)
        {
            return false;
        }

        var set = _sets.FirstOrDefault(s => s.Id == achievement.SetId);
        return set is null || !set.IsDlc || _ownedDlcs.Contains(set.DlcAppId);
    }

    private static string TranslateError(int id) => id switch
    {
        2 => Strings.ErrorGeneric,
        _ => Strings.ErrorCode(id.ToString(CultureInfo.InvariantCulture)),
    };

    /// <summary>Like GetAchievements in the original, but unfiltered: only the view is filtered.</summary>
    private void LoadAchievements(SteamSession session, GameSchema schema)
    {
        _achievements.Clear();
        var index = 0;

        foreach (var definition in schema.Achievements)
        {
            if (string.IsNullOrEmpty(definition.Id))
            {
                continue;
            }

            if (!session.Client.SteamUserStats.GetAchievementAndUnlockTime(definition.Id, out var isAchieved, out var unlockTime))
            {
                continue;
            }

            var item = new AchievementItem(_appId, index++, definition, isAchieved, unlockTime);
            item.Changed += _ => UpdateSummary();
            _achievements.Add(item);
        }
    }

    /// <summary>Like GetStatistics in the original.</summary>
    private void LoadStatistics(SteamSession session, GameSchema schema)
    {
        _stats.Clear();

        foreach (var definition in schema.Stats)
        {
            if (string.IsNullOrEmpty(definition.Id))
            {
                continue;
            }

            StatItem? item = null;

            if (definition is IntegerStatDefinition integer && session.Client.SteamUserStats.GetStatValue(integer.Id, out int intValue))
            {
                item = StatItem.FromInteger(integer, intValue);
            }
            else if (definition is FloatStatDefinition floating && session.Client.SteamUserStats.GetStatValue(floating.Id, out float floatValue))
            {
                item = StatItem.FromFloat(floating, floatValue);
            }

            if (item is not null)
            {
                item.Changed += _ => UpdateSummary();
                _stats.Add(item);
            }
        }
    }

    // ---------------------------------------------------------------- View

    private void ApplyFilter()
    {
        if (!_isReady)
        {
            return;
        }

        var search = SearchBox.Text.Trim();

        if (IsStatsTab)
        {
            var stats = search.Length == 0
                ? _stats
                : _stats.Where(s => s.DisplayName.Contains(search, StringComparison.CurrentCultureIgnoreCase) || s.Id.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
            StatsView.ItemsSource = stats;

            if (_stats.Count == 0)
            {
                ShowEmpty("\uE9D2", Strings.NoStatistics, Strings.NoStatisticsHelp);
            }
            else if (stats.Count == 0)
            {
                ShowEmpty("\uE721", Strings.NoStatisticsFound, Strings.AdjustSearch);
            }
            else
            {
                ShowPanel(StatsPanel);
            }

            return;
        }

        var state = StateFilter.SelectedIndex;
        int? setId = SetFilter.SelectedIndex > 0 && SetFilter.SelectedIndex <= _sets.Count ? _sets[SetFilter.SelectedIndex - 1].Id : null;
        _visibleAchievements = _achievements
            .Where(a => setId is null || a.SetId == setId)
            .Where(a => state switch
            {
                1 => !a.IsAchieved,
                2 => a.IsAchieved,
                3 => a.IsModified,
                _ => true,
            })
            .Where(a => search.Length == 0 ||
                        a.Name.Contains(search, StringComparison.CurrentCultureIgnoreCase) ||
                        a.Description.Contains(search, StringComparison.CurrentCultureIgnoreCase) ||
                        a.Id.Contains(search, StringComparison.OrdinalIgnoreCase))
            .ToList();
        var matched = _visibleAchievements.Count;

        if (_sets.Count > 0)
        {
            // Grouped by sets, in the order of Steam's achievement page. Collapsed sets only show the
            // header; "Unlock all" etc. then only affect the visible achievements.
            _groups = _sets
                .Select(s => new AchievementGroup(
                    s,
                    !s.IsDlc || _ownedDlcs.Contains(s.DlcAppId),
                    _visibleAchievements.Where(a => a.SetId == s.Id).ToList(),
                    _achievements.Where(a => a.SetId == s.Id).ToList(),
                    _collapsedSets.Contains(s.Id)))
                .Where(g => g.MatchCount > 0)
                .ToList();
            _visibleAchievements = _groups.SelectMany(g => g).ToList();
            AchievementsView.ItemsSource = new CollectionViewSource { IsSourceGrouped = true, Source = _groups }.View;
        }
        else
        {
            _groups = [];
            AchievementsView.ItemsSource = _visibleAchievements;
        }

        if (_achievements.Count == 0)
        {
            ShowEmpty("\uE735", Strings.NoAchievements, Strings.NoAchievementsHelp);
        }
        else if (matched == 0)
        {
            ShowEmpty("\uE721", Strings.NoAchievementsFound, Strings.AdjustSearchOrSelection);
        }
        else
        {
            ShowPanel(AchievementsView);
        }

        UpdateCommands();
    }

    /// <summary>Collapse or expand a set (button in the set header).</summary>
    private void OnSetToggleClick(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is not AchievementGroup group)
        {
            return;
        }

        if (!_collapsedSets.Remove(group.Id))
        {
            _collapsedSets.Add(group.Id);
        }

        ApplyFilter();

        // The new ItemsSource starts at the top again - jump back to the set header.
        if (_groups.FirstOrDefault(g => g.Id == group.Id) is { } shown && _groups.IndexOf(shown) > 0)
        {
            AchievementsView.UpdateLayout();
            AchievementsView.MakeVisible(new SemanticZoomLocation { Item = shown });
        }
    }

    private void UpdateSummary()
    {
        // Steam's rules since achievement sets: archived achievements and DLCs not owned do not count.
        var counted = _achievements.Where(IsCounted).ToList();
        var total = counted.Count;
        var achieved = counted.Count(a => a.IsAchieved);
        var ratio = total == 0 ? 0 : (double)achieved / total;

        foreach (var group in _groups)
        {
            group.Refresh();
        }

        var complete = total > 0 && achieved >= total;
        ProgressMedal.Kind = complete ? MedalKind.Game : MedalKind.None;
        ProgressMedal.Visibility = complete ? Visibility.Visible : Visibility.Collapsed;
        UpdateSetBadges(complete);

        ProgressPanel.Visibility = _isReady && total > 0 ? Visibility.Visible : Visibility.Collapsed;
        ProgressText.Text = Strings.ProgressAchievements(Number(achieved), Number(total));
        ProgressPercent.Text = ratio.ToString("P0", CultureInfo.CurrentCulture);
        ProgressBarControl.Value = ratio;

        var pending = PendingCount;

        if (pending > 0)
        {
            ProgressDetail.Text = Strings.PendingChanges(Number(pending));
        }
        else if (_achievements.Where(a => a.IsAchieved && a.UnlockTime is not null).MaxBy(a => a.UnlockTime) is { } latest)
        {
            ProgressDetail.Text = Strings.LatestUnlock(latest.Name, latest.UnlockTime!.Value.ToString("d", CultureInfo.CurrentCulture));
        }
        else
        {
            ProgressDetail.Text = achieved == 0 ? Strings.NothingUnlocked : "";
        }

        if (_isReady)
        {
            GameSubtitle.Text = Strings.AppIdValue(_appId.ToString(CultureInfo.InvariantCulture)) + "  ·  " + Strings.StatisticsCount(Number(_stats.Count)) + SetsSummary();
            AchievementsTab.Text = Strings.Achievements + "  " + Number(_achievements.Count);
            StatsTab.Text = Strings.Statistics + "  " + Number(_stats.Count);
        }

        PendingBadgeText.Text = Number(pending);
        PendingBadge.Visibility = pending > 0 ? Visibility.Visible : Visibility.Collapsed;
        UpdateCommands();
    }

    /// <summary>"  ·  Sets 2/16" - completed out of the sets that count.</summary>
    /// <summary>Completed sets like in Steam's header - only as long as the game is not fully completed.</summary>
    private void UpdateSetBadges(bool complete)
    {
        var done = _sets.Where(s => !s.IsDlc || _ownedDlcs.Contains(s.DlcAppId))
                        .Select(s => (s.IsDlc, Members: _achievements.Where(a => a.SetId == s.Id && !a.IsArchived).ToList()))
                        .Where(s => s.Members.Count > 0 && s.Members.All(a => a.IsAchieved))
                        .ToList();
        var baseDone = done.Count(s => !s.IsDlc);
        var dlcDone = done.Count(s => s.IsDlc);

        SetBadgesPanel.Visibility = !complete && done.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        BaseSetsCount.Text = Number(baseDone);
        BaseSetsCount.Visibility = BaseSetsMedal.Visibility = baseDone > 0 ? Visibility.Visible : Visibility.Collapsed;
        BaseSetsMedal.Kind = baseDone >= 2 ? MedalKind.SetDouble : MedalKind.Set;
        BaseSetsMedal.Width = baseDone >= 2 ? 23 : 15;
        DlcSetsCount.Text = Number(dlcDone);
        DlcSetsCount.Visibility = DlcSetsMedal.Visibility = dlcDone > 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private string SetsSummary()
    {
        if (_sets.Count == 0)
        {
            return "";
        }

        var sets = _sets.Where(s => !s.IsDlc || _ownedDlcs.Contains(s.DlcAppId))
                        .Select(s => _achievements.Where(a => a.SetId == s.Id && !a.IsArchived).ToList())
                        .Where(members => members.Count > 0)
                        .ToList();
        var done = sets.Count(members => members.All(a => a.IsAchieved));
        return "  ·  " + Strings.SetsProgress(Number(done), Number(sets.Count));
    }

    private void OnSetFilterChanged(object sender, SelectionChangedEventArgs e) => ApplyFilter();

    private void UpdateCommands()
    {
        var ready = _isReady && !_isBusy;
        var achievements = ready && !IsStatsTab;

        SaveButton.IsEnabled = ready && PendingCount > 0;
        RefreshButton.IsEnabled = !_isBusy && _session is not null && !_awaitingStats;
        MoreButton.IsEnabled = ready;
        SearchBox.IsEnabled = ready;
        StateFilter.IsEnabled = achievements;
        StateFilter.Visibility = IsStatsTab ? Visibility.Collapsed : Visibility.Visible;
        SetFilter.IsEnabled = achievements;
        SetFilter.Visibility = IsStatsTab || _sets.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        UnlockAllItem.IsEnabled = achievements && _visibleAchievements.Count > 0;
        LockAllItem.IsEnabled = achievements && _visibleAchievements.Count > 0;
        InvertAllItem.IsEnabled = achievements && _visibleAchievements.Count > 0;
        DiscardItem.IsEnabled = ready && PendingCount > 0;
        ResetItem.IsEnabled = ready;
    }

    private static string Number(int count) => count.ToString("N0", CultureInfo.CurrentCulture);

    private void ShowLoading(string text)
    {
        LoadingText.Text = text;
        ShowPanel(LoadingPanel);
        UpdateSummary();
    }

    private void ShowError(string title, string message, bool canRetry = true)
    {
        _isReady = false;
        ErrorTitle.Text = title;
        ErrorMessage.Text = message;
        RetryButton.Visibility = canRetry ? Visibility.Visible : Visibility.Collapsed;
        ShowPanel(ErrorPanel);
        UpdateSummary();
    }

    private void ShowEmpty(string glyph, string title, string message)
    {
        EmptyGlyph.Glyph = glyph;
        EmptyTitle.Text = title;
        EmptyMessage.Text = message;
        ShowPanel(EmptyPanel);
    }

    private void ShowPanel(UIElement panel)
    {
        AchievementsView.Visibility = panel == AchievementsView ? Visibility.Visible : Visibility.Collapsed;
        StatsPanel.Visibility = panel == StatsPanel ? Visibility.Visible : Visibility.Collapsed;
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

    private async Task<ContentDialogResult> ShowDialogAsync(ContentDialog dialog)
    {
        // Only one ContentDialog may be open at a time.
        if (_dialogOpen)
        {
            return ContentDialogResult.None;
        }

        _dialogOpen = true;

        try
        {
            return await dialog.ShowAsync();
        }
        finally
        {
            _dialogOpen = false;
        }
    }

    // ---------------------------------------------------------------- Achievements

    private void OnAchievementContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
    {
        if (!args.InRecycleQueue && args.Item is AchievementItem item)
        {
            item.EnsureIcon();
        }
    }

    private void OnAchievementClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is not AchievementItem item || _isBusy)
        {
            return;
        }

        if (item.IsProtected)
        {
            Notify(Strings.AchievementProtectedNotice(item.Name), InfoBarSeverity.Warning);
            return;
        }

        item.IsAchieved = !item.IsAchieved;
    }

    private void OnUnlockAllClick(object sender, RoutedEventArgs e) => SetVisible(_ => true);

    private void OnLockAllClick(object sender, RoutedEventArgs e) => SetVisible(_ => false);

    private void OnInvertAllClick(object sender, RoutedEventArgs e) => SetVisible(a => !a.IsAchieved);

    private void SetVisible(Func<AchievementItem, bool> state)
    {
        var skipped = 0;

        foreach (var item in _visibleAchievements)
        {
            if (item.IsProtected)
            {
                skipped++;
                continue;
            }

            item.IsAchieved = state(item);
        }

        if (skipped > 0)
        {
            Notify(Strings.ProtectedSkipped(Number(skipped)), InfoBarSeverity.Informational);
        }
    }

    private void OnDiscardClick(object sender, RoutedEventArgs e) => DiscardChanges();

    private void DiscardChanges()
    {
        foreach (var item in _achievements)
        {
            item.Revert();
        }

        foreach (var stat in _stats)
        {
            stat.Revert();
        }

        UpdateSummary();
    }

    // ---------------------------------------------------------------- Statistics

    private void OnEditStatsChanged(object sender, RoutedEventArgs e)
    {
        var enabled = EditStatsCheckBox.IsChecked == true;

        foreach (var stat in _stats)
        {
            stat.IsEditable = enabled;
        }
    }

    private void OnStatValueLostFocus(object sender, RoutedEventArgs e) => ApplyStatText(sender as TextBox);

    private void OnStatValueKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (sender is not TextBox { DataContext: StatItem stat } box)
        {
            return;
        }

        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            e.Handled = true;
            ApplyStatText(box);
            box.SelectAll();
        }
        else if (e.Key == Windows.System.VirtualKey.Escape)
        {
            e.Handled = true;
            stat.ClearError();
            box.Text = stat.ValueText;
            box.SelectAll();
        }
    }

    private static void ApplyStatText(TextBox? box)
    {
        if (box is { DataContext: StatItem stat } && !stat.IsReadOnly && !stat.TryApply(box.Text))
        {
            box.Text = stat.ValueText;
        }
    }

    // ---------------------------------------------------------------- Save & reset

    private async void OnSaveClick(object sender, RoutedEventArgs e) => await SaveAsync();

    private async void OnSaveAccelerator(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        args.Handled = true;
        await SaveAsync();
    }

    /// <summary>Like OnStore in the original: set achievements, set statistics, StoreStats, reload.</summary>
    private async Task SaveAsync()
    {
        if (!_isReady || _isBusy || _session is null && !_demo)
        {
            return;
        }

        // Apply a statistic that is still being typed (Ctrl+S leaves the focus in the box).
        ApplyStatText(FocusManager.GetFocusedElement(Root.XamlRoot) as TextBox);

        var unlock = _achievements.Where(a => a.IsModified && a.IsAchieved).ToList();
        var relock = _achievements.Where(a => a.IsModified && !a.IsAchieved).ToList();
        var stats = _stats.Where(s => s.IsModified).ToList();

        if (unlock.Count + relock.Count + stats.Count == 0)
        {
            Notify(Strings.NothingToSave, InfoBarSeverity.Informational);
            return;
        }

        var dialog = Dialogs.Create(Root.XamlRoot, Strings.SaveDialogTitle, BuildSaveSummary(unlock, relock, stats), Strings.Save);

        if (await ShowDialogAsync(dialog) != ContentDialogResult.Primary)
        {
            return;
        }

        _isBusy = true;
        UpdateCommands();
        string? error = null;

        try
        {
            if (_session is { } session)
            {
                error = WriteToSteam(session, unlock.Concat(relock), stats);
            }
        }
        finally
        {
            _isBusy = false;
        }

        if (error is not null)
        {
            await ShowDialogAsync(Dialogs.Create(Root.XamlRoot, Strings.SaveFailedTitle, new TextBlock { Text = error, TextWrapping = TextWrapping.Wrap }, Strings.Ok, showClose: false));
            RequestStats();
            return;
        }

        foreach (var item in unlock.Concat(relock))
        {
            item.Commit();
        }

        foreach (var stat in stats)
        {
            stat.Commit();
        }

        UpdateSummary();

        if (!_demo)
        {
            RequestStats();
        }

        Notify(Strings.Saved(Number(unlock.Count + relock.Count), Number(stats.Count)), InfoBarSeverity.Success);
    }

    /// <summary>Set achievements and statistics and commit them with StoreStats; returns the error message or null.</summary>
    private static string? WriteToSteam(SteamSession session, IEnumerable<AchievementItem> achievements, List<StatItem> stats)
    {
        lock (session.Gate)
        {
            foreach (var item in achievements)
            {
                if (!session.Client.SteamUserStats.SetAchievement(item.Id, item.IsAchieved))
                {
                    return Strings.SaveAchievementFailed(item.Name);
                }
            }

            foreach (var stat in stats)
            {
                var ok = stat.IsFloat
                    ? session.Client.SteamUserStats.SetStatValue(stat.Id, stat.FloatValue)
                    : session.Client.SteamUserStats.SetStatValue(stat.Id, stat.IntValue);

                if (!ok)
                {
                    return Strings.SaveStatFailed(stat.DisplayName);
                }
            }

            return session.Client.SteamUserStats.StoreStats() ? null : Strings.SaveRejected;
        }
    }

    private static StackPanel BuildSaveSummary(List<AchievementItem> unlock, List<AchievementItem> relock, List<StatItem> stats)
    {
        var panel = new StackPanel { Spacing = 14, MinWidth = 380 };

        void Section(string glyph, string title, IEnumerable<string> names)
        {
            var list = names.ToList();

            if (list.Count == 0)
            {
                return;
            }

            var header = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
            header.Children.Add(new FontIcon { Glyph = glyph, FontSize = 14 });
            header.Children.Add(new TextBlock { Text = title, Style = (Style)Application.Current.Resources["BodyStrongTextBlockStyle"] });

            var shown = list.Take(6).ToList();
            var text = string.Join("\n", shown.Select(n => "·  " + n));

            if (list.Count > shown.Count)
            {
                text += "\n·  " + Strings.AndMore(Number(list.Count - shown.Count));
            }

            var section = new StackPanel { Spacing = 4 };
            section.Children.Add(header);
            section.Children.Add(new TextBlock
            {
                Text = text,
                Margin = new Thickness(22, 0, 0, 0),
                TextWrapping = TextWrapping.Wrap,
                Style = (Style)Application.Current.Resources["SecondaryTextStyle"],
            });
            panel.Children.Add(section);
        }

        Section("\uE785", Strings.SummaryUnlock(Number(unlock.Count)), unlock.Select(a => a.Name));
        Section("\uE72E", Strings.SummaryLock(Number(relock.Count)), relock.Select(a => a.Name));
        Section("\uE9D2", Strings.SummaryStats(Number(stats.Count)), stats.Select(s =>
            s.DisplayName + ": " + (s.IsFloat ? ((float)s.OriginalValue).ToString("G", CultureInfo.CurrentCulture) : ((int)s.OriginalValue).ToString(CultureInfo.CurrentCulture)) + " → " + s.ValueText));

        return panel;
    }

    private async void OnResetClick(object sender, RoutedEventArgs e)
    {
        if (!_isReady || _isBusy || _session is null && !_demo)
        {
            return;
        }

        // Replaces the original's three confirmation prompts with one dialog that needs an explicit confirmation.
        var panel = new StackPanel { Spacing = 12, MaxWidth = 440 };
        panel.Children.Add(new TextBlock
        {
            Text = Strings.ResetMessage(_gameName),
            TextWrapping = TextWrapping.Wrap,
        });
        var withAchievements = new CheckBox { Content = Strings.ResetAchievementsToo };
        var confirm = new CheckBox { Content = Strings.ResetConfirm };
        panel.Children.Add(withAchievements);
        panel.Children.Add(confirm);

        var dialog = Dialogs.Create(Root.XamlRoot, Strings.ResetTitle, panel, Strings.Reset, danger: true);
        dialog.IsPrimaryButtonEnabled = false;
        confirm.Checked += (_, _) => dialog.IsPrimaryButtonEnabled = true;
        confirm.Unchecked += (_, _) => dialog.IsPrimaryButtonEnabled = false;

        if (await ShowDialogAsync(dialog) != ContentDialogResult.Primary)
        {
            return;
        }

        var ok = true;
        var achievementsToo = withAchievements.IsChecked == true;

        if (_session is { } session)
        {
            lock (session.Gate)
            {
                ok = session.Client.SteamUserStats.ResetAllStats(achievementsToo);
            }
        }

        if (!ok)
        {
            await ShowDialogAsync(Dialogs.Create(Root.XamlRoot, Strings.ResetFailedTitle, new TextBlock { Text = Strings.ResetRejected }, Strings.Ok, showClose: false));
            return;
        }

        if (!_demo)
        {
            RequestStats();
        }

        Notify(achievementsToo ? Strings.ResetDoneAll : Strings.ResetDoneStats, InfoBarSeverity.Success);
    }

    // ---------------------------------------------------------------- Toolbar

    private void OnTabChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
    {
        SearchBox.PlaceholderText = IsStatsTab ? Strings.SearchStatistics : Strings.SearchAchievements;
        ApplyFilter();
        UpdateCommands();
    }

    private void OnSearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            ApplyFilter();
        }
    }

    private void OnStateFilterChanged(object sender, SelectionChangedEventArgs e) => ApplyFilter();

    private async void OnRefreshClick(object sender, RoutedEventArgs e) => await RefreshAsync();

    private async void OnRefreshAccelerator(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        args.Handled = true;

        if (RefreshButton.IsEnabled)
        {
            await RefreshAsync();
        }
    }

    private async Task RefreshAsync()
    {
        var pending = PendingCount;

        if (pending > 0)
        {
            var dialog = Dialogs.Create(
                Root.XamlRoot,
                Strings.DiscardReloadTitle,
                new TextBlock { Text = Strings.DiscardReloadMessage(Number(pending)), TextWrapping = TextWrapping.Wrap },
                Strings.DiscardReload);

            if (await ShowDialogAsync(dialog) != ContentDialogResult.Primary)
            {
                return;
            }
        }

        RequestStats();
    }

    private void OnRetryClick(object sender, RoutedEventArgs e)
    {
        if (_session is null)
        {
            Connect();
        }
        else
        {
            RequestStats();
        }
    }

    private void OnSearchAccelerator(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        args.Handled = true;
        SearchBox.Focus(FocusState.Keyboard);
    }

    private void OnStorePageClick(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo("https://store.steampowered.com/app/" + _appId.ToString(CultureInfo.InvariantCulture)) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            Notify(Strings.CouldNotOpen(ex.Message), InfoBarSeverity.Error);
        }
    }

    private async void OnWindowClosing(AppWindow sender, AppWindowClosingEventArgs args)
    {
        var pending = PendingCount;

        if (_allowClose || pending == 0)
        {
            return;
        }

        args.Cancel = true;

        var dialog = Dialogs.Create(
            Root.XamlRoot,
            Strings.UnsavedTitle,
            new TextBlock { Text = Strings.UnsavedCloseMessage(Number(pending)), TextWrapping = TextWrapping.Wrap },
            Strings.DiscardClose,
            danger: true);

        if (await ShowDialogAsync(dialog) == ContentDialogResult.Primary)
        {
            _allowClose = true;
            Close();
        }
    }

#if DEBUG
    /// <summary>UI test without Steam: real schema from disk, states derived from the ID.</summary>
    private void LoadDemo()
    {
        var path = Path.Combine(API.Steam.GetInstallPath() ?? "", "appcache", "stats", "UserGameStatsSchema_" + _appId.ToString(CultureInfo.InvariantCulture) + ".bin");
        var name = KeyValue.LoadAsBinary(path)?[_appId.ToString(CultureInfo.InvariantCulture)]["gamename"].AsString("");

        if (!string.IsNullOrEmpty(name))
        {
            _gameName = name;
        }

        GameTitle.Text = _gameName;
        Title = _gameName + " – Steam Achievement Manager (Demo)";
        // Debug build only: texts and achievement names in the UI language (German or English).
        var german = AppLanguage.Culture.TwoLetterISOLanguageName == "de";
        TitleText.Text = "Steam Achievement Manager  ·  " + _gameName + (german ? "  ·  Demo ohne Steam" : "  ·  Demo without Steam");
        _ = LoadHeroAsync(SteamImages.StoreAsset(_appId, "library_hero.jpg"));

        var schema = SchemaLoader.Load(_appId, german ? "german" : "english");

        if (schema is null)
        {
            ShowError(german ? "Kein Schema" : "No schema", path, canRetry: false);
            return;
        }

        _achievements.Clear();
        _stats.Clear();
        LoadSets(schema, _ => true);
        var index = 0;
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        foreach (var definition in schema.Achievements.Where(d => !string.IsNullOrEmpty(d.Id)))
        {
            var hash = (uint)definition.Id.GetHashCode(StringComparison.Ordinal);
            var achieved = hash % 5 < 2;
            var item = new AchievementItem(_appId, index++, definition, achieved, achieved ? (uint)(now - hash % 30_000_000) : 0);
            item.Changed += _ => UpdateSummary();
            _achievements.Add(item);
        }

        foreach (var definition in schema.Stats.Where(d => !string.IsNullOrEmpty(d.Id)))
        {
            var item = definition switch
            {
                IntegerStatDefinition i => StatItem.FromInteger(i, i.DefaultValue + (int)((uint)i.Id.GetHashCode(StringComparison.Ordinal) % 500)),
                FloatStatDefinition f => StatItem.FromFloat(f, f.DefaultValue + (uint)f.Id.GetHashCode(StringComparison.Ordinal) % 1000 / 7f),
                _ => null,
            };

            if (item is not null)
            {
                item.Changed += _ => UpdateSummary();
                _stats.Add(item);
            }
        }

        _isReady = true;
        ApplyFilter();
        UpdateSummary();
    }
#endif
}
