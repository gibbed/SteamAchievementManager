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

using Xunit;

namespace SAM.Game.Tests
{
    /// <summary>
    /// The EResult values Steam reports back through UserStatsReceived /
    /// UserStatsStored are shown verbatim to the user, so every code we know
    /// about must translate to something actionable.
    /// </summary>
    public class ManagerErrorTests
    {
        [Theory]
        [InlineData(1, "ok")]
        [InlineData(2, "generic failure -- this usually means you don't own the game")]
        [InlineData(3, "no connection -- Steam is offline or unreachable")]
        [InlineData(5, "logged out")]
        [InlineData(6, "invalid account information")]
        [InlineData(15, "timed out")]
        [InlineData(17, "account not found")]
        [InlineData(19, "Steam service unavailable")]
        [InlineData(20, "not logged on to Steam")]
        public void TranslateError_KnownCodes_AreHumanReadable(int id, string expected)
        {
            Assert.Equal(expected, Manager.TranslateError(id));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(14)]
        [InlineData(99)]
        public void TranslateError_UnknownCodes_FallBackToTheNumber(int id)
        {
            var message = Manager.TranslateError(id);

            Assert.StartsWith("error ", message);
            Assert.Contains(id.ToString(), message);
        }

        [Fact]
        public void TranslateError_NeverReturnsAnEmptyMessage()
        {
            for (var id = -5; id <= 40; ++id)
            {
                Assert.False(string.IsNullOrWhiteSpace(Manager.TranslateError(id)));
            }
        }

        [Fact]
        public void DescribeConstraintsSuffix_IsEmptyWithoutConstraints()
        {
            var stat = new Stats.IntStatInfo()
            {
                Id = "TEST_STAT",
                DisplayName = "Test stat",
            };

            Assert.Equal("", Manager.DescribeConstraintsSuffix(stat));
        }

        [Fact]
        public void DescribeConstraintsSuffix_WrapsTheConstraintList()
        {
            var stat = new Stats.IntStatInfo()
            {
                Id = "TEST_STAT",
                DisplayName = "Test stat",
                IsIncrementOnly = true,
                MinValue = 0,
                MaxValue = 100,
            };

            var suffix = Manager.DescribeConstraintsSuffix(stat);

            Assert.StartsWith(" [", suffix);
            Assert.EndsWith("]", suffix);
            Assert.Contains("increment only", suffix);
        }
    }
}
