/* Copyright (c) 2017 Rick (rick 'at' gibbed 'dot' us)
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
using System.Windows.Forms;

namespace SAM.Picker
{
    internal class GameInfo
    {
        public long Id;
        public string Type;
        public ListViewItem Item;

        #region public string Name;
        public string Name
        {
            get { return this.Item.Text; }

            set { this.Item.Text = value ?? "App " + this.Id.ToString(CultureInfo.InvariantCulture); }
        }
        #endregion

        #region public int ImageIndex;
        public int ImageIndex
        {
            get { return this.Item.ImageIndex; }

            set { this.Item.ImageIndex = value; }
        }
        #endregion

        public string Logo;

        public GameInfo(Int64 id, string type)
        {
            this.Id = id;
            this.Type = type;
            this.Item = new ListViewItem()
            {
                Tag = this,
            };
            this.Name = null;
            this.ImageIndex = 0;
            this.Logo = null;
        }
    }
}
