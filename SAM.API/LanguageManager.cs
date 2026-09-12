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
using System.Linq;

namespace SAM.API
{
    public class LanguageInfo
    {
        public string Code { get; }
        public string NativeName { get; }

        public LanguageInfo(string code, string nativeName)
        {
            this.Code = code;
            this.NativeName = nativeName;
        }
    }

    public static class LanguageManager
    {
        public static readonly List<LanguageInfo> SupportedLanguages = new()
        {
            new("english", "English"),
            new("russian", "Русский"),
            new("german", "Deutsch"),
            new("french", "Français"),
            new("spanish", "Español"),
            new("latam", "Español (Latinoamérica)"),
            new("italian", "Italiano"),
            new("japanese", "日本語"),
            new("koreana", "한국어"),
            new("polish", "Polski"),
            new("portuguese", "Português"),
            new("brazilian", "Português (Brasil)"),
            new("schinese", "简体中文"),
            new("tchinese", "繁體中文"),
            new("ukrainian", "Українська"),
            new("turkish", "Türkçe"),
            new("czech", "Čeština"),
            new("hungarian", "Magyar"),
            new("romanian", "Română"),
            new("swedish", "Svenska"),
            new("thai", "ไทย"),
            new("vietnamese", "Tiếng Việt"),
            new("arabic", "العربية"),
            new("bulgarian", "Български"),
            new("danish", "Dansk"),
            new("dutch", "Nederlands"),
            new("finnish", "Suomi"),
            new("greek", "Ελληνικά"),
            new("norwegian", "Norsk"),
        };

        private static string _currentLanguage;

        public static string CurrentLanguage
        {
            get
            {
                if (string.IsNullOrEmpty(_currentLanguage) == true)
                {
                    string saved = Settings.Instance.Language;
                    if (string.IsNullOrEmpty(saved) == false && IsSupported(saved) == true)
                    {
                        _currentLanguage = saved;
                    }
                    else
                    {
                        _currentLanguage = DetectWindowsLanguage();
                    }
                }
                return _currentLanguage;
            }
            set
            {
                if (IsSupported(value) == true)
                {
                    _currentLanguage = value;
                    Settings.Instance.Language = value;
                    Settings.Instance.Save();
                }
            }
        }

        public static bool IsSupported(string code)
        {
            return SupportedLanguages.Any(l => string.Equals(l.Code, code, StringComparison.OrdinalIgnoreCase));
        }

        public static string DetectWindowsLanguage()
        {
            string culture = CultureInfo.CurrentUICulture.Name;
            string twoLetter = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            if (twoLetter == "ru") return "russian";
            if (twoLetter == "de") return "german";
            if (twoLetter == "fr") return "french";
            if (twoLetter == "es") return culture.Equals("es-419", StringComparison.OrdinalIgnoreCase) ? "latam" : "spanish";
            if (twoLetter == "it") return "italian";
            if (twoLetter == "ja") return "japanese";
            if (twoLetter == "ko") return "koreana";
            if (twoLetter == "pl") return "polish";
            if (twoLetter == "pt") return culture.Equals("pt-BR", StringComparison.OrdinalIgnoreCase) ? "brazilian" : "portuguese";
            if (twoLetter == "zh") return (culture.Equals("zh-TW", StringComparison.OrdinalIgnoreCase) || culture.Equals("zh-HK", StringComparison.OrdinalIgnoreCase) || culture.Equals("zh-Hant", StringComparison.OrdinalIgnoreCase)) ? "tchinese" : "schinese";
            if (twoLetter == "uk") return "ukrainian";
            if (twoLetter == "tr") return "turkish";
            if (twoLetter == "cs") return "czech";
            if (twoLetter == "hu") return "hungarian";
            if (twoLetter == "ro") return "romanian";
            if (twoLetter == "sv") return "swedish";
            if (twoLetter == "th") return "thai";
            if (twoLetter == "vi") return "vietnamese";
            if (twoLetter == "ar") return "arabic";
            if (twoLetter == "bg") return "bulgarian";
            if (twoLetter == "da") return "danish";
            if (twoLetter == "nl") return "dutch";
            if (twoLetter == "fi") return "finnish";
            if (twoLetter == "el") return "greek";
            if (twoLetter == "no" || twoLetter == "nb" || twoLetter == "nn") return "norwegian";

            return "english";
        }
    }
}
