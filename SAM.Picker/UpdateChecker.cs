/* Copyright (c) 2024 Rick (rick 'at' gibbed 'dot' us)
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace SAM.Picker
{
    /// <summary>
    /// A published release that is newer than the running build.
    /// </summary>
    internal sealed class UpdateInfo
    {
        /// <summary>Raw tag of the release (for example "7.1.0").</summary>
        public string Tag;

        /// <summary>Version parsed from <see cref="Tag"/>, when it could be parsed.</summary>
        public Version Version;

        /// <summary>Download URL of the archive to install.</summary>
        public string ZipUrl;

        /// <summary>Download URL of the SHA-256 of the archive, when published.</summary>
        public string ChecksumUrl;

        /// <summary>Release notes, when the publisher provided any.</summary>
        public string ReleaseNotes;
    }

    /// <summary>
    /// Outcome of an update check: either a newer release, an error, or nothing
    /// newer to install (both fields null).
    /// </summary>
    internal sealed class UpdateCheckResult
    {
        public UpdateInfo Update;
        public string Error;
    }

    /// <summary>
    /// Checks a GitHub repository for a newer release than this build, downloads
    /// it, verifies it and hands the actual file replacement over to a helper
    /// process (SAM cannot replace the files it is executing).
    /// </summary>
    internal static class UpdateChecker
    {
        // Releases published here are the ones this build will offer.
        // Point this at your own fork when cutting your own release.
        internal const string ReleaseRepository = "gibbed/SteamAchievementManager";

        private const string UserAgent = "SteamAchievementManager-Updater";

        internal static Version CurrentVersion { get; } = ReadCurrentVersion();

        private static Version ReadCurrentVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            return version == null ? new Version(0, 0, 0, 0) : Normalize(version);
        }

        private static Version Normalize(Version version)
        {
            // Version("7.1") reports -1 for the missing components, which would
            // compare strangely against the four component assembly version.
            return new Version(
                version.Major,
                version.Minor < 0 ? 0 : version.Minor,
                version.Build < 0 ? 0 : version.Build,
                version.Revision < 0 ? 0 : version.Revision);
        }

        /// <summary>
        /// Queries the latest release. Never throws; failures are reported as
        /// <see cref="UpdateCheckResult.Error"/> so callers can stay quiet when
        /// the check was not explicitly requested.
        /// </summary>
        internal static UpdateCheckResult Check()
        {
            try
            {
                string json;
                using (var downloader = GamePicker.CreateDownloader())
                {
                    // GitHub refuses requests without a User-Agent.
                    downloader.Headers[HttpRequestHeader.UserAgent] = UserAgent;
                    downloader.Headers[HttpRequestHeader.Accept] = "application/vnd.github+json";
                    json = downloader.DownloadString(
                        $"https://api.github.com/repos/{ReleaseRepository}/releases/latest");
                }

                var info = ParseRelease(json);
                if (info == null)
                {
                    return new UpdateCheckResult()
                    {
                        Error = "The update service returned an unexpected response.",
                    };
                }

                if (info.Version == null)
                {
                    return new UpdateCheckResult()
                    {
                        Error = $"The latest release tag ({info.Tag}) does not look like a version number.",
                    };
                }

                if (info.ZipUrl == null)
                {
                    return new UpdateCheckResult()
                    {
                        Error = $"The latest release ({info.Tag}) does not contain a downloadable archive.",
                    };
                }

                if (info.Version > CurrentVersion)
                {
                    return new UpdateCheckResult() { Update = info };
                }

                return new UpdateCheckResult();
            }
            catch (Exception e)
            {
                return new UpdateCheckResult() { Error = e.Message };
            }
        }

        internal static UpdateInfo ParseRelease(string json)
        {
            var serializer = new JavaScriptSerializer();
            var payload = serializer.Deserialize<Dictionary<string, object>>(json);
            if (payload == null)
            {
                return null;
            }

            if (payload.TryGetValue("tag_name", out var tagValue) == false)
            {
                return null;
            }

            var tag = tagValue as string;
            if (string.IsNullOrEmpty(tag) == true)
            {
                return null;
            }

            var info = new UpdateInfo()
            {
                Tag = tag,
                Version = ParseVersion(tag),
                ReleaseNotes = payload.TryGetValue("body", out var notesValue) == true
                    ? notesValue as string
                    : null,
            };

            if (payload.TryGetValue("assets", out var assetsValue) == true &&
                assetsValue is ArrayList assets)
            {
                foreach (var assetValue in assets)
                {
                    if (assetValue is Dictionary<string, object> asset == false)
                    {
                        continue;
                    }

                    var name = asset.TryGetValue("name", out var nameValue) == true
                        ? nameValue as string
                        : null;
                    var url = asset.TryGetValue("browser_download_url", out var urlValue) == true
                        ? urlValue as string
                        : null;
                    if (string.IsNullOrEmpty(name) == true || string.IsNullOrEmpty(url) == true)
                    {
                        continue;
                    }

                    if (name.EndsWith(".zip.sha256", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        info.ChecksumUrl ??= url;
                    }
                    else if (name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        info.ZipUrl ??= url;
                    }
                }
            }

            return info;
        }

        internal static Version ParseVersion(string tag)
        {
            var text = tag.Trim();
            if (text.StartsWith("v", StringComparison.OrdinalIgnoreCase) == true)
            {
                text = text.Substring(1);
            }

            var match = Regex.Match(text, @"\d+(\.\d+)*");
            if (match.Success == false)
            {
                return null;
            }

            try
            {
                return Normalize(Version.Parse(match.Value));
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        internal static string ExtractChecksum(string text)
        {
            var match = Regex.Match(text ?? "", @"\b[0-9a-fA-F]{64}\b");
            return match.Success == true ? match.Value : null;
        }

        private static string ComputeSha256(string path)
        {
            using var stream = File.OpenRead(path);
            using var sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "");
        }

        /// <summary>
        /// Downloads the archive (and its checksum), verifies it and starts the
        /// helper process that waits for SAM to exit before replacing files.
        /// Throws on any problem, leaving the installation untouched.
        /// </summary>
        /// <param name="info">Release to install.</param>
        /// <param name="report">Called with progress messages (any thread).</param>
        internal static void DownloadAndApply(UpdateInfo info, Action<string> report)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            var folder = Path.Combine(Path.GetTempPath(), "sam-update-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(folder);

            var zipPath = Path.Combine(folder, "update.zip");
            try
            {
                report?.Invoke($"Downloading version {info.Tag}...");
                string checksum;
                using (var downloader = GamePicker.CreateDownloader())
                {
                    downloader.Headers[HttpRequestHeader.UserAgent] = UserAgent;
                    downloader.DownloadProgressChanged += (sender, e) =>
                        report?.Invoke($"Downloading update... {e.ProgressPercentage}% ({e.BytesReceived / 1024} KiB)");
                    downloader.DownloadFileTaskAsync(new Uri(info.ZipUrl), zipPath)
                        .GetAwaiter()
                        .GetResult();

                    if (string.IsNullOrEmpty(info.ChecksumUrl) == false)
                    {
                        // The checksum is published next to the archive: refuse to
                        // install anything whose integrity cannot be checked.
                        var text = downloader.DownloadString(info.ChecksumUrl);
                        checksum = ExtractChecksum(text);
                        if (string.IsNullOrEmpty(checksum) == true)
                        {
                            throw new InvalidDataException(
                                "The published checksum is missing or malformed; the update was not applied.");
                        }
                    }
                    else
                    {
                        checksum = null;
                    }
                }

                if (string.IsNullOrEmpty(checksum) == false)
                {
                    var actual = ComputeSha256(zipPath);
                    if (string.Equals(actual, checksum, StringComparison.OrdinalIgnoreCase) == false)
                    {
                        throw new InvalidDataException(
                            $"Checksum mismatch ({actual} != {checksum}): the download was corrupted " +
                            "and the update was not applied.");
                    }
                }

                // Cheap structural check so a truncated or HTML-failure download
                // never reaches the helper process.
                using (var archive = ZipFile.OpenRead(zipPath))
                {
                    if (archive.Entries.Count == 0)
                    {
                        throw new InvalidDataException("The downloaded archive is empty.");
                    }
                }

                report?.Invoke("Installing update...");
                StartUpdater(folder, zipPath, checksum);
            }
            catch
            {
                TryDelete(folder);
                throw;
            }
        }

        private static void StartUpdater(string folder, string zipPath, string checksum)
        {
            var scriptPath = Path.Combine(folder, "apply-update.ps1");
            File.WriteAllText(scriptPath, UpdaterScript, new UTF8Encoding(false));

            var target = Application.StartupPath;
            var executable = Path.GetFileName(Application.ExecutablePath);

            var arguments =
                "-NoProfile -ExecutionPolicy Bypass -WindowStyle Hidden " +
                $"-File \"{scriptPath}\" " +
                $"-Zip \"{zipPath}\" " +
                $"-Target \"{target}\" " +
                $"-Executable \"{executable}\" " +
                $"-Hash \"{checksum}\"";

            Process.Start(new ProcessStartInfo()
            {
                FileName = "powershell.exe",
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = target,
            });
        }

        private static void TryDelete(string folder)
        {
            try
            {
                if (Directory.Exists(folder) == true)
                {
                    Directory.Delete(folder, true);
                }
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        // ASCII only: PowerShell reads scripts as-is, without a byte order mark.
        private const string UpdaterScript = @"
param(
    [Parameter(Mandatory = $true)][string]$Zip,
    [Parameter(Mandatory = $true)][string]$Target,
    [Parameter(Mandatory = $true)][string]$Executable,
    [string]$Hash
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.IO.Compression.FileSystem

function Fail([string]$Message) {
    [System.Windows.Forms.MessageBox]::Show(
        $Message,
        'Steam Achievement Manager - update failed',
        [System.Windows.Forms.MessageBoxButtons]::OK,
        [System.Windows.Forms.MessageBoxIcon]::Error) | Out-Null
    exit 1
}

# The launcher starts us and quits right away; SAM.Game may also be open.
$deadline = (Get-Date).AddMinutes(3)
while ($true) {
    $running = @(Get-Process -Name 'SAM.Picker','SAM.Game' -ErrorAction SilentlyContinue)
    if ($running.Count -eq 0) { break }
    if ((Get-Date) -gt $deadline) {
        Fail 'SAM is still running, so the update was not applied. Close it and try again.'
    }
    Start-Sleep -Milliseconds 500
}

# Verify the payload before touching a single installed file.
if ($Hash) {
    $stream = [System.IO.File]::OpenRead($Zip)
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try {
        $actual = ([System.BitConverter]::ToString($sha.ComputeHash($stream))).Replace('-', '')
    } finally {
        $stream.Dispose()
        $sha.Dispose()
    }
    if ($actual -ne $Hash.ToUpperInvariant()) {
        Fail ('Checksum mismatch for the downloaded archive (got ' + $actual +
              ', expected ' + $Hash + '). The installed files were left untouched.')
    }
}

# Extract over the application directory, and never outside of it.
try {
    $root = [System.IO.Path]::GetFullPath($Target) + [System.IO.Path]::DirectorySeparatorChar
    $archive = [System.IO.Compression.ZipFile]::OpenRead($Zip)
    try {
        foreach ($entry in $archive.Entries) {
            $name = $entry.FullName -replace '/', '\'
            $destination = [System.IO.Path]::GetFullPath((Join-Path $Target $name))
            if (-not $destination.StartsWith($root, [System.StringComparison]::OrdinalIgnoreCase)) {
                Fail ('The archive contains an unexpected path: ' + $entry.FullName)
            }
            if ($entry.Name -eq '') {
                if (-not (Test-Path -LiteralPath $destination)) {
                    New-Item -ItemType Directory -Force -Path $destination | Out-Null
                }
                continue
            }
            $folder = [System.IO.Path]::GetDirectoryName($destination)
            if (-not (Test-Path -LiteralPath $folder)) {
                New-Item -ItemType Directory -Force -Path $folder | Out-Null
            }
            [System.IO.Compression.ZipFileExtensions]::ExtractToFile($entry, $destination, $true)
        }
    } finally {
        $archive.Dispose()
    }
} catch {
    Fail ('Could not update the files in ' + $Target + ': ' + $_.Exception.Message +
          ' You can apply the archive manually instead.')
}

Remove-Item -Force $Zip -ErrorAction SilentlyContinue

# Restart the tool.
$started = Join-Path $Target $Executable
if (Test-Path -LiteralPath $started) {
    Start-Process -FilePath $started
}
exit 0
";
    }
}
