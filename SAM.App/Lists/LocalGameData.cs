using System.Globalization;
using System.Text.RegularExpressions;
using SAM.App.Schema;
using SAM.Game;
using SAM.Game.Stats;

namespace SAM.App.Lists;

/// <summary>First and last unlock of a game (Unix time) and how many achievements have times.</summary>
internal readonly record struct UnlockTimes(long First, long Last, int Count);

/// <summary>Steam's stats file of a game: unlock times and, per achievement stat, the bit field (cache/&lt;stat&gt;/data).</summary>
internal sealed record LocalStats(UnlockTimes? Times, IReadOnlyDictionary<int, uint> Bits)
{
    public bool IsUnlocked(AchievementDefinition achievement)
        => Bits.TryGetValue(achievement.StatId, out var data) && achievement.Bit is >= 0 and < 32 && (data >> achievement.Bit & 1) != 0;
}

/// <summary>
/// What Steam stores per game on this PC, without the game having to be started:
/// appcache/stats/UserGameStats_&lt;account&gt;_&lt;App ID&gt;.bin with the unlock times
/// (cache/&lt;stat&gt;/AchievementTimes/&lt;bit&gt;) and appcache/librarycache/&lt;App ID&gt;/&lt;hash&gt;.jpg as the icon.
/// The stats file only exists for games whose achievements Steam has already loaded on this PC.
/// </summary>
internal static partial class LocalGameData
{
    [GeneratedRegex("^[0-9a-f]{40}\\.jpg$", RegexOptions.IgnoreCase)]
    private static partial Regex IconFileName();

    public static UnlockTimes? ReadUnlockTimes(string steamPath, uint accountId, uint appId)
        => ReadStats(steamPath, accountId, appId)?.Times;

    public static LocalStats? ReadStats(string steamPath, uint accountId, uint appId)
    {
        var path = Path.Combine(steamPath, "appcache", "stats",
            "UserGameStats_" + accountId.ToString(CultureInfo.InvariantCulture) + "_" + appId.ToString(CultureInfo.InvariantCulture) + ".bin");

        if (KeyValue.LoadAsBinary(path)?["cache"].Children is not { } stats)
        {
            return null;
        }

        long first = long.MaxValue, last = 0;
        var count = 0;
        var bits = new Dictionary<int, uint>();

        foreach (var stat in stats)
        {
            if (int.TryParse(stat.Name, NumberStyles.None, CultureInfo.InvariantCulture, out var statId) && stat["data"].Valid)
            {
                bits[statId] = (uint)stat["data"].AsInteger(0);
            }

            if (stat["AchievementTimes"].Children is not { } times)
            {
                continue;
            }

            foreach (var time in times)
            {
                // Unix time stored as Int32; read as unsigned so that it is still correct after 2038.
                long value = (uint)time.AsInteger(0);

                if (value == 0)
                {
                    continue;
                }

                first = Math.Min(first, value);
                last = Math.Max(last, value);
                count++;
            }
        }

        return new LocalStats(count == 0 ? null : new UnlockTimes(first, last, count), bits);
    }

    /// <summary>The small game icon the Steam library caches itself.</summary>
    public static string? FindIcon(string steamPath, uint appId)
    {
        var folder = Path.Combine(steamPath, "appcache", "librarycache", appId.ToString(CultureInfo.InvariantCulture));

        try
        {
            return Directory.Exists(folder)
                ? Directory.EnumerateFiles(folder, "*.jpg").FirstOrDefault(f => IconFileName().IsMatch(Path.GetFileName(f)))
                : null;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            return null;
        }
    }

    /// <summary>The same icon from Steam's CDN in case it is missing locally (hash from the app data).</summary>
    public static string? IconUrl(uint appId, string? hash)
        => string.IsNullOrWhiteSpace(hash) || !IconFileName().IsMatch(hash.Trim() + ".jpg")
            ? null
            : "https://cdn.steamstatic.com/steamcommunity/public/images/apps/" + appId.ToString(CultureInfo.InvariantCulture) + "/" + hash.Trim() + ".jpg";

    /// <summary>Number of achievements according to the schema Steam stored on this PC; 0 if it is missing.</summary>
    public static int CountAchievements(uint appId) => SchemaLoader.Load(appId, "english")?.Achievements.Count ?? 0;
}
