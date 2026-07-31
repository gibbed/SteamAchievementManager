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
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SAM.Core
{
    /// <summary>
    /// One shared <see cref="HttpClient"/> for the whole process. Creating one per
    /// download exhausts sockets, which is the trap the obsoleted WebClient encouraged.
    /// </summary>
    public static class Downloader
    {
        private static readonly HttpClient _Client = Create();

        private static HttpClient Create()
        {
            HttpClient client = new()
            {
                Timeout = TimeSpan.FromSeconds(30),
            };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("SteamAchievementManager/7.0");
            return client;
        }

        public static Task<byte[]> GetBytesAsync(Uri uri, CancellationToken cancellationToken = default)
        {
            return _Client.GetByteArrayAsync(uri, cancellationToken);
        }

        /// <summary>
        /// Blocking form, for callers already running off the user interface thread.
        /// </summary>
        public static byte[] GetBytes(Uri uri, CancellationToken cancellationToken = default)
        {
            return GetBytesAsync(uri, cancellationToken).GetAwaiter().GetResult();
        }
    }
}
