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
using static SAM.Core.InvariantShorthand;

namespace SAM.Core
{
    /// <summary>
    /// What Steam knows about the signed in user's games: whether they own one, what
    /// it is called, and where its capsule image lives.
    /// </summary>
    public sealed class GameCatalog
    {
        private readonly API.Client _Client;

        public GameCatalog(API.Client client)
        {
            this._Client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public bool OwnsGame(uint id)
        {
            return this._Client.SteamApps008.IsSubscribedApp(id);
        }

        public string GetName(uint id)
        {
            return this._Client.SteamApps001.GetAppData(id, "name");
        }

        /// <summary>
        /// Resolves the best available capsule for a game, preferring the current
        /// language, then English, then the older community logo. Returns null when
        /// Steam has no artwork cached for it.
        /// </summary>
        public string GetLogoUrl(uint id)
        {
            string currentLanguage = this._Client.SteamApps008.GetCurrentGameLanguage();

            string candidate = this._Client.SteamApps001.GetAppData(id, _($"small_capsule/{currentLanguage}"));
            if (string.IsNullOrEmpty(candidate) == false)
            {
                return _($"https://shared.cloudflare.steamstatic.com/store_item_assets/steam/apps/{id}/{candidate}");
            }

            if (currentLanguage != "english")
            {
                candidate = this._Client.SteamApps001.GetAppData(id, "small_capsule/english");
                if (string.IsNullOrEmpty(candidate) == false)
                {
                    return _($"https://shared.cloudflare.steamstatic.com/store_item_assets/steam/apps/{id}/{candidate}");
                }
            }

            candidate = this._Client.SteamApps001.GetAppData(id, "logo");
            if (string.IsNullOrEmpty(candidate) == false)
            {
                return _($"https://cdn.steamstatic.com/steamcommunity/public/images/apps/{id}/{candidate}.jpg");
            }

            return null;
        }
    }
}
