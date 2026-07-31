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
using System.Diagnostics;
using System.IO;

namespace SAM.API
{
    /// <summary>
    /// Locates the sibling SAM executables. Only Windows gives the host an .exe
    /// suffix, and resolving against the current directory finds nothing when SAM
    /// is launched from somewhere else.
    /// </summary>
    public static class AppHost
    {
        public static string GetPath(string name)
        {
            string fileName = OperatingSystem.IsWindows() == true
                ? name + ".exe"
                : name;
            return Path.Combine(AppContext.BaseDirectory, fileName);
        }

        public static Process Start(string name, params string[] arguments)
        {
            ProcessStartInfo startInfo = new(GetPath(name));
            foreach (string argument in arguments)
            {
                startInfo.ArgumentList.Add(argument);
            }
            return Process.Start(startInfo);
        }
    }
}
