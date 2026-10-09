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
using System.IO;
using System.Text;
using SAM.Game;
using Xunit;

namespace SAM.Game.Tests
{
    public class KeyValueTests
    {
        private static void WriteName(List<byte> buffer, string value)
        {
            buffer.AddRange(Encoding.UTF8.GetBytes(value));
            buffer.Add(0);
        }

        private static void WriteString(List<byte> buffer, string name, string value)
        {
            buffer.Add((byte)KeyValueType.String);
            WriteName(buffer, name);
            WriteName(buffer, value);
        }

        private static void WriteInteger(List<byte> buffer, string name, int value)
        {
            buffer.Add((byte)KeyValueType.Int32);
            WriteName(buffer, name);
            buffer.AddRange(System.BitConverter.GetBytes(value));
        }

        private static void WriteFloat(List<byte> buffer, string name, float value)
        {
            buffer.Add((byte)KeyValueType.Float32);
            WriteName(buffer, name);
            buffer.AddRange(System.BitConverter.GetBytes(value));
        }

        private static void BeginObject(List<byte> buffer, string name)
        {
            buffer.Add((byte)KeyValueType.None);
            WriteName(buffer, name);
        }

        private static void EndObject(List<byte> buffer)
        {
            buffer.Add((byte)KeyValueType.End);
        }

        private static KeyValue Read(byte[] data)
        {
            var kv = new KeyValue();
            Assert.True(kv.ReadAsBinary(new MemoryStream(data)));
            Assert.True(kv.Valid);
            return kv;
        }

        [Fact]
        public void ReadAsBinary_ParsesNestedSchemaLikeData()
        {
            var buffer = new List<byte>();
            WriteString(buffer, "name", "ACH_ONE");
            BeginObject(buffer, "display");
            WriteString(buffer, "name", "First blood");
            WriteString(buffer, "desc", "Kill something.");
            EndObject(buffer);
            WriteInteger(buffer, "increment", 42);
            WriteFloat(buffer, "threshold", 0.5f);
            EndObject(buffer);

            var kv = Read(buffer.ToArray());

            Assert.Equal("ACH_ONE", kv["name"].AsString(""));
            Assert.Equal("First blood", kv["display"]["name"].AsString(""));
            Assert.Equal("Kill something.", kv["display"]["desc"].AsString(""));
            Assert.Equal(42, kv["increment"].AsInteger(0));
            Assert.Equal(0.5f, kv["threshold"].AsFloat(0.0f));
        }

        [Fact]
        public void Indexer_IsCaseInsensitive()
        {
            var buffer = new List<byte>();
            WriteString(buffer, "DisplayName", "Something");
            EndObject(buffer);

            var kv = Read(buffer.ToArray());

            Assert.Equal("Something", kv["displayname"].AsString(""));
            Assert.Equal("Something", kv["DISPLAYNAME"].AsString(""));
        }

        [Fact]
        public void Indexer_ReturnsInvalidNode_ForMissingKey()
        {
            var buffer = new List<byte>();
            WriteString(buffer, "name", "ACH_ONE");
            EndObject(buffer);

            var kv = Read(buffer.ToArray());
            var missing = kv["does_not_exist"];

            Assert.False(missing.Valid);
            Assert.Equal("fallback", missing.AsString("fallback"));
            Assert.Equal(-1, missing.AsInteger(-1));
            Assert.Equal(1.5f, missing.AsFloat(1.5f));
            Assert.True(missing.AsBoolean(true));
        }

        [Fact]
        public void Indexer_ReturnsInvalidNode_WhenThereAreNoChildren()
        {
            // An invalid node (or a leaf) must not throw on lookup.
            var kv = new KeyValue();
            Assert.False(kv["anything"].Valid);

            var buffer = new List<byte>();
            WriteString(buffer, "name", "leaf");
            EndObject(buffer);
            var leaf = Read(buffer.ToArray())["name"];
            Assert.False(leaf["child"].Valid);
        }

        [Fact]
        public void Indexer_ReturnsFirstOfDuplicateKeys()
        {
            // Real schema files can repeat a key at the same level; SingleOrDefault
            // used to throw on them.
            var buffer = new List<byte>();
            WriteString(buffer, "name", "first");
            WriteString(buffer, "name", "second");
            EndObject(buffer);

            var kv = Read(buffer.ToArray());

            Assert.Equal("first", kv["name"].AsString(""));
        }

        [Theory]
        [InlineData("1", true)]
        [InlineData("0", false)]
        [InlineData("2", true)]
        [InlineData("-3", true)]
        public void AsBoolean_ParsesStringValues(string raw, bool expected)
        {
            var buffer = new List<byte>();
            WriteString(buffer, "flag", raw);
            EndObject(buffer);

            var kv = Read(buffer.ToArray());

            Assert.Equal(expected, kv["flag"].AsBoolean(false));
        }

        [Fact]
        public void AsBoolean_FallsBack_ForNonNumericString()
        {
            var buffer = new List<byte>();
            WriteString(buffer, "flag", "yes");
            EndObject(buffer);

            var kv = Read(buffer.ToArray());

            Assert.True(kv["flag"].AsBoolean(true));
        }

        [Fact]
        public void AsInteger_ConvertsFloatNodes()
        {
            var buffer = new List<byte>();
            WriteFloat(buffer, "value", 4.75f);
            EndObject(buffer);

            var kv = Read(buffer.ToArray());

            Assert.Equal(4, kv["value"].AsInteger(-1));
            Assert.Equal(4.75f, kv["value"].AsFloat(-1.0f));
        }

        [Fact]
        public void ReadAsBinary_RejectsTruncatedData()
        {
            var buffer = new List<byte>();
            WriteString(buffer, "name", "ACH_ONE");
            // No End marker: the reader runs off the end of the stream.
            Assert.False(new KeyValue().ReadAsBinary(new MemoryStream(buffer.ToArray())));
        }

        [Fact]
        public void ReadAsBinary_RejectsUnknownType()
        {
            var buffer = new List<byte>() { 99, 0 };
            Assert.False(new KeyValue().ReadAsBinary(new MemoryStream(buffer.ToArray())));
        }

        [Fact]
        public void ReadAsBinary_RejectsWideStringType()
        {
            var buffer = new List<byte>();
            buffer.Add((byte)KeyValueType.WideString);
            WriteName(buffer, "name");
            WriteName(buffer, "wide");
            EndObject(buffer);

            Assert.False(new KeyValue().ReadAsBinary(new MemoryStream(buffer.ToArray())));
        }

        [Fact]
        public void LoadAsBinary_ReturnsNull_ForMissingFile()
        {
            var path = Path.Combine(
                Path.GetTempPath(),
                "sam-tests-missing-" + System.Guid.NewGuid().ToString("N") + ".bin");

            Assert.Null(KeyValue.LoadAsBinary(path));
        }

        [Fact]
        public void LoadAsBinary_ReadsAWrittenFile()
        {
            var path = Path.Combine(
                Path.GetTempPath(),
                "sam-tests-" + System.Guid.NewGuid().ToString("N") + ".bin");

            try
            {
                var buffer = new List<byte>();
                WriteString(buffer, "name", "ACH_ONE");
                WriteInteger(buffer, "count", 7);
                EndObject(buffer);
                File.WriteAllBytes(path, buffer.ToArray());

                var kv = KeyValue.LoadAsBinary(path);

                Assert.NotNull(kv);
                Assert.Equal("ACH_ONE", kv["name"].AsString(""));
                Assert.Equal(7, kv["count"].AsInteger(0));
            }
            finally
            {
                if (File.Exists(path) == true)
                {
                    File.Delete(path);
                }
            }
        }
    }
}
