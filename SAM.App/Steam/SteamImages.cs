using System.Globalization;

namespace SAM.App.Steam;

/// <summary>Image addresses from Steam's app data (successor of GetGameImageUrl in SAM.Picker).</summary>
internal static class SteamImages
{
    // The host shared.cloudflare.steamstatic.com used by the original now redirects here.
    private const string StoreAssets = "https://shared.steamstatic.com/store_item_assets/steam/apps/";
    private const string CommunityImages = "https://cdn.steamstatic.com/steamcommunity/public/images/apps/";

    /// <summary>Landscape capsule (460x215) for the library tiles.</summary>
    public static string? Header(API.Client client, uint appId, string language)
    {
        return Store(client, appId, language, "header_image")
            ?? Store(client, appId, language, "small_capsule")
            ?? Logo(client, appId);
    }

    /// <summary>Wide background image (3840x1240) for the header of the game window.</summary>
    public static string? Hero(API.Client client, uint appId, string language)
    {
        return Store(client, appId, language, "library_assets_full/library_hero/image")
            ?? Store(client, appId, language, "header_image");
    }

    public static string StoreAsset(uint appId, string file)
        => StoreAssets + appId.ToString(CultureInfo.InvariantCulture) + "/" + file;

    public static string AchievementIcon(uint appId, string icon)
        => CommunityImages + appId.ToString(CultureInfo.InvariantCulture) + "/" + icon;

    private static string? Store(API.Client client, uint appId, string language, string key)
    {
        var candidate = client.SteamApps001.GetAppData(appId, key + "/" + language);

        if (string.IsNullOrEmpty(candidate) && language != "english")
        {
            candidate = client.SteamApps001.GetAppData(appId, key + "/english");
        }

        return string.IsNullOrEmpty(candidate)
            ? null
            : StoreAssets + appId.ToString(CultureInfo.InvariantCulture) + "/" + candidate;
    }

    private static string? Logo(API.Client client, uint appId)
    {
        var candidate = client.SteamApps001.GetAppData(appId, "logo");

        return string.IsNullOrEmpty(candidate)
            ? null
            : CommunityImages + appId.ToString(CultureInfo.InvariantCulture) + "/" + candidate + ".jpg";
    }
}
