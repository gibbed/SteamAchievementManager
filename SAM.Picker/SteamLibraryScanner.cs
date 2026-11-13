/* Copyright (c) 2024 Rick (rick 'at' gibbed 'dot' us)
 * Local installed-game scanner added for LocalPCSteamAchievementManager.
 * This modified source retains the original project's license notice.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace SAM.Picker
{
    /// <summary>
    /// Discovers Steam games installed on this PC without downloading a remote app catalogue.
    /// It reads Steam's libraryfolders.vdf and each library's appmanifest_*.acf files.
    /// Account access is checked separately by GamePicker through the active Steam client.
    /// </summary>
    internal static class SteamLibraryScanner
    {
        internal sealed class InstalledGame
        {
            public uint AppId { get; }
            public string Name { get; }

            public InstalledGame(uint appId, string name)
            {
                this.AppId = appId;
                this.Name = name;
            }
        }

        public static List<InstalledGame> GetInstalledGames()
        {
            string steamRoot = FindSteamRoot();
            var libraryPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                steamRoot,
            };

            string librariesFile = Path.Combine(steamRoot, "steamapps", "libraryfolders.vdf");
            if (File.Exists(librariesFile))
            {
                var document = KeyValuesReader.Parse(File.ReadAllText(librariesFile));
                if (document.TryGetValue("libraryfolders", out var foldersValue) &&
                    foldersValue is Dictionary<string, object> folders)
                {
                    foreach (var entry in folders)
                    {
                        if (int.TryParse(entry.Key, NumberStyles.None, CultureInfo.InvariantCulture, out _) == false)
                        {
                            continue;
                        }

                        string location = entry.Value as string;
                        if (entry.Value is Dictionary<string, object> details)
                        {
                            location = GetString(details, "path");
                        }

                        if (string.IsNullOrWhiteSpace(location) == false)
                        {
                            libraryPaths.Add(NormalizePath(location));
                        }
                    }
                }
            }

            var games = new Dictionary<uint, InstalledGame>();
            foreach (string library in libraryPaths)
            {
                string steamapps = Path.Combine(library, "steamapps");
                if (Directory.Exists(steamapps) == false)
                {
                    continue; // A removable or offline Steam library may be missing.
                }

                IEnumerable<string> manifestPaths;
                try
                {
                    manifestPaths = Directory.GetFiles(steamapps, "appmanifest_*.acf");
                }
                catch (UnauthorizedAccessException)
                {
                    continue;
                }
                catch (IOException)
                {
                    continue;
                }

                foreach (string manifestPath in manifestPaths)
                {
                    try
                    {
                        var manifest = KeyValuesReader.Parse(File.ReadAllText(manifestPath));
                        if (manifest.TryGetValue("AppState", out var stateValue) == false ||
                            stateValue is not Dictionary<string, object> state ||
                            uint.TryParse(GetString(state, "appid"), NumberStyles.None,
                                CultureInfo.InvariantCulture, out uint appId) == false)
                        {
                            continue;
                        }

                        string name = GetString(state, "name");
                        if (string.IsNullOrWhiteSpace(name))
                        {
                            name = "App " + appId.ToString(CultureInfo.InvariantCulture);
                        }

                        games[appId] = new InstalledGame(appId, name);
                    }
                    catch (IOException)
                    {
                        // Ignore individual unreadable manifests so other games still show.
                    }
                    catch (UnauthorizedAccessException)
                    {
                    }
                    catch (FormatException)
                    {
                    }
                }
            }

            return games.Values.OrderBy(game => game.Name, StringComparer.OrdinalIgnoreCase).ToList();
        }

        private static string FindSteamRoot()
        {
            var candidates = new List<string>
            {
                Environment.GetEnvironmentVariable("SAM_STEAM_PATH"),
            };

            using (var userKey = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam"))
            {
                candidates.Add(userKey?.GetValue("SteamPath") as string);
            }
            using (var machineKey = Registry.LocalMachine.OpenSubKey(@"Software\Valve\Steam"))
            {
                candidates.Add(machineKey?.GetValue("InstallPath") as string);
            }
            using (var machineKey = Registry.LocalMachine.OpenSubKey(@"Software\WOW6432Node\Valve\Steam"))
            {
                candidates.Add(machineKey?.GetValue("InstallPath") as string);
            }

            string programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            if (string.IsNullOrWhiteSpace(programFilesX86) == false)
            {
                candidates.Add(Path.Combine(programFilesX86, "Steam"));
            }

            foreach (string candidate in candidates)
            {
                if (string.IsNullOrWhiteSpace(candidate))
                {
                    continue;
                }

                string location;
                try
                {
                    location = NormalizePath(candidate);
                }
                catch (ArgumentException)
                {
                    continue;
                }

                if (Directory.Exists(Path.Combine(location, "steamapps")))
                {
                    return location;
                }
            }

            throw new DirectoryNotFoundException(
                "Steam's local steamapps folder was not found. Install or open Steam, " +
                "or set SAM_STEAM_PATH to your Steam installation directory.");
        }

        private static string NormalizePath(string path)
        {
            return Path.GetFullPath(path.Trim().Trim('"').Replace('/', Path.DirectorySeparatorChar));
        }

        private static string GetString(Dictionary<string, object> values, string key)
        {
            return values.TryGetValue(key, out var value) ? value as string : null;
        }

        /// <summary>
        /// Small Valve KeyValues text reader. Supports nested objects, quoted strings,
        /// escaped backslashes/quotes, and // comments in library and manifest files.
        /// </summary>
        private sealed class KeyValuesReader
        {
            private readonly string _text;
            private int _offset;

            private KeyValuesReader(string text)
            {
                this._text = text;
            }

            public static Dictionary<string, object> Parse(string text)
            {
                return new KeyValuesReader(text).ReadObject(false);
            }

            private Dictionary<string, object> ReadObject(bool nested)
            {
                var result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                while (true)
                {
                    string key = this.ReadToken();
                    if (key == null)
                    {
                        if (nested)
                        {
                            throw new FormatException("Unclosed KeyValues object.");
                        }
                        return result;
                    }

                    if (key == "}")
                    {
                        if (nested == false)
                        {
                            throw new FormatException("Unexpected KeyValues closing brace.");
                        }
                        return result;
                    }

                    if (key == "{")
                    {
                        throw new FormatException("Unexpected KeyValues opening brace.");
                    }

                    string value = this.ReadToken();
                    if (value == "{")
                    {
                        result[key] = this.ReadObject(true);
                    }
                    else if (value == null || value == "}")
                    {
                        throw new FormatException("Missing value for KeyValues key: " + key);
                    }
                    else
                    {
                        result[key] = value;
                    }
                }
            }

            private string ReadToken()
            {
                while (this._offset < this._text.Length)
                {
                    char c = this._text[this._offset];
                    if (char.IsWhiteSpace(c) || c == '\uFEFF')
                    {
                        this._offset++;
                        continue;
                    }

                    if (c == '/' && this._offset + 1 < this._text.Length &&
                        this._text[this._offset + 1] == '/')
                    {
                        while (this._offset < this._text.Length &&
                               this._text[this._offset] != '\n')
                        {
                            this._offset++;
                        }
                        continue;
                    }

                    break;
                }

                if (this._offset >= this._text.Length)
                {
                    return null;
                }

                char current = this._text[this._offset++];
                if (current == '{' || current == '}')
                {
                    return current.ToString();
                }

                if (current == '"')
                {
                    var value = new StringBuilder();
                    while (this._offset < this._text.Length)
                    {
                        char c = this._text[this._offset++];
                        if (c == '"')
                        {
                            return value.ToString();
                        }

                        if (c == '\\' && this._offset < this._text.Length)
                        {
                            char escaped = this._text[this._offset];
                            if (escaped == '\\' || escaped == '"')
                            {
                                value.Append(escaped);
                                this._offset++;
                                continue;
                            }
                        }

                        value.Append(c);
                    }
                    throw new FormatException("Unterminated KeyValues quoted string.");
                }

                var token = new StringBuilder().Append(current);
                while (this._offset < this._text.Length)
                {
                    char c = this._text[this._offset];
                    if (char.IsWhiteSpace(c) || c == '{' || c == '}')
                    {
                        break;
                    }
                    token.Append(c);
                    this._offset++;
                }
                return token.ToString();
            }
        }
    }
}
