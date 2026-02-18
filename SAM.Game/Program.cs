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
using System.Diagnostics;
using System.Windows.Forms;

namespace SAM.Game
{
    internal static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            long appId;

            if (args.Length == 0)
            {
                Process.Start("SAM.Picker.exe");
                return;
            }

            if (long.TryParse(args[0], out appId) == false)
            {
                MessageBox.Show(
                    "Could not parse application ID from command line argument.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            if (API.Steam.GetInstallPath() == Application.StartupPath)
            {
                MessageBox.Show(
                    "This tool declines to being run from the Steam directory.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            using (API.Client client = new())
            {
                try
                {
                    client.Initialize(appId);
                }
                catch (API.ClientInitializeException e)
                {
                    if (e.Failure == API.ClientInitializeFailure.ConnectToGlobalUser)
                    {
                        MessageBox.Show(
                            "Steam is not running. Please start Steam then run this tool again.\n\n" +
                            "If you have the game through Family Share, the game may be locked due to\n" +
                            "the Family Share account actively playing a game.\n\n" +
                            "(" + e.Message + ")",
                            "Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                    else if (string.IsNullOrEmpty(e.Message) == false)
                    {
                        MessageBox.Show(
                            "Steam is not running. Please start Steam then run this tool again.\n\n" +
                            "(" + e.Message + ")",
                            "Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                    else
                    {
                        MessageBox.Show(
                            "Steam is not running. Please start Steam then run this tool again.",
                            "Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                    return;
                }
                catch (DllNotFoundException)
                {
                    MessageBox.Show(
                        "You've caused an exceptional error!",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                if (args.Length >= 2 && args[1] == "--unlock-all")
                {
                    UnlockAllAchievements(client);
                    return;
                }

                Application.Run(new Manager(appId, client));
            }
        }

        private static void UnlockAllAchievements(API.Client client)
        {
            var steamId = client.SteamUser.GetSteamId();
            var callHandle = client.SteamUserStats.RequestUserStats(steamId);
            if (callHandle == API.CallHandle.Invalid)
            {
                return;
            }

            // Run callbacks until stats are received
            var deadline = DateTime.UtcNow.AddSeconds(10);
            bool statsReceived = false;
            var callback = client.CreateAndRegisterCallback<API.Callbacks.UserStatsReceived>();
            callback.OnRun += param =>
            {
                if (param.Result == 1)
                {
                    statsReceived = true;
                }
            };

            while (statsReceived == false && DateTime.UtcNow < deadline)
            {
                client.RunCallbacks(false);
                System.Threading.Thread.Sleep(100);
            }

            if (statsReceived == false)
            {
                return;
            }

            uint numAchievements = client.SteamUserStats.GetNumAchievements();
            if (numAchievements == 0)
            {
                return;
            }

            for (uint i = 0; i < numAchievements; i++)
            {
                string name = client.SteamUserStats.GetAchievementName(i);
                if (string.IsNullOrEmpty(name) == false)
                {
                    client.SteamUserStats.SetAchievement(name, true);
                }
            }

            client.SteamUserStats.StoreStats();
        }
    }
}
