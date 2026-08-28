using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace SAM.Picker
{
    internal class EmulatorGameInfo
    {
        public string Name { get; set; }
        public uint AppId { get; set; }
        public string Emulator { get; set; }
        public Dictionary<string, bool> Achievements { get; } = new();
n        public int CountUnlocked => Achievements == null ? 0 : (int) (Achievements.Values == null ? 0 : (uint)0);
    }

    internal static class EmulatorScanner
    {
        // Folders to scan (can be customized later)
        private static readonly string[] DefaultPaths = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GSE Saves"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Goldberg SteamEmu Saves"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "OnlineFix"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "Steam\\RUNE"),
        };

        public static IEnumerable<EmulatorGameInfo> ScanDefaultPaths()
        {
            var results = new List<EmulatorGameInfo>();
            foreach (var p in DefaultPaths)
            {
                try
                {
                    if (Directory.Exists(p) == false) continue;
                    foreach (var dir in Directory.GetDirectories(p))
                    {
                        try
                        {
                            var info = ScanDirectoryForGame(dir);
                            if (info != null)
                            {
                                results.Add(info);
                            }
                        }
                        catch { }
                    }
                }
                catch { }
            }
            return results;
        }

        private static EmulatorGameInfo ScanDirectoryForGame(string dir)
        {
            // Determine appid from directory name if numeric
            var name = Path.GetFileName(dir);
            uint appid = 0;
            if (uint.TryParse(name, out var parsed))
            {
                appid = parsed;
            }

            var files = Directory.GetFiles(dir);
            EmulatorGameInfo result = null;
            foreach (var f in files)
            {
                var fn = Path.GetFileName(f).ToLowerInvariant();
                if (fn.Contains("achievement") && fn.EndsWith(".json"))
                {
                    var g = new EmulatorGameInfo() { Name = name, AppId = appid, Emulator = DetectEmulatorFromPath(dir) };
                    ParseGseJson(f, g);
                    result = g;
                    break;
                }
                if (fn.Contains("achievement") && fn.EndsWith(".ini"))
                {
                    var g = new EmulatorGameInfo() { Name = name, AppId = appid, Emulator = DetectEmulatorFromPath(dir) };
                    ParseRuneIni(f, g);
                    result = g;
                    break;
                }
                // fallback: parse any .json or .ini that looks like achievements
                if (fn.EndsWith(".json"))
                {
                    var content = File.ReadAllText(f);
                    if (content.Contains("\"earned\""))
                    {
                        var g = new EmulatorGameInfo() { Name = name, AppId = appid, Emulator = DetectEmulatorFromPath(dir) };
                        ParseGseJson(f, g);
                        result = g;
                        break;
                    }
                }
                if (fn.EndsWith(".ini"))
                {
                    var content = File.ReadAllText(f);
                    if (content.Contains("[SteamAchievements]") || content.Contains("Achieved="))
                    {
                        var g = new EmulatorGameInfo() { Name = name, AppId = appid, Emulator = DetectEmulatorFromPath(dir) };
                        ParseRuneIni(f, g);
                        result = g;
                        break;
                    }
                }
            }

            return result;
        }

        private static string DetectEmulatorFromPath(string path)
        {
            var p = path.ToLowerInvariant();
            if (p.Contains("gse")) return "GSE";
            if (p.Contains("goldberg")) return "Goldberg";
            if (p.Contains("onlinefix")) return "OnlineFix";
            if (p.Contains("rune")) return "RUNE";
            return "Unknown";
        }

        private static void ParseGseJson(string path, EmulatorGameInfo outInfo)
        {
            var text = File.ReadAllText(path);
            // very small naive JSON parser for the structure provided: keys map to { "earned": true/false }
            var regex = new Regex("\"(?<key>[^\"]+)\"\s*:\s*\\{[^}]*?\"earned\"\s*:\s*(?<earned>true|false)", RegexOptions.IgnoreCase);
            foreach (Match m in regex.Matches(text))
            {
                var key = m.Groups["key"].Value;
                var earned = string.Equals(m.Groups["earned"].Value, "true", StringComparison.OrdinalIgnoreCase);
                outInfo.Achievements[key] = earned;
            }
        }

        private static void ParseRuneIni(string path, EmulatorGameInfo outInfo)
        {
            var lines = File.ReadAllLines(path);
            string currentSection = null;
            var steamList = new List<string>();
            var achievedMap = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            foreach (var raw in lines)
            {
                var line = raw.Trim();
                if (line.Length == 0) continue;
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    currentSection = line.Substring(1, line.Length - 2);
                    continue;
                }
                if (currentSection == "SteamAchievements")
                {
                    var parts = line.Split(new[] {'='}, 2);
                    if (parts.Length == 2)
                    {
                        steamList.Add(parts[1].Trim());
                    }
                }
                else if (!string.IsNullOrEmpty(currentSection))
                {
                    if (line.StartsWith("Achieved=", StringComparison.OrdinalIgnoreCase))
                    {
                        var val = line.Substring("Achieved=".Length).Trim();
                        var achieved = val == "1" || string.Equals(val, "true", StringComparison.OrdinalIgnoreCase);
                        achievedMap[currentSection] = achieved;
                    }
                }
            }

            // map steamList entries to achievedMap where possible
            foreach (var key in steamList)
            {
                if (achievedMap.TryGetValue(key, out var v))
                {
                    outInfo.Achievements[key] = v;
                }
                else
                {
                    outInfo.Achievements[key] = false;
                }
            }
        }
    }
}
