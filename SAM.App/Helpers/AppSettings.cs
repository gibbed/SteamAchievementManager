using System.Text.Json;
using System.Text.Json.Nodes;

namespace SAM.App.Helpers;

/// <summary>
/// Settings in %LOCALAPPDATA%\SteamAchievementManager\settings.json (language, collapsed sections, views, columns).
/// Each key is written on its own, the others are kept.
/// </summary>
internal static class AppSettings
{
    private static string FilePath => Path.Combine(AppPaths.Data, "settings.json");

    public static string? ReadString(string key)
    {
        try
        {
            return Read()?[key] is JsonValue value && value.TryGetValue<string>(out var text) && text.Length > 0 ? text : null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static List<string> ReadStrings(string key) => ReadStringsOrNull(key) ?? [];

    /// <summary>Like <see cref="ReadStrings"/>, but null as long as the key is missing - an empty list is then a choice.</summary>
    public static List<string>? ReadStringsOrNull(string key)
    {
        var result = new List<string>();

        try
        {
            if (Read()?[key] is not JsonArray array)
            {
                return null;
            }

            foreach (var item in array)
            {
                if (item is JsonValue value && value.TryGetValue<string>(out var text))
                {
                    result.Add(text);
                }
            }
        }
        catch (Exception)
        {
            // Unreadable file: defaults.
        }

        return result;
    }

    public static void Write(string key, JsonNode? value)
    {
        try
        {
            Directory.CreateDirectory(AppPaths.Data);
            var root = Read() ?? [];
            root[key] = value;
            File.WriteAllText(FilePath, root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception)
        {
            // Without write access the setting only lasts until the app closes.
        }
    }

    private static JsonObject? Read()
    {
        try
        {
            return File.Exists(FilePath) ? JsonNode.Parse(File.ReadAllText(FilePath)) as JsonObject : null;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
