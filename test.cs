using Microsoft.Win32;
using System;
public class Test {
    public static void Main() {
        var val = Registry.GetValue(@"HKEY_LOCAL_MACHINE\Software\Valve\Steam", "InstallPath", "NotFound");
        Console.WriteLine("HLKM Valve: " + val);
        val = Registry.GetValue(@"HKEY_LOCAL_MACHINE\Software\WOW6432Node\Valve\Steam", "InstallPath", "NotFound");
        Console.WriteLine("HLKM WOW6432Node Valve: " + val);
    }
}
