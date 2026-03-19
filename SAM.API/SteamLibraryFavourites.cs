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
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace SAM.API
{
    public static class SteamLibraryFavourites
    {
        public static HashSet<uint> GetFavoriteAppIds()
        {
            var favorites = new HashSet<uint>();
            try
            {
                string steamPath = Steam.GetInstallPath();
                if (string.IsNullOrEmpty(steamPath) || Directory.Exists(steamPath) == false)
                {
                    return favorites;
                }

                string userDataPath = Path.Combine(steamPath, "userdata");
                if (Directory.Exists(userDataPath) == false)
                {
                    return favorites;
                }

                var userDirs = Directory.GetDirectories(userDataPath);
                foreach (var userDir in userDirs)
                {
                    // Check 7/remote/sharedconfig.vdf
                    string sharedConfigPath = Path.Combine(userDir, "7", "remote", "sharedconfig.vdf");
                    if (File.Exists(sharedConfigPath))
                    {
                        ParseVdfForFavorites(sharedConfigPath, favorites);
                    }

                    // Check config/localconfig.vdf
                    string localConfigPath = Path.Combine(userDir, "config", "localconfig.vdf");
                    if (File.Exists(localConfigPath))
                    {
                        ParseVdfForFavorites(localConfigPath, favorites);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }

            return favorites;
        }

        private static void ParseVdfForFavorites(string vdfPath, HashSet<uint> favorites)
        {
            try
            {
                string content = File.ReadAllText(vdfPath);
                var lines = content.Split('\n');
                string currentAppIdStr = null;

                foreach (var rawLine in lines)
                {
                    string line = rawLine.Trim();

                    // Check if line specifies an AppID key like "480"
                    if (line.StartsWith("\"") && line.EndsWith("\"") && line.Length < 15)
                    {
                        string candidate = line.Trim('\"');
                        if (uint.TryParse(candidate, out uint id) && id > 0)
                        {
                            currentAppIdStr = candidate;
                        }
                    }

                    // Check if line specifies "favorite" or "1"
                    if (line.IndexOf("favorite", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        line.IndexOf("favourite", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        if (currentAppIdStr != null && uint.TryParse(currentAppIdStr, out uint favoriteId))
                        {
                            favorites.Add(favoriteId);
                        }
                    }
                }
            }
            catch { }
        }
    }
}
