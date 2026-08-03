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
using System.Globalization;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using SAM.Game.ViewModels;
using SAM.Game.Views;
using SAM.Ui;

namespace SAM.Game
{
    public partial class App : Application
    {
        private API.Client _Client;
        private ManagerViewModel _Model;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (this.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                Window window = this.CreateMainWindow(desktop.Args ?? Array.Empty<string>());
                if (window == null)
                {
                    desktop.Shutdown();
                    return;
                }

                desktop.MainWindow = window;
                desktop.ShutdownRequested += this.OnShutdownRequested;
            }

            base.OnFrameworkInitializationCompleted();
        }

        /// <summary>
        /// Returns null when there is nothing to show, which happens when this was run
        /// with no application id and the picker was launched in its place.
        /// </summary>
        private Window CreateMainWindow(string[] args)
        {
            if (args.Length == 0)
            {
                try
                {
                    API.AppHost.Start("SAM.Picker");
                    return null;
                }
                catch (Exception e)
                {
                    return new MessageWindow("Error", $"Failed to start SAM.Picker.\n\n{e.Message}");
                }
            }

            if (long.TryParse(args[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out long appId) == false)
            {
                return new MessageWindow(
                    "Error",
                    "Could not parse application ID from command line argument.");
            }

            if (IsRunningFromSteamDirectory() == true)
            {
                return new MessageWindow(
                    "Error",
                    "This tool declines to being run from the Steam directory.");
            }

            API.Client client = new();
            try
            {
                client.Initialize(appId);
            }
            catch (API.ClientInitializeException e)
            {
                client.Dispose();
                string detail = string.IsNullOrEmpty(e.Message) == true ? "" : $"\n\n({e.Message})";
                return new MessageWindow(
                    "Error",
                    $"Steam is not running. Please start Steam then run this tool again.{detail}");
            }
            catch (DllNotFoundException)
            {
                client.Dispose();
                return new MessageWindow("Error", "You've caused an exceptional error!");
            }

            this._Client = client;
            this._Model = new ManagerViewModel(appId, client);
            return new ManagerWindow(this._Model);
        }

        private void OnShutdownRequested(object sender, ShutdownRequestedEventArgs e)
        {
            this._Model?.Dispose();
            this._Client?.Dispose();
        }

        private static bool IsRunningFromSteamDirectory()
        {
            string installPath = API.Steam.GetInstallPath();
            if (string.IsNullOrEmpty(installPath) == true)
            {
                return false;
            }

            return string.Equals(
                Path.TrimEndingDirectorySeparator(installPath),
                Path.TrimEndingDirectorySeparator(AppContext.BaseDirectory),
                StringComparison.OrdinalIgnoreCase);
        }
    }
}
