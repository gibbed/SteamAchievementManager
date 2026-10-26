using System.Globalization;
using Microsoft.UI.Xaml.Media;
using SAM.App.Helpers;
using SAM.App.Localization;

namespace SAM.App.Lists;

/// <summary>The state of a game that decides its category.</summary>
public enum ListKind
{
    All = 0,
    Done = 1,
    NotAnymore = 2,
    Started = 3,
    CantFinish = 4,

    // Not from the Steam collections: games with achievement sets by their state.
    SetsCompleted = 6,
    SetsInProgress = 7,
    SetsNotStarted = 8,
    Untouched = 9,

    // Games with completed base game sets or DLC sets (Steam's blue or purple medal).
    SetsBase = 10,
    SetsDlc = 11,
}

/// <summary>Visibility: 1.0 all, 1.1 visible on the profile (👀✔), 1.2 Profile Features Limited (👀✖).</summary>
public enum ListScope
{
    All = 0,
    Visible = 1,
    Limited = 2,
}

/// <summary>A game in the achievement lists.</summary>
public sealed class ListGame : ObservableObject
{
    private ImageSource? _icon;
    private SetBadges? _setBadges;
    private bool _iconRequested;

    internal ListGame(uint appId, string name, int unlocked, int total, bool? vetted)
    {
        AppId = appId;
        Name = name;
        Unlocked = unlocked;
        Total = total;
        Vetted = vetted;
    }

    public uint AppId { get; }

    public string Name { get; }

    public int Unlocked { get; }

    public int Total { get; }

    /// <summary>true = visible on the profile, false = Profile Features Limited, null = not reported by Steam yet.</summary>
    public bool? Vetted { get; }

    public ListKind Kind { get; internal set; }

    /// <summary>Added to the lists by hand via "Add App IDs".</summary>
    public bool IsManual { get; internal set; }

    /// <summary>Achievement sets (Steam beta since October 2026); null for games without sets.</summary>
    internal SetSummary? Sets { get; init; }

    public bool HasSets => Sets is not null;

    public string SetsText => Sets?.SetsText ?? "";

    public string? SetsTooltip => Sets?.Tooltip;

    /// <summary>Completed sets like on Steam: "1" + blue medal, "2" + purple double medal.</summary>
    public SetBadges? SetBadges => Sets is null ? null : _setBadges ??= new SetBadges(Sets);

    /// <summary>Large medal for 100 %, otherwise the medal of completed sets.</summary>
    public MedalKind MedalKind => Kind == ListKind.Done ? MedalKind.Game : Sets?.Medal is MedalKind.Set or MedalKind.SetDouble or MedalKind.DlcSet ? Sets.Medal : MedalKind.None;

    public bool HasMedal => MedalKind != MedalKind.None;

    public string? MedalTooltip => MedalKind == MedalKind.Game ? Strings.CategoryDoneDesc : SetsTooltip;

    /// <summary>First and last unlock according to Steam's stats file on this PC; null if it is missing.</summary>
    public DateTimeOffset? FirstUnlock { get; internal set; }

    public DateTimeOffset? LastUnlock { get; internal set; }

    public string FirstUnlockText => DateText(FirstUnlock);

    public string LastUnlockText => DateText(LastUnlock);

    public string FirstUnlockTooltip => DateTooltip(FirstUnlock);

    public string LastUnlockTooltip => DateTooltip(LastUnlock);

    /// <summary>App ID as a badge after the name (selectable and copyable).</summary>
    public string AppIdText => AppId.ToString(CultureInfo.InvariantCulture);

    /// <summary>Local icon file from Steam's library cache or address on the CDN.</summary>
    internal string? IconPath { get; set; }

    internal string? IconUrl { get; set; }

    public ImageSource? Icon
    {
        get => _icon;
        private set => Set(ref _icon, value);
    }

    public string? CantFinishReason { get; internal set; }

    public bool IsNotAnymoreManual { get; internal set; }

    public double Ratio => Total == 0 ? 0 : (double)Unlocked / Total;

    public string ProgressText => Unlocked.ToString("N0", CultureInfo.CurrentCulture) + " / " + Total.ToString("N0", CultureInfo.CurrentCulture);

    public string PercentText => Ratio.ToString("P0", CultureInfo.CurrentCulture);

    public string VisibilityText => Vetted switch
    {
        true => "👀✔",
        false => "👀✖",
        null => "👀?",
    };

    public string VisibilityTooltip => Vetted switch
    {
        true => Strings.VisibilityVisibleTooltip,
        false => Strings.VisibilityLimitedTooltip,
        null => Strings.VisibilityUnknownTooltip,
    };

    public bool HasReason => !string.IsNullOrEmpty(CantFinishReason);

    public bool IsCantFinish => Kind == ListKind.CantFinish;

    /// <summary>Marked - even if the game has reached 100 % since.</summary>
    public bool IsCantFinishMarked => CantFinishReason is not null;

    public bool IsNotAnymore => Kind == ListKind.NotAnymore;

    /// <summary>A game at 100 % cannot be "can't finish"; an existing mark stays editable.</summary>
    public bool CanMarkCantFinish => Kind != ListKind.Done || IsCantFinishMarked;

    public bool CanMarkNotAnymore => Kind is ListKind.Started or ListKind.Untouched or ListKind.CantFinish;

    /// <summary>Line for the guide text, as in the model guide: "Name (reason)".</summary>
    public string GuideLine => HasReason ? Name + " (" + CantFinishReason + ")" : Name;

    /// <summary>Load the icon as soon as the row becomes visible.</summary>
    public async void EnsureIcon()
    {
        if (_iconRequested)
        {
            return;
        }

        _iconRequested = true;
        Icon = await ImageCache.LoadIconAsync(IconPath, IconUrl);
    }

    private static string DateText(DateTimeOffset? date)
        => date is { } d ? d.LocalDateTime.ToString("d", CultureInfo.CurrentCulture) : "—";

    private static string DateTooltip(DateTimeOffset? date)
        => date is { } d ? d.LocalDateTime.ToString("f", CultureInfo.CurrentCulture) : Strings.UnlockTimeUnknown;

    public override string ToString() => Name + ", " + ProgressText;
}

/// <summary>A category like "👀✔ Not 100 % Anymore".</summary>
public sealed class ListCategory : ObservableObject
{
    internal ListCategory(ListScope scope, ListKind kind, string title, string description, string group)
    {
        Scope = scope;
        Kind = kind;
        Title = title;
        Description = description;
        Group = group;
    }

    public ListScope Scope { get; }

    public ListKind Kind { get; }

    /// <summary>Name as in the Steam collection - deliberately not translated.</summary>
    public string Title { get; }

    public string Description { get; }

    /// <summary>Heading of the group in the category list ("All games", "Sets", ...).</summary>
    public string Group { get; }

    /// <summary>Achievement set categories are listed under "Sets", without "Sets:" in the name.</summary>
    public bool IsSetCategory => Kind is ListKind.SetsCompleted or ListKind.SetsInProgress or ListKind.SetsNotStarted or ListKind.SetsBase or ListKind.SetsDlc;

    /// <summary>
    /// Medal like on Steam: large blue for "Done", blue double medal for "all sets done", blue for base game
    /// sets, purple for DLC sets, grey for everything open; "All" is mixed and has none.
    /// </summary>
    public MedalKind IconKind => Kind switch
    {
        ListKind.All => MedalKind.None,
        ListKind.Done => MedalKind.Game,
        ListKind.SetsCompleted => MedalKind.SetDouble,
        ListKind.SetsBase => MedalKind.Set,
        ListKind.SetsDlc => MedalKind.DlcSet,
        _ => MedalKind.Unfinished,
    };

    /// <summary>Key of the group for collapsing and expanding (independent of the language).</summary>
    public string GroupKey => IsSetCategory ? "sets"
        : Kind == ListKind.Untouched ? "other"
        : Scope switch { ListScope.Visible => "visible", ListScope.Limited => "limited", _ => "all" };

    /// <summary>Name for screen readers and copying: for the set categories with "Sets:" in front.</summary>
    public string FullTitle => IsSetCategory ? Strings.SetsCategory(Title) : Title;

    public List<ListGame> Games { get; internal set; } = [];

    public string CountText => Games.Count.ToString("N0", CultureInfo.CurrentCulture);

    public override string ToString() => FullTitle + ", " + CountText;
}

/// <summary>A group of the category list with its own heading; when collapsed it has no entries, the heading stays.</summary>
public sealed class ListCategoryGroup : List<ListCategory>
{
    internal ListCategoryGroup(string key, string title, IEnumerable<ListCategory> categories, bool isCollapsed)
        : base(isCollapsed ? [] : categories)
    {
        Key = key;
        Title = title;
        IsCollapsed = isCollapsed;
    }

    internal string Key { get; }

    public string Title { get; }

    internal bool IsCollapsed { get; }

    /// <summary>Like in the library: ∧ (ChevronUp) to collapse, ∨ (ChevronDown) to expand.</summary>
    public string ToggleGlyph => IsCollapsed ? "\uE70D" : "\uE70E";

    public string ToggleTooltip => IsCollapsed ? Strings.SectionExpand : Strings.SectionCollapse;

    public string ToggleLabel => Title + ", " + ToggleTooltip;

    public override string ToString() => Title;
}
