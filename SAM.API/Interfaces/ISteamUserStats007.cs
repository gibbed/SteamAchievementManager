﻿/* Copyright (c) 2019 Rick (rick 'at' gibbed 'dot' us)
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
using System.Runtime.InteropServices;

namespace SAM.API.Interfaces
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class ISteamUserStats007
    {
        public IntPtr ClearAchievement;
        public IntPtr DownloadLeaderboardEntries;
        public IntPtr FindLeaderboard;
        public IntPtr FindOrCreateLeaderboard;
        public IntPtr GetAchievement;
        public IntPtr GetAchievementAndUnlockTime;
        public IntPtr GetAchievementDisplayAttribute;
        public IntPtr GetAchievementIcon;
        public IntPtr GetDownloadedLeaderboardEntry;
        public IntPtr GetLeaderboardDisplayType;
        public IntPtr GetLeaderboardEntryCount;
        public IntPtr GetLeaderboardName;
        public IntPtr GetLeaderboardSortMethod;
        public IntPtr GetNumberOfCurrentPlayers;
        public IntPtr GetStatFloat;
        public IntPtr GetStatInteger;
        public IntPtr GetUserAchievement;
        public IntPtr GetUserAchievementAndUnlockTime;
        public IntPtr GetUserStatFloat;
        public IntPtr GetUserStatInt;
        public IntPtr IndicateAchievementProgress;
        public IntPtr RequestCurrentStats;
        public IntPtr RequestUserStats;
        public IntPtr ResetAllStats;
        public IntPtr SetAchievement;
        public IntPtr SetStatFloat;
        public IntPtr SetStatInteger;
        public IntPtr StoreStats;
        public IntPtr UpdateAvgRateStat;
        public IntPtr UploadLeaderboardScore;
    }
}