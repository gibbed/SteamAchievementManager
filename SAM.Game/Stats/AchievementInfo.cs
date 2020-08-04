/* Copyright (c) 2019 Rick (rick 'at' gibbed 'dot' us)
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

using System.Windows.Forms;

namespace SAM.Game.Stats
{
    internal class AchievementInfo
    {
        public string Description;
        public string IconLocked;
        public string IconNormal;
        public string Id;
        public bool IsAchieved;
        public ListViewItem Item;
        public string Name;
        public int Permission;

        #region public int ImageIndex;

        public int ImageIndex
        {
            get => Item.ImageIndex;

            set => Item.ImageIndex = value;
        }

        #endregion public int ImageIndex;
    }
}