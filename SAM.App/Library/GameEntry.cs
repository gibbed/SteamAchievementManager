using System.Globalization;
using Microsoft.UI.Xaml.Media;
using SAM.App.Helpers;
using SAM.App.Localization;
using Windows.UI;

namespace SAM.App.Library;

/// <summary>A game in the library (successor of GameInfo from SAM.Picker).</summary>
public sealed class GameEntry : ObservableObject
{
    private string _name;
    private ImageSource? _image;
    private bool _imageRequested;
    private int _imageVersion;
    private MedalKind _medal;
    private Lists.SetSummary? _sets;
    private Lists.SetBadges? _setBadges;
    private string? _badgeTooltip;
    private int _unlocked;
    private int _total;
    private bool? _vetted;
    private ImageSource? _icon;
    private bool _iconRequested;

    public GameEntry(uint id, string type, string? name, string? imageUrl)
    {
        Id = id;
        Type = type;
        _name = name ?? FallbackName(id);
        ImageUrl = imageUrl;
    }

    public uint Id { get; }

    public string Type { get; }

    /// <summary>
    /// Large medal for 100 %, otherwise the one for completed achievement sets; games in progress and
    /// not started games without such a medal get the grey "Unfinished".
    /// </summary>
    public MedalKind TileMedalKind => ShownMedal;

    public bool HasMedal => ShownMedal != MedalKind.None;

    /// <summary>"Sets 1/3" for games with achievement sets.</summary>
    public string SetsText => _sets?.SetsText ?? "";

    public bool HasSets => _sets is not null;

    /// <summary>Tile: achievements in short as "21/57" next to "Sets 1/3".</summary>
    public string AchievementsText => _total > 0 ? Count(_unlocked) + "/" + Count(_total) : "";

    public bool HasAchievementCount => _total > 0;

    public string AchievementsTooltip => Strings.ProgressAchievements(Count(_unlocked), Count(_total));

    // ---------------------------------------------------------------- Tile: what is shown (TileOptions)

    public bool TileShowsCaption => TileOptions.HasCaption;

    public bool TileShowsName => TileOptions.Shows(TileOptions.Name);

    public bool TileShowsAppId => TileOptions.Shows(TileOptions.AppId);

    public bool TileShowsAchievements => TileOptions.Shows(TileOptions.Achievements) && HasAchievementCount;

    public bool TileShowsSets => TileOptions.Shows(TileOptions.Sets) && HasSets;

    public bool TileShowsMedal => TileOptions.Shows(TileOptions.Medal) && HasMedal;

    public bool TileShowsVisibility => TileOptions.Shows(TileOptions.Visibility);

    /// <summary>Redraw after a change in the tile options.</summary>
    internal void RefreshTile()
    {
        foreach (var name in new[] { nameof(TileShowsCaption), nameof(TileShowsName), nameof(TileShowsAppId), nameof(TileShowsAchievements), nameof(TileShowsSets), nameof(TileShowsMedal), nameof(TileShowsVisibility) })
        {
            OnPropertyChanged(name);
        }
    }

    // ---------------------------------------------------------------- List view

    public string AppIdText => Id.ToString(CultureInfo.InvariantCulture);

    /// <summary>Medal column: large blue for 100 %, otherwise the grey one.</summary>
    public MedalKind GameMedalKind => Progress == Lists.ProgressState.Completed ? MedalKind.Game : MedalKind.Unfinished;

    public string GameMedalTooltip => Progress == Lists.ProgressState.Completed ? Strings.CategoryDoneDesc : CategoryText;

    public string ProgressText => _total > 0 ? Count(_unlocked) + " / " + Count(_total) : "—";

    public string PercentText => _total > 0 ? ProgressRatio.ToString("P0", CultureInfo.CurrentCulture) : "";

    public string SetsCountText => _sets is null ? "—" : Count(_sets.CompletedSets) + " / " + Count(_sets.SetCount);

    public string SetsPercentText => _sets is null ? "" : _sets.SetsRatio.ToString("P0", CultureInfo.CurrentCulture);

    public double SetsRatio => _sets?.SetsRatio ?? 0;

    /// <summary>Completed sets like on Steam: "1" + blue medal, "2" + purple double medal.</summary>
    public Lists.SetBadges? SetBadges => _setBadges;

    public string? SetsTooltip => _sets?.Tooltip;

    public string CategoryText => GameGroup.TitleFor(Progress);

    /// <summary>Profile visibility from Steam's achievement cache, as in the lists; unknown as long as Steam reports nothing.</summary>
    public string VisibilityText => _vetted switch
    {
        true => "👀✔",
        false => "👀✖",
        null => "👀?",
    };

    public string VisibilityTooltip => _vetted switch
    {
        true => Strings.VisibilityVisibleTooltip,
        false => Strings.VisibilityLimitedTooltip,
        null => Strings.VisibilityUnknownTooltip,
    };

    internal int VisibilityRank => _vetted switch { true => 0, false => 1, null => 2 };

    internal int TotalAchievements => _total;

    internal int SetCount => _sets?.SetCount ?? 0;

    /// <summary>Small game icon for the list view, loaded as soon as the row becomes visible.</summary>
    public ImageSource? Icon
    {
        get => _icon;
        private set => Set(ref _icon, value);
    }

    public bool HasBadge => HasMedal || HasSets;

    public string? BadgeTooltip => _badgeTooltip ?? (ShownMedal == MedalKind.Unfinished
        ? GameGroup.TitleFor(Progress) + (_total > 0 ? " · " + Strings.ProgressAchievements(Count(_unlocked), Count(_total)) : "")
        : null);

    private MedalKind ShownMedal => _medal != MedalKind.None ? _medal
        : Progress == Lists.ProgressState.Completed ? MedalKind.None
        : MedalKind.Unfinished;

    /// <summary>Section in the library: Completed / In Progress / Not Started.</summary>
    internal Lists.ProgressState Progress { get; private set; }

    /// <summary>Share of unlocked achievements (0 to 1) - for sorting by progress.</summary>
    internal double ProgressRatio { get; private set; }

    /// <summary>Set while loading the library from Steam's achievement cache and stats files.</summary>
    internal void SetCompletion(CompletionInfo info)
    {
        Progress = info.State;
        ProgressRatio = info.Ratio;
        _unlocked = info.Unlocked;
        _total = info.Total;
        _medal = info.Medal;
        _sets = info.Sets;
        _setBadges = info.Sets is null ? null : new Lists.SetBadges(info.Sets);
        _vetted = info.Vetted;
        _badgeTooltip = info.Sets?.Tooltip ?? (info.Medal == MedalKind.Game ? Strings.CategoryDoneDesc : null);

        foreach (var name in new[]
                 {
                     nameof(TileMedalKind), nameof(HasMedal), nameof(SetsText), nameof(HasSets), nameof(HasBadge), nameof(BadgeTooltip),
                     nameof(AchievementsText), nameof(HasAchievementCount), nameof(AchievementsTooltip), nameof(GameMedalKind),
                     nameof(GameMedalTooltip), nameof(ProgressText), nameof(PercentText), nameof(SetsCountText), nameof(SetsPercentText),
                     nameof(SetsRatio), nameof(SetBadges), nameof(SetsTooltip), nameof(CategoryText), nameof(VisibilityText),
                     nameof(VisibilityTooltip), nameof(TileShowsAchievements), nameof(TileShowsSets), nameof(TileShowsMedal),
                 })
        {
            OnPropertyChanged(name);
        }
    }

    public string Name
    {
        get => _name;
        set
        {
            if (Set(ref _name, value ?? FallbackName(Id)))
            {
                OnPropertyChanged(nameof(Initial));
            }
        }
    }

    public string? ImageUrl { get; set; }

    public ImageSource? Image
    {
        get => _image;
        private set => Set(ref _image, value);
    }

    public string TypeLabel => Type switch
    {
        "demo" => Strings.TypeDemo,
        "mod" => Strings.TypeMod,
        "junk" => Strings.TypeOther,
        _ => Strings.TypeGame,
    };

    /// <summary>Demo, mod or other - shown under the name in the list view.</summary>
    public bool IsSpecialType => Type != "normal";

    public string Subtitle => Type == "normal"
        ? Strings.AppIdValue(Id.ToString(CultureInfo.InvariantCulture))
        : Strings.AppIdValue(Id.ToString(CultureInfo.InvariantCulture)) + " · " + TypeLabel;

    public string Initial => Name.Length > 0 ? Name[..1].ToUpperInvariant() : "?";

    /// <summary>Placeholder gradient while (or if) there is no capsule image - colour derived from the app ID.</summary>
    public Brush PlaceholderBrush
    {
        get
        {
            var hue = Id * 47 % 360;
            return new LinearGradientBrush
            {
                StartPoint = new Windows.Foundation.Point(0, 0),
                EndPoint = new Windows.Foundation.Point(1, 1),
                GradientStops =
                {
                    new GradientStop { Color = FromHsl(hue, 0.45, 0.42), Offset = 0 },
                    new GradientStop { Color = FromHsl((hue + 40) % 360, 0.50, 0.28), Offset = 1 },
                },
            };
        }
    }

    /// <summary>Load the capsule image once the tile becomes visible.</summary>
    public async void EnsureImage()
    {
        if (_imageRequested)
        {
            return;
        }

        _imageRequested = true;
        var version = ++_imageVersion;
        var image = await ImageCache.LoadAsync(ImageUrl);

        if (version == _imageVersion)
        {
            Image = image;
        }
    }

    /// <summary>Load the icon for the list view; <paramref name="resolve"/> returns the file from Steam's cache or the CDN address.</summary>
    public async void EnsureIcon(Func<uint, (string? Path, string? Url)> resolve)
    {
        if (_iconRequested)
        {
            return;
        }

        _iconRequested = true;
        var (path, url) = resolve(Id);
        Icon = await ImageCache.LoadIconAsync(path, url);
    }

    /// <summary>Release the image when the tile is recycled out of the visible area - saves memory with large libraries.</summary>
    public void ReleaseImage()
    {
        _imageRequested = false;
        _imageVersion++;
        Image = null;
    }

    public override string ToString() => Name;

    private static string FallbackName(uint id) => Strings.AppFallbackName(id.ToString(CultureInfo.InvariantCulture));

    private static string Count(int value) => value.ToString("N0", CultureInfo.CurrentCulture);

    private static Color FromHsl(double h, double s, double l)
    {
        var c = (1 - Math.Abs(2 * l - 1)) * s;
        var x = c * (1 - Math.Abs(h / 60 % 2 - 1));
        var m = l - c / 2;
        var (r, g, b) = h switch
        {
            < 60 => (c, x, 0d),
            < 120 => (x, c, 0d),
            < 180 => (0d, c, x),
            < 240 => (0d, x, c),
            < 300 => (x, 0d, c),
            _ => (c, 0d, x),
        };
        return Color.FromArgb(255, (byte)((r + m) * 255), (byte)((g + m) * 255), (byte)((b + m) * 255));
    }
}
