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

using SAM.API.Interfaces;
using System;
using System.Runtime.InteropServices;

namespace SAM.API.Wrappers
{
    public class SteamUserStats007 : NativeWrapper<ISteamUserStats007>
    {
        #region RequestCurrentStats

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool NativeRequestCurrentStats(IntPtr self);

        public bool RequestCurrentStats()
        {
            return Call<bool, NativeRequestCurrentStats>(Functions.RequestCurrentStats, ObjectAddress);
        }

        #endregion RequestCurrentStats

        #region GetStatValue (int)

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool NativeGetStatInt(IntPtr self, IntPtr name, out int data);

        public bool GetStatValue(string name, out int value)
        {
            using (var nativeName = NativeStrings.StringToStringHandle(name))
            {
                var call = GetFunction<NativeGetStatInt>(Functions.GetStatInteger);
                return call(ObjectAddress, nativeName.Handle, out value);
            }
        }

        #endregion GetStatValue (int)

        #region GetStatValue (float)

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool NativeGetStatFloat(IntPtr self, IntPtr name, out float data);

        public bool GetStatValue(string name, out float value)
        {
            using (var nativeName = NativeStrings.StringToStringHandle(name))
            {
                var call = GetFunction<NativeGetStatFloat>(Functions.GetStatFloat);
                return call(ObjectAddress, nativeName.Handle, out value);
            }
        }

        #endregion GetStatValue (float)

        #region SetStatValue (int)

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool NativeSetStatInt(IntPtr self, IntPtr name, int data);

        public bool SetStatValue(string name, int value)
        {
            using (var nativeName = NativeStrings.StringToStringHandle(name))
            {
                return Call<bool, NativeSetStatInt>(
                    Functions.SetStatInteger,
                    ObjectAddress,
                    nativeName.Handle,
                    value);
            }
        }

        #endregion SetStatValue (int)

        #region SetStatValue (float)

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool NativeSetStatFloat(IntPtr self, IntPtr name, float data);

        public bool SetStatValue(string name, float value)
        {
            using (var nativeName = NativeStrings.StringToStringHandle(name))
            {
                return Call<bool, NativeSetStatFloat>(
                    Functions.SetStatFloat,
                    ObjectAddress,
                    nativeName.Handle,
                    value);
            }
        }

        #endregion SetStatValue (float)

        #region GetAchievement

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool NativeGetAchievement(
            IntPtr self,
            IntPtr name,
            [MarshalAs(UnmanagedType.I1)] out bool isAchieved);

        public bool GetAchievementState(string name, out bool isAchieved)
        {
            using (var nativeName = NativeStrings.StringToStringHandle(name))
            {
                var call = GetFunction<NativeGetAchievement>(Functions.GetAchievement);
                return call(ObjectAddress, nativeName.Handle, out isAchieved);
            }
        }

        #endregion GetAchievement

        #region SetAchievementState

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool NativeSetAchievement(IntPtr self, IntPtr name);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool NativeClearAchievement(IntPtr self, IntPtr name);

        public bool SetAchievement(string name, bool state)
        {
            using (var nativeName = NativeStrings.StringToStringHandle(name))
            {
                if (state == false)
                {
                    return Call<bool, NativeClearAchievement>(
                        Functions.ClearAchievement,
                        ObjectAddress,
                        nativeName.Handle);
                }

                return Call<bool, NativeSetAchievement>(
                    Functions.SetAchievement,
                    ObjectAddress,
                    nativeName.Handle);
            }
        }

        #endregion SetAchievementState

        #region StoreStats

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool NativeStoreStats(IntPtr self);

        public bool StoreStats()
        {
            return Call<bool, NativeStoreStats>(Functions.StoreStats, ObjectAddress);
        }

        #endregion StoreStats

        #region GetAchievementIcon

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate int NativeGetAchievementIcon(IntPtr self, IntPtr name);

        public int GetAchievementIcon(string name)
        {
            using (var nativeName = NativeStrings.StringToStringHandle(name))
            {
                return Call<int, NativeGetAchievementIcon>(
                    Functions.GetAchievementIcon,
                    ObjectAddress,
                    nativeName.Handle);
            }
        }

        #endregion GetAchievementIcon

        #region GetAchievementDisplayAttribute

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate IntPtr NativeGetAchievementDisplayAttribute(IntPtr self, IntPtr name, IntPtr key);

        public string GetAchievementDisplayAttribute(string name, string key)
        {
            using (var nativeName = NativeStrings.StringToStringHandle(name))
            using (var nativeKey = NativeStrings.StringToStringHandle(key))
            {
                var result = Call<IntPtr, NativeGetAchievementDisplayAttribute>(
                    Functions.GetAchievementDisplayAttribute,
                    ObjectAddress,
                    nativeName.Handle,
                    nativeKey.Handle);
                return NativeStrings.PointerToString(result);
            }
        }

        #endregion GetAchievementDisplayAttribute

        #region ResetAllStats

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool NativeResetAllStats(IntPtr self, [MarshalAs(UnmanagedType.I1)] bool achievementsToo);

        public bool ResetAllStats(bool achievementsToo)
        {
            return Call<bool, NativeResetAllStats>(
                Functions.ResetAllStats,
                ObjectAddress,
                achievementsToo);
        }

        #endregion ResetAllStats
    }
}