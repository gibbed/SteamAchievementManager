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
using System.Linq;
using System.Threading;
using static SAM.Game.InvariantShorthand;
using APITypes = SAM.API.Types;

namespace SAM.Game
{
    internal static class AutoUnlocker
    {
        // Polls the Steam callback system until UserStatsReceived fires or the
        // 30-second deadline is reached.  Returns the result code from Steam
        // (1 = success) or -1 on timeout.
        private static int WaitForUserStats(API.Client client)
        {
            int result = -1;
            bool done = false;

            var callback = client.CreateAndRegisterCallback<API.Callbacks.UserStatsReceived>();
            callback.OnRun += param =>
            {
                result = param.Result;
                done = true;
            };

            var steamId = client.SteamUser.GetSteamId();
            var callHandle = client.SteamUserStats.RequestUserStats(steamId);
            if (callHandle == API.CallHandle.Invalid)
            {
                return -1;
            }

            var deadline = DateTime.UtcNow.AddSeconds(30);
            while (!done && DateTime.UtcNow < deadline)
            {
                client.RunCallbacks(false);
                Thread.Sleep(50);
            }

            return done ? result : -1;
        }

        // Reads the cached Steam stats schema for the given game and extracts
        // every achievement ID that is not permission-protected.
        private static List<(string Id, int Permission)> LoadAchievementIds(
            long gameId,
            API.Client client)
        {
            string schemaPath;
            try
            {
                string fileName = _($"UserGameStatsSchema_{gameId}.bin");
                schemaPath = Path.Combine(
                    API.Steam.GetInstallPath(),
                    "appcache",
                    "stats",
                    fileName);

                if (!File.Exists(schemaPath))
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }

            var kv = KeyValue.LoadAsBinary(schemaPath);
            if (kv == null)
            {
                return null;
            }

            var statsNode = kv[gameId.ToString(CultureInfo.InvariantCulture)]["stats"];
            if (!statsNode.Valid || statsNode.Children == null)
            {
                // No stats node means the game has no achievements — not an error.
                return new List<(string, int)>();
            }

            var ids = new List<(string Id, int Permission)>();

            foreach (var stat in statsNode.Children)
            {
                if (!stat.Valid)
                {
                    continue;
                }

                // Resolve stat type using both new (string) and old (int) formats.
                APITypes.UserStatType type;
                var typeNode = stat["type"];
                if (typeNode.Valid && typeNode.Type == KeyValueType.String)
                {
                    if (!Enum.TryParse((string)typeNode.Value, true, out type))
                    {
                        type = APITypes.UserStatType.Invalid;
                    }
                }
                else
                {
                    type = APITypes.UserStatType.Invalid;
                }

                if (type == APITypes.UserStatType.Invalid)
                {
                    var typeIntNode = stat["type_int"];
                    int rawType = typeIntNode.Valid
                        ? typeIntNode.AsInteger(0)
                        : typeNode.AsInteger(0);
                    type = (APITypes.UserStatType)rawType;
                }

                if (type != APITypes.UserStatType.Achievements &&
                    type != APITypes.UserStatType.GroupAchievements)
                {
                    continue;
                }

                if (stat.Children == null)
                {
                    continue;
                }

                foreach (var bits in stat.Children.Where(b =>
                    string.Compare(b.Name, "bits", StringComparison.InvariantCultureIgnoreCase) == 0))
                {
                    if (!bits.Valid || bits.Children == null)
                    {
                        continue;
                    }

                    foreach (var bit in bits.Children)
                    {
                        string id = bit["name"].AsString("");
                        if (!string.IsNullOrEmpty(id))
                        {
                            ids.Add((id, bit["permission"].AsInteger(0)));
                        }
                    }
                }
            }

            return ids;
        }

        // Silently unlocks every non-protected, not-yet-unlocked achievement for
        // the given game and commits the result to Steam.
        // Returns true on success (including when there is nothing to unlock).
        internal static bool RunUnlockAll(long gameId, API.Client client)
        {
            int statsResult = WaitForUserStats(client);
            if (statsResult != 1)
            {
                return false;
            }

            var achievementIds = LoadAchievementIds(gameId, client);
            if (achievementIds == null)
            {
                return false;
            }

            if (achievementIds.Count == 0)
            {
                return true; // game has no achievements; treat as success
            }

            int unlocked = 0;
            foreach (var (id, permission) in achievementIds)
            {
                // Skip achievements that are permission-protected (same guard as Manager.cs).
                if ((permission & 3) != 0)
                {
                    continue;
                }

                if (!client.SteamUserStats.GetAchievementAndUnlockTime(id, out bool isAchieved, out uint ignoredUnlockTime))
                {
                    continue;
                }

                if (isAchieved)
                {
                    continue;
                }

                if (!client.SteamUserStats.SetAchievement(id, true))
                {
                    continue;
                }

                unlocked++;
            }

            if (unlocked == 0)
            {
                return true; // nothing to store
            }

            if (!client.SteamUserStats.StoreStats())
            {
                return false;
            }

            // Allow Steam sufficient time to acknowledge the stored stats before
            // the process exits.
            Thread.Sleep(1500);

            return true;
        }
    }
}
