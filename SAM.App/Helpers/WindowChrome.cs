using System.Runtime.InteropServices;
using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;
using Windows.Graphics;
using Windows.UI;

namespace SAM.App.Helpers;

/// <summary>Custom title bar with Mica, icon and window size - shared by all windows.</summary>
internal sealed partial class WindowChrome
{
    private readonly Window _window;
    private readonly FrameworkElement _root;
    private readonly FrameworkElement _titleBar;
    private readonly ColumnDefinition _rightInset;
    private readonly FrameworkElement[] _interactive;

    private WindowChrome(Window window, FrameworkElement root, FrameworkElement titleBar, ColumnDefinition rightInset, FrameworkElement[] interactive)
    {
        _window = window;
        _root = root;
        _titleBar = titleBar;
        _rightInset = rightInset;
        _interactive = interactive;
    }

    public static WindowChrome Apply(Window window, FrameworkElement root, FrameworkElement titleBar, ColumnDefinition rightInset, params FrameworkElement[] interactive)
    {
        var chrome = new WindowChrome(window, root, titleBar, rightInset, interactive);

        window.ExtendsContentIntoTitleBar = true;
        window.SetTitleBar(titleBar);
        window.AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
        window.AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets", "SAM.ico"));

        root.Loaded += (_, _) => { chrome.UpdateCaptionButtons(); chrome.UpdateRegions(); };
        root.ActualThemeChanged += (_, _) => chrome.UpdateCaptionButtons();
        titleBar.SizeChanged += (_, _) => chrome.UpdateRegions();

        foreach (var element in interactive)
        {
            element.SizeChanged += (_, _) => chrome.UpdateRegions();
            element.RegisterPropertyChangedCallback(UIElement.VisibilityProperty, (_, _) => chrome.UpdateRegions());
        }

        return chrome;
    }

    public double Scale => GetDpiForWindow(WinRT.Interop.WindowNative.GetWindowHandle(_window)) / 96.0;

    /// <summary>Centre the window on the current screen, in logical pixels.</summary>
    public void CenterOnScreen(double width, double height, double minWidth, double minHeight)
    {
        var scale = Scale;
        var area = DisplayArea.GetFromWindowId(_window.AppWindow.Id, DisplayAreaFallback.Primary).WorkArea;
        var w = (int)Math.Min(width * scale, area.Width - 40);
        var h = (int)Math.Min(height * scale, area.Height - 40);
        _window.AppWindow.MoveAndResize(new RectInt32(area.X + (area.Width - w) / 2, area.Y + (area.Height - h) / 2, w, h));

        if (_window.AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.PreferredMinimumWidth = (int)(minWidth * scale);
            presenter.PreferredMinimumHeight = (int)(minHeight * scale);
        }
    }

    private void UpdateCaptionButtons()
    {
        var dark = _root.ActualTheme == ElementTheme.Dark;
        var bar = _window.AppWindow.TitleBar;
        var foreground = dark ? Colors.White : Colors.Black;

        bar.ButtonBackgroundColor = Colors.Transparent;
        bar.ButtonInactiveBackgroundColor = Colors.Transparent;
        bar.ButtonForegroundColor = foreground;
        bar.ButtonHoverForegroundColor = foreground;
        bar.ButtonPressedForegroundColor = foreground;
        bar.ButtonInactiveForegroundColor = dark ? Color.FromArgb(0x80, 255, 255, 255) : Color.FromArgb(0x80, 0, 0, 0);
        bar.ButtonHoverBackgroundColor = dark ? Color.FromArgb(0x15, 255, 255, 255) : Color.FromArgb(0x0A, 0, 0, 0);
        bar.ButtonPressedBackgroundColor = dark ? Color.FromArgb(0x0B, 255, 255, 255) : Color.FromArgb(0x06, 0, 0, 0);
    }

    /// <summary>The title bar drags the window - only the search box and buttons in it stay clickable.</summary>
    public void UpdateRegions()
    {
        if (_titleBar.XamlRoot is null)
        {
            return;
        }

        var scale = _titleBar.XamlRoot.RasterizationScale;
        _rightInset.Width = new GridLength(_window.AppWindow.TitleBar.RightInset / scale);

        var rects = new List<RectInt32>();

        foreach (var element in _interactive)
        {
            if (element.Visibility != Visibility.Visible || element.ActualWidth <= 0)
            {
                continue;
            }

            var bounds = element.TransformToVisual(null).TransformBounds(new Rect(0, 0, element.ActualWidth, element.ActualHeight));
            rects.Add(new RectInt32((int)(bounds.X * scale), (int)(bounds.Y * scale), (int)(bounds.Width * scale), (int)(bounds.Height * scale)));
        }

        var source = InputNonClientPointerSource.GetForWindowId(_window.AppWindow.Id);

        if (rects.Count == 0)
        {
            source.ClearRegionRects(NonClientRegionKind.Passthrough);
        }
        else
        {
            source.SetRegionRects(NonClientRegionKind.Passthrough, rects.ToArray());
        }
    }

    [LibraryImport("user32.dll")]
    private static partial uint GetDpiForWindow(nint hwnd);
}
