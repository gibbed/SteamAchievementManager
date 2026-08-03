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
using static SAM.Core.InvariantShorthand;
using APITypes = SAM.API.Types;

namespace SAM.Core
{
    /// <summary>
    /// The stat and achievement definitions Steam caches on disk for one game.
    /// </summary>
    public sealed class UserGameStatsSchema
    {
        public List<Stats.StatDefinition> Stats { get; } = new();

        public List<Stats.AchievementDefinition> Achievements { get; } = new();

        /// <summary>
        /// Reads the cached schema for <paramref name="gameId"/>, preferring display
        /// strings in <paramref name="language"/>. Returns null when there is no
        /// readable schema, which is the normal case for a game Steam has never run.
        /// </summary>
        public static UserGameStatsSchema Load(long gameId, string language)
        {
            string path;
            try
            {
                string installPath = API.Steam.GetInstallPath();
                if (string.IsNullOrEmpty(installPath) == true)
                {
                    return null;
                }

                string fileName = _($"UserGameStatsSchema_{gameId}.bin");
                path = Path.Combine(installPath, "appcache", "stats", fileName);
                if (File.Exists(path) == false)
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }

            KeyValue kv = KeyValue.LoadAsBinary(path);
            if (kv == null)
            {
                return null;
            }

            KeyValue stats = kv[gameId.ToString(CultureInfo.InvariantCulture)]["stats"];
            if (stats.Valid == false || stats.Children == null)
            {
                return null;
            }

            UserGameStatsSchema schema = new();
            foreach (KeyValue stat in stats.Children)
            {
                if (stat.Valid == false)
                {
                    continue;
                }

                schema.AddStat(stat, language);
            }

            return schema;
        }

        private void AddStat(KeyValue stat, string language)
        {
            APITypes.UserStatType type;

            // schema in the new format?
            var typeNode = stat["type"];
            if (typeNode.Valid == true && typeNode.Type == KeyValueType.String)
            {
                if (Enum.TryParse((string)typeNode.Value, true, out type) == false)
                {
                    type = APITypes.UserStatType.Invalid;
                }
            }
            else
            {
                type = APITypes.UserStatType.Invalid;
            }

            // schema in the old format?
            if (type == APITypes.UserStatType.Invalid)
            {
                var typeIntNode = stat["type_int"];
                var rawType = typeIntNode.Valid == true
                    ? typeIntNode.AsInteger(0)
                    : typeNode.AsInteger(0);
                type = (APITypes.UserStatType)rawType;
            }

            switch (type)
            {
                case APITypes.UserStatType.Invalid:
                {
                    break;
                }

                case APITypes.UserStatType.Integer:
                {
                    var id = stat["name"].AsString("");
                    string name = GetLocalizedString(stat["display"]["name"], language, id);

                    this.Stats.Add(new Stats.IntegerStatDefinition()
                    {
                        Id = stat["name"].AsString(""),
                        DisplayName = name,
                        MinValue = stat["min"].AsInteger(int.MinValue),
                        MaxValue = stat["max"].AsInteger(int.MaxValue),
                        MaxChange = stat["maxchange"].AsInteger(0),
                        IncrementOnly = stat["incrementonly"].AsBoolean(false),
                        SetByTrustedGameServer = stat["bSetByTrustedGS"].AsBoolean(false),
                        DefaultValue = stat["default"].AsInteger(0),
                        Permission = stat["permission"].AsInteger(0),
                    });
                    break;
                }

                case APITypes.UserStatType.Float:
                case APITypes.UserStatType.AverageRate:
                {
                    var id = stat["name"].AsString("");
                    string name = GetLocalizedString(stat["display"]["name"], language, id);

                    this.Stats.Add(new Stats.FloatStatDefinition()
                    {
                        Id = stat["name"].AsString(""),
                        DisplayName = name,
                        MinValue = stat["min"].AsFloat(float.MinValue),
                        MaxValue = stat["max"].AsFloat(float.MaxValue),
                        MaxChange = stat["maxchange"].AsFloat(0.0f),
                        IncrementOnly = stat["incrementonly"].AsBoolean(false),
                        DefaultValue = stat["default"].AsFloat(0.0f),
                        Permission = stat["permission"].AsInteger(0),
                    });
                    break;
                }

                case APITypes.UserStatType.Achievements:
                case APITypes.UserStatType.GroupAchievements:
                {
                    if (stat.Children != null)
                    {
                        foreach (var bits in stat.Children.Where(
                            b => string.Compare(b.Name, "bits", StringComparison.InvariantCultureIgnoreCase) == 0))
                        {
                            if (bits.Valid == false || bits.Children == null)
                            {
                                continue;
                            }

                            foreach (var bit in bits.Children)
                            {
                                string id = bit["name"].AsString("");
                                string name = GetLocalizedString(bit["display"]["name"], language, id);
                                string desc = GetLocalizedString(bit["display"]["desc"], language, "");

                                this.Achievements.Add(new()
                                {
                                    Id = id,
                                    Name = name,
                                    Description = desc,
                                    IconNormal = bit["display"]["icon"].AsString(""),
                                    IconLocked = bit["display"]["icon_gray"].AsString(""),
                                    IsHidden = bit["display"]["hidden"].AsBoolean(false),
                                    Permission = bit["permission"].AsInteger(0),
                                });
                            }
                        }
                    }

                    break;
                }

                default:
                {
                    throw new InvalidOperationException("invalid stat type");
                }
            }
        }

        private static string GetLocalizedString(KeyValue kv, string language, string defaultValue)
        {
            var name = kv[language].AsString("");
            if (string.IsNullOrEmpty(name) == false)
            {
                return name;
            }

            if (language != "english")
            {
                name = kv["english"].AsString("");
                if (string.IsNullOrEmpty(name) == false)
                {
                    return name;
                }
            }

            name = kv.AsString("");
            if (string.IsNullOrEmpty(name) == false)
            {
                return name;
            }

            return defaultValue;
        }
    }
}
