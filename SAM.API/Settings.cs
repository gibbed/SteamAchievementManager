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
using System.IO;

namespace SAM.API
{
    public class Settings
    {
        private static readonly string SettingsPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "settings.json");

        public string Language { get; set; }

        private static Settings _instance;
        public static Settings Instance => _instance ??= Load();

        public static Settings Load()
        {
            Settings settings = new();
            try
            {
                if (File.Exists(SettingsPath) == true)
                {
                    string json = File.ReadAllText(SettingsPath);
                    string lang = ParseJsonValue(json, "language");
                    if (string.IsNullOrEmpty(lang) == false)
                    {
                        settings.Language = lang;
                    }
                }
            }
            catch
            {
                // ignore load errors
            }
            return settings;
        }

        public void Save()
        {
            try
            {
                string json = "{\n" +
                              "  \"language\": " + FormatJsonString(this.Language ?? "") + "\n" +
                              "}";
                File.WriteAllText(SettingsPath, json);
            }
            catch
            {
                // ignore save errors
            }
        }

        private static string FormatJsonString(string value)
        {
            return "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
        }

        private static string ParseJsonValue(string json, string key)
        {
            if (string.IsNullOrEmpty(json) == true)
            {
                return null;
            }
            string pattern = "\"" + key + "\"";
            int idx = json.IndexOf(pattern, StringComparison.OrdinalIgnoreCase);
            if (idx < 0)
            {
                return null;
            }
            int colon = json.IndexOf(':', idx + pattern.Length);
            if (colon < 0)
            {
                return null;
            }
            int startQuote = json.IndexOf('"', colon + 1);
            if (startQuote < 0)
            {
                return null;
            }
            int endQuote = json.IndexOf('"', startQuote + 1);
            if (endQuote < 0)
            {
                return null;
            }
            return json.Substring(startQuote + 1, endQuote - startQuote - 1);
        }
    }
}
