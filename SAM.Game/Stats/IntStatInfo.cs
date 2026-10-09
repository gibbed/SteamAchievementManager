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
    internal class IntStatInfo : StatInfo
    {
        public int OriginalValue;
        public int IntValue;

        public int MinValue = int.MinValue;
        public int MaxValue = int.MaxValue;
        public int? MaxChange;

        public override object Value
        {
            get => this.IntValue;
            set
            {
                // The grid can hand back the boxed int it originally displayed;
                // only strings need parsing (a raw cast threw InvalidCastException,
                // reported as a misleading "Invalid value").
                var i = value is int rawValue
                    ? rawValue
                    : int.Parse(
                        System.Convert.ToString(value, System.Globalization.CultureInfo.CurrentCulture),
                        System.Globalization.CultureInfo.CurrentCulture);
                if ((this.Permission & 2) != 0 &&
                    this.IntValue != i)
                {
                    throw new StatIsProtectedException();
                }
                if (this.IsIncrementOnly == true && i < this.IntValue)
                {
                    throw new StatConstraintException(
                        $"increment only: the value can only go up (currently {this.IntValue})");
                }
                if (i < this.MinValue || i > this.MaxValue)
                {
                    throw new StatConstraintException(
                        $"outside the allowed range {this.MinValue}..{this.MaxValue}");
                }
                if (this.MaxChange.HasValue == true &&
                    System.Math.Abs((long)i - this.IntValue) > this.MaxChange.Value)
                {
                    throw new StatConstraintException(
                        $"the schema allows changing by at most {this.MaxChange.Value} at a time");
                }
                this.IntValue = i;
            }
        }

        public override bool IsModified => this.IntValue != this.OriginalValue;

        public override string DescribeConstraints()
        {
            var parts = new List<string>();
            if (this.IsIncrementOnly == true)
            {
                parts.Add("increment only");
            }
            if (this.MinValue != int.MinValue || this.MaxValue != int.MaxValue)
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
