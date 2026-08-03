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
using System.Threading;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace SAM.Core
{
    /// <summary>
    /// One entry of the published game list. <see cref="Type"/> is the category the
    /// picker filters on, and is "normal" when the list does not say otherwise.
    /// </summary>
    public readonly record struct GameListEntry(uint Id, string Type);

    public static class GameList
    {
        public static readonly Uri DefaultUri = new("https://gib.me/sam/games.xml");

        public static async Task<List<GameListEntry>> DownloadAsync(
            CancellationToken cancellationToken = default)
        {
            byte[] bytes = await Downloader
                .GetBytesAsync(DefaultUri, cancellationToken)
                .ConfigureAwait(false);
            return Parse(bytes);
        }

        /// <summary>
        /// Blocking form, for callers already running off the user interface thread.
        /// </summary>
        public static List<GameListEntry> Download(CancellationToken cancellationToken = default)
        {
            return Parse(Downloader.GetBytes(DefaultUri, cancellationToken));
        }

        public static List<GameListEntry> Parse(byte[] bytes)
        {
            List<GameListEntry> entries = new();

            using MemoryStream stream = new(bytes, false);
            XPathDocument document = new(stream);
            XPathNavigator navigator = document.CreateNavigator();
            XPathNodeIterator nodes = navigator.Select("/games/game");

            while (nodes.MoveNext() == true)
            {
                string type = nodes.Current.GetAttribute("type", "");
                if (string.IsNullOrEmpty(type) == true)
                {
                    type = "normal";
                }

                entries.Add(new((uint)nodes.Current.ValueAsLong, type));
            }

            return entries;
        }
    }
}
