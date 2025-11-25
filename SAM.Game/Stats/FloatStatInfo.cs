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
    internal class FloatStatInfo : StatInfo
    {
        public float OriginalValue;
        public float FloatValue;
        public float MinValue;
        public float MaxValue;

        public override object Value
        {
            get => FloatValue;
            set
            {
                var f = float.Parse((string)value, System.Globalization.CultureInfo.CurrentCulture);

                // STRICT: Reject NaN and Infinity
                if (float.IsNaN(f) || float.IsInfinity(f))
                {
                    throw new System.InvalidOperationException(
                        $"Stat '{Id}' cannot be set to NaN or Infinity.");
                }

                // STRICT: Check protected flag (permission bit 2)
                if ((Permission & 2) != 0 &&
                    FloatValue.Equals(f) == false)
                {
                    throw new StatIsProtectedException();
                }

                // STRICT: Check increment-only flag
                if (IsIncrementOnly && f < FloatValue)
                {
                    throw new System.InvalidOperationException(
                        $"Stat '{Id}' is increment-only and cannot be decreased from {FloatValue} to {f}.");
                }

                // GRACEFUL: Clamp to min/max bounds (prevents setting invalid stat values)
                float originalValue = f;
                if (f < MinValue)
                {
                    f = MinValue;
                }
                else if (f > MaxValue)
                {
                    f = MaxValue;
                }

                // Log if value was clamped
                if (!f.Equals(originalValue))
                {
                    API.SecurityLogger.LogWithRateLimit(
                        API.LogLevel.Warning,
                        API.LogContext.Validation,
                        $"stat_{Id}",
                        $"Stat '{Id}' value clamped from {originalValue} to {f} (min={MinValue}, max={MaxValue})");
                }

                FloatValue = f;
            }
        }

        public override bool IsModified => FloatValue.Equals(OriginalValue) == false;
    }
}
