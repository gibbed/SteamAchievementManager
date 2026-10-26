using System.ComponentModel;
using System.Globalization;
using SAM.App.Helpers;
using SAM.App.Localization;
using SAM.App.Schema;

namespace SAM.App.Game;

/// <summary>
/// An achievement set in the game window: the displayed achievements of the set plus a header with medal and progress.
/// Progress counts all achievements of the set (except archived ones), regardless of search and filters - like on Steam's page.
/// When collapsed, the set contains no rows; the header stays.
/// The properties are deliberately named Set..., because the window has elements like "ProgressText".
/// </summary>
public sealed class AchievementGroup : List<AchievementItem>, INotifyPropertyChanged
{
    private readonly List<AchievementItem> _members;

    internal AchievementGroup(AchievementSet set, bool owned, IReadOnlyList<AchievementItem> shown, List<AchievementItem> members, bool isCollapsed)
        : base(isCollapsed ? [] : shown)
    {
        Id = set.Id;
        Name = set.Name ?? Strings.SetBaseGame;
        IsDlc = set.IsDlc;
        IsOwned = owned;
        IsCollapsed = isCollapsed;
        MatchCount = shown.Count;
        _members = members;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public int Id { get; }

    public string Name { get; }

    /// <summary>Set of a DLC - purple on Steam, otherwise blue.</summary>
    public bool IsDlc { get; }

    public bool IsBaseGame => !IsDlc;

    public bool IsOwned { get; }

    public bool IsNotOwned => !IsOwned;

    public string NotOwnedText => Strings.SetNotOwned;

    internal bool IsCollapsed { get; }

    /// <summary>Achievements of the set matched by search and filters - even when the set is collapsed.</summary>
    internal int MatchCount { get; }

    /// <summary>Like in the library: ∧ (ChevronUp) to collapse, ∨ (ChevronDown) to expand.</summary>
    public string ToggleGlyph => IsCollapsed ? "\uE70D" : "\uE70E";

    public string ToggleTooltip => IsCollapsed ? Strings.SectionExpand : Strings.SectionCollapse;

    public string ToggleLabel => Name + ", " + ToggleTooltip;

    private int Unlocked => _members.Count(a => a.IsAchieved && !a.IsArchived);

    private int Total => _members.Count(a => !a.IsArchived);

    public bool IsComplete => Total > 0 && Unlocked >= Total;

    public double SetRatio => Total == 0 ? 0 : (double)Unlocked / Total;

    public string SetProgressText => Unlocked.ToString("N0", CultureInfo.CurrentCulture) + " / " + Total.ToString("N0", CultureInfo.CurrentCulture);

    public string SetPercentText => SetRatio.ToString("P0", CultureInfo.CurrentCulture);

    /// <summary>Like on Steam: base game set blue, DLC set purple (double), open grey.</summary>
    public MedalKind MedalKind => IsComplete ? IsDlc ? MedalKind.DlcSet : MedalKind.Set : MedalKind.Unfinished;

    public string MedalTooltip => IsComplete ? Strings.SetCompleted : SetProgressText;

    /// <summary>Redraw the header after an achievement changed.</summary>
    public void Refresh()
    {
        foreach (var name in new[] { nameof(IsComplete), nameof(SetRatio), nameof(SetProgressText), nameof(SetPercentText), nameof(MedalKind), nameof(MedalTooltip) })
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
