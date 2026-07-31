using System;
using System.Runtime.InteropServices;
using System.IO;
using Microsoft.Win32;

public class Test {
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    internal static extern IntPtr LoadLibraryEx(string path, IntPtr file, uint flags);

    public static void Main() {
        string path = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\Software\Valve\Steam", "InstallPath", null);
        Console.WriteLine("Path: " + path);
        
        string dll = Path.Combine(path, "steamclient.dll");
        Console.WriteLine("Testing: " + dll);
        
        IntPtr handle = LoadLibraryEx(dll, IntPtr.Zero, 8);
        if (handle == IntPtr.Zero) {
            int err = Marshal.GetLastWin32Error();
            Console.WriteLine("Failed with error: " + err);
        } else {
            Console.WriteLine("Success!");
        }
    }
}
