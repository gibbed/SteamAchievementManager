using System.Xml.XPath;
using SAM.App.Helpers;

namespace SAM.App.Library;

internal sealed record GameListResult(IReadOnlyList<(uint Id, string Type)> Games, bool FromCache, Exception? Error);

/// <summary>The list of known games with achievements (as in SAM.Picker), with an offline copy as fallback.</summary>
internal static class GameListSource
{
    private const string Url = "https://gib.me/sam/games.xml";

    public static async Task<GameListResult> LoadAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            var bytes = await http.GetByteArrayAsync(Url, cancellationToken);
            var games = Parse(bytes);

            try
            {
                Directory.CreateDirectory(AppPaths.Data);
                await File.WriteAllBytesAsync(AppPaths.GameListCache, bytes, CancellationToken.None);
            }
            catch (IOException)
            {
                // Carry on even without an offline copy.
            }

            return new GameListResult(games, false, null);
        }
        catch (Exception e) when (e is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            if (File.Exists(AppPaths.GameListCache))
            {
                try
                {
                    return new GameListResult(Parse(await File.ReadAllBytesAsync(AppPaths.GameListCache, cancellationToken)), true, e);
                }
                catch (Exception) when (!cancellationToken.IsCancellationRequested)
                {
                }
            }

            // Like AddDefaultGames in the original: offer at least Spacewar.
            return new GameListResult([(480u, "normal")], false, e);
        }
    }

    private static List<(uint, string)> Parse(byte[] bytes)
    {
        List<(uint, string)> pairs = [];
        using MemoryStream stream = new(bytes, false);
        XPathDocument document = new(stream);
        var navigator = document.CreateNavigator();
        var nodes = navigator.Select("/games/game");

        while (nodes.MoveNext())
        {
            var type = nodes.Current!.GetAttribute("type", "");

            if (string.IsNullOrEmpty(type))
            {
                type = "normal";
            }

            pairs.Add(((uint)nodes.Current.ValueAsLong, type));
        }

        return pairs;
    }
}
