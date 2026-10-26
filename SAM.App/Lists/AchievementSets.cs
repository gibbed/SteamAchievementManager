using System.Globalization;
using System.Text.Json;
using SAM.App.Helpers;
using SAM.App.Localization;
using SAM.App.Schema;
using SAM.Game.Stats;

namespace SAM.App.Lists;

/// <summary>State of a game (library) or of its achievement sets (lists).</summary>
public enum ProgressState
{
    NotStarted,
    InProgress,
    Completed,
}

/// <summary>Progress in an achievement set.</summary>
/// <param name="Accessible">Base game sets always; DLC sets only if the account owns the DLC.</param>
/// <param name="WasCompleted">Completed before - Steam keeps the completion even if achievements are added later.</param>
internal sealed record SetProgress(int Id, string Name, bool IsDlc, bool Accessible, int Unlocked, int Total, bool WasCompleted)
{
    public bool IsComplete => Total > 0 && Unlocked >= Total;

    public bool HasMedal => IsComplete || WasCompleted;

    public MedalKind Medal => !HasMedal ? MedalKind.Unfinished : IsDlc ? MedalKind.DlcSet : MedalKind.Set;

    public string ProgressText => Unlocked.ToString("N0", CultureInfo.CurrentCulture) + " / " + Total.ToString("N0", CultureInfo.CurrentCulture);

    /// <summary>One line for tooltips: "Hot Wheels (DLC)  27 / 27  ✓".</summary>
    public string Line
    {
        get
        {
            var line = Name + (IsDlc ? " (DLC)" : "") + "   " + ProgressText;

            if (!Accessible)
            {
                return line + "   – " + Strings.SetNotOwned;
            }

            return HasMedal ? line + "   ✓" + (IsComplete ? "" : " (" + Strings.SetKept + ")") : line;
        }
    }
}

/// <summary>All sets of a game and the overall progress under Steam's new rules.</summary>
/// <param name="Unlocked">Without archived achievements and without sets of DLCs the account does not own.</param>
internal sealed record SetSummary(IReadOnlyList<SetProgress> Sets, int Unlocked, int Total)
{
    public bool HasSets => Sets.Count > 0;

    public int SetCount => Sets.Count(s => s.Accessible);

    public int CompletedSets => Sets.Count(s => s.Accessible && s.HasMedal);

    public bool IsComplete => Total > 0 && Unlocked >= Total;

    /// <summary>All sets that count: completed / something unlocked / nothing yet.</summary>
    public ProgressState State => SetCount > 0 && CompletedSets == SetCount ? ProgressState.Completed
        : Sets.Any(s => s.Accessible && s.Unlocked > 0) ? ProgressState.InProgress
        : ProgressState.NotStarted;

    /// <summary>Completed base game sets - on Steam "1" with a blue medal, from two on with the blue double medal.</summary>
    public int CompletedBaseSets => Sets.Count(s => s.Accessible && !s.IsDlc && s.HasMedal);

    /// <summary>Completed DLC sets - always with the purple double medal on Steam.</summary>
    public int CompletedDlcSets => Sets.Count(s => s.Accessible && s.IsDlc && s.HasMedal);

    public MedalKind BaseSetsMedal => CompletedBaseSets >= 2 ? MedalKind.SetDouble : MedalKind.Set;

    /// <summary>Large medal for a completed game, otherwise the one for completed sets (DLC before base game).</summary>
    public MedalKind Medal => IsComplete ? MedalKind.Game
        : CompletedDlcSets > 0 ? MedalKind.DlcSet
        : CompletedBaseSets > 0 ? BaseSetsMedal
        : MedalKind.None;

    public string SetsText => Strings.SetsProgress(CompletedSets.ToString("N0", CultureInfo.CurrentCulture), SetCount.ToString("N0", CultureInfo.CurrentCulture));

    /// <summary>One set icon where only one fits: purple once a DLC set is completed, otherwise blue (double from two on), otherwise grey.</summary>
    public MedalKind SetsMedal => CompletedSets == 0 ? MedalKind.Unfinished
        : CompletedDlcSets > 0 ? MedalKind.DlcSet
        : BaseSetsMedal;

    public double SetsRatio => SetCount == 0 ? 0 : (double)CompletedSets / SetCount;

    public string Tooltip => string.Join("\n", Sets.Select(s => s.Line));
}

/// <summary>
/// Completed sets like in the header of a Steam achievement page: count and blue medal (double from two on) for the
/// base game, count and purple double medal for DLCs; the grey medal as long as none is completed.
/// </summary>
public sealed class SetBadges
{
    internal SetBadges(SetSummary sets)
    {
        var count = (int value) => value.ToString("N0", CultureInfo.CurrentCulture);
        HasBase = sets.CompletedBaseSets > 0;
        BaseCount = count(sets.CompletedBaseSets);
        BaseKind = sets.BaseSetsMedal;
        HasDlc = sets.CompletedDlcSets > 0;
        DlcCount = count(sets.CompletedDlcSets);
        HasNone = sets.CompletedSets == 0;
    }

    public bool HasBase { get; }

    public string BaseCount { get; }

    public MedalKind BaseKind { get; }

    /// <summary>Width for the base set medal: the double medal is wider.</summary>
    public double BaseWidth => BaseKind == MedalKind.SetDouble ? 16 : 10;

    public bool HasDlc { get; }

    public string DlcCount { get; }

    public MedalKind DlcKind => MedalKind.DlcSet;

    public bool HasNone { get; }

    public MedalKind NoneKind => MedalKind.Unfinished;
}

/// <summary>
/// Steam's rules for achievement sets (beta since 2026-10-16): archived achievements count neither for the set nor
/// for the game, DLC sets only if the account owns the DLC.
/// </summary>
internal static class SetEvaluator
{
    public static SetSummary Evaluate(GameSchema schema, Func<AchievementDefinition, bool> isUnlocked, Func<uint, bool> ownsDlc, IReadOnlySet<int>? completedBefore)
    {
        var achievements = schema.Achievements.Where(a => !string.IsNullOrEmpty(a.Id) && !a.IsArchived).ToList();

        if (schema.Sets.Count == 0)
        {
            return new SetSummary([], achievements.Count(isUnlocked), achievements.Count);
        }

        var sets = new List<SetProgress>();
        int unlocked = 0, total = 0;

        foreach (var set in schema.Sets)
        {
            var members = achievements.Where(a => a.SetId == set.Id).ToList();

            // Steam no longer shows a set whose achievements are all archived.
            if (members.Count == 0)
            {
                continue;
            }

            var done = members.Count(isUnlocked);
            var accessible = !set.IsDlc || ownsDlc(set.DlcAppId);
            sets.Add(new SetProgress(set.Id, set.Name ?? Strings.SetBaseGame, set.IsDlc, accessible, done, members.Count, completedBefore?.Contains(set.Id) == true));

            if (accessible)
            {
                unlocked += done;
                total += members.Count;
            }
        }

        return new SetSummary(sets, unlocked, total);
    }

    /// <summary>Number of all achievements in the schema, including archived ones - for comparison with Steam's achievement cache.</summary>
    public static int SchemaTotal(GameSchema schema) => schema.Achievements.Count(a => !string.IsNullOrEmpty(a.Id));

    /// <summary>
    /// Sets of a game from schema and stats file. UseSetTotals only if the schema has as many achievements as Steam's
    /// achievement cache reports - an outdated schema then only provides the sets, not the adjusted totals.
    /// </summary>
    public static (SetSummary? Sets, bool UseSetTotals) ForGame(uint appId, string language, LocalStats stats, int cacheTotal, Func<uint, bool> ownsDlc, IReadOnlySet<int>? completedBefore)
    {
        if (SchemaLoader.Load(appId, language) is not { } schema)
        {
            return (null, false);
        }

        var summary = Evaluate(schema, stats.IsUnlocked, ownsDlc, completedBefore);
        return (summary, cacheTotal == 0 || SchemaTotal(schema) == cacheTotal);
    }

    /// <summary>Completed under Steam's new rules, otherwise according to Steam's achievement cache.</summary>
    public static bool IsComplete(CachedProgress progress, SetSummary? sets, bool useSetTotals) => sets is not null && useSetTotals
        ? sets.IsComplete
        : progress.Total > 0 && (progress.AllUnlocked || progress.Unlocked >= progress.Total);
}

/// <summary>
/// Which games have achievement sets or archived achievements. For this the schema files (about 200 MB in total)
/// only need to be searched for "groupid" and "archived"; the result is remembered per file with its modification time.
/// </summary>
internal static class SetIndex
{
    private static readonly byte[] GroupKey = "groupid\0"u8.ToArray();
    private static readonly byte[] ArchivedKey = "archived\0"u8.ToArray();
    private static readonly object Gate = new();

    private static string IndexPath => Path.Combine(AppPaths.Data, "sets-index.json");

    /// <summary>App IDs whose schema contains sets or archived achievements. Only rereads changed files.</summary>
    public static HashSet<uint> GamesWithSetData(string steamPath)
    {
        lock (Gate)
        {
            var known = Load();
            var result = new HashSet<uint>();
            var changed = false;
            var folder = Path.Combine(steamPath, "appcache", "stats");

            if (!Directory.Exists(folder))
            {
                return result;
            }

            var seen = new HashSet<uint>();

            foreach (var file in Directory.EnumerateFiles(folder, "UserGameStatsSchema_*.bin"))
            {
                var name = Path.GetFileNameWithoutExtension(file);

                if (!uint.TryParse(name.AsSpan(name.LastIndexOf('_') + 1), NumberStyles.None, CultureInfo.InvariantCulture, out var appId))
                {
                    continue;
                }

                seen.Add(appId);
                var stamp = File.GetLastWriteTimeUtc(file).Ticks;

                if (!known.TryGetValue(appId, out var entry) || entry.Stamp != stamp)
                {
                    entry = new IndexEntry(stamp, HasSetData(file));
                    known[appId] = entry;
                    changed = true;
                }

                if (entry.HasSets)
                {
                    result.Add(appId);
                }
            }

            foreach (var stale in known.Keys.Where(id => !seen.Contains(id)).ToList())
            {
                known.Remove(stale);
                changed = true;
            }

            if (changed)
            {
                Save(known);
            }

            return result;
        }
    }

    private static bool HasSetData(string file)
    {
        try
        {
            var bytes = File.ReadAllBytes(file);
            return bytes.AsSpan().IndexOf(GroupKey) >= 0 || bytes.AsSpan().IndexOf(ArchivedKey) >= 0;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }

    private sealed record IndexEntry(long Stamp, bool HasSets);

    private static Dictionary<uint, IndexEntry> Load()
    {
        try
        {
            if (File.Exists(IndexPath) && JsonSerializer.Deserialize<Dictionary<uint, IndexEntry>>(File.ReadAllText(IndexPath)) is { } index)
            {
                return index;
            }
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or JsonException)
        {
            // Then search again.
        }

        return [];
    }

    private static void Save(Dictionary<uint, IndexEntry> index)
    {
        try
        {
            Directory.CreateDirectory(AppPaths.Data);
            File.WriteAllText(IndexPath, JsonSerializer.Serialize(index));
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            // Only a cache.
        }
    }
}
