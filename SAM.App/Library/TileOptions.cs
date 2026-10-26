using SAM.App.Helpers;

namespace SAM.App.Library;

/// <summary>
/// What a library tile shows (gear next to "Tiles | List"), saved in settings.json
/// (<c>tileHiddenParts</c>). With nothing checked only the cover remains.
/// </summary>
internal static class TileOptions
{
    public const string Name = "Name";
    public const string AppId = "AppId";
    public const string Achievements = "Achievements";
    public const string Sets = "Sets";
    public const string Medal = "Medal";
    public const string Visibility = "Visibility";

    private const string SettingsKey = "tileHiddenParts";
    private static readonly string[] All = [Name, AppId, Achievements, Sets, Medal, Visibility];
    /// <summary>On first start only cover and name.</summary>
    private static readonly string[] HiddenByDefault = [AppId, Achievements, Sets, Medal, Visibility];

    private static HashSet<string> _hidden = Load();

    public static bool Shows(string part) => !_hidden.Contains(part);

    /// <summary>Second line under the name: App ID, achievements, sets, visibility.</summary>
    public static bool HasInfoRow => Shows(AppId) || Shows(Achievements) || Shows(Sets) || Shows(Visibility);

    public static bool HasCaption => Shows(Name) || HasInfoRow || Shows(Medal);

    /// <summary>Height of the caption under the image - for tiles of equal height.</summary>
    public static double CaptionHeight => !HasCaption ? 0 : Shows(Name) && HasInfoRow ? 58 : 40;

    public static void Apply(IEnumerable<string> hidden)
    {
        _hidden = hidden.Where(All.Contains).ToHashSet();
        AppSettings.Write(SettingsKey, new System.Text.Json.Nodes.JsonArray([.. _hidden.Order().Select(p => System.Text.Json.Nodes.JsonValue.Create(p))]));
    }

    public static void Reset() => Apply(HiddenByDefault);

    private static HashSet<string> Load()
    {
        var stored = AppSettings.ReadStringsOrNull(SettingsKey);
        return stored is null ? [.. HiddenByDefault] : stored.Where(All.Contains).ToHashSet();
    }
}
