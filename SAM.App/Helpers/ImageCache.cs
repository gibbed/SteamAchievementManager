using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Microsoft.UI.Xaml.Media.Imaging;

namespace SAM.App.Helpers;

/// <summary>
/// Downloads game images and achievement icons from Steam's CDN and keeps them on disk.
/// Replaces the download queues of SAM.Picker and SAM.Game.
/// </summary>
internal static class ImageCache
{
    private static readonly HttpClient Http = CreateClient();
    private static readonly SemaphoreSlim Throttle = new(6);
    private static readonly ConcurrentDictionary<string, Task<string?>> Files = new();

    private static HttpClient CreateClient()
    {
        var client = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
        client.DefaultRequestHeaders.UserAgent.ParseAdd("SteamAchievementManager-WinUI/1.0");
        return client;
    }

    /// <summary>
    /// Load a game icon: first the file from Steam's library cache, otherwise from the CDN
    /// (Steam may be replacing the file right now, or it may be damaged).
    /// </summary>
    public static async Task<BitmapImage?> LoadIconAsync(string? path, string? url)
    {
        if (path is not null)
        {
            try
            {
                var bitmap = new BitmapImage();
                await using var stream = File.OpenRead(path);
                await bitmap.SetSourceAsync(stream.AsRandomAccessStream());
                return bitmap;
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException or System.Runtime.InteropServices.COMException)
            {
                // Then from the CDN.
            }
        }

        return await LoadAsync(url);
    }

    /// <summary>Load an image; null if it does not exist or could not be loaded.</summary>
    /// <param name="decodePixelWidth">Only for large images: decode scaled down to this width (physical pixels).</param>
    public static async Task<BitmapImage?> LoadAsync(string? url, int? decodePixelWidth = null)
    {
        if (string.IsNullOrEmpty(url))
        {
            return null;
        }

        var path = await Files.GetOrAdd(url, DownloadAsync);

        if (path is null)
        {
            return null;
        }

        try
        {
            var bitmap = new BitmapImage();

            if (decodePixelWidth is int width)
            {
                bitmap.DecodePixelType = DecodePixelType.Physical;
                bitmap.DecodePixelWidth = width;
            }

            await using var stream = File.OpenRead(path);
            await bitmap.SetSourceAsync(stream.AsRandomAccessStream());
            return bitmap;
        }
        catch (Exception)
        {
            // Damaged file: download it again next time.
            Files.TryRemove(url, out _);
            TryDelete(path);
            return null;
        }
    }

    private static async Task<string?> DownloadAsync(string url)
    {
        var extension = Path.GetExtension(new Uri(url).AbsolutePath);
        var name = Convert.ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes(url))) + (extension.Length is > 1 and <= 5 ? extension : ".img");
        var path = Path.Combine(AppPaths.ImageCache, name);

        if (File.Exists(path))
        {
            File.SetLastAccessTimeUtc(path, DateTime.UtcNow);
            return path;
        }

        await Throttle.WaitAsync().ConfigureAwait(false);

        try
        {
            using var response = await Http.GetAsync(url).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            Directory.CreateDirectory(AppPaths.ImageCache);
            var temp = path + "." + Environment.ProcessId + ".tmp";
            await File.WriteAllBytesAsync(temp, bytes).ConfigureAwait(false);
            File.Move(temp, path, overwrite: true);
            return path;
        }
        catch (Exception)
        {
            return null;
        }
        finally
        {
            Throttle.Release();
        }
    }

    /// <summary>Remove images that have not been used for 60 days.</summary>
    public static void TrimInBackground()
    {
        _ = Task.Run(() =>
        {
            try
            {
                if (!Directory.Exists(AppPaths.ImageCache))
                {
                    return;
                }

                var limit = DateTime.UtcNow.AddDays(-60);

                foreach (var file in new DirectoryInfo(AppPaths.ImageCache).EnumerateFiles())
                {
                    if (file.LastAccessTimeUtc < limit || file.Name.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase) && file.LastWriteTimeUtc < DateTime.UtcNow.AddHours(-1))
                    {
                        TryDelete(file.FullName);
                    }
                }
            }
            catch (Exception)
            {
                // Cleaning up is only cosmetic.
            }
        });
    }

    private static void TryDelete(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (Exception)
        {
        }
    }
}
