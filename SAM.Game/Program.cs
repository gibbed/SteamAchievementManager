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
using System.IO;
using System.Windows.Forms;

namespace SAM.Game
{
    internal static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            try
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
                        API.Localization.ParseAppIdFailed,
                        API.Localization.Error,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                if (API.Steam.GetInstallPath() == Application.StartupPath)
                {
                    MessageBox.Show(
                        API.Localization.RunFromSteamDeclined,
                        API.Localization.Error,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                bool isIdleMode = false;
                foreach (var arg in args)
                {
                    if (string.Equals(arg, "-idle", StringComparison.OrdinalIgnoreCase))
                    {
                        isIdleMode = true;
                    }
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
                                API.Localization.SteamNotRunning + "\n\n" +
                                API.Localization.FamilyShareLocked + "\n\n" +
                                "(" + e.Message + ")",
                                API.Localization.Error,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                        else if (string.IsNullOrEmpty(e.Message) == false)
                        {
                            MessageBox.Show(
                                API.Localization.SteamNotRunning + "\n\n" +
                                "(" + e.Message + ")",
                                API.Localization.Error,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                        else
                        {
                            MessageBox.Show(
                                API.Localization.SteamNotRunning,
                                API.Localization.Error,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                        return;
                    }
                    catch (DllNotFoundException)
                    {
                        MessageBox.Show(
                            API.Localization.ExceptionalError,
                            API.Localization.Error,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        return;
                    }

                    if (isIdleMode)
                    {
                        lock (API.Steam.SteamLock)
                        {
                            try
                            {
                                client.SteamFriends?.SetPersonaState(7); // k_EPersonaStateInvisible
                            }
                            catch { }
                        }

                        var idleTimer = new System.Windows.Forms.Timer { Interval = 1000 };
                        idleTimer.Tick += (s, ev) =>
                        {
                            lock (API.Steam.SteamLock)
                            {
                                client.RunCallbacks(false);
                            }
                        };
                        idleTimer.Start();
                        Application.Run();
                        return;
                    }

                    var app = new System.Windows.Application();
                    app.DispatcherUnhandledException += (s, eArgs) =>
                    {
                        File.WriteAllText("game_crash.log", eArgs.Exception.ToString());
                        System.Windows.MessageBox.Show(eArgs.Exception.ToString(), "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        eArgs.Handled = true;
                    };
                    app.Run(new ManagerWindow(appId, client));
                }
            }
            catch (Exception ex)
            {
                File.WriteAllText("game_crash.log", ex.ToString());
            }
        }
    }
}
