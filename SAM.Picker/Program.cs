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
using System.Threading;
using System.Windows.Forms;

namespace SAM.Picker
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            // Without these, any unexpected exception (missing Steam interface, callback
            // failure, ...) ends the process silently: "SAM won't open" (#491/#435/#424).
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += OnUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            if (string.Equals(
                API.Steam.GetInstallPath(),
                Application.StartupPath,
                StringComparison.OrdinalIgnoreCase) == true)
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
                    TryInitialize(client);
                }
                catch (API.ClientInitializeException e)
                {
                    MessageBox.Show(
                        DescribeInitializeFailure(e),
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }
                catch (DllNotFoundException e)
                {
                    MessageBox.Show(
                        "Failed to load the Steam API:\n" + e.Message,
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }
                catch (BadImageFormatException e)
                {
                    MessageBox.Show(
                        "Failed to load the Steam API:\n" + e.Message,
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new GamePicker(client));
            }
        }

        private static void TryInitialize(API.Client client)
        {
            try
            {
                client.Initialize(0);
            }
            catch (API.ClientInitializeException e)
                when (e.Failure == API.ClientInitializeFailure.CreateSteamPipe)
            {
                // Steam may still be starting up; give it one chance before giving up.
                Thread.Sleep(2000);
                client.Initialize(0);
            }
        }

        private static string DescribeInitializeFailure(API.ClientInitializeException e)
        {
            const string startSteam = "Steam could not be reached. Please make sure Steam is running, then start this tool again.";

            switch (e.Failure)
            {
                case API.ClientInitializeFailure.GetInstallPath:
                    return "Could not find the Steam installation.\n\n(" + e.Message + ")";

                case API.ClientInitializeFailure.Load:
                    return "Could not load the Steam client library.\n\n(" + e.Message + ")";

                case API.ClientInitializeFailure.CreateSteamClient:
                    return "Could not create the Steam client interface.\n\n(" + e.Message + ")";

                case API.ClientInitializeFailure.GetInterfaces:
                    return "Steam did not provide the expected interfaces.\n" +
                           "Your Steam client may be too old or still starting up.\n\n(" + e.Message + ")";

                case API.ClientInitializeFailure.ConnectToGlobalUser:
                    return startSteam +
                           "\n\nIf you have a game through Family Share, it may be locked because the\n" +
                           "Family Share account is actively playing a game.\n\n(" + e.Message + ")";

                case API.ClientInitializeFailure.AppIdMismatch:
                    return "The requested application does not match the running Steam client.\n\n(" + e.Message + ")";

                default:
                    return startSteam + "\n\n(" + e.Message + ")";
            }
        }

        private static void OnUnhandledException(object sender, ThreadExceptionEventArgs e)
        {
            ShowUnhandledException(e.Exception);
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ShowUnhandledException(e.ExceptionObject as Exception);
        }

        private static void ShowUnhandledException(Exception exception)
        {
            MessageBox.Show(
                "An unexpected error occurred:\n\n" + (exception?.ToString() ?? "unknown error"),
                "Steam Achievement Manager",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
