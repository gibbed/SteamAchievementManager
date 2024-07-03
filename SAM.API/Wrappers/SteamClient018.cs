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
    public class SteamClient018 : NativeWrapper<ISteamClient018>
    {
        #region CreateSteamPipe

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate int NativeCreateSteamPipe(IntPtr self);

        public int CreateSteamPipe()
        {
            return Call<int, NativeCreateSteamPipe>(Functions.CreateSteamPipe, ObjectAddress);
        }

        #endregion CreateSteamPipe

        #region ReleaseSteamPipe

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool NativeReleaseSteamPipe(IntPtr self, int pipe);

        public bool ReleaseSteamPipe(int pipe)
        {
            return Call<bool, NativeReleaseSteamPipe>(Functions.ReleaseSteamPipe, ObjectAddress, pipe);
        }

        #endregion ReleaseSteamPipe

        #region CreateLocalUser

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate int NativeCreateLocalUser(IntPtr self, ref int pipe, Types.AccountType type);

        public int CreateLocalUser(ref int pipe, Types.AccountType type)
        {
            var call = GetFunction<NativeCreateLocalUser>(Functions.CreateLocalUser);
            return call(ObjectAddress, ref pipe, type);
        }

        #endregion CreateLocalUser

        #region ConnectToGlobalUser

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate int NativeConnectToGlobalUser(IntPtr self, int pipe);

        public int ConnectToGlobalUser(int pipe)
        {
            return Call<int, NativeConnectToGlobalUser>(
                Functions.ConnectToGlobalUser,
                ObjectAddress,
                pipe);
        }

        #endregion ConnectToGlobalUser

        #region ReleaseUser

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void NativeReleaseUser(IntPtr self, int pipe, int user);

        public void ReleaseUser(int pipe, int user)
        {
            Call<NativeReleaseUser>(Functions.ReleaseUser, ObjectAddress, pipe, user);
        }

        #endregion ReleaseUser

        #region SetLocalIPBinding

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void NativeSetLocalIPBinding(IntPtr self, uint host, ushort port);

        public void SetLocalIPBinding(uint host, ushort port)
        {
            Call<NativeSetLocalIPBinding>(Functions.SetLocalIPBinding, ObjectAddress, host, port);
        }

        #endregion SetLocalIPBinding

        #region GetISteamUser

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate IntPtr NativeGetISteamUser(IntPtr self, int user, int pipe, IntPtr version);

        private TClass GetISteamUser<TClass>(int user, int pipe, string version)
            where TClass : INativeWrapper, new()
        {
            using (var nativeVersion = NativeStrings.StringToStringHandle(version))
            {
                IntPtr address = Call<IntPtr, NativeGetISteamUser>(
                    Functions.GetISteamUser,
                    ObjectAddress,
                    user,
                    pipe,
                    nativeVersion.Handle);
                var result = new TClass();
                result.SetupFunctions(address);
                return result;
            }
        }

        #endregion GetISteamUser

        #region GetSteamUser012

        public SteamUser012 GetSteamUser012(int user, int pipe)
        {
            return GetISteamUser<SteamUser012>(user, pipe, "SteamUser012");
        }

        #endregion GetSteamUser012

        #region GetISteamUserStats

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate IntPtr NativeGetISteamUserStats(IntPtr self, int user, int pipe, IntPtr version);

        private TClass GetISteamUserStats<TClass>(int user, int pipe, string version)
            where TClass : INativeWrapper, new()
        {
            using (var nativeVersion = NativeStrings.StringToStringHandle(version))
            {
                IntPtr address = Call<IntPtr, NativeGetISteamUserStats>(
                    Functions.GetISteamUserStats,
                    ObjectAddress,
                    user,
                    pipe,
                    nativeVersion.Handle);
                var result = new TClass();
                result.SetupFunctions(address);
                return result;
            }
        }

        #endregion GetISteamUserStats

        #region GetSteamUserStats007

        public SteamUserStats007 GetSteamUserStats006(int user, int pipe)
        {
            return GetISteamUserStats<SteamUserStats007>(user, pipe, "STEAMUSERSTATS_INTERFACE_VERSION007");
        }

        #endregion GetSteamUserStats007

        #region GetISteamUtils

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate IntPtr NativeGetISteamUtils(IntPtr self, int pipe, IntPtr version);

        public TClass GetISteamUtils<TClass>(int pipe, string version)
            where TClass : INativeWrapper, new()
        {
            using (var nativeVersion = NativeStrings.StringToStringHandle(version))
            {
                IntPtr address = Call<IntPtr, NativeGetISteamUtils>(
                    Functions.GetISteamUtils,
                    ObjectAddress,
                    pipe,
                    nativeVersion.Handle);
                var result = new TClass();
                result.SetupFunctions(address);
                return result;
            }
        }

        #endregion GetISteamUtils

        #region GetSteamUtils004

        public SteamUtils005 GetSteamUtils004(int pipe)
        {
            return GetISteamUtils<SteamUtils005>(pipe, "SteamUtils005");
        }

        #endregion GetSteamUtils004

        #region GetISteamApps

        private delegate IntPtr NativeGetISteamApps(int user, int pipe, IntPtr version);

        private TClass GetISteamApps<TClass>(int user, int pipe, string version)
            where TClass : INativeWrapper, new()
        {
            using (var nativeVersion = NativeStrings.StringToStringHandle(version))
            {
                IntPtr address = Call<IntPtr, NativeGetISteamApps>(
                    Functions.GetISteamApps,
                    user,
                    pipe,
                    nativeVersion.Handle);
                var result = new TClass();
                result.SetupFunctions(address);
                return result;
            }
        }

        #endregion GetISteamApps

        #region GetSteamApps001

        public SteamApps001 GetSteamApps001(int user, int pipe)
        {
            return GetISteamApps<SteamApps001>(user, pipe, "STEAMAPPS_INTERFACE_VERSION001");
        }

        #endregion GetSteamApps001

        #region GetSteamApps008

        public SteamApps008 GetSteamApps008(int user, int pipe)
        {
            return GetISteamApps<SteamApps008>(user, pipe, "STEAMAPPS_INTERFACE_VERSION008");
        }

        #endregion GetSteamApps008
    }
}