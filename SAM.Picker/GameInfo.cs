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

using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace SAM.Picker
{
    internal class GameInfo : INotifyPropertyChanged
    {
        private string _Name;
        private bool _IsSelected;

        public uint Id { get; }
        public string Type { get; }
        public int ImageIndex { get; set; }
        public bool IsFavorite { get; set; }
        public Visibility FavoriteBadgeVisibility => this.IsFavorite ? Visibility.Visible : Visibility.Collapsed;

        public bool IsSelected
        {
            get => this._IsSelected;
            set
            {
                if (this._IsSelected != value)
                {
                    this._IsSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                    OnPropertyChanged(nameof(CardBorderBrush));
                }
            }
        }

        public SolidColorBrush CardBorderBrush => this.IsSelected
            ? new SolidColorBrush(Color.FromRgb(102, 192, 244)) // Steam Blue
            : new SolidColorBrush(Color.FromRgb(42, 56, 78));   // BorderCard

        public string Name
        {
            get => this._Name;
            set => this._Name = value ?? "App " + this.Id.ToString(CultureInfo.InvariantCulture);
        }

        public string ImageUrl { get; set; }

        public System.Windows.Forms.ListViewItem Item { get; set; }

        public GameInfo(uint id, string type)
        {
            this.Id = id;
            this.Type = type;
            this.Name = null;
            this.ImageUrl = null;
            this.IsFavorite = false;
            this.IsSelected = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
