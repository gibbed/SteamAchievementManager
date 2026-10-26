namespace SAM.App.Columns;

/// <summary>Sorting with the direction as a switch - for column headers that support both directions.</summary>
internal static class SortExtensions
{
    public static IOrderedEnumerable<T> OrderBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> key, bool descending)
        => descending ? source.OrderByDescending(key) : source.OrderBy(key);

    public static IOrderedEnumerable<T> ThenBy<T, TKey>(this IOrderedEnumerable<T> source, Func<T, TKey> key, bool descending)
        => descending ? source.ThenByDescending(key) : source.ThenBy(key);
}
