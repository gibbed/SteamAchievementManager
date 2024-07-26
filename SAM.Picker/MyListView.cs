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
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SAM.Picker
{
    internal class MyListView : ListView
    {
        public event ScrollEventHandler Scroll;

        public MyListView()
        {
            base.DoubleBuffered = true;
        }

        protected virtual void OnScroll(ScrollEventArgs e)
        {
            this.Scroll?.Invoke(this, e);
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            switch (m.Msg)
            {
                case 0x0100: // WM_KEYDOWN
                {
                    ScrollEventType type;
                    if (TranslateKeyScrollEvent((Keys)m.WParam.ToInt32(), out type) == true)
                    {
                        this.OnScroll(new(type, Win32.GetScrollPos(this.Handle, 1 /*SB_VERT*/)));
                    }
                    break;
                }

                case 0x0115: // WM_VSCROLL
                case 0x020A: // WM_MOUSEWHEEL
                {
                    this.OnScroll(new(ScrollEventType.EndScroll, Win32.GetScrollPos(this.Handle, 1 /*SB_VERT*/)));
                    break;
                }
            }
        }

        private static bool TranslateKeyScrollEvent(Keys keys, out ScrollEventType type)
        {
            switch (keys)
            {
                case Keys.Down:
                {
                    type = ScrollEventType.SmallIncrement;
                    return true;
                }

                case Keys.Up:
                {
                    type = ScrollEventType.SmallDecrement;
                    return true;
                }

                case Keys.PageDown:
                {
                    type = ScrollEventType.LargeIncrement;
                    return true;
                }

                case Keys.PageUp:
                {
                    type = ScrollEventType.SmallDecrement;
                    return true;
                }

                case Keys.Home:
                {
                    type = ScrollEventType.First;
                    return true;
                }

                case Keys.End:
                {
                    type = ScrollEventType.Last;
                    return true;
                }
            }

            type = default;
            return false;
        }

        private static class Win32
        {
            [DllImport("user32.dll", SetLastError = true)]
            public static extern int GetScrollPos(IntPtr hWnd, int nBar);
        }
    }
}
