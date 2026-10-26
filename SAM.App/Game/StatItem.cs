using System.Globalization;
using SAM.App.Helpers;
using SAM.App.Localization;

namespace SAM.App.Game;

/// <summary>A statistic with its Steam value and the edited value (successor of StatInfo/IntStatInfo/FloatStatInfo).</summary>
public sealed class StatItem : ObservableObject
{
    private double _value;
    private bool _isEditable;
    private string? _error;

    private StatItem(string id, string displayName, bool isFloat, double value, bool incrementOnly, int permission, string range)
    {
        Id = id;
        DisplayName = string.IsNullOrEmpty(displayName) ? id : displayName;
        IsFloat = isFloat;
        OriginalValue = value;
        _value = value;
        IsIncrementOnly = incrementOnly;
        Permission = permission;
        RangeText = range;
    }

    internal static StatItem FromInteger(SAM.Game.Stats.IntegerStatDefinition definition, int value)
        => new(definition.Id, definition.DisplayName, false, value, definition.IncrementOnly, definition.Permission,
            Range(definition.MinValue == int.MinValue ? null : definition.MinValue, definition.MaxValue == int.MaxValue ? null : definition.MaxValue));

    internal static StatItem FromFloat(SAM.Game.Stats.FloatStatDefinition definition, float value)
        => new(definition.Id, definition.DisplayName, true, value, definition.IncrementOnly, definition.Permission,
            Range(definition.MinValue == float.MinValue ? null : definition.MinValue, definition.MaxValue == float.MaxValue ? null : definition.MaxValue));

    public string Id { get; }

    public string DisplayName { get; }

    public bool ShowId => !string.Equals(Id, DisplayName, StringComparison.Ordinal);

    public bool IsFloat { get; }

    public string TypeText => IsFloat ? Strings.TypeFloat : Strings.TypeInteger;

    public double OriginalValue { get; private set; }

    public bool IsIncrementOnly { get; }

    public int Permission { get; }

    /// <summary>Like StatFlags.Protected in the original.</summary>
    public bool IsProtected => (Permission & 2) != 0;

    public bool HasUnknownPermission => (Permission & ~2) != 0;

    public string RangeText { get; }

    public bool HasRange => RangeText.Length > 0;

    public double Value => _value;

    public int IntValue => (int)_value;

    public float FloatValue => (float)_value;

    public string ValueText => IsFloat
        ? ((float)_value).ToString("G", CultureInfo.CurrentCulture)
        : ((int)_value).ToString(CultureInfo.CurrentCulture);

    public bool IsModified => IsFloat
        ? !((float)_value).Equals((float)OriginalValue)
        : (int)_value != (int)OriginalValue;

    /// <summary>Set by the window once editing has been unlocked.</summary>
    public bool IsEditable
    {
        get => _isEditable;
        set
        {
            if (Set(ref _isEditable, value))
            {
                OnPropertyChanged(nameof(IsReadOnly));
                OnPropertyChanged(nameof(CanEditValue));
            }
        }
    }

    public bool IsReadOnly => !_isEditable || IsProtected;

    public bool CanEditValue => !IsReadOnly;

    public string? Error
    {
        get => _error;
        private set
        {
            if (Set(ref _error, value))
            {
                OnPropertyChanged(nameof(HasError));
            }
        }
    }

    public bool HasError => _error is not null;

    internal event Action<StatItem>? Changed;

    /// <summary>Apply the input. Accepts the system language's number format and a dot as decimal separator.</summary>
    public bool TryApply(string text)
    {
        text = text.Trim();

        if (text == ValueText)
        {
            Error = null;
            return true;
        }

        double parsed;

        if (IsFloat)
        {
            if (!float.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out var f) &&
                !float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out f) ||
                float.IsNaN(f) || float.IsInfinity(f))
            {
                Error = Strings.InvalidNumber;
                return false;
            }

            parsed = f;
        }
        else
        {
            if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.CurrentCulture, out var i))
            {
                Error = Strings.InvalidInteger;
                return false;
            }

            parsed = i;
        }

        // Like StatIsProtectedException in the original.
        if (IsProtected)
        {
            Error = Strings.ProtectedValue;
            return false;
        }

        Error = null;
        SetValue(parsed);
        return true;
    }

    internal void Revert()
    {
        Error = null;
        SetValue(OriginalValue);
    }

    internal void Commit()
    {
        OriginalValue = _value;
        OnPropertyChanged(nameof(IsModified));
    }

    internal void ClearError() => Error = null;

    private void SetValue(double value)
    {
        _value = value;
        OnPropertyChanged(nameof(Value));
        OnPropertyChanged(nameof(ValueText));
        OnPropertyChanged(nameof(IsModified));
        Changed?.Invoke(this);
    }

    public override string ToString() => DisplayName + ": " + ValueText;

    private static string Range(double? min, double? max) => (min, max) switch
    {
        (null, null) => "",
        ({ } a, null) => "≥ " + a.ToString("G", CultureInfo.CurrentCulture),
        (null, { } b) => "≤ " + b.ToString("G", CultureInfo.CurrentCulture),
        ({ } a, { } b) => a.ToString("G", CultureInfo.CurrentCulture) + " – " + b.ToString("G", CultureInfo.CurrentCulture),
    };
}
