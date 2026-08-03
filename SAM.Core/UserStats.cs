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
using System.Linq;
using static SAM.Core.InvariantShorthand;

namespace SAM.Core
{
    /// <summary>
    /// Reads and writes the signed in user's stats and achievements for one game.
    /// Store methods return the id of the entry that failed, or null on success, so
    /// callers decide how to report it.
    /// </summary>
    public sealed class UserStats
    {
        private readonly API.Client _Client;

        public UserStats(API.Client client)
        {
            this._Client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public string GetCurrentGameLanguage()
        {
            return this._Client.SteamApps008.GetCurrentGameLanguage();
        }

        public static string TranslateError(int id) => id switch
        {
            2 => "generic error -- this usually means you don't own the game",
            _ => _($"{id}"),
        };

        /// <summary>
        /// Reads the current value of every definition Steam will report a value for.
        /// Definitions Steam declines are skipped rather than reported as an error.
        /// </summary>
        public List<Stats.StatInfo> ReadStatistics(IEnumerable<Stats.StatDefinition> definitions)
        {
            List<Stats.StatInfo> statistics = new();

            foreach (Stats.StatDefinition definition in definitions)
            {
                if (string.IsNullOrEmpty(definition.Id) == true)
                {
                    continue;
                }

                if (definition is Stats.IntegerStatDefinition intStat)
                {
                    if (this._Client.SteamUserStats.GetStatValue(intStat.Id, out int value) == false)
                    {
                        continue;
                    }

                    statistics.Add(new Stats.IntStatInfo()
                    {
                        Id = intStat.Id,
                        DisplayName = intStat.DisplayName,
                        IntValue = value,
                        OriginalValue = value,
                        IsIncrementOnly = intStat.IncrementOnly,
                        Permission = intStat.Permission,
                    });
                }
                else if (definition is Stats.FloatStatDefinition floatStat)
                {
                    if (this._Client.SteamUserStats.GetStatValue(floatStat.Id, out float value) == false)
                    {
                        continue;
                    }

                    statistics.Add(new Stats.FloatStatInfo()
                    {
                        Id = floatStat.Id,
                        DisplayName = floatStat.DisplayName,
                        FloatValue = value,
                        OriginalValue = value,
                        IsIncrementOnly = floatStat.IncrementOnly,
                        Permission = floatStat.Permission,
                    });
                }
            }

            return statistics;
        }

        /// <summary>
        /// Reads whether one achievement is unlocked, and when. Returns false when Steam
        /// has no state for it, which callers treat as a reason to skip the achievement.
        /// </summary>
        public bool TryReadAchievement(string id, out bool isAchieved, out uint unlockTime)
        {
            return this._Client.SteamUserStats.GetAchievementAndUnlockTime(id, out isAchieved, out unlockTime);
        }

        public string StoreAchievements(IEnumerable<Stats.AchievementInfo> achievements)
        {
            foreach (Stats.AchievementInfo info in achievements)
            {
                if (this._Client.SteamUserStats.SetAchievement(info.Id, info.IsAchieved) == false)
                {
                    return info.Id;
                }
            }

            return null;
        }

        public string StoreStatistics(IEnumerable<Stats.StatInfo> statistics)
        {
            foreach (Stats.StatInfo stat in statistics)
            {
                bool stored = stat switch
                {
                    Stats.IntStatInfo intStat =>
                        this._Client.SteamUserStats.SetStatValue(intStat.Id, intStat.IntValue),
                    Stats.FloatStatInfo floatStat =>
                        this._Client.SteamUserStats.SetStatValue(floatStat.Id, floatStat.FloatValue),
                    _ => throw new InvalidOperationException("unsupported stat type"),
                };

                if (stored == false)
                {
                    return stat.Id;
                }
            }

            return null;
        }

        /// <summary>
        /// Requests a fresh copy of the user's stats. The result arrives on the
        /// UserStatsReceived callback.
        /// </summary>
        public API.CallHandle Request()
        {
            ulong steamId = this._Client.SteamUser.GetSteamId();
            return this._Client.SteamUserStats.RequestUserStats(steamId);
        }

        /// <summary>
        /// Flushes everything set through this instance back to Steam.
        /// </summary>
        public bool Commit()
        {
            return this._Client.SteamUserStats.StoreStats();
        }

        public bool ResetAll(bool achievementsToo)
        {
            return this._Client.SteamUserStats.ResetAllStats(achievementsToo);
        }
    }
}
