using System.Globalization;
using Microsoft.UI.Xaml.Media;
using SAM.App.Helpers;
using SAM.App.Localization;
using SAM.App.Steam;

namespace SAM.App.Game;

/// <summary>An achievement with its Steam state and a pending change (successor of AchievementInfo + ListViewItem).</summary>
public sealed class AchievementItem : ObservableObject
{
    private bool _isAchieved;
    private ImageSource? _icon;
    private string? _iconShown;
    private int _iconVersion;

    internal AchievementItem(uint appId, int index, SAM.Game.Stats.AchievementDefinition definition, bool isAchieved, uint unlockTime)
    {
        AppId = appId;
        Index = index;
        Id = definition.Id;
        Permission = definition.Permission;
        IsHidden = definition.IsHidden;
        SetId = definition.SetId;
        IsArchived = definition.IsArchived;
        IconNormal = string.IsNullOrEmpty(definition.IconNormal) ? null : definition.IconNormal;
        IconLocked = string.IsNullOrEmpty(definition.IconLocked) ? IconNormal : definition.IconLocked;

        // As in the original: names starting with "#" are unresolved tokens - show the ID instead.
        var placeholder = definition.Name?.StartsWith('#') ?? true;
        Name = placeholder ? definition.Id : definition.Name!;
        Description = placeholder ? "" : definition.Description ?? "";

        OriginalAchieved = isAchieved;
        _isAchieved = isAchieved;
        UnlockTime = isAchieved && unlockTime > 0
            ? DateTimeOffset.FromUnixTimeSeconds(unlockTime).LocalDateTime
            : null;
    }

    public uint AppId { get; }

    /// <summary>Position in the schema (the game's order).</summary>
    public int Index { get; }

    public string Id { get; }

    public string Name { get; }

    public string Description { get; }

    public bool HasDescription => Description.Length > 0;

    public int Permission { get; }

    public bool IsHidden { get; }

    /// <summary>Achievement set according to the schema (0 = base game).</summary>
    public int SetId { get; }

    /// <summary>Archived: can no longer be unlocked and does not count towards progress.</summary>
    public bool IsArchived { get; }

    public string? IconNormal { get; }

    public string? IconLocked { get; }

    public bool OriginalAchieved { get; private set; }

    public DateTime? UnlockTime { get; }

    /// <summary>Protected achievements are set by the game server and cannot be changed.</summary>
    public bool IsProtected => (Permission & 3) != 0;

    public bool CanEdit => !IsProtected;

    public bool IsAchieved
    {
        get => _isAchieved;
        set
        {
            if (IsProtected || !Set(ref _isAchieved, value))
            {
                return;
            }

            OnPropertyChanged(nameof(IsModified));
            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(StatusGlyph));
            OnPropertyChanged(nameof(IconOpacity));

            if (_icon is not null || _iconShown is not null)
            {
                EnsureIcon();
            }

            Changed?.Invoke(this);
        }
    }

    public bool IsModified => _isAchieved != OriginalAchieved;

    public string StatusText
    {
        get
        {
            if (IsModified)
            {
                return _isAchieved ? Strings.StatusUnlocking : Strings.StatusLocking;
            }

            if (!_isAchieved)
            {
                return Strings.StatusLocked;
            }

            return UnlockTime is { } time
                ? time.ToString("g", CultureInfo.CurrentCulture)
                : Strings.StatusUnlocked;
        }
    }

    public string StatusGlyph => IsModified ? "" : _isAchieved ? "" : "";

    public double IconOpacity => _isAchieved ? 1.0 : 0.55;

    public string? ProtectedTooltip => IsProtected
        ? Strings.ProtectedAchievementTooltip
        : null;

    public ImageSource? Icon
    {
        get => _icon;
        private set => Set(ref _icon, value);
    }

    internal event Action<AchievementItem>? Changed;

    /// <summary>Load the icon for the current state (coloured or grey).</summary>
    public async void EnsureIcon()
    {
        var file = _isAchieved ? IconNormal : IconLocked;

        if (file is null || file == _iconShown)
        {
            return;
        }

        _iconShown = file;
        var version = ++_iconVersion;
        var image = await ImageCache.LoadAsync(SteamImages.AchievementIcon(AppId, file));

        if (version == _iconVersion)
        {
            Icon = image;
        }
    }

    /// <summary>After saving: the current state is the new Steam state.</summary>
    internal void Commit()
    {
        OriginalAchieved = _isAchieved;
        OnPropertyChanged(nameof(IsModified));
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(StatusGlyph));
    }

    internal void Revert() => IsAchieved = OriginalAchieved;

    public override string ToString() => Name + ", " + StatusText;
}
