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

using SAM.Game.Stats;
using Xunit;

namespace SAM.Game.Tests
{
    public class StatInfoTests
    {
        private static IntStatInfo MakeIntStat(
            int value = 10,
            bool incrementOnly = false,
            int min = int.MinValue,
            int max = int.MaxValue,
            int? maxChange = null,
            int permission = 0)
        {
            return new IntStatInfo()
            {
                Id = "TEST_STAT",
                DisplayName = "Test stat",
                IntValue = value,
                OriginalValue = value,
                IsIncrementOnly = incrementOnly,
                MinValue = min,
                MaxValue = max,
                MaxChange = maxChange,
                Permission = permission,
            };
        }

        [Fact]
        public void IntStat_ValueSetter_AcceptsBoxedInteger()
        {
            // The grid can hand back the boxed int it displayed (#421 style
            // "Invalid value" errors came from casting straight to string).
            var stat = MakeIntStat();
            stat.Value = 42;
            Assert.Equal(42, stat.IntValue);
            Assert.True(stat.IsModified);
        }

        [Fact]
        public void IntStat_ValueSetter_AcceptsString()
        {
            var stat = MakeIntStat();
            stat.Value = "7";
            Assert.Equal(7, stat.IntValue);
        }

        [Fact]
        public void IntStat_ValueSetter_RejectsGarbage()
        {
            var stat = MakeIntStat();
            Assert.ThrowsAny<System.FormatException>(() => stat.Value = "not a number");
            Assert.Equal(10, stat.IntValue);
        }

        [Fact]
        public void IntStat_IncrementOnly_RejectsDecrease()
        {
            var stat = MakeIntStat(value: 10, incrementOnly: true, min: 0, max: 100);

            var exception = Assert.Throws<StatConstraintException>(() => stat.Value = "5");

            Assert.Contains("increment only", exception.Message);
            Assert.Equal(10, stat.IntValue);
        }

        [Fact]
        public void IntStat_IncrementOnly_AllowsIncrease()
        {
            var stat = MakeIntStat(value: 10, incrementOnly: true, min: 0, max: 100);
            stat.Value = "11";
            Assert.Equal(11, stat.IntValue);
        }

        [Fact]
        public void IntStat_Range_RejectsValueBelowMinimum()
        {
            var stat = MakeIntStat(value: 10, min: 0, max: 100);

            var exception = Assert.Throws<StatConstraintException>(() => stat.Value = "-1");

            Assert.Contains("0..100", exception.Message);
            Assert.Equal(10, stat.IntValue);
        }

        [Fact]
        public void IntStat_Range_RejectsValueAboveMaximum()
        {
            var stat = MakeIntStat(value: 10, min: 0, max: 100);
            Assert.Throws<StatConstraintException>(() => stat.Value = "101");
            Assert.Equal(10, stat.IntValue);
        }

        [Fact]
        public void IntStat_MaxChange_RejectsLargeDelta()
        {
            var stat = MakeIntStat(value: 10, maxChange: 3);

            var exception = Assert.Throws<StatConstraintException>(() => stat.Value = "20");

            Assert.Contains("at most 3", exception.Message);
            Assert.Equal(10, stat.IntValue);
        }

        [Fact]
        public void IntStat_MaxChange_AllowsSmallDelta()
        {
            var stat = MakeIntStat(value: 10, maxChange: 3);
            stat.Value = "12";
            Assert.Equal(12, stat.IntValue);
        }

        [Fact]
        public void IntStat_Protected_RejectsChange()
        {
            var stat = MakeIntStat(value: 10, permission: 2);
            Assert.Throws<StatIsProtectedException>(() => stat.Value = "11");
            Assert.Equal(10, stat.IntValue);
        }

        [Fact]
        public void IntStat_Protected_AllowsSameValue()
        {
            var stat = MakeIntStat(value: 10, permission: 2);
            stat.Value = "10";
            Assert.Equal(10, stat.IntValue);
            Assert.False(stat.IsModified);
        }

        [Fact]
        public void IntStat_IsModified_TracksOriginalValue()
        {
            var stat = MakeIntStat(value: 10);
            Assert.False(stat.IsModified);
            stat.Value = "11";
            Assert.True(stat.IsModified);
        }

        [Theory]
        [InlineData(true, false, 0, "IncrementOnly")]
        [InlineData(false, true, 0, "AverageRate")]
        [InlineData(false, false, 2, "Protected")]
        [InlineData(false, false, 0, "None")]
        public void Extra_ReportsFlags(
            bool incrementOnly,
            bool averageRate,
            int permission,
            string expected)
        {
            var stat = MakeIntStat(incrementOnly: incrementOnly, permission: permission);
            stat.IsAverageRate = averageRate;
            Assert.Equal(expected, stat.Extra);
        }

        [Fact]
        public void DescribeConstraints_IsEmpty_WhenNoConstraintApplies()
        {
            var stat = MakeIntStat();
            Assert.Equal("", stat.DescribeConstraints());
        }

        [Fact]
        public void DescribeConstraints_ListsKnownConstraints()
        {
            var stat = MakeIntStat(incrementOnly: true, min: 0, max: 100, maxChange: 5);
            var description = stat.DescribeConstraints();

            Assert.Contains("increment only", description);
            Assert.Contains("range 0..100", description);
            Assert.Contains("max change 5", description);
        }

        [Fact]
        public void FloatStat_ValueSetter_AcceptsBoxedFloat()
        {
            var stat = new FloatStatInfo()
            {
                Id = "TEST_FLOAT",
                DisplayName = "Test float",
                FloatValue = 1.5f,
                OriginalValue = 1.5f,
                MinValue = 0.0f,
                MaxValue = 10.0f,
            };

            stat.Value = 2.5f;
            Assert.Equal(2.5f, stat.FloatValue);
            Assert.True(stat.IsModified);
        }

        [Fact]
        public void FloatStat_Range_IsEnforced()
        {
            var stat = new FloatStatInfo()
            {
                Id = "TEST_FLOAT",
                DisplayName = "Test float",
                FloatValue = 1.5f,
                OriginalValue = 1.5f,
                MinValue = 0.0f,
                MaxValue = 10.0f,
            };

            Assert.Throws<StatConstraintException>(() => stat.Value = "11");
            Assert.Equal(1.5f, stat.FloatValue);
        }
    }
}
