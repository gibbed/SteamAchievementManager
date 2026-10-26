using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using SAM.App.Columns;
using SAM.App.Helpers;
using SAM.App.Lists;
using SAM.App.Localization;
using SAM.App.Schema;
using SAM.App.Steam;
using Windows.ApplicationModel.DataTransfer;

namespace SAM.App.Views;

/// <summary>
/// Achievement lists following the scheme of the Steam collections "All" to "👀✖ Can't Finish",
/// with export as guide text. Only reads Steam's cache - no game is started and nothing is written to Steam.
/// </summary>
public sealed partial class ListsPage : UserControl, INotifier, IColumnHost
{
    private readonly SteamSession _session;
    private readonly DispatcherQueueTimer _notifyTimer;

    private uint _accountId;
    private string _steamPath = "";
    private string _cachePath = "";
    private ListState _state = new();
    private List<RawGame> _raw = [];
    private List<ListCategory> _categories = [];
    private DateTime _cacheTime;
    private bool _loading;
    private bool _dialogOpen;

    private SortField _sortField = SortField.Name;
    private bool _sortDescending;
    private readonly Dictionary<string, IReadOnlySet<string>> _columnFilters = [];
    private readonly HashSet<string> _collapsedGroups = [.. AppSettings.ReadStrings("listsCollapsedGroups")];
    private List<ListCategoryGroup> _groups = [];

    /// <summary>Selected category - stays even when its group is collapsed.</summary>
    private ListCategory? _current;
    private bool _showingCategories;
    private bool _periodUsesLastUnlock;
    private DateTimeOffset? _periodFrom;
    private DateTimeOffset? _periodTo;

    private enum SortField
    {
        Name,
        AppId,
        FirstUnlock,
        LastUnlock,
        Progress,
    }

    internal ListsPage(SteamSession session)
    {
        _session = session;
        InitializeComponent();

        Root.FlowDirection = AppLanguage.FlowDirection;
        SearchBox.FlowDirection = AppLanguage.FlowDirection;
        _notifyTimer = DispatcherQueue.CreateTimer();
        _notifyTimer.Interval = TimeSpan.FromSeconds(4);
        _notifyTimer.IsRepeating = false;
        _notifyTimer.Tick += (_, _) => NotifyBar.IsOpen = false;

        SortBox.ItemsSource = new[] { Strings.SortName, Strings.SortAppId, Strings.ColumnFirstUnlock, Strings.ColumnLastUnlock, Strings.ColumnProgress };
        SortBox.SelectedIndex = 0;
        PeriodField.ItemsSource = new[] { Strings.ColumnFirstUnlock, Strings.ColumnLastUnlock };

        // Otherwise the date picker follows the Windows language instead of the app language.
        foreach (var picker in new[] { PeriodFrom, PeriodTo })
        {
            picker.Language = CultureInfo.CurrentCulture.Name;
            picker.PlaceholderText = Strings.DatePlaceholder;
        }

        PeriodField.SelectedIndex = 0;
        UpdateSortDirection();
        UpdatePeriodButton();

        Root.Loaded += async (_, _) => await LoadAsync();
    }

    // ---------------------------------------------------------------- Loading

    private async Task LoadAsync()
    {
        if (_loading || _session.IsDisposed)
        {
            return;
        }

        _loading = true;
        SetCommandsEnabled(false);
        ShowPanel(LoadingPanel);
        LoadingText.Text = Strings.ListsLoading;

        try
        {
            ulong steamId;

            lock (_session.Gate)
            {
                steamId = _session.Client.SteamUser.GetSteamId();
            }

            _accountId = (uint)(steamId & 0xFFFFFFFF);
            _steamPath = API.Steam.GetInstallPath() ?? "";
            _cachePath = AchievementCache.PathFor(_steamPath, _accountId);

            if (!File.Exists(_cachePath))
            {
                ShowError(Strings.ListsCacheMissingTitle, Strings.ListsCacheMissing(_cachePath));
                return;
            }

            _state = ListState.Load(_accountId);
            _cacheTime = File.GetLastWriteTime(_cachePath);
            var source = new GameSource(_session, _state, _cachePath, _steamPath, _accountId);
            LoadingText.Text = Strings.ListsMatching;

            _raw = await Task.Run(() => ReadGames(source, only: null));
            Rebuild(updateHistory: true);
            ShowPanel(ContentGrid);
        }
        catch (OperationCanceledException)
        {
            // The library was closed, the Steam session is gone.
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or System.Text.Json.JsonException)
        {
            ShowError(Strings.ListsCacheMissingTitle, Strings.ListsReadError(e.Message));
        }
        finally
        {
            _loading = false;
            SetCommandsEnabled(true);
        }
    }

    /// <summary>Everything the background loading needs.</summary>
    private sealed record GameSource(SteamSession Session, ListState State, string CachePath, string SteamPath, uint AccountId);

    /// <summary>A loaded game; <see cref="Automatic"/> = in the lists even without being added by hand.</summary>
    /// <param name="Sets">Achievement sets, if the game has any (or archived achievements).</param>
    /// <param name="UseSetTotals">Schema matches Steam's achievement cache - then the numbers follow Steam's new rules.</param>
    private sealed record RawGame(CachedProgress Progress, string Name, UnlockTimes? Times, string? IconPath, string? IconUrl, bool Manual, bool Automatic,
                                  SetSummary? Sets = null, bool UseSetTotals = false);

    /// <summary>
    /// Games with achievements owned by the account, plus the ones added by hand - with names from Steam's app data,
    /// unlock times and icon from Steam's local files. <paramref name="only"/> limits it to these App IDs.
    /// </summary>
    private static List<RawGame> ReadGames(GameSource source, IReadOnlySet<uint>? only)
    {
        var state = source.State;
        var candidates = new List<(CachedProgress Progress, bool InCache)>();
        var inCache = new HashSet<uint>();
        var withSets = SetIndex.GamesWithSetData(source.SteamPath);
        string language;

        lock (source.Session.Gate)
        {
            language = source.Session.Client.SteamApps008.GetCurrentGameLanguage() ?? "english";
        }

        foreach (var entry in AchievementCache.Read(source.CachePath))
        {
            inCache.Add(entry.AppId);

            if (only is null || only.Contains(entry.AppId))
            {
                candidates.Add((entry, true));
            }
        }

        // Added by hand but not in Steam's cache: progress from the stats and schema files.
        foreach (var appId in state.ManualApps.Where(id => !inCache.Contains(id) && (only is null || only.Contains(id))))
        {
            var times = LocalGameData.ReadStats(source.SteamPath, source.AccountId, appId)?.Times;
            var total = LocalGameData.CountAchievements(appId);
            var unlocked = total > 0 ? Math.Min(times?.Count ?? 0, total) : times?.Count ?? 0;
            candidates.Add((new CachedProgress(appId, unlocked, total, total > 0 && unlocked >= total, null, 0), false));
        }

        var result = new List<RawGame>();

        foreach (var (entry, cached) in candidates)
        {
            var manual = state.ManualApps.Contains(entry.AppId);
            var relevant = entry.Total > 0 || state.CantFinish.ContainsKey(entry.AppId) || state.NotAnymoreManual.Contains(entry.AppId);

            if (!manual && !relevant)
            {
                continue;
            }

            var iconPath = LocalGameData.FindIcon(source.SteamPath, entry.AppId);
            string? name, iconHash = null;
            bool owned;

            lock (source.Session.Gate)
            {
                if (source.Session.IsDisposed)
                {
                    throw new OperationCanceledException();
                }

                owned = source.Session.Client.SteamApps008.IsSubscribedApp(entry.AppId);

                if (!owned && !manual)
                {
                    continue;
                }

                name = source.Session.Client.SteamApps001.GetAppData(entry.AppId, "name");

                if (iconPath is null)
                {
                    iconHash = source.Session.Client.SteamApps001.GetAppData(entry.AppId, "icon");
                }
            }

            var stats = LocalGameData.ReadStats(source.SteamPath, source.AccountId, entry.AppId);
            var (sets, useSetTotals) = withSets.Contains(entry.AppId) && stats is not null
                ? EvaluateSets(source, entry, stats, language)
                : (null, false);

            result.Add(new RawGame(
                entry,
                string.IsNullOrWhiteSpace(name) ? Strings.AppFallbackName(entry.AppId.ToString(CultureInfo.InvariantCulture)) : name.Trim(),
                stats?.Times,
                iconPath,
                LocalGameData.IconUrl(entry.AppId, iconHash),
                manual,
                cached && relevant && owned,
                sets,
                useSetTotals));
        }

        return result;
    }

    /// <summary>Rebuild the categories, e.g. after a mark. The selection is kept.</summary>
    private void Rebuild(bool updateHistory)
    {
        var selected = _current;
        var games = _raw.Select(r => (Game: CreateGame(r), Complete: IsComplete(r))).ToList();

        if (updateHistory &&
            ListBuilder.UpdateHistory(_state, games, DateTimeOffset.UtcNow.ToUnixTimeSeconds()) |
            ListBuilder.UpdateSetHistory(_state, games.Select(g => g.Game)))
        {
            SaveState();
        }

        foreach (var (game, complete) in games)
        {
            ListBuilder.Classify(_state, game, complete);
        }

        _categories = ListBuilder.Build(games.Select(g => g.Game).ToList());
        _current = _categories.FirstOrDefault(c => selected is not null && c.Scope == selected.Scope && c.Kind == selected.Kind) ?? _categories[0];
        ShowCategories();
        ShowGames();

        var withAchievements = _categories.Sum(c => c.Scope == ListScope.All && c.Kind is ListKind.All or ListKind.Untouched ? c.Games.Count : 0);
        SummaryText.Text = Strings.ListsSummary(_cacheTime.ToString("g", CultureInfo.CurrentCulture), withAchievements.ToString("N0", CultureInfo.CurrentCulture));
    }

    /// <summary>
    /// Evaluate sets under Steam's rules: unlocks from the bit field of the stats file, DLC ownership
    /// via Steam. An outdated schema (different achievement count than Steam's cache) only provides the sets, not the totals.
    /// </summary>
    private static (SetSummary? Sets, bool UseSetTotals) EvaluateSets(GameSource source, CachedProgress entry, LocalStats stats, string language)
    {
        source.State.CompletedSets.TryGetValue(entry.AppId, out var before);
        return SetEvaluator.ForGame(entry.AppId, language, stats, entry.Total, dlc =>
        {
            lock (source.Session.Gate)
            {
                return !source.Session.IsDisposed && source.Session.Client.SteamApps008.IsSubscribedApp(dlc);
            }
        }, before);
    }

    private static ListGame CreateGame(RawGame raw)
    {
        var useSets = raw.Sets is not null && raw.UseSetTotals;
        var unlocked = useSets ? raw.Sets!.Unlocked : raw.Progress.Unlocked;
        var total = useSets ? raw.Sets!.Total : raw.Progress.Total;
        return new ListGame(raw.Progress.AppId, raw.Name, unlocked, total, raw.Progress.Vetted)
        {
            Sets = raw.Sets is { HasSets: true } ? raw.Sets : null,
        IsManual = raw.Manual,
        IconPath = raw.IconPath,
        IconUrl = raw.IconUrl,
            FirstUnlock = raw.Times is { } first ? DateTimeOffset.FromUnixTimeSeconds(first.First) : null,
            LastUnlock = raw.Times is { } last ? DateTimeOffset.FromUnixTimeSeconds(last.Last) : null,
        };
    }

    /// <summary>Under Steam's new rules: archived achievements and DLCs not owned do not count.</summary>
    private static bool IsComplete(RawGame raw) => SetEvaluator.IsComplete(raw.Progress, raw.Sets, raw.UseSetTotals);

    private void SaveState()
    {
        try
        {
            _state.Save(_accountId);
        }
        catch (Exception e)
        {
            Notify(Strings.ListsSaveError(e.Message), InfoBarSeverity.Error);
        }
    }

    // ---------------------------------------------------------------- View

    private void OnCategoryChanged(object sender, SelectionChangedEventArgs e)
    {
        // When the list is rebuilt (collapsing), the selection briefly disappears - the selected category stays.
        if (_showingCategories || CategoryList.SelectedItem is not ListCategory category)
        {
            return;
        }

        _current = category;
        ShowGames();
    }

    /// <summary>
    /// Groups with their own heading: All games, Visible on profile, Profile Features Limited, Sets, Not in the
    /// collections - each collapsible (saved in settings.json, listsCollapsedGroups).
    /// </summary>
    private void ShowCategories()
    {
        _showingCategories = true;
        _groups = _categories.GroupBy(c => c.GroupKey)
            .Select(g => new ListCategoryGroup(g.Key, g.First().Group, g, _collapsedGroups.Contains(g.Key)))
            .ToList();
        CategoryList.ItemsSource = new CollectionViewSource { IsSourceGrouped = true, Source = _groups }.View;
        CategoryList.SelectedItem = _current is not null && _groups.Any(g => g.Contains(_current)) ? _current : null;
        _showingCategories = false;
    }

    private void OnCategoryGroupToggleClick(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is not ListCategoryGroup group)
        {
            return;
        }

        if (!_collapsedGroups.Remove(group.Key))
        {
            _collapsedGroups.Add(group.Key);
        }

        AppSettings.Write("listsCollapsedGroups", new System.Text.Json.Nodes.JsonArray([.. _collapsedGroups.Order().Select(k => System.Text.Json.Nodes.JsonValue.Create(k))]));
        ShowCategories();

        // The new ItemsSource starts at the top again - jump back to the group heading.
        if (_groups.FirstOrDefault(g => g.Key == group.Key) is { } shown && _groups.IndexOf(shown) > 0)
        {
            CategoryList.UpdateLayout();
            CategoryList.MakeVisible(new SemanticZoomLocation { Item = shown });
        }
    }

    private void OnSearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            ShowGames();
        }
    }

    private List<ListGame> VisibleGames() => Sort(ApplyColumnFilters(CategoryGames(), null)).ToList();

    /// <summary>Games of the category after search and time range, before the column filters and unsorted.</summary>
    private IEnumerable<ListGame> CategoryGames()
    {
        if (_current is not { } category)
        {
            return [];
        }

        IEnumerable<ListGame> games = category.Games;
        var search = SearchBox.Text.Trim();

        if (search.Length > 0)
        {
            games = games.Where(g => g.Name.Contains(search, StringComparison.CurrentCultureIgnoreCase) ||
                                     g.AppId.ToString(CultureInfo.InvariantCulture) == search);
        }

        if (_periodFrom is not null || _periodTo is not null)
        {
            games = games.Where(g => PeriodDate(g) is { } date &&
                                     (_periodFrom is not { } from || date.LocalDateTime.Date >= from.Date) &&
                                     (_periodTo is not { } to || date.LocalDateTime.Date <= to.Date));
        }

        return games;
    }

    private IEnumerable<ListGame> ApplyColumnFilters(IEnumerable<ListGame> games, string? except)
    {
        foreach (var (key, values) in _columnFilters)
        {
            if (key != except && ColumnLayouts.Lists.IsVisible(key))
            {
                games = games.Where(g => values.Contains(ColumnValue(key, g)));
            }
        }

        return games;
    }

    /// <summary>Value of a cell for the column filter; for dates the year counts.</summary>
    private static string ColumnValue(string key, ListGame game) => key switch
    {
        ColumnLayouts.AppId => game.AppIdText,
        ColumnLayouts.FirstUnlock => game.FirstUnlock?.LocalDateTime.Year.ToString(CultureInfo.CurrentCulture) ?? "—",
        ColumnLayouts.LastUnlock => game.LastUnlock?.LocalDateTime.Year.ToString(CultureInfo.CurrentCulture) ?? "—",
        ColumnLayouts.Progress => game.PercentText,
        _ => game.Name,
    };

    private static SortField? FieldFor(string key) => key switch
    {
        ColumnLayouts.AppId => SortField.AppId,
        ColumnLayouts.Name => SortField.Name,
        ColumnLayouts.FirstUnlock => SortField.FirstUnlock,
        ColumnLayouts.LastUnlock => SortField.LastUnlock,
        ColumnLayouts.Progress => SortField.Progress,
        _ => null,
    };

    (string Key, bool Descending) IColumnHost.Sort => (_sortField switch
    {
        SortField.FirstUnlock => ColumnLayouts.FirstUnlock,
        SortField.LastUnlock => ColumnLayouts.LastUnlock,
        SortField.Progress => ColumnLayouts.Progress,
        SortField.AppId => ColumnLayouts.AppId,
        _ => ColumnLayouts.Name,
    }, _sortDescending);

    void IColumnHost.SortBy(string key, bool? descending)
    {
        if (FieldFor(key) is not { } field)
        {
            return;
        }

        _sortDescending = descending ?? (field == _sortField && !_sortDescending);
        UpdateSortDirection();

        if (field != _sortField)
        {
            // Triggers OnSortChanged, which redraws.
            SortBox.SelectedIndex = (int)field;
        }
        else
        {
            ShowGames();
        }
    }

    bool IColumnHost.IsFiltered(string key) => _columnFilters.ContainsKey(key);

    /// <summary>Like in Excel: the values under all other filters, in the order of the column.</summary>
    IReadOnlyList<FilterOption> IColumnHost.FilterOptions(string key)
    {
        _columnFilters.TryGetValue(key, out var chosen);
        var games = ApplyColumnFilters(CategoryGames(), key);
        games = key switch
        {
            ColumnLayouts.FirstUnlock => games.OrderBy(g => g.FirstUnlock is null ? 1 : 0).ThenBy(g => g.FirstUnlock),
            ColumnLayouts.LastUnlock => games.OrderBy(g => g.LastUnlock is null ? 1 : 0).ThenBy(g => g.LastUnlock),
            ColumnLayouts.Progress => games.OrderBy(g => g.Ratio),
            ColumnLayouts.AppId => games.OrderBy(g => g.AppId),
            _ => games,
        };

        var seen = new HashSet<string>();
        var options = new List<FilterOption>();

        foreach (var game in games)
        {
            var value = ColumnValue(key, game);

            if (seen.Add(value))
            {
                options.Add(new FilterOption(value, value, chosen is null || chosen.Contains(value)));
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

        ShowGames();
    }

    private void OnColumnsFlyoutOpening(object sender, object e) => ColumnsFlyout.Content = new ColumnChooserView(ColumnLayouts.Lists);

    private DateTimeOffset? PeriodDate(ListGame game) => _periodUsesLastUnlock ? game.LastUnlock : game.FirstUnlock;

    /// <summary>The categories are already sorted by name; games without a date are always at the end when sorting by date.</summary>
    private IEnumerable<ListGame> Sort(IEnumerable<ListGame> games)
    {
        switch (_sortField)
        {
            case SortField.AppId:
                return _sortDescending ? games.OrderByDescending(g => g.AppId) : games.OrderBy(g => g.AppId);

            case SortField.Progress:
                return _sortDescending ? games.OrderByDescending(g => g.Ratio).ThenByDescending(g => g.Total) : games.OrderBy(g => g.Ratio).ThenBy(g => g.Total);

            case SortField.FirstUnlock or SortField.LastUnlock:
                var list = games.ToList();
                Func<ListGame, DateTimeOffset?> key = _sortField == SortField.FirstUnlock ? g => g.FirstUnlock : g => g.LastUnlock;
                var dated = list.Where(g => key(g) is not null);
                dated = _sortDescending ? dated.OrderByDescending(g => key(g)) : dated.OrderBy(g => key(g));
                return dated.Concat(list.Where(g => key(g) is null));

            default:
                return _sortDescending ? games.Reverse() : games;
        }
    }

    private void OnSortChanged(object sender, SelectionChangedEventArgs e)
    {
        _sortField = (SortField)Math.Max(0, SortBox.SelectedIndex);
        ShowGames();
    }

    private void OnSortDirectionClick(object sender, RoutedEventArgs e)
    {
        _sortDescending = !_sortDescending;
        UpdateSortDirection();
        ShowGames();
    }

    private void UpdateSortDirection()
    {
        SortDirectionIcon.Glyph = _sortDescending ? "\uE74B" : "\uE74A";
        var text = _sortDescending ? Strings.SortDescending : Strings.SortAscending;
        ToolTipService.SetToolTip(SortDirectionButton, text);
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(SortDirectionButton, text);
    }

    private void OnPeriodChanged(object sender, object e)
    {
        _periodUsesLastUnlock = PeriodField.SelectedIndex == 1;
        _periodFrom = PeriodFrom.Date;
        _periodTo = PeriodTo.Date;
        UpdatePeriodButton();
        ShowGames();
    }

    private void OnPeriodResetClick(object sender, RoutedEventArgs e)
    {
        PeriodFrom.Date = null;
        PeriodTo.Date = null;
        OnPeriodChanged(sender, e);
    }

    /// <summary>A set time range is shown on the button, which is then highlighted.</summary>
    private void UpdatePeriodButton()
    {
        var active = _periodFrom is not null || _periodTo is not null;
        PeriodText.Text = active
            ? (_periodUsesLastUnlock ? Strings.ColumnLastUnlock : Strings.ColumnFirstUnlock) + ": " +
              (_periodFrom?.ToString("d", CultureInfo.CurrentCulture) ?? "…") + " – " + (_periodTo?.ToString("d", CultureInfo.CurrentCulture) ?? "…")
            : Strings.DateRange;
        PeriodButton.Style = active ? (Style)Application.Current.Resources["AccentButtonStyle"] : null;
    }

    private void OnGameContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
    {
        if (!args.InRecycleQueue && args.Item is ListGame game)
        {
            game.EnsureIcon();
        }
    }

    private void ShowGames()
    {
        if (_current is not { } category)
        {
            return;
        }

        var games = VisibleGames();
        var shown = games.Count == category.Games.Count ? "" : "  ·  " + Strings.LibraryShown(games.Count.ToString("N0", CultureInfo.CurrentCulture));
        CategoryTitle.Text = category.Title + "  (" + category.CountText + ")" + shown
            + (_columnFilters.Count > 0 ? "  ·  " + Strings.ColumnFiltersActive : "");
        ColumnHeaders.Build(ListHeader, this);
        CategoryIcon.Kind = category.IconKind;
        CategoryIcon.Visibility = category.IconKind == MedalKind.None ? Visibility.Collapsed : Visibility.Visible;
        CategoryDescription.Text = category.Description;
        GamesList.ItemsSource = games;
        EmptyText.Visibility = games.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void ShowError(string title, string message)
    {
        ErrorTitle.Text = title;
        ErrorMessage.Text = message;
        ShowPanel(ErrorPanel);
    }

    private void ShowPanel(UIElement panel)
    {
        ContentGrid.Visibility = panel == ContentGrid ? Visibility.Visible : Visibility.Collapsed;
        LoadingPanel.Visibility = panel == LoadingPanel ? Visibility.Visible : Visibility.Collapsed;
        ErrorPanel.Visibility = panel == ErrorPanel ? Visibility.Visible : Visibility.Collapsed;
    }

    private void SetCommandsEnabled(bool enabled)
    {
        RefreshButton.IsEnabled = enabled;
        AddAppIdsButton.IsEnabled = enabled && _accountId != 0;
        ExportButton.IsEnabled = enabled && _categories.Count > 0;
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

    // ---------------------------------------------------------------- Copy

    private void OnCopyIdsClick(object sender, RoutedEventArgs e)
        => Copy(VisibleGames().Select(g => g.AppId.ToString(CultureInfo.InvariantCulture)).ToList());

    private void OnCopyNamesClick(object sender, RoutedEventArgs e)
        => Copy(VisibleGames().Select(g => g.GuideLine).ToList());

    private void Copy(List<string> lines)
    {
        CopyText(string.Join("\r\n", lines));
        Notify(Strings.CopiedCount(lines.Count.ToString("N0", CultureInfo.CurrentCulture)), InfoBarSeverity.Success);
    }

    private static void CopyText(string text)
    {
        var package = new DataPackage();
        package.SetText(text);
        Clipboard.SetContent(package);
    }

    // ---------------------------------------------------------------- Marks

    private static uint MenuAppId(object sender) => (sender as FrameworkElement)?.Tag is uint id ? id : 0;

    private ListGame? FindGame(uint appId) => _categories.SelectMany(c => c.Games).FirstOrDefault(g => g.AppId == appId);

    private async void OnMarkCantFinishClick(object sender, RoutedEventArgs e)
    {
        if (FindGame(MenuAppId(sender)) is not { } game)
        {
            return;
        }

        var reason = new TextBox
        {
            PlaceholderText = Strings.CantFinishReasonPlaceholder,
            Text = game.CantFinishReason ?? "",
            FlowDirection = AppLanguage.FlowDirection,
        };
        var panel = new StackPanel { Spacing = 10, MinWidth = 380 };
        panel.Children.Add(new TextBlock { Text = Strings.CantFinishReasonHint, TextWrapping = TextWrapping.Wrap, Style = (Style)Application.Current.Resources["SecondaryTextStyle"] });
        panel.Children.Add(reason);

        var dialog = Dialogs.Create(Root.XamlRoot, Strings.CantFinishDialogTitle(game.Name), panel, Strings.Mark);

        if (await ShowDialogAsync(dialog) != ContentDialogResult.Primary)
        {
            return;
        }

        _state.CantFinish[game.AppId] = reason.Text.Trim();
        SaveState();
        Rebuild(updateHistory: false);
    }

    private void OnUnmarkCantFinishClick(object sender, RoutedEventArgs e)
    {
        if (_state.CantFinish.Remove(MenuAppId(sender)))
        {
            SaveState();
            Rebuild(updateHistory: false);
        }
    }

    private void OnMarkNotAnymoreClick(object sender, RoutedEventArgs e)
    {
        var appId = MenuAppId(sender);
        _state.NotAnymoreManual.Add(appId);
        _state.NotAnymoreDismissed.Remove(appId);
        SaveState();
        Rebuild(updateHistory: false);
    }

    private void OnUnmarkNotAnymoreClick(object sender, RoutedEventArgs e)
    {
        var appId = MenuAppId(sender);
        _state.NotAnymoreManual.Remove(appId);

        // Detected automatically: remember that it should not be shown any more.
        if (_state.Perfect.ContainsKey(appId))
        {
            _state.NotAnymoreDismissed.Add(appId);
        }

        SaveState();
        Rebuild(updateHistory: false);
    }

    // ---------------------------------------------------------------- Games added by hand

    private async void OnAddAppIdsClick(object sender, RoutedEventArgs e)
    {
        var input = new TextBox { PlaceholderText = Strings.AddAppIdsPlaceholder, FlowDirection = FlowDirection.LeftToRight };
        var panel = new StackPanel { Spacing = 10, MinWidth = 380 };
        panel.Children.Add(new TextBlock { Text = Strings.AddAppIdsHint, TextWrapping = TextWrapping.Wrap, Style = (Style)Application.Current.Resources["SecondaryTextStyle"] });
        panel.Children.Add(input);

        var dialog = Dialogs.Create(Root.XamlRoot, Strings.AddAppIds, panel, Strings.Add);

        if (await ShowDialogAsync(dialog) != ContentDialogResult.Primary)
        {
            return;
        }

        // Numbers from the input - so store links like store.steampowered.com/app/440/ work too.
        var ids = Regex.Matches(input.Text, "\\d+")
                       .Select(m => uint.TryParse(m.Value, NumberStyles.None, CultureInfo.InvariantCulture, out var id) ? id : 0)
                       .Where(id => id > 0)
                       .ToHashSet();

        if (ids.Count == 0)
        {
            Notify(Strings.AddAppIdsNone, InfoBarSeverity.Warning);
            return;
        }

        _state.ManualApps.UnionWith(ids);
        SaveState();

        var source = new GameSource(_session, _state, _cachePath, _steamPath, _accountId);
        SetCommandsEnabled(false);

        try
        {
            var added = await Task.Run(() => ReadGames(source, ids));
            _raw = _raw.Where(r => !ids.Contains(r.Progress.AppId)).Concat(added).ToList();
            Rebuild(updateHistory: true);
            Notify(Strings.AddAppIdsAdded(ids.Count.ToString("N0", CultureInfo.CurrentCulture)), InfoBarSeverity.Success);
        }
        catch (OperationCanceledException)
        {
            // The library was closed.
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Text.Json.JsonException)
        {
            Notify(Strings.ListsReadError(ex.Message), InfoBarSeverity.Error);
        }
        finally
        {
            SetCommandsEnabled(true);
        }
    }

    private void OnRemoveManualClick(object sender, RoutedEventArgs e)
    {
        var appId = MenuAppId(sender);

        if (!_state.ManualApps.Remove(appId))
        {
            return;
        }

        SaveState();

        // If Steam recognizes the game by itself, it stays - just without the note.
        _raw = _raw.Where(r => r.Progress.AppId != appId || r.Automatic)
                   .Select(r => r.Progress.AppId == appId ? r with { Manual = false } : r)
                   .ToList();
        Rebuild(updateHistory: false);
    }

    private void OnOpenGameClick(object sender, RoutedEventArgs e)
    {
        if (GameLauncher.OpenGame(MenuAppId(sender)) is { } error)
        {
            Notify(Strings.GameWindowFailed(error), InfoBarSeverity.Error);
        }
    }

    private void OnStorePageClick(object sender, RoutedEventArgs e)
    {
        if (GameLauncher.OpenUri("https://store.steampowered.com/app/" + MenuAppId(sender).ToString(CultureInfo.InvariantCulture)) is { } error)
        {
            Notify(Strings.CouldNotOpen(error), InfoBarSeverity.Error);
        }
    }

    // ---------------------------------------------------------------- Guide text

    private async void OnExportClick(object sender, RoutedEventArgs e)
    {
        if (_categories.Count == 0)
        {
            return;
        }

        var includeUntouched = new CheckBox { Content = Strings.ExportIncludeUntouched, IsChecked = true };
        var limit = new NumberBox
        {
            Header = Strings.ExportLimit,
            Value = GuideExport.DefaultLimit,
            Minimum = 500,
            Maximum = 100000,
            SmallChange = 500,
            LargeChange = 1000,
            SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Inline,
            Width = 220,
        };
        var sectionCount = new TextBlock { VerticalAlignment = VerticalAlignment.Bottom, Style = (Style)Application.Current.Resources["SecondaryTextStyle"] };
        var sectionList = new ListView { SelectionMode = ListViewSelectionMode.Single, Width = 340, Height = 420 };
        var preview = new TextBox
        {
            IsReadOnly = true,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.NoWrap,
            Height = 420,
            FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Cascadia Mono, Consolas"),
        };
        ScrollViewer.SetHorizontalScrollBarVisibility(preview, ScrollBarVisibility.Auto);
        ScrollViewer.SetVerticalScrollBarVisibility(preview, ScrollBarVisibility.Auto);

        var copyTitle = new Button { Content = Strings.ExportCopyTitle };
        var copyText = new Button { Content = Strings.ExportCopyText, Style = (Style)Application.Current.Resources["AccentButtonStyle"] };
        var saveAll = new Button { Content = Strings.ExportSaveAll };

        List<GuideSection> sections = [];

        void Generate()
        {
            var value = double.IsNaN(limit.Value) ? GuideExport.DefaultLimit : (int)limit.Value;
            sections = GuideExport.Build(_categories, includeUntouched.IsChecked == true, value);
            sectionList.ItemsSource = sections.Select(s => s.Title + "\n" + s.LengthText).ToList();
            sectionList.SelectedIndex = 0;
            sectionCount.Text = Strings.ExportSections(sections.Count.ToString(CultureInfo.CurrentCulture));
        }

        GuideSection? Current() => sectionList.SelectedIndex >= 0 && sectionList.SelectedIndex < sections.Count ? sections[sectionList.SelectedIndex] : null;

        sectionList.SelectionChanged += (_, _) => preview.Text = Current()?.Body ?? "";
        includeUntouched.Click += (_, _) => Generate();
        limit.ValueChanged += (_, _) => Generate();
        copyTitle.Click += (_, _) =>
        {
            if (Current() is { } s)
            {
                CopyText(s.Title);
            }
        };
        copyText.Click += (_, _) =>
        {
            if (Current() is { } s)
            {
                CopyText(s.Body.Replace("\n", "\r\n"));
            }
        };

        var savedInfo = new TextBlock { TextWrapping = TextWrapping.Wrap, Style = (Style)Application.Current.Resources["SecondaryCaptionStyle"], IsTextSelectionEnabled = true };
        saveAll.Click += (_, _) =>
        {
            try
            {
                var folder = SaveSections(sections);
                savedInfo.Text = Strings.ExportSaved(folder);
                GameLauncher.OpenUri(folder);
            }
            catch (Exception ex)
            {
                savedInfo.Text = Strings.CouldNotOpen(ex.Message);
            }
        };

        var options = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 20 };
        options.Children.Add(limit);
        options.Children.Add(new StackPanel { VerticalAlignment = VerticalAlignment.Bottom, Children = { includeUntouched } });
        options.Children.Add(sectionCount);

        var body = new Grid { ColumnSpacing = 12 };
        body.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        body.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        body.Children.Add(sectionList);
        Grid.SetColumn(preview, 1);
        body.Children.Add(preview);

        var buttons = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, HorizontalAlignment = HorizontalAlignment.Right };
        buttons.Children.Add(copyTitle);
        buttons.Children.Add(copyText);
        buttons.Children.Add(saveAll);

        var content = new StackPanel { Spacing = 12, Width = 940 };
        content.Children.Add(new TextBlock { Text = Strings.ExportHint, TextWrapping = TextWrapping.Wrap, Style = (Style)Application.Current.Resources["SecondaryTextStyle"] });
        content.Children.Add(options);
        content.Children.Add(body);
        content.Children.Add(buttons);
        content.Children.Add(savedInfo);

        var dialog = Dialogs.Create(Root.XamlRoot, Strings.ExportTitle, content, Strings.Close, showClose: false);
        dialog.Resources["ContentDialogMaxWidth"] = 1040.0;
        dialog.Resources["ContentDialogMaxHeight"] = 860.0;
        Generate();
        await ShowDialogAsync(dialog);
    }

    /// <summary>Each section as a text file of its own (heading in the first line) plus one file with everything.</summary>
    private static string SaveSections(List<GuideSection> sections)
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "Steam Achievement Manager",
            "Guide " + DateTime.Now.ToString("yyyy-MM-dd HH-mm", CultureInfo.InvariantCulture));
        Directory.CreateDirectory(folder);
        var invalid = Path.GetInvalidFileNameChars();
        var all = new List<string>();

        for (var i = 0; i < sections.Count; i++)
        {
            var section = sections[i];
            var safe = new string(section.Title.Select(c => invalid.Contains(c) ? '_' : c).ToArray());
            var text = section.Title + "\r\n\r\n" + section.Body.Replace("\n", "\r\n") + "\r\n";
            File.WriteAllText(Path.Combine(folder, (i + 1).ToString("00", CultureInfo.InvariantCulture) + " " + safe + ".txt"), text);
            all.Add(text);
        }

        File.WriteAllText(Path.Combine(folder, "00 All sections.txt"), string.Join("\r\n\r\n", all));
        return folder;
    }

    // ---------------------------------------------------------------- Keyboard

    private void OnSearchAccelerator(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        args.Handled = true;
        FocusSearch();
    }

    private async void OnRefreshAccelerator(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        args.Handled = true;
        await LoadAsync();
    }

    /// <summary>Ctrl+F and F5 in the main window while the lists are shown.</summary>
    internal void FocusSearch() => SearchBox.Focus(FocusState.Keyboard);

    internal Task RefreshAsync() => LoadAsync();

    private async void OnRefreshClick(object sender, RoutedEventArgs e) => await LoadAsync();
}
