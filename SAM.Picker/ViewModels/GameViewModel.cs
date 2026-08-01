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

using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using SAM.Core;

namespace SAM.Picker.ViewModels
{
    public sealed partial class GameViewModel : ObservableObject
    {
        private readonly GameInfo _Info;

        public GameViewModel(GameInfo info)
        {
            this._Info = info;
        }

        public GameInfo Info => this._Info;

        public uint Id => this._Info.Id;

        public string Type => this._Info.Type;

        public string Name => this._Info.Name;

        public string ImageUrl
        {
            get => this._Info.ImageUrl;
            set => this._Info.ImageUrl = value;
        }

        [ObservableProperty]
        private Bitmap _Logo;

        /// <summary>
        /// Steam reports names asynchronously through the AppDataChanged callback, so the
        /// name can arrive after the game is already on screen.
        /// </summary>
        public void NotifyNameChanged()
        {
            this.OnPropertyChanged(nameof(this.Name));
        }
    }
}
