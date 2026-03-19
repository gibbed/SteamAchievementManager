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
using System.IO;

namespace SAM.API
{
    public static class Localization
    {
        private static string _LoadedLanguage = null;
        private static readonly Dictionary<string, string> _Dictionary = new(StringComparer.OrdinalIgnoreCase);

        public static string AppVersion
        {
            get
            {
                try
                {
                    var ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                    if (ver != null && ver.Major > 0)
                    {
                        return $"{ver.Major}.{ver.Minor}.{ver.Build}";
                    }
                }
                catch { }
                return "8.0.0";
            }
        }

        private static void EnsureLanguageLoaded()
        {
            string current = LanguageManager.CurrentLanguage;
            if (string.Equals(_LoadedLanguage, current, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            _Dictionary.Clear();
            _LoadedLanguage = current;

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string langFile = Path.Combine(baseDir, "languages", current + ".json");
            if (File.Exists(langFile) == false)
            {
                langFile = Path.Combine(baseDir, "..", "languages", current + ".json");
            }

            if (File.Exists(langFile))
            {
                try
                {
                    string json = File.ReadAllText(langFile);
                    var parsed = ParseSimpleJson(json);
                    foreach (var kv in parsed)
                    {
                        _Dictionary[kv.Key] = kv.Value;
                    }
                }
                catch { }
            }
        }

        private static Dictionary<string, string> ParseSimpleJson(string json)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(json)) return dict;

            var lines = json.Split('\n');
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("\"") == false) continue;

                int colonIdx = trimmed.IndexOf(':');
                if (colonIdx <= 0) continue;

                string keyPart = trimmed.Substring(0, colonIdx).Trim();
                string valPart = trimmed.Substring(colonIdx + 1).Trim();

                if (keyPart.StartsWith("\"") && keyPart.EndsWith("\""))
                {
                    keyPart = keyPart.Substring(1, keyPart.Length - 2);
                }

                if (valPart.StartsWith("\""))
                {
                    int lastQuote = valPart.LastIndexOf('\"');
                    if (lastQuote > 0)
                    {
                        valPart = valPart.Substring(1, lastQuote - 1);
                    }
                }
                else if (valPart.EndsWith(","))
                {
                    valPart = valPart.Substring(0, valPart.Length - 1).Trim();
                }

                valPart = valPart.Replace("\\\"", "\"").Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\\\", "\\");
                dict[keyPart] = valPart;
            }
            return dict;
        }

        public static string Get(string key, string fallback)
        {
            EnsureLanguageLoaded();
            if (_Dictionary.TryGetValue(key, out var value) && string.IsNullOrEmpty(value) == false)
            {
                return value;
            }
            return fallback;
        }

        // GamePicker strings
        public static string PickerTitle => $"Steam Achievement Manager {AppVersion}";
        public static string GameTitle(string gameName) => string.IsNullOrEmpty(gameName)
            ? $"Steam Achievement Manager {AppVersion}"
            : $"Steam Achievement Manager {AppVersion} | {gameName}";

        public static string RefreshGames => Get("RefreshGames", "Обновить список");
        public static string AddGame => Get("AddGame", "Добавить по ID");
        public static string Filter => Get("Filter", "Фильтр");
        public static string GameFiltering => Get("GameFiltering", "Фильтр игр");
        public static string ShowGames => Get("ShowGames", "Показывать игры");
        public static string ShowDemos => Get("ShowDemos", "Показывать демо");
        public static string ShowMods => Get("ShowMods", "Показывать моды");
        public static string ShowJunk => Get("ShowJunk", "Показывать мусор");
        public static string DownloadStatus => Get("DownloadStatus", "Статус загрузки");
        public static string DownloadingGameList => Get("DownloadingGameList", "Загрузка списка игр...");
        public static string CheckingGameOwnership => Get("CheckingGameOwnership", "Проверка наличия игр...");
        public static string Language => Get("Language", "Язык");

        public static string DisplayingGames(int count, int total) =>
            string.Format(Get("DisplayingGamesFormat", "Отображено игр: {0}. Всего игр: {1}."), count, total);

        public static string DownloadingGameIcons(int count) =>
            string.Format(Get("DownloadingGameIconsFormat", "Загрузка {0} иконок игр..."), count);

        public static string FailedToStartGameExe => Get("FailedToStartGameExe", "Не удалось запустить SAM.Game.exe.");
        public static string PleaseEnterValidGameId => Get("PleaseEnterValidGameId", "Пожалуйста, введите корректный ID игры.");
        public static string DontOwnGame => Get("DontOwnGame", "Вы не владеете этой игрой в вашей библиотеке Steam.");

        public static string Error => Get("Error", "Ошибка");
        public static string Warning => Get("Warning", "Предупреждение");
        public static string Question => Get("Question", "Вопрос");
        public static string Information => Get("Information", "Информация");

        // Manager (SAM.Game) strings
        public static string CommitChanges => Get("CommitChanges", "Сохранить изменения");
        public static string CommitChangesToolTip => Get("CommitChangesToolTip", "Сохранить выбранные достижения и изменения статистики на серверах Steam");

        public static string Refresh => Get("Refresh", "Обновить");
        public static string RefreshToolTip => Get("RefreshToolTip", "Перезагрузить достижения и статистику из Steam");

        public static string Reset => Get("Reset", "Сбросить");
        public static string ResetToolTip => Get("ResetToolTip", "Сбросить несохранённые изменения");

        public static string AchievementsTab => Get("AchievementsTab", "Достижения");
        public static string StatisticsTab => Get("StatisticsTab", "Статистика");

        public static string HeaderName => Get("HeaderName", "Название");
        public static string HeaderDescription => Get("HeaderDescription", "Описание");
        public static string HeaderUnlockTime => Get("HeaderUnlockTime", "Время разблокировки");
        public static string HeaderValue => Get("HeaderValue", "Значение");
        public static string HeaderExtra => Get("HeaderExtra", "Дополнительно");

        public static string LockAll => Get("LockAll", "Заблокировать все");
        public static string LockAllToolTip => Get("LockAllToolTip", "Заблокировать все достижения.");
        public static string InvertAll => Get("InvertAll", "Инвертировать");
        public static string InvertAllToolTip => Get("InvertAllToolTip", "Инвертировать все достижения.");
        public static string UnlockAll => Get("UnlockAll", "Разблокировать все");
        public static string UnlockAllToolTip => Get("UnlockAllToolTip", "Разблокировать все достижения.");

        public static string AchievementView => Get("AchievementView", "Вид достижений");
        public static string ShowUnlocked => Get("ShowUnlocked", "Показывать полученные");
        public static string ShowLocked => Get("ShowLocked", "Показывать заблокированные");
        public static string ShowHidden => Get("ShowHidden", "Показывать скрытые");

        public static string NormalCountFormat(int count) =>
            string.Format(Get("NormalCountFormat", "🏆 Обычных: {0}"), count);

        public static string HiddenCountFormat(int count) =>
            string.Format(Get("HiddenCountFormat", "👁️ Скрытых: {0}"), count);

        public static string HiddenTag => Get("HiddenTag", "Скрытое");

        public static string ShowOnly => Get("ShowOnly", "Показывать:");
        public static string Locked => Get("Locked", "закрытые");
        public static string Unlocked => Get("Unlocked", "полученные");
        public static string MatchingStringToolTip => Get("MatchingStringToolTip", "Введите минимум 3 символа из названия или описания");

        public static string EnableStatsEditing => Get("EnableStatsEditing", "Я понимаю, что изменение значений статистики может всё испортить, и винить в этом могу только себя.");

        public static string DownloadingIcons(int count) =>
            string.Format(Get("DownloadingIconsFormat", "Загрузка {0} иконок..."), count);

        public static string GenericErrorNotOwned => Get("GenericErrorNotOwned", "Общая ошибка — обычно это означает, что вы не владеете игрой.");

        public static string ErrorRetrievingStats(string err) =>
            string.Format(Get("ErrorRetrievingStatsFormat", "Ошибка при получении статистики из Steam: {0}"), err);

        public static string FailedToLoadSchema => Get("FailedToLoadSchema", "Не удалось загрузить схему достижений и статистики для этой игры.");

        public static string ErrorHandlingAchievements => Get("ErrorHandlingAchievements", "Ошибка при обработке полученных достижений.");
        public static string ErrorHandlingStats => Get("ErrorHandlingStats", "Ошибка при обработке полученной статистики.");

        public static string RetrievedAchievementsAndStats(int ach, int stats) =>
            string.Format(Get("RetrievedAchievementsAndStatsFormat", "Получено достижений: {0}, элементов статистики: {1}."), ach, stats);

        public static string RetrievingStatInfo => Get("RetrievingStatInfo", "Получение информации о достижениях и статистике...");

        public static string ErrorSettingState(string id) =>
            string.Format(Get("ErrorSettingStateFormat", "Произошла ошибка при установке состояния для {0}, сохранение отменено."), id);

        public static string ErrorSettingValue(string id) =>
            string.Format(Get("ErrorSettingValueFormat", "Произошла ошибка при установке значения для {0}, сохранение отменено."), id);

        public static string ErrorStoringAborting => Get("ErrorStoringAborting", "Произошла ошибка при сохранении, отмена.");

        public static string StoredAchievementsAndStats(int ach, int stats) =>
            string.Format(Get("StoredAchievementsAndStatsFormat", "Сохранено достижений: {0}, элементов статистики: {1}."), ach, stats);

        public static string StatIsProtected => Get("StatIsProtected", "Статистика защищена! Вы не можете её изменить.");
        public static string InvalidValue => Get("InvalidValue", "Некорректное значение");

        public static string ConfirmResetStats => Get("ConfirmResetStats", "Вы абсолютно уверены, что хотите сбросить статистику?");
        public static string ConfirmResetAchievementsToo => Get("ConfirmResetAchievementsToo", "Вы хотите сбросить и достижения тоже?");
        public static string ConfirmReallySure => Get("ConfirmReallySure", "Вы точно-точно уверены?");

        public static string ProtectedAchievementNotice => Get("ProtectedAchievementNotice", "К сожалению, это защищённое достижение, и им нельзя управлять с помощью Steam Achievement Manager.");

        public static string ParseAppIdFailed => Get("ParseAppIdFailed", "Не удалось прочитать ID приложения из аргумента командной строки.");
        public static string RunFromSteamDeclined => Get("RunFromSteamDeclined", "Инструмент не может запускаться из папки Steam.");
        public static string SteamNotRunning => Get("SteamNotRunning", "Steam не запущен. Пожалуйста, запустите Steam и попробуйте снова.");
        public static string FamilyShareLocked => Get("FamilyShareLocked", "Если игра доступна по Family Share, возможно, доступ заблокирован, так как владелец аккаунта в данный момент играет.");
        public static string ExceptionalError => Get("ExceptionalError", "Произошла непредвиденная ошибка!");
    }
}
