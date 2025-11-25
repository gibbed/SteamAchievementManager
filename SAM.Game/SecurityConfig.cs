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

namespace SAM.Game
{
    /// <summary>
    /// Security configuration constants for Steam Achievement Manager Game layer
    /// </summary>
    internal static class SecurityConfig
    {
        // VDF Parsing Security (Game-specific)
        public const int MAX_VDF_RECURSION_DEPTH = 32; // Realistic Steam VDF depth is 5-8
        public const int MAX_VDF_STRING_LENGTH = 32 * 1024; // 32 KB per string (Steam typically < 1KB)
        public const int MAX_VDF_STRING_ALLOCATION = 32 * 1024 * 1024; // 32 MB total per-string allocation limit

        // XML Processing Security (Picker-specific)
        public const int MAX_XML_ENTITY_EXPANSION = 1024;
        public const long MAX_XML_DOCUMENT_SIZE = 10 * 1024 * 1024; // 10 MB

        // Input Validation (duplicated from SAM.API for convenience - keep in sync with SAM.API.SecurityConfig)
        public const long MIN_APP_ID = 1;
        public const long MAX_APP_ID = 1_000_000_000; // Generous cap for forward compatibility
    }
}
