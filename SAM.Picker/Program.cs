using System;
using System.IO;
using System.Windows;

namespace SAM.Picker
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Log("=== STARTUP LOG ===");
            Log("Runtime: .NET 8.0 Windows x86");
            
            try
            {
                Log("Initializing WPF Application...");
                var app = new App();
                
                Log("Loading MainWindow...");
                var mainWindow = new MainWindow();
                
                Log("Starting UI Loop...");
                app.Run(mainWindow);
                
                Log("Clean exit.");
            }
            catch (Exception ex)
            {
                string error = $"CRITICAL STARTUP ERROR:\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}";
                if (ex.InnerException != null)
                {
                    error += $"\n\nInner Exception:\n{ex.InnerException.Message}";
                }
                
                Log(error);
                
                // Try to show a message box even if UI failed
                try {
                    System.Windows.MessageBox.Show(error, "SAM Diagnostic Shield", MessageBoxButton.OK, MessageBoxImage.Error);
                } catch {
                    // Fail silently if even MessageBox fails
                }
            }
        }

        private static void Log(string message)
        {
            try
            {
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash_report.txt");
                File.AppendAllText(logPath, $"[{DateTime.Now:HH:mm:ss}] {message}\n");
            }
            catch { }
        }
    }
}
