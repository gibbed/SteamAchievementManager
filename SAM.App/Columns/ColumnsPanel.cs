using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace SAM.App.Columns;

/// <summary>
/// Arranges its children side by side according to a <see cref="ColumnLayout"/>: each child carries the key of its
/// column via <c>ColumnsPanel.Column</c>. Hidden columns are collapsed, the column
/// with width 0 takes the rest of the row. Rows and header with the same layout line up exactly.
/// </summary>
public sealed partial class ColumnsPanel : Panel
{
    public static readonly DependencyProperty LayoutProperty = DependencyProperty.Register(
        nameof(Layout), typeof(ColumnLayout), typeof(ColumnsPanel), new PropertyMetadata(null, OnLayoutChanged));

    public static readonly DependencyProperty ColumnProperty = DependencyProperty.RegisterAttached(
        "Column", typeof(string), typeof(ColumnsPanel), new PropertyMetadata(null));

    private ColumnLayout? _subscribed;

    public ColumnsPanel()
    {
        Loaded += (_, _) => Subscribe(Layout);
        Unloaded += (_, _) => Subscribe(null);
    }

    public ColumnLayout? Layout
    {
        get => (ColumnLayout?)GetValue(LayoutProperty);
        set => SetValue(LayoutProperty, value);
    }

    /// <summary>Gap between two visible columns.</summary>
    public double Spacing { get; set; } = 14;

    public static string? GetColumn(DependencyObject element) => (string?)element.GetValue(ColumnProperty);

    public static void SetColumn(DependencyObject element, string? value) => element.SetValue(ColumnProperty, value);

    protected override Size MeasureOverride(Size availableSize)
    {
        ApplyVisibility();
        var widths = Widths(availableSize.Width);
        var height = 0.0;

        foreach (var child in Children)
        {
            if (child.Visibility == Visibility.Collapsed)
            {
                continue;
            }

            var width = GetColumn(child) is { } key && widths.TryGetValue(key, out var w) ? w : 0;
            child.Measure(new Size(width, availableSize.Height));
            height = Math.Max(height, child.DesiredSize.Height);
        }

        var total = double.IsInfinity(availableSize.Width) ? widths.Values.Sum() + Gaps() : availableSize.Width;
        return new Size(total, height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var widths = Widths(finalSize.Width);
        var x = 0.0;
        var positions = new Dictionary<string, double>();

        foreach (var column in Layout?.Visible ?? [])
        {
            positions[column.Key] = x;
            x += widths[column.Key] + Spacing;
        }

        foreach (var child in Children)
        {
            if (child.Visibility != Visibility.Collapsed && GetColumn(child) is { } key && positions.TryGetValue(key, out var left))
            {
                child.Arrange(new Rect(left, 0, widths[key], finalSize.Height));
            }
            else
            {
                child.Arrange(new Rect(0, 0, 0, 0));
            }
        }

        return finalSize;
    }

    private static void OnLayoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var panel = (ColumnsPanel)d;

        if (panel.IsLoaded)
        {
            panel.Subscribe((ColumnLayout?)e.NewValue);
        }

        panel.InvalidateMeasure();
    }

    private void Subscribe(ColumnLayout? layout)
    {
        if (_subscribed is not null)
        {
            _subscribed.Changed -= OnColumnsChanged;
        }

        _subscribed = layout;

        if (layout is not null)
        {
            layout.Changed += OnColumnsChanged;
            InvalidateMeasure();
        }
    }

    private void OnColumnsChanged(object? sender, EventArgs e) => InvalidateMeasure();

    /// <summary>Only children of hidden columns are collapsed - the children do not bind their own visibility.</summary>
    private void ApplyVisibility()
    {
        foreach (var child in Children)
        {
            var visible = Layout is { } layout && GetColumn(child) is { } key && layout.IsVisible(key);
            var wanted = visible ? Visibility.Visible : Visibility.Collapsed;

            if (child.Visibility != wanted)
            {
                child.Visibility = wanted;
            }
        }
    }

    private Dictionary<string, double> Widths(double available)
    {
        var visible = Layout?.Visible.ToList() ?? [];
        var fixedWidth = visible.Where(c => !c.IsStar).Sum(c => c.Width) + Gaps();
        var star = double.IsInfinity(available) ? 200 : Math.Max(0, available - fixedWidth);
        var stars = Math.Max(1, visible.Count(c => c.IsStar));
        return visible.ToDictionary(c => c.Key, c => c.IsStar ? star / stars : c.Width);
    }

    private double Gaps() => Math.Max(0, (Layout?.Visible.Count() ?? 0) - 1) * Spacing;
}
