using SAM.App.Helpers;
using SAM.App.Localization;

namespace SAM.App.Columns;

/// <summary>One column of a list view.</summary>
/// <param name="Key">Fixed name under which order, sorting and filters are saved.</param>
/// <param name="Header">Column header in the current language.</param>
/// <param name="Width">Fixed width in pixels; 0 = takes the rest of the row.</param>
public sealed record ColumnSpec(string Key, Func<string> Header, double Width, bool CanHide = true, bool Sortable = true, bool Filterable = true)
{
    public bool IsStar => Width <= 0;

    /// <summary>Too narrow for a header (e.g. the medal) - the name only appears in the column chooser.</summary>
    public bool ShowsHeader => IsStar || Width >= 60;
}

/// <summary>
/// Order and visibility of the columns of a view, saved in settings.json
/// (<c>&lt;Name&gt;ColumnOrder</c>, <c>&lt;Name&gt;HiddenColumns</c>). All rows and the header follow it.
/// </summary>
public sealed class ColumnLayout
{
    private readonly string _name;
    private readonly IReadOnlyList<ColumnSpec> _defaults;
    private readonly HashSet<string> _hiddenByDefault;

    internal ColumnLayout(string name, IReadOnlyList<ColumnSpec> columns, IEnumerable<string>? hiddenByDefault = null)
    {
        _name = name;
        _defaults = columns;
        _hiddenByDefault = [.. hiddenByDefault ?? []];
        Load();
    }

    public event EventHandler? Changed;

    /// <summary>All columns in the chosen order, including hidden ones.</summary>
    public IReadOnlyList<ColumnSpec> Columns { get; private set; } = [];

    public IReadOnlySet<string> Hidden { get; private set; } = new HashSet<string>();

    public IEnumerable<ColumnSpec> Visible => Columns.Where(c => !Hidden.Contains(c.Key));

    public ColumnSpec? Find(string key) => Columns.FirstOrDefault(c => c.Key == key);

    public bool IsVisible(string key) => !Hidden.Contains(key) && Find(key) is not null;

    /// <summary>Apply a new order and visibility; unknown keys are dropped, missing ones go to the end.</summary>
    public void Apply(IEnumerable<string> order, IEnumerable<string> hidden)
    {
        Set(order, hidden);
        AppSettings.Write(_name + "ColumnOrder", new System.Text.Json.Nodes.JsonArray([.. Columns.Select(c => System.Text.Json.Nodes.JsonValue.Create(c.Key))]));
        AppSettings.Write(_name + "HiddenColumns", new System.Text.Json.Nodes.JsonArray([.. Hidden.Order().Select(k => System.Text.Json.Nodes.JsonValue.Create(k))]));
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void Reset() => Apply(_defaults.Select(c => c.Key), _hiddenByDefault);

    private void Load()
    {
        var order = AppSettings.ReadStrings(_name + "ColumnOrder");
        var hidden = AppSettings.ReadStrings(_name + "HiddenColumns");
        Set(order.Count > 0 ? order : _defaults.Select(c => c.Key), order.Count > 0 ? hidden : _hiddenByDefault);
    }

    private void Set(IEnumerable<string> order, IEnumerable<string> hidden)
    {
        var byKey = _defaults.ToDictionary(c => c.Key);
        var columns = order.Distinct().Where(byKey.ContainsKey).Select(k => byKey[k]).ToList();

        // Columns a saved order does not know yet are placed after their predecessor from the defaults.
        for (var i = 0; i < _defaults.Count; i++)
        {
            if (!columns.Contains(_defaults[i]))
            {
                columns.Insert(i == 0 ? 0 : columns.IndexOf(_defaults[i - 1]) + 1, _defaults[i]);
            }
        }
        Columns = columns;
        Hidden = hidden.Where(k => byKey.TryGetValue(k, out var c) && c.CanHide).ToHashSet();
    }
}

/// <summary>The columns of both lists: library (list view) and achievement lists.</summary>
public static class ColumnLayouts
{
    public const string AppId = "AppId";
    public const string Name = "Name";
    public const string Visibility = "Visibility";
    public const string Medal = "Medal";
    public const string Achievements = "Achievements";
    public const string Sets = "Sets";
    public const string Category = "Category";
    public const string FirstUnlock = "FirstUnlock";
    public const string LastUnlock = "LastUnlock";
    public const string Progress = "Progress";

    /// <summary>Library list view - App ID before the name.</summary>
    public static ColumnLayout Library { get; } = new("library",
    [
        new(AppId, () => Strings.SortAppId, 100),
        new(Name, () => Strings.SortName, 0, CanHide: false),
        new(Visibility, () => Strings.ColumnVisibility, 110),
        new(Medal, () => Strings.ColumnMedal, 20, Sortable: false, Filterable: false),
        new(Achievements, () => Strings.Achievements, 170),
        new(Sets, () => Strings.SetsTitle, 150),
        new(Category, () => Strings.ColumnCategory, 130),
    ]);

    /// <summary>Achievement lists - App ID before icon and name; visibility and sets are badges after the name.</summary>
    public static ColumnLayout Lists { get; } = new("lists",
    [
        new(AppId, () => Strings.SortAppId, 80),
        new(Name, () => Strings.ColumnGame, 0, CanHide: false),
        new(FirstUnlock, () => Strings.ColumnFirstUnlock, 115),
        new(LastUnlock, () => Strings.ColumnLastUnlock, 115),
        new(Medal, () => Strings.ColumnMedal, 20, Sortable: false, Filterable: false),
        new(Progress, () => Strings.ColumnProgress, 170),
    ]);
}
