using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SAM.App.Localization;

namespace SAM.App.Columns;

/// <summary>
/// Filter of one column: values with checkboxes like in Excel. With a search text, applying keeps only what the search matches
/// and is checked; if all values are checked, the filter is removed.
/// </summary>
public sealed partial class ColumnFilterView : UserControl
{
    private readonly IReadOnlyList<FilterOption> _options;
    private List<FilterOption> _shown;

    internal ColumnFilterView(ColumnSpec column, IReadOnlyList<FilterOption> options, bool isFiltered)
    {
        _options = options;
        _shown = [.. options];
        InitializeComponent();

        SortAscendingButton.Visibility = column.Sortable ? Visibility.Visible : Visibility.Collapsed;
        SortDescendingButton.Visibility = column.Sortable ? Visibility.Visible : Visibility.Collapsed;
        ClearText.Text = Strings.FilterClear(column.Header());
        ClearButton.IsEnabled = isFiltered;
        ValuesList.ItemsSource = _shown;
        UpdateSelectAll();
    }

    /// <summary>Sorting: true = descending.</summary>
    public event Action<bool>? SortRequested;

    /// <summary>Selected values; null = no filter.</summary>
    public event Action<IReadOnlySet<string>?>? Applied;

    public event Action? Cancelled;

    private void OnSortAscendingClick(object sender, RoutedEventArgs e) => SortRequested?.Invoke(false);

    private void OnSortDescendingClick(object sender, RoutedEventArgs e) => SortRequested?.Invoke(true);

    private void OnClearClick(object sender, RoutedEventArgs e) => Applied?.Invoke(null);

    private void OnCancelClick(object sender, RoutedEventArgs e) => Cancelled?.Invoke();

    private void OnSearchChanged(object sender, TextChangedEventArgs e)
    {
        var search = SearchBox.Text.Trim();
        _shown = search.Length == 0
            ? [.. _options]
            : _options.Where(o => o.Label.Contains(search, StringComparison.CurrentCultureIgnoreCase)).ToList();

        // Like in Excel: search results start out all checked.
        if (search.Length > 0)
        {
            foreach (var option in _shown)
            {
                option.IsChecked = true;
            }
        }

        ValuesList.ItemsSource = _shown;
        UpdateSelectAll();
    }

    private void OnSelectAllClick(object sender, RoutedEventArgs e)
    {
        var check = SelectAllBox.IsChecked == true;

        foreach (var option in _shown)
        {
            option.IsChecked = check;
        }

        UpdateSelectAll();
    }

    private void OnOptionClick(object sender, RoutedEventArgs e) => UpdateSelectAll();

    private void OnOkClick(object sender, RoutedEventArgs e)
    {
        var searching = SearchBox.Text.Trim().Length > 0;
        var chosen = (searching ? _shown : _options).Where(o => o.IsChecked).Select(o => o.Value).ToHashSet();
        Applied?.Invoke(!searching && chosen.Count == _options.Count ? null : chosen);
    }

    private void UpdateSelectAll()
    {
        var checkedCount = _shown.Count(o => o.IsChecked);
        SelectAllBox.IsChecked = checkedCount == 0 ? false : checkedCount == _shown.Count ? true : null;
        OkButton.IsEnabled = _shown.Any(o => o.IsChecked);
    }
}
