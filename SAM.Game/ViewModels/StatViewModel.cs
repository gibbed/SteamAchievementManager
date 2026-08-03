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
using CommunityToolkit.Mvvm.ComponentModel;
using SAM.Core.Stats;

namespace SAM.Game.ViewModels
{
    public sealed partial class StatViewModel : ObservableObject
    {
        private readonly StatInfo _Info;

        public StatViewModel(StatInfo info)
        {
            this._Info = info;
        }

        /// <summary>
        /// Raised when a typed value is rejected, either because it will not parse or
        /// because Steam protects the stat.
        /// </summary>
        public event Action<StatViewModel, string> EditRejected;

        public StatInfo Info => this._Info;

        public string DisplayName => this._Info.DisplayName;

        public string Extra => this._Info.Extra;

        public string Value
        {
            get => Convert.ToString(this._Info.Value, CultureInfo.CurrentCulture);
            set
            {
                try
                {
                    this._Info.Value = value;
                }
                catch (StatIsProtectedException)
                {
                    this.EditRejected?.Invoke(this, "This stat is protected and cannot be modified.");
                }
                catch (FormatException)
                {
                    this.EditRejected?.Invoke(this, $"'{value}' is not a valid value for {this._Info.DisplayName}.");
                }
                catch (OverflowException)
                {
                    this.EditRejected?.Invoke(this, $"'{value}' is out of range for {this._Info.DisplayName}.");
                }

                // Always report back so a rejected edit reverts to the stored value.
                this.OnPropertyChanged();
            }
        }
    }
}
