using System.Numerics;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;

namespace SAM.App.Helpers;

/// <summary>
/// A medal in the size of the element - static (SVG) or animated like on Steam's achievement pages.
/// The animation is an image strip moved via Composition: it runs on the GPU and costs the
/// UI thread nothing. Sparks may extend beyond the edge of the medal, just like on Steam.
/// </summary>
public sealed partial class MedalView : Grid
{
    public static readonly DependencyProperty KindProperty = DependencyProperty.Register(
        nameof(Kind), typeof(MedalKind), typeof(MedalView), new PropertyMetadata(MedalKind.None, (d, _) => ((MedalView)d).Update()));

    private readonly Image _static = new() { Stretch = Stretch.Uniform };
    private readonly Canvas _layer = new() { IsHitTestVisible = false };
    private readonly Canvas _window = new();
    private readonly Image _sprite = new() { Stretch = Stretch.Fill };
    private bool _isLoaded;
    private string? _running;

    public MedalView()
    {
        Children.Add(_static);
        Children.Add(_layer);
        _layer.Children.Add(_window);
        _window.Children.Add(_sprite);
        _sprite.ImageOpened += (_, _) => _static.Opacity = _running is null ? 1 : 0;

        Loaded += (_, _) =>
        {
            _isLoaded = true;
            Update();
        };
        Unloaded += (_, _) =>
        {
            _isLoaded = false;
            Stop();
        };
        SizeChanged += (_, _) => Update();
    }

    public MedalKind Kind
    {
        get => (MedalKind)GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    private void Update()
    {
        var kind = Kind;
        _static.Source = Medals.Get(kind);

        if (!_isLoaded || ActualWidth <= 0 || ActualHeight <= 0 || MedalAnimations.For(kind) is not { } animation)
        {
            Stop();
            return;
        }

        // Where the static medal sits in the element (Uniform) - the frame also covers the margin for the sparks.
        var (sx, sy, sw, sh) = (animation.StaticBox[0], animation.StaticBox[1], animation.StaticBox[2], animation.StaticBox[3]);
        var (fx, fy, fw, fh) = (animation.FrameBox[0], animation.FrameBox[1], animation.FrameBox[2], animation.FrameBox[3]);
        var scale = Math.Min(ActualWidth / sw, ActualHeight / sh);
        var width = fw * scale;
        var height = fh * scale;
        var key = kind + "|" + width.ToString("F2", System.Globalization.CultureInfo.InvariantCulture) + "|" + height.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);

        if (_running == key)
        {
            return;
        }

        Stop();
        _running = key;

        Canvas.SetLeft(_window, (ActualWidth - sw * scale) / 2 + (fx - sx) * scale);
        Canvas.SetTop(_window, (ActualHeight - sh * scale) / 2 + (fy - sy) * scale);
        _window.Width = width;
        _window.Height = height;
        _window.Clip = new RectangleGeometry { Rect = new Rect(0, 0, width, height) };
        _sprite.Width = width * animation.FrameCount;
        _sprite.Height = height;

        var sheet = MedalAnimations.Sheet(kind, animation);
        _sprite.Source = sheet;
        _static.Opacity = sheet.PixelWidth > 0 ? 0 : 1;
        _window.Visibility = Visibility.Visible;

        ElementCompositionPreview.SetIsTranslationEnabled(_sprite, true);
        var visual = ElementCompositionPreview.GetElementVisual(_sprite);
        var compositor = visual.Compositor;
        var frames = compositor.CreateVector3KeyFrameAnimation();
        var step = compositor.CreateStepEasingFunction();

        for (var i = 0; i < animation.Times.Length; i++)
        {
            var offset = new Vector3((float)(-animation.Frames[i] * width), 0, 0);
            frames.InsertKeyFrame((float)(animation.Times[i] / animation.DurationMs), offset, step);
        }

        frames.InsertKeyFrame(1f, new Vector3((float)(-animation.Frames[^1] * width), 0, 0), step);
        frames.Duration = TimeSpan.FromMilliseconds(animation.DurationMs);
        frames.IterationBehavior = AnimationIterationBehavior.Forever;
        visual.StartAnimation("Translation", frames);
    }

    private void Stop()
    {
        if (_running is not null)
        {
            ElementCompositionPreview.GetElementVisual(_sprite).StopAnimation("Translation");
            _running = null;
        }

        _sprite.Source = null;
        _window.Visibility = Visibility.Collapsed;
        _static.Opacity = 1;
    }
}
