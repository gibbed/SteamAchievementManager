using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using SAM.API;
using APITypes = SAM.API.Types;
using JsonFormatting = Newtonsoft.Json.Formatting; // Alias para evitar conflito

namespace SAM.CLI
{
    // Classe para resultado padronizado
    public class CommandResult
    {
        public bool success { get; set; }
        public string error { get; set; }
        public object data { get; set; }
    }

    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.Error.WriteLine("Usage: SAM.CLI.exe <command> <appid> [args...]");
                Console.Error.WriteLine("Commands:");
                Console.Error.WriteLine("  get-achievements <appid>           - List all achievements");
                Console.Error.WriteLine("  unlock <appid> <achievement_id>   - Unlock an achievement");
                Console.Error.WriteLine("  lock <appid> <achievement_id>     - Lock an achievement");
                return 1;
            }

            string command = args[0].ToLower();
            if (!long.TryParse(args[1], out long appId))
            {
                var errorResult = new { success = false, error = $"Invalid appid: {args[1]}" };
                Console.Error.WriteLine(JsonConvert.SerializeObject(errorResult, JsonFormatting.Indented));
                return 1;
            }

            try
            {
                using (var client = new API.Client())
                {
                    client.Initialize(appId);

                    object result = null;

                    // Usar if/else ao invés de switch expression
                    if (command == "get-achievements")
                    {
                        result = GetAchievementsCommand.Execute(client, appId);
                    }
                    else if (command == "unlock")
                    {
                        string achievementId = args.Length > 2 ? args[2] : null;
                        result = UnlockCommand.Execute(client, appId, achievementId);
                    }
                    else if (command == "lock")
                    {
                        string achievementId = args.Length > 2 ? args[2] : null;
                        result = LockCommand.Execute(client, appId, achievementId);
                    }
                    else
                    {
                        result = new { success = false, error = $"Unknown command: {command}" };
                    }

                    string json = JsonConvert.SerializeObject(result, JsonFormatting.Indented);
                    Console.WriteLine(json);

                    // Verificar success usando reflection ou try-cast
                    try
                    {
                        var resultType = result.GetType();
                        var successProp = resultType.GetProperty("success");
                        if (successProp != null)
                        {
                            var successValue = successProp.GetValue(result);
                            if (successValue is bool boolVal && boolVal == false)
                            {
                                return 1;
                            }
                        }
                    }
                    catch
                    {
                        // Se não conseguir verificar, assume sucesso
                    }

                    return 0;
                }
            }
            catch (API.ClientInitializeException ex)
            {
                var error = new
                {
                    success = false,
                    error = "Steam is not running or failed to initialize",
                    details = ex.Message
                };
                Console.Error.WriteLine(JsonConvert.SerializeObject(error, JsonFormatting.Indented));
                return 1;
            }
            catch (Exception ex)
            {
                var error = new
                {
                    success = false,
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                };
                Console.Error.WriteLine(JsonConvert.SerializeObject(error, JsonFormatting.Indented));
                return 1;
            }
        }
    }
}