using Microsoft.UI.Xaml.Media.Imaging;

namespace SAM.App.Helpers;

/// <summary>
/// Animation of a medal as an image strip (from tools/make-medal-frames.py).
/// </summary>
/// <param name="Sheet">Image strip with all distinct frames side by side.</param>
/// <param name="FrameWidth">Width of one frame in pixels.</param>
/// <param name="FrameHeight">Height of one frame in pixels.</param>
/// <param name="FrameCount">Number of frames in the strip.</param>
/// <param name="DurationMs">Length of one cycle; afterwards it starts over.</param>
/// <param name="Times">From when (ms) a frame applies ...</param>
/// <param name="Frames">... and which one.</param>
/// <param name="FrameBox">viewBox of the original (with a margin for the sparks) - this is what one frame shows.</param>
/// <param name="StaticBox">viewBox of the static version - the size of the medal follows it.</param>
internal sealed record MedalAnimation(
    string Sheet, int FrameWidth, int FrameHeight, int FrameCount, int DurationMs,
    double[] Times, double[] Frames, double[] FrameBox, double[] StaticBox);

/// <summary>The animated medals - shine and sparkle like on Steam's achievement pages.</summary>
internal static partial class MedalAnimations
{
    private static readonly Dictionary<MedalKind, BitmapImage> Sheets = [];

    /// <summary>Follows the Windows setting "Animation effects".</summary>
    public static bool Enabled { get; } = new Windows.UI.ViewManagement.UISettings().AnimationsEnabled;

    public static MedalAnimation? For(MedalKind kind) => Enabled && Data.TryGetValue(kind, out var animation) ? animation : null;

    /// <summary>One image strip per kind, shared by all medals - belongs to the UI thread.</summary>
    public static BitmapImage Sheet(MedalKind kind, MedalAnimation animation)
    {
        if (!Sheets.TryGetValue(kind, out var sheet))
        {
            sheet = new BitmapImage(new Uri(animation.Sheet));
            Sheets[kind] = sheet;
        }

        return sheet;
    }
}
