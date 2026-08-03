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

namespace SAM.Ui
{
    /// <summary>
    /// Avalonia has no built in message box, and the alternative is a package for
    /// something this small. Built in code rather than markup to keep it in one file.
    /// </summary>
    public sealed class MessageWindow : Window
    {
        private bool _Result;

        public MessageWindow(string title, string message, bool askYesNo = false)
        {
            this.Title = title;
            this.Width = 420;
            this.SizeToContent = SizeToContent.Height;
            this.CanResize = false;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            this.ShowInTaskbar = false;

            StackPanel buttons = new()
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                HorizontalAlignment = HorizontalAlignment.Right,
            };

            if (askYesNo == true)
            {
                Button yes = new() { Content = "Yes", MinWidth = 88 };
                yes.Click += (_, _) =>
                {
                    this._Result = true;
                    this.Close();
                };

                Button no = new() { Content = "No", MinWidth = 88, IsDefault = true, IsCancel = true };
                no.Click += (_, _) => this.Close();

                buttons.Children.Add(yes);
                buttons.Children.Add(no);
            }
            else
            {
                Button ok = new() { Content = "OK", MinWidth = 88, IsDefault = true, IsCancel = true };
                ok.Click += (_, _) => this.Close();
                buttons.Children.Add(ok);
            }

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
                    buttons,
                },
            };
        }

        public static Task ShowAsync(Window owner, string title, string message)
        {
            MessageWindow window = new(title, message);
            return owner != null
                ? window.ShowDialog(owner)
                : Task.CompletedTask;
        }

        /// <summary>
        /// Defaults to no, so dismissing the dialog never confirms a destructive action.
        /// </summary>
        public static async Task<bool> ConfirmAsync(Window owner, string title, string question)
        {
            if (owner == null)
            {
                return false;
            }

            MessageWindow window = new(title, question, true);
            await window.ShowDialog(owner);
            return window._Result;
        }
    }
}
