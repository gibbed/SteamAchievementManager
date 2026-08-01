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

using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Layout;

namespace SAM.Picker.Views
{
    /// <summary>
    /// Avalonia has no built in message box, and the alternative is a package for
    /// something this small. Built in code rather than markup to keep it in one file.
    /// </summary>
    public sealed class MessageWindow : Window
    {
        public MessageWindow(string title, string message)
        {
            this.Title = title;
            this.Width = 420;
            this.SizeToContent = SizeToContent.Height;
            this.CanResize = false;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            this.ShowInTaskbar = false;

            Button ok = new()
            {
                Content = "OK",
                MinWidth = 88,
                IsDefault = true,
                HorizontalAlignment = HorizontalAlignment.Right,
            };
            ok.Click += (_, _) => this.Close();

            this.Content = new StackPanel()
            {
                Margin = new(16),
                Spacing = 16,
                Children =
                {
                    new TextBlock()
                    {
                        Text = message,
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                    },
                    ok,
                },
            };
        }

        public static Task ShowAsync(Window owner, string title, string message)
        {
            MessageWindow window = new(title, message);
            return owner != null
                ? window.ShowDialog(owner)
                : Task.Run(() => { });
        }
    }
}
