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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace SAM.Picker
{
    internal static class SteamLibraryFavorites
    {
        private const string FavoritesKey = "user-collections.favorite";

        public static HashSet<uint> Load()
        {
            HashSet<uint> favorites = new();

            try
            {
                string steamPath = API.Steam.GetInstallPath();
                if (string.IsNullOrEmpty(steamPath) == true)
                {
                    return favorites;
                }

                foreach (string accountId in GetAccountIds(steamPath))
                {
                    string path = Path.Combine(
                        steamPath,
                        "userdata",
                        accountId,
                        "config",
                        "cloudstorage",
                        "cloud-storage-namespace-1.json");

                    var loaded = LoadFromCloudStorage(path);
                    if (loaded.Found == true)
                    {
                        return loaded.Favorites;
                    }
                }
            }
            catch (Exception)
            {
                return favorites;
            }

            return favorites;
        }

        private static IEnumerable<string> GetAccountIds(string steamPath)
        {
            HashSet<string> accountIds = new();

            foreach (var accountId in GetMostRecentLoginAccountIds(steamPath))
            {
                if (accountIds.Add(accountId) == true)
                {
                    yield return accountId;
                }
            }

            string userdataPath = Path.Combine(steamPath, "userdata");
            if (Directory.Exists(userdataPath) == false)
            {
                yield break;
            }

            foreach (string path in Directory.EnumerateDirectories(userdataPath)
                         .OrderByDescending(Directory.GetLastWriteTimeUtc))
            {
                string accountId = Path.GetFileName(path);
                if (uint.TryParse(accountId, NumberStyles.Integer, CultureInfo.InvariantCulture, out _) == false)
                {
                    continue;
                }

                if (accountIds.Add(accountId) == true)
                {
                    yield return accountId;
                }
            }
        }

        private static IEnumerable<string> GetMostRecentLoginAccountIds(string steamPath)
        {
            string path = Path.Combine(steamPath, "config", "loginusers.vdf");
            if (File.Exists(path) == false)
            {
                yield break;
            }

            string text;
            using (var input = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader streamReader = new(input, Encoding.UTF8, true))
            {
                text = streamReader.ReadToEnd();
            }

            VdfTokenReader tokenReader = new(text);
            while (tokenReader.TryRead(out string steamId) == true)
            {
                if (ulong.TryParse(steamId, NumberStyles.Integer, CultureInfo.InvariantCulture, out ulong steamId64) == false)
                {
                    continue;
                }

                if (tokenReader.TryRead(out string next) == false || next != "{")
                {
                    continue;
                }

                if (ReadLoginUserBlock(tokenReader) == false)
                {
                    continue;
                }

                uint accountId = (uint)(steamId64 & 0xFFFFFFFF);
                yield return accountId.ToString(CultureInfo.InvariantCulture);
            }
        }

        private static bool ReadLoginUserBlock(VdfTokenReader reader)
        {
            bool mostRecent = false;
            int depth = 1;
            while (depth > 0 && reader.TryRead(out string token) == true)
            {
                if (token == "{")
                {
                    depth++;
                    continue;
                }

                if (token == "}")
                {
                    depth--;
                    continue;
                }

                if (depth != 1 || string.Compare(token, "MostRecent", StringComparison.InvariantCultureIgnoreCase) != 0)
                {
                    continue;
                }

                if (reader.TryRead(out string value) == true)
                {
                    mostRecent = value == "1";
                }
            }

            return mostRecent;
        }

        private static LoadResult LoadFromCloudStorage(string path)
        {
            HashSet<uint> favorites = new();
            if (File.Exists(path) == false)
            {
                return LoadResult.CreateNotFound(favorites);
            }

            string text;
            using (var input = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader reader = new(input, Encoding.UTF8, true))
            {
                text = reader.ReadToEnd();
            }

            JsonValue root = JsonParser.Parse(text);
            if (root?.Array == null)
            {
                return LoadResult.CreateFound(favorites);
            }

            foreach (var entry in root.Array)
            {
                if (entry.Array == null || entry.Array.Count < 2)
                {
                    continue;
                }

                if (string.Compare(entry.Array[0].String, FavoritesKey, StringComparison.InvariantCultureIgnoreCase) != 0)
                {
                    continue;
                }

                var value = entry.Array[1].GetObjectValue("value");
                if (value?.String == null)
                {
                    return LoadResult.CreateFound(favorites);
                }

                return LoadResult.CreateFound(LoadFromCollection(value.String));
            }

            return LoadResult.CreateFound(favorites);
        }

        private static HashSet<uint> LoadFromCollection(string text)
        {
            HashSet<uint> favorites = new();

            JsonValue root = JsonParser.Parse(text);
            var added = root?.GetObjectValue("added");
            if (added?.Array == null)
            {
                return favorites;
            }

            foreach (var value in added.Array)
            {
                if (value.Number.HasValue == false)
                {
                    continue;
                }

                double number = value.Number.Value;
                if (number < uint.MinValue ||
                    number > uint.MaxValue ||
                    Math.Truncate(number) != number)
                {
                    continue;
                }

                favorites.Add((uint)number);
            }

            return favorites;
        }

        private readonly struct LoadResult
        {
            public readonly bool Found;
            public readonly HashSet<uint> Favorites;

            private LoadResult(bool found, HashSet<uint> favorites)
            {
                this.Found = found;
                this.Favorites = favorites;
            }

            public static LoadResult CreateFound(HashSet<uint> favorites)
            {
                return new(true, favorites);
            }

            public static LoadResult CreateNotFound(HashSet<uint> favorites)
            {
                return new(false, favorites);
            }
        }

        private sealed class JsonValue
        {
            public string String;
            public double? Number;
            public List<JsonValue> Array;
            public Dictionary<string, JsonValue> Object;

            public JsonValue GetObjectValue(string key)
            {
                if (this.Object == null)
                {
                    return null;
                }

                return this.Object.TryGetValue(key, out var value) == true
                    ? value
                    : null;
            }
        }

        private sealed class JsonParser
        {
            private readonly string _Text;
            private int _Offset;

            private JsonParser(string text)
            {
                this._Text = text ?? "";
            }

            public static JsonValue Parse(string text)
            {
                return new JsonParser(text).ParseValue();
            }

            private JsonValue ParseValue()
            {
                this.SkipWhitespace();
                if (this._Offset >= this._Text.Length)
                {
                    return null;
                }

                char c = this._Text[this._Offset];
                switch (c)
                {
                    case '"':
                    {
                        return new()
                        {
                            String = this.ParseString(),
                        };
                    }

                    case '[':
                    {
                        return this.ParseArray();
                    }

                    case '{':
                    {
                        return this.ParseObject();
                    }

                    default:
                    {
                        if (c == '-' || char.IsDigit(c) == true)
                        {
                            return this.ParseNumber();
                        }

                        this.ParseLiteral();
                        return new();
                    }
                }
            }

            private JsonValue ParseArray()
            {
                JsonValue value = new()
                {
                    Array = new(),
                };

                this._Offset++;
                while (true)
                {
                    this.SkipWhitespace();
                    if (this.TryRead(']') == true)
                    {
                        return value;
                    }

                    value.Array.Add(this.ParseValue());
                    this.SkipWhitespace();

                    if (this.TryRead(',') == true)
                    {
                        continue;
                    }

                    this.TryRead(']');
                    return value;
                }
            }

            private JsonValue ParseObject()
            {
                JsonValue value = new()
                {
                    Object = new(StringComparer.InvariantCultureIgnoreCase),
                };

                this._Offset++;
                while (true)
                {
                    this.SkipWhitespace();
                    if (this.TryRead('}') == true)
                    {
                        return value;
                    }

                    if (this._Offset >= this._Text.Length || this._Text[this._Offset] != '"')
                    {
                        return value;
                    }

                    string key = this.ParseString();
                    this.SkipWhitespace();
                    if (this.TryRead(':') == false)
                    {
                        return value;
                    }

                    value.Object[key] = this.ParseValue();
                    this.SkipWhitespace();

                    if (this.TryRead(',') == true)
                    {
                        continue;
                    }

                    this.TryRead('}');
                    return value;
                }
            }

            private JsonValue ParseNumber()
            {
                int start = this._Offset;
                if (this._Text[this._Offset] == '-')
                {
                    this._Offset++;
                }

                while (this._Offset < this._Text.Length && char.IsDigit(this._Text[this._Offset]) == true)
                {
                    this._Offset++;
                }

                if (this._Offset < this._Text.Length && this._Text[this._Offset] == '.')
                {
                    this._Offset++;
                    while (this._Offset < this._Text.Length && char.IsDigit(this._Text[this._Offset]) == true)
                    {
                        this._Offset++;
                    }
                }

                string text = this._Text.Substring(start, this._Offset - start);
                return new()
                {
                    Number = double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double number) == true
                        ? number
                        : null,
                };
            }

            private string ParseString()
            {
                StringBuilder builder = new();
                this._Offset++;

                while (this._Offset < this._Text.Length)
                {
                    char c = this._Text[this._Offset++];
                    if (c == '"')
                    {
                        break;
                    }

                    if (c != '\\' || this._Offset >= this._Text.Length)
                    {
                        builder.Append(c);
                        continue;
                    }

                    c = this._Text[this._Offset++];
                    switch (c)
                    {
                        case '"':
                        case '\\':
                        case '/':
                        {
                            builder.Append(c);
                            break;
                        }

                        case 'b':
                        {
                            builder.Append('\b');
                            break;
                        }

                        case 'f':
                        {
                            builder.Append('\f');
                            break;
                        }

                        case 'n':
                        {
                            builder.Append('\n');
                            break;
                        }

                        case 'r':
                        {
                            builder.Append('\r');
                            break;
                        }

                        case 't':
                        {
                            builder.Append('\t');
                            break;
                        }

                        case 'u' when this._Offset + 4 <= this._Text.Length:
                        {
                            string hex = this._Text.Substring(this._Offset, 4);
                            if (ushort.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ushort code) == true)
                            {
                                builder.Append((char)code);
                                this._Offset += 4;
                            }
                            break;
                        }
                    }
                }

                return builder.ToString();
            }

            private void ParseLiteral()
            {
                while (this._Offset < this._Text.Length &&
                       char.IsLetter(this._Text[this._Offset]) == true)
                {
                    this._Offset++;
                }
            }

            private bool TryRead(char expected)
            {
                if (this._Offset >= this._Text.Length ||
                    this._Text[this._Offset] != expected)
                {
                    return false;
                }

                this._Offset++;
                return true;
            }

            private void SkipWhitespace()
            {
                while (this._Offset < this._Text.Length &&
                       char.IsWhiteSpace(this._Text[this._Offset]) == true)
                {
                    this._Offset++;
                }
            }
        }

        private sealed class VdfTokenReader
        {
            private readonly string _Text;
            private int _Offset;

            public VdfTokenReader(string text)
            {
                this._Text = text ?? "";
            }

            public bool TryRead(out string token)
            {
                token = null;
                this.SkipWhitespace();
                if (this._Offset >= this._Text.Length)
                {
                    return false;
                }

                char c = this._Text[this._Offset++];
                if (c == '{' || c == '}')
                {
                    token = c.ToString();
                    return true;
                }

                if (c == '"')
                {
                    StringBuilder builder = new();
                    while (this._Offset < this._Text.Length)
                    {
                        c = this._Text[this._Offset++];
                        if (c == '"')
                        {
                            break;
                        }

                        if (c == '\\' && this._Offset < this._Text.Length)
                        {
                            c = this._Text[this._Offset++];
                        }

                        builder.Append(c);
                    }

                    token = builder.ToString();
                    return true;
                }

                int start = this._Offset - 1;
                while (this._Offset < this._Text.Length)
                {
                    c = this._Text[this._Offset];
                    if (char.IsWhiteSpace(c) == true || c == '{' || c == '}')
                    {
                        break;
                    }

                    this._Offset++;
                }

                token = this._Text.Substring(start, this._Offset - start);
                return true;
            }

            private void SkipWhitespace()
            {
                while (this._Offset < this._Text.Length &&
                       char.IsWhiteSpace(this._Text[this._Offset]) == true)
                {
                    this._Offset++;
                }
            }
        }
    }
}
