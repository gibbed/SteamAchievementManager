using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using SAM.App.Helpers;
using SAM.App.Localization;

namespace SAM.App.Columns;

/// <summary>One value in a column filter (like the checkbox list in Excel).</summary>
public sealed class FilterOption : ObservableObject
{
    private bool _isChecked;

    public FilterOption(string value, string label, bool isChecked)
    {
        Value = value;
        Label = label;
        _isChecked = isChecked;
    }

    public string Value { get; }

    public string Label { get; }

    public bool IsChecked
    {
        get => _isChecked;
        set => Set(ref _isChecked, value);
    }

    public override string ToString() => Label;
}

/// <summary>The window behind a column header: current sorting, filters and the values of a column.</summary>
public interface IColumnHost
{
    /// <summary>Column the list is sorted by, and the direction.</summary>
    (string Key, bool Descending) Sort { get; }

    /// <summary>Sort by a column; without a direction, a second click on the same column reverses it.</summary>
    void SortBy(string key, bool? descending);

    bool IsFiltered(string key);

    /// <summary>All values of the column under the other filters, the currently selected ones checked.</summary>
    IReadOnlyList<FilterOption> FilterOptions(string key);

    /// <summary>Show only these values; null removes the column filter.</summary>
    void SetFilter(string key, IReadOnlySet<string>? values);
}

/// <summary>
/// Builds the header of a list: per column the name with ∧/∨ (click sorts, a second click reverses the direction)
/// and a filter button with a menu like in Excel (sort, search, check and uncheck values).
/// </summary>
public static class ColumnHeaders
{
    private const string Ascending = "\uE70E";
    private const string Descending = "\uE70D";
    private const string FilterGlyph = "\uE71C";

    public static void Build(ColumnsPanel panel, IColumnHost host)
    {
        panel.Children.Clear();

        foreach (var column in panel.Layout?.Columns ?? [])
        {
            var cell = new Grid { ColumnSpacing = 2 };
            ColumnsPanel.SetColumn(cell, column.Key);
            panel.Children.Add(cell);

            if (!column.ShowsHeader)
            {
                continue;
            }

            cell.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            cell.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            cell.Children.Add(SortButton(column, host));

            if (column.Filterable)
            {
                var filter = FilterButton(column, host);
                Grid.SetColumn(filter, 1);
                cell.Children.Add(filter);
            }
        }
    }

    private static Button SortButton(ColumnSpec column, IColumnHost host)
    {
        var active = column.Sortable && host.Sort.Key == column.Key;
        var label = new TextBlock
        {
            Text = column.Header(),
            Style = (Style)Application.Current.Resources["SecondaryCaptionStyle"],
            TextTrimming = TextTrimming.CharacterEllipsis,
            VerticalAlignment = VerticalAlignment.Center,
        };

        if (active)
        {
            label.FontWeight = Microsoft.UI.Text.FontWeights.SemiBold;
        }

        var content = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6 };
        content.Children.Add(label);

        if (column.Sortable)
        {
            // Active column: direction clearly visible; the others show a faint ∨ - sortable.
            content.Children.Add(new FontIcon
            {
                Glyph = active && !host.Sort.Descending ? Ascending : Descending,
                FontSize = 10,
                Opacity = active ? 1 : 0.35,
                VerticalAlignment = VerticalAlignment.Center,
            });
        }

        var button = new Button
        {
            Content = content,
            Padding = new Thickness(6, 3, 6, 3),
            Margin = new Thickness(-6, 0, 0, 0),
            MinHeight = 0,
            IsEnabled = column.Sortable,
            Style = (Style)Application.Current.Resources["SubtleButtonStyle"],
        };

        var state = !active ? "" : host.Sort.Descending ? ", " + Strings.SortDescending : ", " + Strings.SortAscending;
        AutomationProperties.SetName(button, column.Header() + state);
        ToolTipService.SetToolTip(button, Strings.SortByColumnTooltip);
        button.Click += (_, _) => host.SortBy(column.Key, null);
        return button;
    }

    private static Button FilterButton(ColumnSpec column, IColumnHost host)
    {
        var filtered = host.IsFiltered(column.Key);
        var icon = new FontIcon { Glyph = FilterGlyph, FontSize = 10, Opacity = filtered ? 1 : 0.35 };

        if (filtered)
        {
            icon.Foreground = (Brush)Application.Current.Resources["AccentTextFillColorPrimaryBrush"];
        }

        var button = new Button
        {
            Content = icon,
            Padding = new Thickness(5, 4, 5, 4),
            MinHeight = 0,
            Style = (Style)Application.Current.Resources["SubtleButtonStyle"],
        };

        AutomationProperties.SetName(button, Strings.ColumnFilterTooltip + ": " + column.Header());
        ToolTipService.SetToolTip(button, Strings.ColumnFilterTooltip);

        var flyout = new Flyout { Placement = FlyoutPlacementMode.BottomEdgeAlignedLeft };
        flyout.Opening += (_, _) =>
        {
            var view = new ColumnFilterView(column, host.FilterOptions(column.Key), host.IsFiltered(column.Key));
            view.SortRequested += descending =>
            {
                flyout.Hide();
                host.SortBy(column.Key, descending);
            };
            view.Applied += values =>
            {
                flyout.Hide();
                host.SetFilter(column.Key, values);
            };
            view.Cancelled += flyout.Hide;
            flyout.Content = view;
        };
        button.Flyout = flyout;
        return button;
    }
}
