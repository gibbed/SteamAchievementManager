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

namespace SAM.Game.Stats
{
    internal class IntStatInfo : StatInfo
    {
        public int OriginalValue;
        public int IntValue;
        public int MinValue;
        public int MaxValue;

        public override object Value
        {
            get => IntValue;
            set
            {
                var i = int.Parse((string)value, System.Globalization.CultureInfo.CurrentCulture);

                // STRICT: Check protected flag (permission bit 2)
                if ((Permission & 2) != 0 &&
                    IntValue != i)
                {
                    throw new StatIsProtectedException();
                }

                // STRICT: Check increment-only flag
                if (IsIncrementOnly && i < IntValue)
                {
                    throw new System.InvalidOperationException(
                        $"Stat '{Id}' is increment-only and cannot be decreased from {IntValue} to {i}.");
                }

                // GRACEFUL: Clamp to min/max bounds (prevents setting invalid stat values)
                int originalValue = i;
                if (i < MinValue)
                {
                    i = MinValue;
                }
                else if (i > MaxValue)
                {
                    i = MaxValue;
                }

                // Log if value was clamped
                if (i != originalValue)
                {
                    API.SecurityLogger.LogWithRateLimit(
                        API.LogLevel.Warning,
                        API.LogContext.Validation,
                        $"stat_{Id}",
                        $"Stat '{Id}' value clamped from {originalValue} to {i} (min={MinValue}, max={MaxValue})");
                }

                IntValue = i;
            }
        }

        public override bool IsModified => IntValue != OriginalValue;
    }
}
