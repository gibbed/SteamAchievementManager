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
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace SAM.API
{
    public static class Steam
    {
        private struct Native
        {
            [DllImport("kernel32.dll", SetLastError = true, BestFitMapping = false, ThrowOnUnmappableChar = true)]
            internal static extern IntPtr GetProcAddress(IntPtr module, string name);

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern IntPtr LoadLibraryEx(string path, IntPtr file, uint flags);

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool SetDllDirectory(string path);

            internal const uint LoadWithAlteredSearchPath = 8;
        }

        private static Delegate GetExportDelegate<TDelegate>(IntPtr module, string name)
        {
            IntPtr address = Native.GetProcAddress(module, name);
            return address == IntPtr.Zero ? null : Marshal.GetDelegateForFunctionPointer(address, typeof(TDelegate));
        }

        private static TDelegate GetExportFunction<TDelegate>(IntPtr module, string name)
            where TDelegate : class
        {
            return (TDelegate)((object)GetExportDelegate<TDelegate>(module, name));
        }

        private static IntPtr _Handle = IntPtr.Zero;

        public static string GetInstallPath()
        {
            // Try HKLM first (system-wide install), then HKCU (user install)
            string path = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\Software\Valve\Steam", "InstallPath", null);

            if (string.IsNullOrWhiteSpace(path))
            {
                path = (string)Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "InstallPath", null);
            }

            return path;
        }

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

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool NativeSteamGetCallback(int pipe, out Types.CallbackMessage message, out int call);

        private static NativeSteamGetCallback _CallSteamBGetCallback;

        public static bool GetCallback(int pipe, out Types.CallbackMessage message, out int call)
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

            // Get and validate Steam installation path
            string path = GetInstallPath();
            if (string.IsNullOrWhiteSpace(path))
            {
                SecurityLogger.Log(LogLevel.Error, LogContext.Init, "DLL load failed: Steam install path not found in registry (HKLM or HKCU)");
                return false;
            }

            // Validate path length
            if (path.Length > SecurityConfig.MAX_PATH_LENGTH_WARN)
            {
                SecurityLogger.Log(LogLevel.Warning, LogContext.Init, $"Path length {path.Length} exceeds MAX_PATH (260), attempting anyway - may fail on .NET 4.8");
            }

            // Canonicalize path (handles some path issues, but doesn't resolve symlinks/junctions)
            try
            {
                path = Path.GetFullPath(path);
            }
            catch (Exception ex)
            {
                SecurityLogger.Log(LogLevel.Error, LogContext.Init, $"DLL load failed: Invalid path from registry: {ex.Message}");
                return false;
            }

            // Validate path is rooted and doesn't contain dangerous characters
            if (!Path.IsPathRooted(path))
            {
                SecurityLogger.Log(LogLevel.Error, LogContext.Init, "DLL load failed: Steam path is not rooted");
                return false;
            }

            if (path.IndexOfAny(Path.GetInvalidPathChars()) >= 0 || path.Contains("\0"))
            {
                SecurityLogger.Log(LogLevel.Error, LogContext.Init, "DLL load failed: Steam path contains invalid characters");
                return false;
            }

            // Check for path traversal attempts
            if (path.Contains(".."))
            {
                SecurityLogger.Log(LogLevel.Error, LogContext.Init, "DLL load failed: Steam path contains traversal sequence (..)");
                return false;
            }

            // Verify Steam directory exists
            if (!Directory.Exists(path))
            {
                SecurityLogger.Log(LogLevel.Error, LogContext.Init, $"DLL load failed: Steam directory does not exist: {path}");
                return false;
            }

            // Verify bin subdirectory exists
            string binPath = Path.Combine(path, "bin");
            if (!Directory.Exists(binPath))
            {
                SecurityLogger.Log(LogLevel.Error, LogContext.Init, $"DLL load failed: Steam bin directory does not exist: {binPath}");
                return false;
            }

            // Note: We don't call SetDllDirectory because LoadLibraryEx with an absolute path
            // doesn't use the DLL search path. The absolute dllPath below bypasses search logic.

            // Construct steamclient.dll path
            string dllPath = Path.Combine(path, "steamclient.dll");

            // Verify DLL file exists
            if (!File.Exists(dllPath))
            {
                SecurityLogger.Log(LogLevel.Error, LogContext.Init, $"DLL load failed: steamclient.dll not found at {dllPath}");
                return false;
            }

            // Check for DLL hijacking attempt (DLL in current directory)
            string currentDir = Directory.GetCurrentDirectory();
            string currentDirDll = Path.Combine(currentDir, "steamclient.dll");
            if (File.Exists(currentDirDll) && !currentDirDll.Equals(dllPath, StringComparison.OrdinalIgnoreCase))
            {
                SecurityLogger.Log(LogLevel.Error, LogContext.Init, "DLL load failed: Potential DLL hijacking detected (steamclient.dll in current directory)");
                return false;
            }

            // Log the resolved DLL path (Debug only)
            SecurityLogger.Log(LogLevel.Info, LogContext.Init, $"Loading Steam DLL from: {dllPath}");

            // Load the DLL
            IntPtr module = Native.LoadLibraryEx(dllPath, IntPtr.Zero, Native.LoadWithAlteredSearchPath);
            if (module == IntPtr.Zero)
            {
                int errorCode = Marshal.GetLastWin32Error();
                SecurityLogger.Log(LogLevel.Error, LogContext.Init, $"DLL load failed: LoadLibraryEx returned null (error code: {errorCode})");
                return false;
            }

            // Get required function exports
            _CallCreateInterface = GetExportFunction<NativeCreateInterface>(module, "CreateInterface");
            if (_CallCreateInterface == null)
            {
                SecurityLogger.Log(LogLevel.Error, LogContext.Init, "DLL load failed: CreateInterface export not found");
                return false;
            }

            _CallSteamBGetCallback = GetExportFunction<NativeSteamGetCallback>(module, "Steam_BGetCallback");
            if (_CallSteamBGetCallback == null)
            {
                SecurityLogger.Log(LogLevel.Error, LogContext.Init, "DLL load failed: Steam_BGetCallback export not found");
                return false;
            }

            _CallSteamFreeLastCallback = GetExportFunction<NativeSteamFreeLastCallback>(module, "Steam_FreeLastCallback");
            if (_CallSteamFreeLastCallback == null)
            {
                SecurityLogger.Log(LogLevel.Error, LogContext.Init, "DLL load failed: Steam_FreeLastCallback export not found");
                return false;
            }

            _Handle = module;
            SecurityLogger.Log(LogLevel.Info, LogContext.Init, "Steam DLL loaded successfully");
            return true;
        }
    }
}
