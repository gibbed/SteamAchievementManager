using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace SAM.App.Helpers;

/// <summary>Which medal a game or achievement set gets.</summary>
public enum MedalKind
{
    None,

    /// <summary>Not completed yet.</summary>
    Unfinished,

    /// <summary>One base game set completed (blue).</summary>
    Set,

    /// <summary>Several base game sets completed (blue double medal, like on Steam).</summary>
    SetDouble,

    /// <summary>DLC sets completed - always the purple double medal on Steam.</summary>
    DlcSet,

    /// <summary>All achievable achievements of the game (large blue medal).</summary>
    Game,
}

/// <summary>
/// The medals from SAM.App/Assets/Medals (generated with tools/make-medals.py from our own SVGs).
/// One shared image source per kind - it belongs to the UI thread that first requested it.
/// </summary>
internal static class Medals
{
    private static readonly Dictionary<MedalKind, ImageSource> Cache = [];

    public static ImageSource? Get(MedalKind kind)
    {
        if (kind == MedalKind.None)
        {
            return null;
        }

        if (!Cache.TryGetValue(kind, out var source))
        {
            var file = kind switch
            {
                MedalKind.Game => "game-complete.svg",
                MedalKind.Set => "set-complete.svg",
                MedalKind.SetDouble => "set-double-complete.svg",
                MedalKind.DlcSet => "dlc-set-complete.svg",
                _ => "unfinished.svg",
            };
            source = new SvgImageSource(new Uri("ms-appx:///Assets/Medals/" + file));
            Cache[kind] = source;
        }

        return source;
    }
}
