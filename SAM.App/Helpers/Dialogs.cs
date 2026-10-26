using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using SAM.App.Localization;
using Windows.UI;

namespace SAM.App.Helpers;

/// <summary>Windows 11 style ContentDialogs - they replace the original's message boxes.</summary>
internal static class Dialogs
{
    public static ContentDialog Create(XamlRoot root, string title, object content, string primaryText, bool showClose = true, bool danger = false)
    {
        var dialog = new ContentDialog
        {
            XamlRoot = root,
            Title = title,
            Content = content,
            PrimaryButtonText = primaryText,
            CloseButtonText = showClose ? Strings.Cancel : null,
            FlowDirection = AppLanguage.FlowDirection,
            DefaultButton = ContentDialogButton.Primary,
            Style = (Style)Application.Current.Resources["DefaultContentDialogStyle"],
            RequestedTheme = (root.Content as FrameworkElement)?.ActualTheme ?? ElementTheme.Default,
        };

        if (danger)
        {
            // The primary button uses the accent style - colour it red for irreversible actions.
            var normal = new SolidColorBrush(Color.FromArgb(255, 0xC4, 0x2B, 0x1C));
            var hover = new SolidColorBrush(Color.FromArgb(255, 0xD1, 0x34, 0x38));
            var pressed = new SolidColorBrush(Color.FromArgb(255, 0xA4, 0x26, 0x2C));
            var white = new SolidColorBrush(Colors.White);

            dialog.Resources["AccentButtonBackground"] = normal;
            dialog.Resources["AccentButtonBackgroundPointerOver"] = hover;
            dialog.Resources["AccentButtonBackgroundPressed"] = pressed;
            dialog.Resources["AccentButtonBorderBrush"] = normal;
            dialog.Resources["AccentButtonBorderBrushPointerOver"] = hover;
            dialog.Resources["AccentButtonBorderBrushPressed"] = pressed;
            dialog.Resources["AccentButtonForeground"] = white;
            dialog.Resources["AccentButtonForegroundPointerOver"] = white;
            dialog.Resources["AccentButtonForegroundPressed"] = white;
        }

        return dialog;
    }

    public static async Task ShowErrorAsync(XamlRoot root, string title, string message)
    {
        var text = new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap, IsTextSelectionEnabled = true };
        await Create(root, title, text, Strings.Ok, showClose: false).ShowAsync();
    }

    public static async Task<bool> ConfirmAsync(XamlRoot root, string title, string message, string primaryText, bool danger = false)
    {
        var text = new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap };
        return await Create(root, title, text, primaryText, danger: danger).ShowAsync() == ContentDialogResult.Primary;
    }
}
