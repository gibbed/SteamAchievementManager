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

using System.Collections.Generic;

namespace SAM.Game.Stats
{
    internal class FloatStatInfo : StatInfo
    {
        public float OriginalValue;
        public float FloatValue;

        public float MinValue = float.MinValue;
        public float MaxValue = float.MaxValue;
        public float? MaxChange;

        public override object Value
        {
            get => this.FloatValue;
            set
            {
                // The grid can hand back the boxed float it originally displayed;
                // only strings need parsing (a raw cast threw InvalidCastException).
                var f = value is float rawValue
                    ? rawValue
                    : float.Parse(
                        System.Convert.ToString(value, System.Globalization.CultureInfo.CurrentCulture),
                        System.Globalization.CultureInfo.CurrentCulture);
                if ((this.Permission & 2) != 0 &&
                    this.FloatValue.Equals(f) == false)
                {
                    throw new StatIsProtectedException();
                }
                if (this.IsIncrementOnly == true && f < this.FloatValue)
                {
                    throw new StatConstraintException(
                        $"increment only: the value can only go up (currently {this.FloatValue})");
                }
                if (f < this.MinValue || f > this.MaxValue)
                {
                    throw new StatConstraintException(
                        $"outside the allowed range {this.MinValue}..{this.MaxValue}");
                }
                if (this.MaxChange.HasValue == true &&
                    System.Math.Abs((double)f - this.FloatValue) > this.MaxChange.Value)
                {
                    throw new StatConstraintException(
                        $"the schema allows changing by at most {this.MaxChange.Value} at a time");
                }
                this.FloatValue = f;
            }
        }

        public override bool IsModified => this.FloatValue.Equals(this.OriginalValue) == false;

        public override string DescribeConstraints()
        {
            var parts = new List<string>();
            if (this.IsIncrementOnly == true)
            {
                parts.Add("increment only");
            }
            if (this.MinValue != float.MinValue || this.MaxValue != float.MaxValue)
            {
                parts.Add($"range {this.MinValue}..{this.MaxValue}");
            }
            if (this.MaxChange.HasValue == true)
            {
                parts.Add($"max change {this.MaxChange.Value}");
            }
            return string.Join(", ", parts);
        }
    }
}
