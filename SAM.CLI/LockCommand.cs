using System;
using System.Threading;
using Newtonsoft.Json;
using SAM.API;

namespace SAM.CLI
{
    class LockCommand
    {
        public static object Execute(API.Client client, long appId, string achievementId)
        {
            if (string.IsNullOrEmpty(achievementId))
            {
                return new
                {
                    success = false,
                    error = "Achievement ID is required",
                    usage = "SAM.CLI.exe lock <appid> <achievement_id>"
                };
            }

            try
            {
                // Request stats first
                var steamId = client.SteamUser.GetSteamId();
                var callHandle = client.SteamUserStats.RequestUserStats(steamId);

                bool statsReceived = false;
                API.Callbacks.UserStatsReceived callback = null;

                callback = client.CreateAndRegisterCallback<API.Callbacks.UserStatsReceived>();
                callback.OnRun += (param) => {
                    if (param.Result == 1)
                    {
                        statsReceived = true;
                    }
                };

                int attempts = 0;
                while (!statsReceived && attempts < 50)
                {
                    client.RunCallbacks(false);
                    Thread.Sleep(100);
                    attempts++;
                }

                if (!statsReceived)
                {
                    return new
                    {
                        success = false,
                        error = "Timeout waiting for stats to load"
                    };
                }

                // Set achievement to locked (false)
                if (!client.SteamUserStats.SetAchievement(achievementId, false))
                {
                    return new
                    {
                        success = false,
                        error = $"Failed to lock achievement {achievementId}"
                    };
                }

                // Store stats
                if (!client.SteamUserStats.StoreStats())
                {
                    return new
                    {
                        success = false,
                        error = "Failed to store stats"
                    };
                }

                return new
                {
                    success = true,
                    appid = appId,
                    achievement_id = achievementId,
                    message = $"Achievement {achievementId} locked successfully"
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    success = false,
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                };
            }
        }
    }
}