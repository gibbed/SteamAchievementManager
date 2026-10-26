using System.Globalization;
using System.Text.Json;

namespace SAM.App.Lists;

/// <summary>A game's progress according to Steam's library cache.</summary>
internal readonly record struct CachedProgress(uint AppId, int Unlocked, int Total, bool AllUnlocked, bool? Vetted, long CacheTime);

/// <summary>
/// Reads userdata/&lt;account&gt;/config/librarycache/achievement_progress.json - the same numbers the
/// Steam library shows. "vetted" is false for games with "Profile Features Limited".
/// This way all games can be evaluated without launching any of them.
/// </summary>
internal static class AchievementCache
{
    public static string PathFor(string steamInstallPath, uint accountId)
        => Path.Combine(steamInstallPath, "userdata", accountId.ToString(CultureInfo.InvariantCulture), "config", "librarycache", "achievement_progress.json");

    public static List<CachedProgress> Read(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        using var document = JsonDocument.Parse(stream);
        var result = new List<CachedProgress>();

        if (!document.RootElement.TryGetProperty("mapCache", out var map) || map.ValueKind != JsonValueKind.Array)
        {
            return result;
        }

        foreach (var pair in map.EnumerateArray())
        {
            if (pair.ValueKind != JsonValueKind.Array || pair.GetArrayLength() < 2)
            {
                continue;
            }

            var entry = pair[1];

            if (entry.ValueKind != JsonValueKind.Object || !entry.TryGetProperty("appid", out var appId) || !appId.TryGetUInt32(out var id))
            {
                continue;
            }

            result.Add(new CachedProgress(
                id,
                GetInt(entry, "unlocked"),
                GetInt(entry, "total"),
                entry.TryGetProperty("all_unlocked", out var all) && all.ValueKind == JsonValueKind.True,
                entry.TryGetProperty("vetted", out var vetted) && vetted.ValueKind is JsonValueKind.True or JsonValueKind.False ? vetted.GetBoolean() : null,
                entry.TryGetProperty("cache_time", out var time) && time.TryGetInt64(out var t) ? t : 0));
        }

        return result;
    }

    private static int GetInt(JsonElement entry, string name)
        => entry.TryGetProperty(name, out var value) && value.TryGetInt32(out var number) ? number : 0;
}
