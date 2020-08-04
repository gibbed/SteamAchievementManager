﻿/* Copyright (c) 2019 Rick (rick 'at' gibbed 'dot' us)
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

using SAM.API.Interfaces;
using System;
using System.Runtime.InteropServices;

namespace SAM.API.Wrappers
{
    public class SteamApps001 : NativeWrapper<ISteamApps001>
    {
        #region GetAppData

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate int NativeGetAppData(
            IntPtr self,
            uint appId,
            IntPtr key,
            IntPtr value,
            int valueLength);

        public string GetAppData(uint appId, string key)
        {
            using (var nativeHandle = NativeStrings.StringToStringHandle(key))
            {
                const int valueLength = 1024;
                var valuePointer = Marshal.AllocHGlobal(valueLength);
                var result = Call<int, NativeGetAppData>(
                    Functions.GetAppData,
                    ObjectAddress,
                    appId,
                    nativeHandle.Handle,
                    valuePointer,
                    valueLength);
                var value = result == 0 ? null : NativeStrings.PointerToString(valuePointer, valueLength);
                Marshal.FreeHGlobal(valuePointer);
                return value;
            }
        }

        #endregion GetAppData
    }
}