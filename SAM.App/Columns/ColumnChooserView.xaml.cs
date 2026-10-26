using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SAM.App.Helpers;
using SAM.App.Localization;

namespace SAM.App.Columns;

/// <summary>One row of the column chooser.</summary>
public sealed class ColumnChoice : ObservableObject
{
    private bool _isVisible;

    internal ColumnChoice(ColumnSpec column, bool isVisible)
    {
        Key = column.Key;
        Header = column.Header();
        CanHide = column.CanHide;
        _isVisible = isVisible;
    }

    public string Key { get; }

    public string Header { get; }

    public bool CanHide { get; }

    public bool IsVisible
    {
        get => _isVisible;
        set => Set(ref _isVisible, value);
    }

    public string MoveUpLabel => Strings.MoveUp + ": " + Header;

    public string MoveDownLabel => Strings.MoveDown + ": " + Header;

    public override string ToString() => Header;
}

/// <summary>Show, hide and reorder columns (arrows or drag); every change applies immediately and is saved.</summary>
public sealed partial class ColumnChooserView : UserControl
{
    private readonly ColumnLayout _layout;
    private readonly ObservableCollection<ColumnChoice> _choices = [];
    private bool _updating;

    internal ColumnChooserView(ColumnLayout layout)
    {
        _layout = layout;
        InitializeComponent();
        Fill();
        _choices.CollectionChanged += OnChoicesChanged;
        ColumnsList.ItemsSource = _choices;
    }

    private void Fill()
    {
        _updating = true;

        foreach (var choice in _choices)
        {
            choice.PropertyChanged -= OnChoiceChanged;
        }

        _choices.Clear();

        foreach (var column in _layout.Columns)
        {
            var choice = new ColumnChoice(column, !_layout.Hidden.Contains(column.Key));
            choice.PropertyChanged += OnChoiceChanged;
            _choices.Add(choice);
        }

        _updating = false;
    }

    private void OnChoiceChanged(object? sender, PropertyChangedEventArgs e) => Apply();

    /// <summary>Dragging in the list removes the row and inserts it again - only then is the order complete.</summary>
    private void OnChoicesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action != NotifyCollectionChangedAction.Remove)
        {
            Apply();
        }
    }

    private void Apply()
    {
        if (!_updating && _choices.Count == _layout.Columns.Count)
        {
            _layout.Apply(_choices.Select(c => c.Key), _choices.Where(c => !c.IsVisible).Select(c => c.Key));
        }
    }

    private void OnMoveUpClick(object sender, RoutedEventArgs e) => Move(sender, -1);

    private void OnMoveDownClick(object sender, RoutedEventArgs e) => Move(sender, 1);

    private void Move(object sender, int step)
    {
        var index = _choices.ToList().FindIndex(c => c.Key == (sender as FrameworkElement)?.Tag as string);
        var target = index + step;

        if (index >= 0 && target >= 0 && target < _choices.Count)
        {
            _choices.Move(index, target);
        }
    }

    private void OnResetClick(object sender, RoutedEventArgs e)
    {
        _layout.Reset();
        Fill();
    }
}
