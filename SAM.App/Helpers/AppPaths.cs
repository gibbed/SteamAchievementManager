namespace SAM.App.Helpers;

internal static class AppPaths
{
    /// <summary>%LOCALAPPDATA%\SteamAchievementManager - game list and image cache.</summary>
    public static string Data { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "SteamAchievementManager");

    public static string GameListCache => Path.Combine(Data, "games.xml");

    public static string ImageCache => Path.Combine(Data, "Cache", "Images");
}
