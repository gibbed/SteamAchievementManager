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
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using SAM.Picker.ViewModels;
using SAM.Picker.Views;
using SAM.Ui;

namespace SAM.Picker
{
    public partial class App : Application
    {
        private API.Client _Client;
        private GamePickerViewModel _Model;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (this.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = this.CreateMainWindow();
                desktop.ShutdownRequested += this.OnShutdownRequested;
            }

            base.OnFrameworkInitializationCompleted();
        }

        /// <summary>
        /// Steam has to be reachable before the picker is worth showing, so a failure
        /// here becomes the only window rather than an empty list.
        /// </summary>
        private Avalonia.Controls.Window CreateMainWindow()
        {
            if (IsRunningFromSteamDirectory() == true)
            {
                return new MessageWindow(
                    "Error",
                    "This tool declines to being run from the Steam directory.");
            }

            API.Client client = new();
            try
            {
                client.Initialize(0);
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
            this._Model = new GamePickerViewModel(client);
            return new GamePickerWindow(this._Model);
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
