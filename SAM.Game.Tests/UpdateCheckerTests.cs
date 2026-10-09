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

using SAM.Picker;
using Xunit;

namespace SAM.Game.Tests
{
    public class UpdateCheckerTests
    {
        private const string SampleRelease = @"{
            ""tag_name"": ""7.2.0"",
            ""body"": ""Release notes."",
            ""assets"": [
                {
                    ""name"": ""SteamAchievementManager-7.2.0.zip.sha256"",
                    ""browser_download_url"": ""https://example.test/SteamAchievementManager-7.2.0.zip.sha256""
                },
                {
                    ""name"": ""SteamAchievementManager-7.2.0.zip"",
                    ""browser_download_url"": ""https://example.test/SteamAchievementManager-7.2.0.zip""
                }
            ]
        }";

        [Fact]
        public void CurrentVersion_ComesFromTheAssembly()
        {
            Assert.NotNull(UpdateChecker.CurrentVersion);
            Assert.NotEqual(new System.Version(0, 0, 0, 0), UpdateChecker.CurrentVersion);
        }

        [Theory]
        [InlineData("7.1.0", 7, 1, 0)]
        [InlineData("v7.1.0", 7, 1, 0)]
        [InlineData("V7.2", 7, 2, 0)]
        [InlineData("release-8.0.1", 8, 0, 1)]
        [InlineData(" 7.0.123 ", 7, 0, 123)]
        public void ParseVersion_AcceptsRealisticTags(string tag, int major, int minor, int build)
        {
            var version = UpdateChecker.ParseVersion(tag);

            Assert.NotNull(version);
            Assert.Equal(new System.Version(major, minor, build, 0), version);
        }

        [Theory]
        [InlineData("latest")]
        [InlineData("vnext")]
        [InlineData("")]
        public void ParseVersion_RejectsTagsWithoutANumber(string tag)
        {
            Assert.Null(UpdateChecker.ParseVersion(tag));
        }

        [Fact]
        public void ParseRelease_ReadsTagNotesAndAssets()
        {
            var info = UpdateChecker.ParseRelease(SampleRelease);

            Assert.NotNull(info);
            Assert.Equal("7.2.0", info.Tag);
            Assert.Equal(new System.Version(7, 2, 0, 0), info.Version);
            Assert.Equal("Release notes.", info.ReleaseNotes);
            Assert.Equal("https://example.test/SteamAchievementManager-7.2.0.zip", info.ZipUrl);
            Assert.Equal(
                "https://example.test/SteamAchievementManager-7.2.0.zip.sha256",
                info.ChecksumUrl);
        }

        [Fact]
        public void ParseRelease_ReportsNoArchiveWhenTheReleaseHasNone()
        {
            var info = UpdateChecker.ParseRelease(@"{""tag_name"": ""7.2.0"", ""assets"": []}");

            Assert.NotNull(info);
            Assert.Equal(new System.Version(7, 2, 0, 0), info.Version);
            Assert.Null(info.ZipUrl);
            Assert.Null(info.ChecksumUrl);
            Assert.Null(info.ReleaseNotes);
        }

        [Fact]
        public void ParseRelease_RequiresATag()
        {
            Assert.Null(UpdateChecker.ParseRelease(@"{""name"": ""no tag""}"));
            Assert.Null(UpdateChecker.ParseRelease(@"{""tag_name"": """"}"));
        }

        [Theory]
        [InlineData(
            "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef  update.zip",
            "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef")]
        [InlineData(
            "ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789",
            "ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789")]
        public void ExtractChecksum_FindsASha256(string text, string expected)
        {
            Assert.Equal(expected, UpdateChecker.ExtractChecksum(text));
        }

        [Theory]
        [InlineData("no hash here")]
        [InlineData("0123456789abcdef too short")]
        [InlineData("")]
        public void ExtractChecksum_RejectsAnythingThatIsNotAsha256(string text)
        {
            Assert.Null(UpdateChecker.ExtractChecksum(text));
        }
    }
}
