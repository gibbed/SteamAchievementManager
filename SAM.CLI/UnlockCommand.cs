using System;
using System.Threading;
using Newtonsoft.Json;
using SAM.API;

namespace SAM.CLI
{
    class UnlockCommand
    {
        public static object Execute(API.Client client, long appId, string achievementId)
        {
            if (string.IsNullOrEmpty(achievementId))
            {
                return new
                {
                    success = false,
                    error = "Achievement ID is required",
                    usage = "SAM.CLI.exe unlock <appid> <achievement_id>"
                };
            }

            try
            {
                // Request stats first to ensure they're loaded
                var steamId = client.SteamUser.GetSteamId();
                var callHandle = client.SteamUserStats.RequestUserStats(steamId);

                // Wait for stats to load
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

                // Set achievement to unlocked
                if (!client.SteamUserStats.SetAchievement(achievementId, true))
                {
                    return new
                    {
                        success = false,
                        error = $"Failed to set achievement {achievementId}"
                    };
                }

                // Store stats to file
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
                    message = $"Achievement {achievementId} unlocked successfully"
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