using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using SAM.API;
using APITypes = SAM.API.Types;

namespace SAM.CLI
{
    class GetAchievementsCommand
    {
        public static object Execute(API.Client client, long appId)
        {
            try
            {
                // 1. Carregar o Schema (O Mapa das Conquistas)
                string steamPath = API.Steam.GetInstallPath();
                string schemaPath = System.IO.Path.Combine(
                    steamPath,
                    "appcache", "stats",
                    $"UserGameStatsSchema_{appId}.bin"
                );
                
                if (!System.IO.File.Exists(schemaPath))
                {
                    return new { 
                        success = false, 
                        error = $"Schema file not found at {schemaPath}. Run the game at least once.",
                        appid = appId 
                    };
                }
                
                // Usando o KeyValue que copiamos para o projeto
                var schemaKv = KeyValue.LoadAsBinary(schemaPath);
                if (schemaKv == null)
                {
                    return new { success = false, error = "Failed to parse schema file", appid = appId };
                }
                
                // 2. Extrair definições do Schema
                var currentLanguage = client.SteamApps008.GetCurrentGameLanguage();
                var achievementDefs = LoadAchievementDefinitions(schemaKv, appId, currentLanguage);
                
                // 3. Pedir Stats atuais para o Steam
                var steamId = client.SteamUser.GetSteamId();
                client.SteamUserStats.RequestUserStats(steamId);
                
                // Esperar um pouco para garantir que o Steam carregou (callback)
                // O SAM original usa callbacks, aqui vamos fazer um loop simples de espera
                bool statsReceived = false;
                var callback = client.CreateAndRegisterCallback<API.Callbacks.UserStatsReceived>();
                callback.OnRun += (param) => { statsReceived = true; };
                
                for (int i = 0; i < 20; i++) // Espera até 2 segundos
                {
                    client.RunCallbacks(false);
                    if (statsReceived) break;
                    Thread.Sleep(100);
                }
                
                // 4. Cruzar os dados: Schema + Status Real
                var achievements = new List<object>();
                foreach (var def in achievementDefs)
                {
                    // Pergunta ao Steam o status desta conquista específica
                    bool isAchieved;
                    uint unlockTime;
                    bool success = client.SteamUserStats.GetAchievementAndUnlockTime(def.Id, out isAchieved, out unlockTime);
                    
                    if (success)
                    {
                        achievements.Add(new {
                            id = def.Id,
                            name = def.Name,
                            description = def.Description,
                            unlocked = isAchieved,
                            unlocktime = isAchieved && unlockTime > 0 ? (long)unlockTime : 0,
                            icon = def.IconNormal,
                            hidden = def.IsHidden
                        });
                    }
                }
                
                return new {
                    success = true,
                    appid = appId,
                    achievements = achievements,
                    total = achievements.Count,
                    unlocked = achievements.Count(a => ((dynamic)a).unlocked == true)
                };
            }
            catch (Exception ex)
            {
                return new { success = false, error = ex.Message, stackTrace = ex.StackTrace };
            }
        }
        
        // Lógica extraída do SAM.Game/Manager.cs para ler a estrutura do arquivo binário
        private static List<AchievementDefinition> LoadAchievementDefinitions(KeyValue schemaKv, long appId, string language)
        {
            var defs = new List<AchievementDefinition>();
            
            // Navega na estrutura: AppID -> stats
            var stats = schemaKv[appId.ToString(CultureInfo.InvariantCulture)]["stats"];
            if (!stats.Valid || stats.Children == null) return defs;
            
            foreach (var stat in stats.Children)
            {
                if (!stat.Valid) continue;
                
                // Verifica o tipo (Achievements = 3 ou 4)
                var type = (APITypes.UserStatType)(stat["type_int"].Valid ? stat["type_int"].AsInteger(0) : stat["type"].AsInteger(0));
                
                if (type == APITypes.UserStatType.Achievements || type == APITypes.UserStatType.GroupAchievements)
                {
                    if (stat.Children == null) continue;
                    
                    // Procura pela seção "bits" onde ficam as conquistas
                    foreach (var bits in stat.Children.Where(b => string.Compare(b.Name, "bits", StringComparison.InvariantCultureIgnoreCase) == 0))
                    {
                        if (!bits.Valid || bits.Children == null) continue;
                        
                        foreach (var bit in bits.Children)
                        {
                            string id = bit["name"].AsString("");
                            if (string.IsNullOrEmpty(id)) continue;
                            
                            string name = GetLocalizedString(bit["display"]["name"], language, id);
                            string desc = GetLocalizedString(bit["display"]["desc"], language, "");
                            
                            defs.Add(new AchievementDefinition {
                                Id = id,
                                Name = name,
                                Description = desc,
                                IconNormal = bit["display"]["icon"].AsString(""),
                                IconLocked = bit["display"]["icon_gray"].AsString(""),
                                IsHidden = bit["display"]["hidden"].AsBoolean(false)
                            });
                        }
                    }
                }
            }
            return defs;
        }
        
        private static string GetLocalizedString(KeyValue kv, string language, string defaultValue)
        {
            var val = kv[language].AsString("");
            if (!string.IsNullOrEmpty(val)) return val;
            
            val = kv["english"].AsString("");
            if (!string.IsNullOrEmpty(val)) return val;
            
            val = kv.AsString("");
            return !string.IsNullOrEmpty(val) ? val : defaultValue;
        }
    }
    
    class AchievementDefinition
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string IconNormal { get; set; }
        public string IconLocked { get; set; }
        public bool IsHidden { get; set; }
    }
}
