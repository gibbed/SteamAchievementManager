using System.Globalization;
using System.Text;

namespace SAM.App.Lists;

/// <summary>A section for a Steam guide: heading and text in Steam's formatting (BBCode).</summary>
/// <param name="Count">Number of games for list sections - for the overview of untouched games.</param>
public sealed record GuideSection(string Title, string Body, int Count = 0)
{
    public string LengthText => Localization.Strings.ExportSectionInfo(Body.Length.ToString("N0", CultureInfo.CurrentCulture));

    public override string ToString() => Title;
}

/// <summary>
/// Builds the guide text in the style of "Steam Achievement Statistics": VG = visible games, PFL = Profile Features
/// Limited, UG = untouched games. Formatted with Steam's markup like the original - overviews as [table] with
/// [h1]/[h2], games as a numbered list [olist][*]. Sections that are too long are split alphabetically ("A - H", "Sa - St"),
/// so that none exceeds the character limit of a guide section including markup. Headings stay in English.
/// </summary>
internal static class GuideExport
{
    public const int DefaultLimit = 8000;

    private const string ListStart = "[olist]\n";
    private const string ListEnd = "\n[/olist]";
    private const string Item = "[*]";

    /// <summary>Numbers like in the original: "11,733".</summary>
    private static readonly CultureInfo Numbers = CultureInfo.GetCultureInfo("en-US");

    public static List<GuideSection> Build(IReadOnlyList<ListCategory> categories, bool includeUntouched, int limit)
    {
        limit = Math.Max(500, limit);
        var sections = new List<GuideSection> { Overview(categories, includeUntouched), Legend() };

        foreach (var (scope, prefix) in new[] { (ListScope.Visible, "VG: "), (ListScope.Limited, "PFL: ") })
        {
            foreach (var (kind, label) in new[]
                     {
                         (ListKind.Done, "Completed Games"),
                         (ListKind.NotAnymore, "Not 100 % Anymore"),
                         (ListKind.Started, "Unfinished Games"),
                         (ListKind.CantFinish, "Games I can't finish"),
                     })
            {
                var games = Find(categories, scope, kind).Games.Select(g => g.GuideLine).ToList();

                // Empty lists only appear in the overview; an empty section would be useless in the guide.
                if (games.Count > 0)
                {
                    sections.AddRange(Split(prefix + label, prefix + label, games, limit, letters: 1));
                }
            }
        }

        // Games without reported visibility are only in 1.0.x - listed separately in the guide.
        var unknown = Find(categories, ListScope.All, ListKind.All).Games.Where(g => g.Vetted is null).Select(g => g.GuideLine).ToList();

        if (unknown.Count > 0)
        {
            sections.AddRange(Split("Unknown visibility: Games", "Unknown visibility: Games", unknown, limit, letters: 1));
        }

        if (includeUntouched)
        {
            var untouched = Find(categories, ListScope.All, ListKind.Untouched).Games;
            var groups = new List<GuideSection>();

            foreach (var group in untouched.GroupBy(g => InitialGroup(g.Name)).OrderBy(g => GroupOrder(g.Key)).ThenBy(g => g.Key, StringComparer.Ordinal))
            {
                groups.AddRange(Split("UG: " + group.Key, "UG:", group.Select(g => g.Name).ToList(), limit, letters: 2));
            }

            // Like in the original: first a table of all UG sections, then the sections themselves.
            var rows = groups.Select(g => Row(g.Title[..g.Title.LastIndexOf(" (", StringComparison.Ordinal)], g.Count));
            sections.Add(new GuideSection("Untouched Games: Quick Overview", Table("Section", "Number of Games", rows)));
            sections.AddRange(groups);
        }

        return sections;
    }

    private static ListCategory Find(IReadOnlyList<ListCategory> categories, ListScope scope, ListKind kind)
        => categories.First(c => c.Scope == scope && c.Kind == kind);

    /// <summary>"Own Stats" like in the original: table with [h2] rows for the groups and the categories below them.</summary>
    private static GuideSection Overview(IReadOnlyList<ListCategory> categories, bool includeUntouched)
    {
        int Count(ListScope scope, ListKind kind) => Find(categories, scope, kind).Games.Count;
        var rows = new List<string>();

        foreach (var (scope, title) in new[] { (ListScope.All, "Games with Achievements"), (ListScope.Visible, "Visible Games"), (ListScope.Limited, "Profile Features Limited") })
        {
            rows.Add(GroupRow(title, Count(scope, ListKind.All)));
            rows.Add(Row("Completed Games", Count(scope, ListKind.Done)));
            rows.Add(Row("Not 100 % Anymore", Count(scope, ListKind.NotAnymore)));
            rows.Add(Row("Unfinished Games", Count(scope, ListKind.Started)));
            rows.Add(Row("Games I can't finish", Count(scope, ListKind.CantFinish)));

            var unknown = scope == ListScope.All ? Find(categories, ListScope.All, ListKind.All).Games.Count(g => g.Vetted is null) : 0;

            if (unknown > 0)
            {
                rows.Add(Row("Unknown visibility", unknown));
            }
        }

        var sets = new[] { ListKind.SetsCompleted, ListKind.SetsInProgress, ListKind.SetsNotStarted }.Sum(k => Count(ListScope.All, k));
        rows.Add(GroupRow("Games with Achievement Sets", sets));
        rows.Add(Row("All Sets Completed", Count(ListScope.All, ListKind.SetsCompleted)));
        rows.Add(Row("Sets In Progress", Count(ListScope.All, ListKind.SetsInProgress)));
        rows.Add(Row("Sets Not Started", Count(ListScope.All, ListKind.SetsNotStarted)));
        rows.Add(Row("Base Game Sets Completed", Count(ListScope.All, ListKind.SetsBase)));
        rows.Add(Row("DLC Sets Completed", Count(ListScope.All, ListKind.SetsDlc)));

        if (includeUntouched)
        {
            rows.Add(GroupRow("Untouched Games", Count(ListScope.All, ListKind.Untouched)));
            rows.Add(Row("Visible Games", Count(ListScope.Visible, ListKind.Untouched)));
            rows.Add(Row("Profile Features Limited", Count(ListScope.Limited, ListKind.Untouched)));
        }

        var body = Heading("Own Stats") + "\n" + Table("Category", "Number of Games", rows)
                   + "\n[hr][/hr]\nDate: " + DateTime.Now.ToString("MMMM yyyy", CultureInfo.InvariantCulture);
        return new GuideSection("Quick Overview", body);
    }

    /// <summary>"Note for the listing of my Games": what VG, PFL and UG mean.</summary>
    private static GuideSection Legend()
    {
        var rows = new[]
        {
            "[tr][td][b]V[/b]isible [b]G[/b]ames (👀✔)[/td][td][b]VG[/b][/td][/tr]",
            "[tr][td][b]P[/b]rofile [b]F[/b]eatures [b]L[/b]imited (👀✖)[/td][td][b]PFL[/b][/td][/tr]",
            "[tr][td][b]U[/b]ntouched [b]G[/b]ames[/td][td][b]UG[/b][/td][/tr]",
        };
        return new GuideSection("Note for the listing of my Games", Table("Full Description", "Short Description", rows));
    }

    private static string Heading(string text) => "[h1][u][b]" + text + "[/b][/u][/h1]";

    private static string Table(string first, string second, IEnumerable<string> rows)
        => "[table]\n[tr][th][b][h1][u]" + first + "[/u][/h1][/b][/th][th][b][h1][u]" + second + "[/u][/h1][/b][/th][/tr]\n"
           + string.Join("\n", rows) + "\n[/table]";

    private static string GroupRow(string title, int count) => "[tr][td][h2]" + title + "[/h2][/td][td][h2]" + Number(count) + "[/h2][/td][/tr]";

    private static string Row(string title, int count) => "[tr][td]" + title + "[/td][td]" + Number(count) + "[/td][/tr]";

    private static string Number(int count) => count.ToString("N0", Numbers);

    /// <summary>Games as a numbered list: [olist], one [*] and one line per game.</summary>
    private static string OrderedList(List<string> lines) => ListStart + string.Join("\n", lines.Select(l => Item + l)) + ListEnd;

    /// <summary>If everything fits into one section, the heading stays "Label (n)"; otherwise "SplitLabel A - H (n)", for UG "UG: Sa - St (n)".</summary>
    private static IEnumerable<GuideSection> Split(string label, string splitLabel, List<string> lines, int limit, int letters)
    {
        if (Length(lines) <= limit)
        {
            yield return new GuideSection(label + " (" + lines.Count.ToString(CultureInfo.InvariantCulture) + ")", OrderedList(lines), lines.Count);
            yield break;
        }

        var chunk = new List<string>();
        var length = ListStart.Length + ListEnd.Length;

        foreach (var line in lines)
        {
            var added = Item.Length + line.Length + (chunk.Count == 0 ? 0 : 1);

            if (chunk.Count > 0 && length + added > limit)
            {
                yield return Chunk(splitLabel, chunk, letters);
                chunk = [];
                added = Item.Length + line.Length;
                length = ListStart.Length + ListEnd.Length;
            }

            chunk.Add(line);
            length += added;
        }

        if (chunk.Count > 0)
        {
            yield return Chunk(splitLabel, chunk, letters);
        }
    }

    private static GuideSection Chunk(string label, List<string> chunk, int letters)
    {
        var first = RangeKey(chunk[0], letters);
        var last = RangeKey(chunk[^1], letters);
        var range = first == last ? first : first + " - " + last;
        return new GuideSection(label + " " + range + " (" + chunk.Count.ToString(CultureInfo.InvariantCulture) + ")", OrderedList(chunk), chunk.Count);
    }

    /// <summary>Length of the finished section including [olist] and [*].</summary>
    private static int Length(List<string> lines)
        => ListStart.Length + ListEnd.Length + lines.Sum(l => Item.Length + l.Length) + Math.Max(0, lines.Count - 1);

    /// <summary>Initial letters for range labels, e.g. "Sa" for "Satisfactory".</summary>
    private static string RangeKey(string name, int letters)
    {
        var chars = SortKey(name).TakeWhile(char.IsLetterOrDigit).Take(letters).ToArray();

        if (chars.Length == 0)
        {
            return "#";
        }

        return char.ToUpperInvariant(chars[0]) + new string(chars.Skip(1).Select(char.ToLowerInvariant).ToArray());
    }

    /// <summary>Group for untouched games as in the model guide: "0 - 9", "A" … "Z", "Chinese Symbols" etc.</summary>
    public static string InitialGroup(string name)
    {
        var first = SortKey(name).FirstOrDefault();

        if (first == default || !char.IsLetter(first))
        {
            return "0 - 9";
        }

        var upper = char.ToUpperInvariant(first);

        return first switch
        {
            _ when upper is >= 'A' and <= 'Z' => upper.ToString(),
            _ when first is >= 'Ͱ' and <= 'Ͽ' => "Greek Symbols",
            _ when first is >= 'Ѐ' and <= 'ӿ' => "Cyrillic Symbols",
            _ when first is >= '぀' and <= 'ヿ' => "Japanese Symbols",
            _ when first is >= '가' and <= '힯' or >= 'ᄀ' and <= 'ᇿ' => "Korean Symbols",
            _ when first is >= '一' and <= '鿿' or >= '㐀' and <= '䶿' => "Chinese Symbols",
            _ => "Other Symbols",
        };
    }

    private static int GroupOrder(string key) => key switch
    {
        "0 - 9" => 0,
        { Length: 1 } => 1,
        _ => 2,
    };

    /// <summary>
    /// Sort key: without accents and punctuation, whitespace collapsed - "[TDA00] Muv-Luv" sorts under T,
    /// "★ Fallalypse ★" under F and "S.W.I.N.E." like "SWINE". This keeps the range labels of split sections in line with the order.
    /// </summary>
    public static string SortKey(string name)
    {
        var builder = new StringBuilder(name.Length);

        foreach (var c in Strip(name))
        {
            if (char.IsLetterOrDigit(c))
            {
                builder.Append(c);
            }
            else if (char.IsWhiteSpace(c) && builder.Length > 0 && builder[^1] != ' ')
            {
                builder.Append(' ');
            }
        }

        var key = builder.ToString().TrimEnd();
        return key.Length == 0 ? name : key;
    }

    /// <summary>Remove accents so "Élite" ends up under E.</summary>
    private static string Strip(string text)
    {
        var decomposed = text.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);

        foreach (var c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(c);
            }
        }

        return builder.ToString();
    }
}
