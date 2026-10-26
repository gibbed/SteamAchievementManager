using System.Globalization;
using SAM.App.Helpers;
using SAM.App.Lists;
using SAM.App.Localization;

namespace SAM.App.Library;

/// <summary>
/// A section of the library: Completed, In Progress or Not Started. When collapsed it contains no tiles,
/// but the heading still shows the number of games.
/// </summary>
public sealed class GameGroup : List<GameEntry>
{
    private readonly int _total;

    internal GameGroup(ProgressState state, IReadOnlyList<GameEntry> games, bool isCollapsed)
        : base(isCollapsed ? [] : games)
    {
        State = state;
        IsCollapsed = isCollapsed;
        _total = games.Count;
    }

    internal ProgressState State { get; }

    internal bool IsCollapsed { get; }

    public string GroupTitle => TitleFor(State);

    public string GroupCount => _total.ToString("N0", CultureInfo.CurrentCulture);

    public MedalKind GroupMedalKind => State == ProgressState.Completed ? MedalKind.Game : MedalKind.None;

    public bool HasGroupMedal => State == ProgressState.Completed;

    /// <summary>Like the Expander: ∧ (ChevronUp) to collapse, ∨ (ChevronDown) to expand.</summary>
    public string ToggleGlyph => IsCollapsed ? "\uE70D" : "\uE70E";

    public string ToggleTooltip => IsCollapsed ? Strings.SectionExpand : Strings.SectionCollapse;

    public string ToggleLabel => GroupTitle + ", " + GroupCount + ", " + ToggleTooltip;

    internal static string TitleFor(ProgressState state) => state switch
    {
        ProgressState.Completed => Strings.ProgressCompleted,
        ProgressState.InProgress => Strings.ProgressInProgress,
        _ => Strings.ProgressNotStarted,
    };
}
