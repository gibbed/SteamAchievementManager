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

// WinUI 3 port: LoadUserGameStatsSchema and GetLocalizedString moved out of SAM.Game/Manager.cs
// so the schema can be read without a WinForms form. The parsing logic is unchanged.
#nullable disable

using System.Globalization;
using SAM.Game;
using SAM.Game.Stats;
using APITypes = SAM.API.Types;

namespace SAM.App.Schema
{
    internal sealed class GameSchema
    {
        public readonly List<AchievementDefinition> Achievements = new();
        public readonly List<StatDefinition> Stats = new();

        /// <summary>Achievement sets according to groups; empty for games without sets.</summary>
        public readonly List<AchievementSet> Sets = new();
    }

    internal static class SchemaLoader
    {
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

        public static GameSchema Load(uint gameId, string currentLanguage)
        {
            string path;
            try
            {
                string fileName = FormattableString.Invariant($"UserGameStatsSchema_{gameId}.bin");
                path = API.Steam.GetInstallPath();
                path = Path.Combine(path, "appcache", "stats", fileName);
                if (File.Exists(path) == false)
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }

            var kv = KeyValue.LoadAsBinary(path);
            if (kv == null)
            {
                return null;
            }

            GameSchema schema = new();

            var stats = kv[gameId.ToString(CultureInfo.InvariantCulture)]["stats"];
            if (stats.Valid == false || stats.Children == null)
            {
                return null;
            }

            foreach (var stat in stats.Children)
            {
                if (stat.Valid == false)
                {
                    continue;
                }

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
                        string name = GetLocalizedString(stat["display"]["name"], currentLanguage, id);

                        schema.Stats.Add(new IntegerStatDefinition()
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
                        string name = GetLocalizedString(stat["display"]["name"], currentLanguage, id);

                        schema.Stats.Add(new FloatStatDefinition()
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
                                    _ = int.TryParse(stat.Name, NumberStyles.None, CultureInfo.InvariantCulture, out var statId);
                                    _ = int.TryParse(bit.Name, NumberStyles.None, CultureInfo.InvariantCulture, out var bitIndex);
                                    string name = GetLocalizedString(bit["display"]["name"], currentLanguage, id);
                                    string desc = GetLocalizedString(bit["display"]["desc"], currentLanguage, "");

                                    schema.Achievements.Add(new()
                                    {
                                        Id = id,
                                        Name = name,
                                        Description = desc,
                                        IconNormal = bit["display"]["icon"].AsString(""),
                                        IconLocked = bit["display"]["icon_gray"].AsString(""),
                                        IsHidden = bit["display"]["hidden"].AsBoolean(false),
                                        Permission = bit["permission"].AsInteger(0),
                                        SetId = bit["groupid"].AsInteger(AchievementSet.BaseGameId),
                                        IsArchived = bit["archived"].AsBoolean(false),
                                        StatId = statId,
                                        Bit = bitIndex,
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

            LoadSets(kv[gameId.ToString(CultureInfo.InvariantCulture)]["groups"], currentLanguage, schema);
            return schema;
        }

        /// <summary>groups/&lt;id&gt;/name, dlcappid, order - new with achievement sets (Steam beta, October 2026).</summary>
        private static void LoadSets(KeyValue groups, string currentLanguage, GameSchema schema)
        {
            if (groups.Children == null)
            {
                return;
            }

            foreach (var group in groups.Children)
            {
                if (int.TryParse(group.Name, NumberStyles.None, CultureInfo.InvariantCulture, out var id) == false || id == AchievementSet.BaseGameId)
                {
                    continue;
                }

                var name = GetLocalizedString(group["name"], currentLanguage, "");
                schema.Sets.Add(new AchievementSet
                {
                    Id = id,
                    Name = string.IsNullOrEmpty(name) || name.StartsWith('#') ? null : name,
                    DlcAppId = (uint)group["dlcappid"].AsInteger(0),
                    Order = group["order"].AsInteger(id),
                });
            }

            // Achievements without groupid form the base game set.
            if (schema.Sets.Count > 0 && schema.Achievements.Any(a => schema.Sets.All(s => s.Id != a.SetId)))
            {
                foreach (var achievement in schema.Achievements.Where(a => schema.Sets.All(s => s.Id != a.SetId)))
                {
                    achievement.SetId = AchievementSet.BaseGameId;
                }

                schema.Sets.Add(new AchievementSet { Id = AchievementSet.BaseGameId, Order = -1 });
            }

            // Like on Steam's achievement page: base game sets before DLC sets.
            schema.Sets.Sort((a, b) => (a.IsDlc, a.Order, a.Id).CompareTo((b.IsDlc, b.Order, b.Id)));
        }
    }
}
