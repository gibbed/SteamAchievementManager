using System.Diagnostics;
using System.Globalization;

namespace SAM.App.Helpers;

/// <summary>Opens a game window (own process, as in the original) or an address in the default program.</summary>
internal static class GameLauncher
{
    public static string? OpenGame(uint appId)
    {
        try
        {
            Process.Start(new ProcessStartInfo(Environment.ProcessPath!, appId.ToString(CultureInfo.InvariantCulture))
            {
                UseShellExecute = false,
                WorkingDirectory = AppContext.BaseDirectory,
            });
            return null;
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }

    public static string? OpenUri(string uri)
    {
        try
        {
            Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true });
            return null;
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }
}
