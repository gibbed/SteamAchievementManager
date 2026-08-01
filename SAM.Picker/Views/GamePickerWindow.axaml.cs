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

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using SAM.Picker.ViewModels;

namespace SAM.Picker.Views
{
    public partial class GamePickerWindow : Window
    {
        public GamePickerWindow()
        {
            this.InitializeComponent();
        }

        public GamePickerWindow(GamePickerViewModel model)
            : this()
        {
            this.DataContext = model;
            model.ErrorRaised += this.OnErrorRaised;
        }

        private GamePickerViewModel Model => this.DataContext as GamePickerViewModel;

        protected override async void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);

            if (this.Model != null)
            {
                await this.Model.RefreshAsync();
            }
        }

        private void OnErrorRaised(string message)
        {
            _ = MessageWindow.ShowAsync(this, "Error", message);
        }

        private void OnGameActivated(object sender, TappedEventArgs e)
        {
            this.Model?.LaunchSelected();
        }

        private void OnGameListKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter && e.Key != Key.Space)
            {
                return;
            }

            this.Model?.LaunchSelected();
            e.Handled = true;
        }
    }
}
