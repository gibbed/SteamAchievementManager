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
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace SAM.API
{
    public static class Steam
    {
        private struct Native
        {
            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool SetDllDirectory(string path);
        }

        private static IntPtr _Handle = IntPtr.Zero;

        #region Install path
        /// <summary>
        /// Points SAM at a Steam installation that the probes below do not know about.
        /// </summary>
        private const string InstallPathVariable = "SAM_STEAM_PATH";

        /// <summary>
        /// Prefers a candidate that actually holds a client module. Stale directories are
        /// common: a leftover ~/.steam/root will sit ahead of the live install in the probe
        /// order and existence alone is not enough to tell them apart. Falls back to the
        /// first directory that exists so callers only wanting a path still get one.
        /// </summary>
        public static string GetInstallPath()
        {
            string firstExisting = null;

            foreach (string path in EnumerateExistingInstallPaths())
            {
                firstExisting ??= path;

                if (FindClientModule(path) != null)
                {
                    return path;
                }
            }

            return firstExisting;
        }

        private static IEnumerable<string> EnumerateExistingInstallPaths()
        {
            foreach (string candidate in EnumerateInstallPaths())
            {
                if (string.IsNullOrEmpty(candidate) == true)
                {
                    continue;
                }

                if (Directory.Exists(candidate) == false)
                {
                    continue;
                }

                // The Linux probes go through symlinks that Steam maintains, so resolve
                // them here. Callers compare this against their own location to refuse
                // running from inside the Steam directory, which needs a canonical path.
                // Trim before resolving: a trailing separator makes ResolveLinkTarget
                // report no link, which would hand back two different answers for the
                // same directory depending on how the candidate was spelled.
                string path = Path.TrimEndingDirectorySeparator(Path.GetFullPath(candidate));
                FileSystemInfo target = Directory.ResolveLinkTarget(path, true);
                if (target != null)
                {
                    path = Path.TrimEndingDirectorySeparator(target.FullName);
                }

                yield return path;
            }
        }

        private static IEnumerable<string> EnumerateInstallPaths()
        {
            yield return Environment.GetEnvironmentVariable(InstallPathVariable);

            if (OperatingSystem.IsWindows() == true)
            {
                yield return GetWindowsInstallPath();
                yield break;
            }

            string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (string.IsNullOrEmpty(home) == true)
            {
                yield break;
            }

            if (OperatingSystem.IsMacOS() == true)
            {
                yield return Path.Combine(home, "Library", "Application Support", "Steam");
                yield break;
            }

            string dataHome = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
            if (string.IsNullOrEmpty(dataHome) == false)
            {
                yield return Path.Combine(dataHome, "Steam");
            }

            // Steam keeps these two as symlinks to wherever it actually lives, so they
            // also cover installations that have been moved off the default location.
            yield return Path.Combine(home, ".steam", "root");
            yield return Path.Combine(home, ".steam", "steam");
            yield return Path.Combine(home, ".local", "share", "Steam");
            yield return Path.Combine(home, ".var", "app", "com.valvesoftware.Steam", ".local", "share", "Steam");
        }

        private static string GetWindowsInstallPath()
        {
            if (OperatingSystem.IsWindows())
            {
                return (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\Software\Valve\Steam", "InstallPath", null);
            }

            return null;
        }
        #endregion

        #region Client module
        private static IEnumerable<string> EnumerateClientModules(string installPath)
        {
            bool is64Bit = IntPtr.Size == 8;

            if (OperatingSystem.IsWindows() == true)
            {
                yield return Path.Combine(installPath, is64Bit == true ? "steamclient64.dll" : "steamclient.dll");
                yield break;
            }

            if (OperatingSystem.IsMacOS() == true)
            {
                yield return Path.Combine(
                    installPath,
                    "Steam.AppBundle",
                    "Steam",
                    "Contents",
                    "MacOS",
                    "steamclient.dylib");
                yield return Path.Combine(installPath, "steamclient.dylib");
                yield break;
            }

            yield return Path.Combine(installPath, is64Bit == true ? "linux64" : "linux32", "steamclient.so");
            yield return Path.Combine(installPath, is64Bit == true ? "steamrt64" : "steamrt32", "steamclient.so");
            yield return Path.Combine(installPath, is64Bit == true ? "ubuntu12_64" : "ubuntu12_32", "steamclient.so");
        }

        private static string FindClientModule(string installPath)
        {
            foreach (string candidate in EnumerateClientModules(installPath))
            {
                if (File.Exists(candidate) == true)
                {
                    return candidate;
                }
            }

            return null;
        }

        private static IntPtr LoadClientModule()
        {
            foreach (string installPath in EnumerateExistingInstallPaths())
            {
                string candidate = FindClientModule(installPath);
                if (candidate == null)
                {
                    continue;
                }

                // steamclient resolves its sibling libraries through the process search
                // path instead of its own directory, so Windows needs that directory added
                // first. Unix hosts get the same from the RPATH the libraries were built with.
                if (OperatingSystem.IsWindows() == true)
                {
                    Native.SetDllDirectory(installPath);
                }

                if (NativeLibrary.TryLoad(candidate, out IntPtr module) == true)
                {
                    return module;
                }
            }

            return IntPtr.Zero;
        }

        private static TDelegate GetExportFunction<TDelegate>(IntPtr module, string name)
            where TDelegate : Delegate
        {
            if (NativeLibrary.TryGetExport(module, name, out IntPtr address) == false)
            {
                return null;
            }

            return Marshal.GetDelegateForFunctionPointer<TDelegate>(address);
        }
        #endregion

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private delegate IntPtr NativeCreateInterface(string version, IntPtr returnCode);

        private static NativeCreateInterface _CallCreateInterface;

        public static TClass CreateInterface<TClass>(string version)
            where TClass : INativeWrapper, new()
        {
            IntPtr address = _CallCreateInterface(version, IntPtr.Zero);

            if (address == IntPtr.Zero)
            {
                return default;
            }

            TClass instance = new();
            instance.SetupFunctions(address);
            return instance;
        }

        // The call handle is written through a pointer, so widening the slot cannot change
        // the ABI. It can only stop a 64 bit write from spilling into the neighbouring one.
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool NativeSteamGetCallback(int pipe, out Types.CallbackMessage message, out CallHandle call);

        private static NativeSteamGetCallback _CallSteamBGetCallback;

        public static bool GetCallback(int pipe, out Types.CallbackMessage message, out CallHandle call)
        {
            return _CallSteamBGetCallback(pipe, out message, out call);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool NativeSteamFreeLastCallback(int pipe);

        private static NativeSteamFreeLastCallback _CallSteamFreeLastCallback;

        public static bool FreeLastCallback(int pipe)
        {
            return _CallSteamFreeLastCallback(pipe);
        }

        public static bool Load()
        {
            if (_Handle != IntPtr.Zero)
            {
                return true;
            }

            IntPtr module = LoadClientModule();
            if (module == IntPtr.Zero)
            {
                return false;
            }

            _CallCreateInterface = GetExportFunction<NativeCreateInterface>(module, "CreateInterface");
            if (_CallCreateInterface == null)
            {
                return false;
            }

            _CallSteamBGetCallback = GetExportFunction<NativeSteamGetCallback>(module, "Steam_BGetCallback");
            if (_CallSteamBGetCallback == null)
            {
                return false;
            }

            _CallSteamFreeLastCallback = GetExportFunction<NativeSteamFreeLastCallback>(module, "Steam_FreeLastCallback");
            if (_CallSteamFreeLastCallback == null)
            {
                return false;
            }

            _Handle = module;
            return true;
        }
    }
}
