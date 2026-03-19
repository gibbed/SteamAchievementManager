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
using System.Net;

namespace SAM.API
{
    public static class ImageCache
    {
        private static string CacheDirectory
        {
            get
            {
                string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cache");
                if (Directory.Exists(dir) == false)
                {
                    Directory.CreateDirectory(dir);
                }
                return dir;
            }
        }

        public static string GetLocalCachePath(string url)
        {
            if (string.IsNullOrEmpty(url)) return null;

            try
            {
                Uri uri = new Uri(url);
                string fileName = Path.GetFileName(uri.LocalPath);
                if (string.IsNullOrEmpty(fileName)) return null;

                string fullPath = Path.Combine(CacheDirectory, fileName);
                return fullPath;
            }
            catch
            {
                return null;
            }
        }

        public static byte[] GetOrDownloadImage(string url)
        {
            if (string.IsNullOrEmpty(url)) return null;

            string localPath = GetLocalCachePath(url);
            if (string.IsNullOrEmpty(localPath) == false && File.Exists(localPath))
            {
                try
                {
                    return File.ReadAllBytes(localPath);
                }
                catch { }
            }

            try
            {
                using WebClient client = new();
                byte[] bytes = client.DownloadData(url);
                if (bytes != null && bytes.Length > 0 && string.IsNullOrEmpty(localPath) == false)
                {
                    try
                    {
                        File.WriteAllBytes(localPath, bytes);
                    }
                    catch { }
                }
                return bytes;
            }
            catch
            {
                return null;
            }
        }
    }
}
