using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SAM.App.Helpers;

/// <summary>Minimal INotifyPropertyChanged base for list items.</summary>
public abstract class ObservableObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(name);
        return true;
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

/// <summary>Windows that can show short notifications.</summary>
public interface INotifier
{
    void Notify(string message, Microsoft.UI.Xaml.Controls.InfoBarSeverity severity);
}
