using System.Globalization;
using Microsoft.UI.Xaml;
using SAM.App.Helpers;

namespace SAM.App.Localization;

/// <summary>A selectable language in the list.</summary>
public sealed record LanguageOption(string? Locale, string Title, string Subtitle)
{
    /// <summary>Name for screen readers in the language list.</summary>
    public override string ToString() => Title + ", " + Subtitle;
}

/// <summary>The app's language: selection, persistence and mapping to the translation files.</summary>
public static class AppLanguage
{
    /// <summary>Supported language and country variants in display order.</summary>
    public static readonly IReadOnlyList<string> Locales =
    [
        "de-DE", "de-AT", "de-CH", "de-LI", "lb-LU", "nl-BE", "nl-NL", "fr-FR", "es-ES", "pt-PT", "it-IT",
        "en-GB", "en-IE", "da-DK", "sv-SE", "nb-NO", "fi-FI", "is-IS", "pl-PL", "cs-CZ", "sk-SK", "hu-HU",
        "sl-SI", "hr-HR", "bs-BA", "sr-RS", "sr-ME", "mk-MK", "sq-AL", "el-GR", "bg-BG", "ro-RO", "ro-MD",
        "uk-UA", "be-BY", "et-EE", "lv-LV", "lt-LT", "mt-MT", "el-CY", "tr-TR", "ru-RU", "en-US", "en-CA",
        "es-MX", "pt-BR", "es-AR", "es-CL", "es-CO", "es-PE", "es-VE", "es-UY", "en-AU", "en-NZ", "ja-JP",
        "zh-CN", "ko-KR", "en-IN", "id-ID", "th-TH", "vi-VN", "en-PH", "en-SG", "ms-MY", "zh-TW", "zh-HK",
        "ar-AE", "ar-SA", "he-IL", "ar-EG", "en-ZA",
    ];

    /// <summary>Chosen language, or null for the Windows setting.</summary>
    public static string? Selected { get; private set; }

    public static CultureInfo Culture { get; private set; } = CultureInfo.CurrentUICulture;

    public static FlowDirection FlowDirection => Culture.TextInfo.IsRightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

    /// <summary>Call at startup, before the first window.</summary>
    /// <param name="overrideLocale">Testing only: language for this run, not saved.</param>
    public static void Initialize(string? overrideLocale = null)
    {
        Selected = overrideLocale ?? ReadSetting();
        Culture = CultureInfo.CurrentUICulture;

        if (Selected is not null)
        {
            try
            {
                // A selected language also applies to date and number formats.
                Culture = CultureInfo.GetCultureInfo(Selected);
                CultureInfo.DefaultThreadCurrentCulture = Culture;
                CultureInfo.DefaultThreadCurrentUICulture = Culture;
                CultureInfo.CurrentCulture = Culture;
                CultureInfo.CurrentUICulture = Culture;
            }
            catch (CultureNotFoundException)
            {
                Selected = null;
                Culture = CultureInfo.CurrentUICulture;
            }
        }

        Strings.Load(ResolveTranslation(Culture));

        try
        {
            // Texts of the WinUI controls themselves (e.g. tooltips) in the same language.
            Microsoft.Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = Culture.Name;
        }
        catch (Exception)
        {
            // Older runtime: WinUI then follows the Windows language.
        }
    }

    /// <summary>Which translation file fits a language, e.g. de-LI -> de-CH, zh-HK -> zh-Hant.</summary>
    public static string ResolveTranslation(CultureInfo culture)
    {
        var name = culture.Name;
        var parts = name.Split('-');
        var language = parts[0].ToLowerInvariant();
        var region = parts.Length > 1 && parts[^1].Length == 2 ? parts[^1].ToUpperInvariant() : "";
        var script = parts.FirstOrDefault(p => p.Length == 4) ?? "";

        var id = language switch
        {
            "de" => region is "CH" or "LI" ? "de-CH" : "de",
            "pt" => region is "" or "BR" ? "pt-BR" : "pt-PT",
            "zh" => script == "Hant" || region is "TW" or "HK" or "MO" ? "zh-Hant" : "zh-Hans",
            "sr" => script == "Latn" || script == "" && region is "ME" or "BA" ? "sr-Latn" : "sr-Cyrl",
            "no" or "nn" or "nb" => "nb",
            "iw" => "he",
            "in" => "id",
            _ => language,
        };

        return Strings.Exists(id) ? id : "en";
    }

    /// <summary>
    /// 1. Windows setting, 2. the variant that matches the Windows language, 3. all others by country code (af-ZA, am-ET, ...).
    /// </summary>
    public static IReadOnlyList<LanguageOption> Options()
    {
        var windows = WindowsLanguage();
        var options = new List<LanguageOption>
        {
            new(null, Strings.LanguageSystem, Capitalize(windows.NativeName)),
        };

        var detected = DetectedLocale(windows);

        if (detected is not null)
        {
            options.Add(Option(detected, Strings.LanguageDetected));
        }

        foreach (var locale in Locales.Where(l => l != detected).OrderBy(l => l, StringComparer.OrdinalIgnoreCase))
        {
            options.Add(Option(locale, null));
        }

        return options;
    }

    private static LanguageOption Option(string locale, string? note)
    {
        var culture = CultureInfo.GetCultureInfo(locale);
        var subtitle = Capitalize(culture.DisplayName);
        subtitle = subtitle == Capitalize(culture.NativeName) ? locale : subtitle + "  ·  " + locale;
        return new LanguageOption(locale, Capitalize(culture.NativeName), note is null ? subtitle : subtitle + "  ·  " + note);
    }

    /// <summary>The offered variant that matches the Windows language: same tag, otherwise same language.</summary>
    private static string? DetectedLocale(CultureInfo windows)
    {
        var exact = Locales.FirstOrDefault(l => string.Equals(l, windows.Name, StringComparison.OrdinalIgnoreCase));

        if (exact is not null)
        {
            return exact;
        }

        var translation = ResolveTranslation(windows);
        return Locales.FirstOrDefault(l => ResolveTranslation(CultureInfo.GetCultureInfo(l)) == translation &&
                                           CultureInfo.GetCultureInfo(l).TwoLetterISOLanguageName == windows.TwoLetterISOLanguageName);
    }

    /// <summary>
    /// Name for the language button: with the country for a selected variant ("Deutsch (Schweiz)"),
    /// just the language for the Windows setting ("Deutsch").
    /// </summary>
    public static string ShortName
    {
        get
        {
            var name = Culture.NativeName;

            if (Selected is not null)
            {
                return Capitalize(name);
            }

            var cut = name.IndexOfAny(['(', '（']);
            return Capitalize((cut > 0 ? name[..cut] : name).Trim());
        }
    }

    /// <summary>Without write access the previous language stays.</summary>
    public static void Save(string? locale) => AppSettings.Write("language", locale);

    private static string? ReadSetting() => AppSettings.ReadString("language");

    /// <summary>Windows display language - even if a different language is selected in the app.</summary>
    private static CultureInfo WindowsLanguage()
    {
        try
        {
            var languages = Windows.System.UserProfile.GlobalizationPreferences.Languages;
            return languages.Count > 0 ? CultureInfo.GetCultureInfo(languages[0]) : CultureInfo.InstalledUICulture;
        }
        catch (Exception)
        {
            return CultureInfo.InstalledUICulture;
        }
    }

    private static string Capitalize(string text)
        => text.Length > 0 ? char.ToUpperInvariant(text[0]) + text[1..] : text;
}
