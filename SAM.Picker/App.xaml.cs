using System.Configuration;
using System.Data;
using System.Windows;

namespace SAM.Picker;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    public App()
    {
        // Register handlers FIRST, before anything else
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        this.DispatcherUnhandledException += OnDispatcherUnhandledException;

        try {
            System.IO.File.WriteAllText("start_log.txt", "App Starting...");
            InitializeComponent();
        } catch (Exception ex) {
            System.Windows.MessageBox.Show($"Startup Error: {ex.Message}");
        }
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var ex = e.ExceptionObject as Exception;
        string msg = $"Fatal Error: {ex?.Message}\n\nStack Trace:\n{ex?.StackTrace}";
        System.IO.File.AppendAllText("start_log.txt", "\nFATAL: " + msg);
        System.Windows.MessageBox.Show(msg, "Universal Crash Handler", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
    }

    private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        string msg = $"UI Error: {e.Exception.Message}";
        System.IO.File.AppendAllText("start_log.txt", "\nUI ERROR: " + msg);
        System.Windows.MessageBox.Show(msg, "Universal Crash Handler", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        e.Handled = true;
    }
}

