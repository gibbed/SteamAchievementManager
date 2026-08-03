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
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using SAM.Core.Stats;

namespace SAM.Game.ViewModels
{
    public sealed partial class AchievementViewModel : ObservableObject
    {
        private readonly AchievementInfo _Info;

        public AchievementViewModel(AchievementInfo info)
        {
            this._Info = info;
        }

        /// <summary>
        /// Raised when the user tries to toggle an achievement Steam will not let us set.
        /// </summary>
        public event Action<AchievementViewModel> ProtectedChangeRejected;

        public AchievementInfo Info => this._Info;

        public string Id => this._Info.Id;

        public string Name => this._Info.Name;

        public string Description => this._Info.Description;

        public string UnlockTimeText => this._Info.UnlockTime?.ToString() ?? "";

        public bool IsProtected => (this._Info.Permission & 3) != 0;

        [ObservableProperty]
        private Bitmap _Icon;

        public bool IsAchieved
        {
            get => this._Info.IsAchieved;
            set
            {
                if (this._Info.IsAchieved == value)
                {
                    return;
                }

                if (this.IsProtected == true)
                {
                    this.ProtectedChangeRejected?.Invoke(this);
                    // Report no change so the check box snaps back to the real state.
                    this.OnPropertyChanged();
                    return;
                }

                this._Info.IsAchieved = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Bulk operations go through here so a protected achievement is skipped quietly
        /// rather than raising a dialog per entry.
        /// </summary>
        public void SetAchievedQuietly(bool value)
        {
            if (this.IsProtected == true || this._Info.IsAchieved == value)
            {
                return;
            }

            this._Info.IsAchieved = value;
            this.OnPropertyChanged(nameof(this.IsAchieved));
        }
    }
}
