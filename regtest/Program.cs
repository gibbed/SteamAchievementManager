using System;
using System.Runtime.InteropServices;
using System.IO;
using Microsoft.Win32;

class Program {
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    internal static extern IntPtr LoadLibraryEx(string path, IntPtr file, uint flags);

    static void Main() {
        string path = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\Software\Valve\Steam", "InstallPath", null);
        Console.WriteLine("Path found: " + path);
        
        if (string.IsNullOrEmpty(path)) {
            Console.WriteLine("ABORT: Registry path is null");
            return;
        }

        string dll = Path.Combine(path, "steamclient.dll");
        Console.WriteLine("Testing: " + dll);
        Console.WriteLine("Exists: " + File.Exists(dll));
        
        IntPtr handle = LoadLibraryEx(dll, IntPtr.Zero, 8);
        if (handle == IntPtr.Zero) {
            int err = Marshal.GetLastWin32Error();
            Console.WriteLine("Failed with error: " + err);
            
            // Try loading tier0_s first
            string dep = Path.Combine(path, "tier0_s.dll");
            IntPtr hDep = LoadLibraryEx(dep, IntPtr.Zero, 8);
            Console.WriteLine("tier0_s.dll load: " + (hDep != IntPtr.Zero ? "SUCCESS" : "FAILED (" + Marshal.GetLastWin32Error() + ")"));
        } else {
            Console.WriteLine("Success!");
        }
    }
}
