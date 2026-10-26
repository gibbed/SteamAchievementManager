using System.Globalization;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SAM.App.Helpers;
using SAM.App.Localization;
using SAM.App.Views;

namespace SAM.App;

public partial class App : Application
{
    private Window? _window;

    public App()
    {
#if DEBUG
        // For checking both themes without changing the Windows theme.
        if (Environment.GetEnvironmentVariable("SAM_THEME") is "light" or "dark")
        {
            RequestedTheme = Environment.GetEnvironmentVariable("SAM_THEME") == "light" ? ApplicationTheme.Light : ApplicationTheme.Dark;
        }
#endif

        // Set the language before the first window: texts, date and number formats.
        string? languageOverride = null;
#if DEBUG
        // Testing only: --language de-CH applies to this run without saving the setting.
        var debugArguments = Environment.GetCommandLineArgs();
        var languageIndex = Array.IndexOf(debugArguments, "--language");
        languageOverride = languageIndex > 0 && languageIndex + 1 < debugArguments.Length ? debugArguments[languageIndex + 1] : null;
#endif
        AppLanguage.Initialize(languageOverride);

#if DEBUG
        // Testing only: --dump-languages <file> writes the mapping of all languages and exits.
        var dumpIndex = Array.IndexOf(debugArguments, "--dump-languages");
        if (dumpIndex > 0 && dumpIndex + 1 < debugArguments.Length)
        {
            var lines = AppLanguage.Locales.Select(locale =>
            {
                var culture = CultureInfo.GetCultureInfo(locale);
                return string.Join('\t', locale, AppLanguage.ResolveTranslation(culture), culture.NativeName, culture.TextInfo.IsRightToLeft ? "RTL" : "LTR");
            });
            File.WriteAllLines(debugArguments[dumpIndex + 1], lines);
            Environment.Exit(0);
        }
#endif

        InitializeComponent();

        UnhandledException += (_, e) =>
        {
            // Show unexpected errors instead of closing the app silently.
            e.Handled = true;
            (_window as INotifier)?.Notify(Strings.UnexpectedError(e.Exception.Message), Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error);
        };
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        ImageCache.TrimInBackground();

        // As in the original: without arguments the game picker, with an app ID the manager for that game.
        // Steam binds the app ID to the process, so every game runs in a process of its own.
        var arguments = Environment.GetCommandLineArgs().Skip(1).ToArray();

#if DEBUG
        // Testing only: all medals in several sizes on light and dark backgrounds.
        if (arguments.Length > 0 && arguments[0] == "--medals")
        {
            _window = CreateMedalTestWindow();
            _window.Activate();
            return;
        }

        // UI testing only: real schema from disk, simulated states, no Steam.
        if (arguments.Length > 1 && arguments[0] == "--demo" && uint.TryParse(arguments[1], out var demoId))
        {
            _window = new GameWindow(demoId, demo: true);
            _window.Activate();
            return;
        }
#endif

        if (arguments.Length > 0 && uint.TryParse(arguments[0], NumberStyles.None, CultureInfo.InvariantCulture, out var appId) && appId > 0)
        {
            _window = new GameWindow(appId);
        }
        else
        {
            var library = new LibraryWindow();

            if (arguments.Length > 0)
            {
                library.StartupMessage = Strings.InvalidAppIdArgument(arguments[0]);
            }

            _window = library;
        }

        _window.Activate();
    }

#if DEBUG
    private static Window CreateMedalTestWindow()
    {
        var rows = new StackPanel { Spacing = 0 };

        foreach (var background in new[] { Microsoft.UI.Colors.White, Windows.UI.Color.FromArgb(255, 32, 32, 32) })
        {
            var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 24, Padding = new Thickness(24), Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(background) };

            foreach (var kind in new[] { MedalKind.Game, MedalKind.Set, MedalKind.SetDouble, MedalKind.DlcSet, MedalKind.Unfinished })
            {
                foreach (var size in new[] { 20.0, 40.0, 128.0 })
                {
                    row.Children.Add(new MedalView { Kind = kind, Width = size, Height = size, VerticalAlignment = VerticalAlignment.Bottom });
                }
            }

            rows.Children.Add(row);
        }

        return new Window { Content = rows, Title = "SAM Medaillen" };
    }
#endif
}
