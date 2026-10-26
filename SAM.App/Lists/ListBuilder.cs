using System.Globalization;
using SAM.App.Localization;

namespace SAM.App.Lists;

/// <summary>
/// Sorts the games into the categories of the Steam collections:
/// All, Done, Not 100 % Anymore, Started, Can't Finish
/// for all games, visible ones (👀✔) and Profile Features Limited (👀✖).
/// </summary>
internal static class ListBuilder
{
    public static readonly StringComparer NameComparer = StringComparer.Create(CultureInfo.InvariantCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace);

    /// <summary>Remembers every game at 100 %. Returns true if anything changed.</summary>
    public static bool UpdateHistory(ListState state, IEnumerable<(ListGame Game, bool Complete)> games, long now)
    {
        var changed = false;

        foreach (var (game, complete) in games)
        {
            if (!complete)
            {
                continue;
            }

            if (state.Perfect.TryGetValue(game.AppId, out var record))
            {
                record.Total = game.Total;
                record.LastSeen = now;
            }
            else
            {
                state.Perfect[game.AppId] = new PerfectRecord { Total = game.Total, FirstSeen = now, LastSeen = now };
            }

            // Back at 100 %: a later drop should be detected again.
            state.NotAnymoreDismissed.Remove(game.AppId);
            changed = true;
        }

        return changed;
    }

    /// <summary>Remembers completed achievement sets. Returns true if a new one was added.</summary>
    public static bool UpdateSetHistory(ListState state, IEnumerable<ListGame> games)
    {
        var changed = false;

        foreach (var game in games)
        {
            foreach (var set in game.Sets?.Sets.Where(s => s.Accessible && s.IsComplete) ?? [])
            {
                if (!state.CompletedSets.TryGetValue(game.AppId, out var ids))
                {
                    ids = [];
                    state.CompletedSets[game.AppId] = ids;
                }

                changed |= ids.Add(set.Id);
            }
        }

        return changed;
    }

    /// <summary>Order: 100 % &gt; Can't Finish &gt; Not 100 % Anymore &gt; Started &gt; Untouched.</summary>
    public static void Classify(ListState state, ListGame game, bool complete)
    {
        game.CantFinishReason = state.CantFinish.GetValueOrDefault(game.AppId);
        game.IsNotAnymoreManual = state.NotAnymoreManual.Contains(game.AppId);

        var notAnymore = game.IsNotAnymoreManual ||
                         state.Perfect.ContainsKey(game.AppId) && !state.NotAnymoreDismissed.Contains(game.AppId);

        game.Kind = complete ? ListKind.Done
            : game.CantFinishReason is not null ? ListKind.CantFinish
            : notAnymore ? ListKind.NotAnymore
            : game.Unlocked > 0 ? ListKind.Started
            : ListKind.Untouched;
    }

    public static List<ListCategory> Build(IReadOnlyCollection<ListGame> games)
    {
        var sorted = games.OrderBy(g => GuideExport.SortKey(g.Name), NameComparer).ThenBy(g => g.Name, NameComparer).ThenBy(g => g.AppId).ToList();
        var categories = new List<ListCategory>();

        foreach (var scope in new[] { ListScope.All, ListScope.Visible, ListScope.Limited })
        {
            var group = scope switch { ListScope.Visible => Strings.GroupVisible, ListScope.Limited => Strings.GroupLimited, _ => Strings.GroupAll };

            foreach (var kind in new[] { ListKind.All, ListKind.Done, ListKind.NotAnymore, ListKind.Started, ListKind.CantFinish })
            {
                categories.Add(new ListCategory(scope, kind, Title(scope, kind), Description(kind), group)
                {
                    Games = sorted.Where(g => InScope(g, scope) && InKind(g, kind)).ToList(),
                });
            }
        }

        // Games with completed base game sets (blue medal) or DLC sets (purple) - at the top of "Sets".
        categories.Add(new ListCategory(ListScope.All, ListKind.SetsBase, Strings.SetsBaseCategory, Strings.SetsBaseDesc, Strings.SetsTitle)
        {
            Games = sorted.Where(g => g.Sets?.CompletedBaseSets > 0).ToList(),
        });
        categories.Add(new ListCategory(ListScope.All, ListKind.SetsDlc, Strings.SetsDlcCategory, Strings.SetsDlcDesc, Strings.SetsTitle)
        {
            Games = sorted.Where(g => g.Sets?.CompletedDlcSets > 0).ToList(),
        });

        // Games with achievement sets by their state - deliberately not split by visibility.
        foreach (var (kind, state, title, description) in new[]
                 {
                     (ListKind.SetsCompleted, ProgressState.Completed, Strings.ProgressCompleted, Strings.CategorySetsCompletedDesc),
                     (ListKind.SetsInProgress, ProgressState.InProgress, Strings.ProgressInProgress, Strings.CategorySetsInProgressDesc),
                     (ListKind.SetsNotStarted, ProgressState.NotStarted, Strings.ProgressNotStarted, Strings.CategorySetsNotStartedDesc),
                 })
        {
            categories.Add(new ListCategory(ListScope.All, kind, title, description, Strings.SetsTitle)
            {
                Games = sorted.Where(g => g.Sets?.State == state).ToList(),
            });
        }

        // Untouched games, plus split by profile visibility.
        foreach (var scope in new[] { ListScope.All, ListScope.Visible, ListScope.Limited })
        {
            categories.Add(new ListCategory(scope, ListKind.Untouched, Eyes(scope) + Strings.CategoryUntouched, Strings.CategoryUntouchedDesc, Strings.GroupOther)
            {
                Games = sorted.Where(g => InScope(g, scope) && g.Kind == ListKind.Untouched).ToList(),
            });
        }

        return categories;
    }

    /// <summary>Names of the Steam collections without medal and number, e.g. "All Done" or "👀✖ Can't Finish".</summary>
    public static string Title(ListScope scope, ListKind kind)
    {
        var name = kind switch
        {
            ListKind.All => "All",
            ListKind.Done => scope == ListScope.All ? "All Done" : "Done",
            ListKind.NotAnymore => "Not 100 % Anymore",
            ListKind.Started => scope == ListScope.All ? "All Started" : "Started",
            ListKind.CantFinish => scope == ListScope.All ? "All Can't Finish" : "Can't Finish",
            _ => "",
        };
        return Eyes(scope) + name;
    }

    /// <summary>"👀✔ " or "👀✖ " before the name of a category with visibility.</summary>
    public static string Eyes(ListScope scope) => scope switch { ListScope.Visible => "👀✔ ", ListScope.Limited => "👀✖ ", _ => "" };

    private static string Description(ListKind kind) => kind switch
    {
        ListKind.Done => Strings.CategoryDoneDesc,
        ListKind.NotAnymore => Strings.CategoryNotAnymoreDesc,
        ListKind.Started => Strings.CategoryStartedDesc,
        ListKind.CantFinish => Strings.CategoryCantFinishDesc,
        _ => Strings.CategoryAllDesc,
    };

    private static bool InScope(ListGame game, ListScope scope) => scope switch
    {
        ListScope.Visible => game.Vetted == true,
        ListScope.Limited => game.Vetted == false,
        _ => true,
    };

    private static bool InKind(ListGame game, ListKind kind)
        => kind == ListKind.All ? game.Kind != ListKind.Untouched : game.Kind == kind;
}
