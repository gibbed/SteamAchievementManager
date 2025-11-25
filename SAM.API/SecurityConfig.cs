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

namespace SAM.API
{
    /// <summary>
    /// Security configuration constants for Steam Achievement Manager API layer
    /// </summary>
    public static class SecurityConfig
    {
        // DLL Loading Security
        public const int MAX_PATH_LENGTH_WARN = 260; // Windows MAX_PATH limit

        // Network Download Security (used by SafeWebClient)
        public const int MAX_ICON_SIZE_BYTES = 1 * 1024 * 1024; // 1 MB for achievement icons
        public const int MAX_XML_SIZE_BYTES = 10 * 1024 * 1024; // 10 MB for game list XML
        public const int DOWNLOAD_TIMEOUT_MS = 30000; // 30 seconds timeout
        public const int MAX_DOWNLOAD_RETRIES = 2; // Small retry budget to avoid UI stalls

        // Input Validation (shared across projects)
        public const long MIN_APP_ID = 1;
        public const long MAX_APP_ID = 1_000_000_000; // Generous cap for forward compatibility
    }
}
