using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;
using System.IO;

namespace ProVoiceLedger;

public partial class AppShell : Shell
{
    public AppShell()
    {
        try
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string logPath = Path.Combine(FileSystem.AppDataDirectory, "launchlog.txt");

            File.AppendAllText(logPath, $"{timestamp} - 🚪 Entered AppShell constructor\n");
            Debug.WriteLine($"{timestamp}: 🚪 Entered AppShell constructor");

            InitializeComponent();

            Routing.RegisterRoute("login", typeof(Pages.LoginPage));
            Routing.RegisterRoute("main", typeof(Pages.RecordingPage));
            Routing.RegisterRoute("history", typeof(Pages.SessionHistoryPage));

            File.AppendAllText(logPath, $"{timestamp} - ✅ AppShell InitializeComponent succeeded\n");
            Debug.WriteLine($"{timestamp}: ✅ AppShell InitializeComponent succeeded");
        }
        catch (Exception ex)
        {
            string fatalPath = Path.Combine(FileSystem.AppDataDirectory, "fatal.txt");
            File.WriteAllText(fatalPath, $"⛔ AppShell crash at {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n{ex}\n");

            Debug.WriteLine($"AppShell fatal error: {ex}");
            Console.WriteLine($"AppShell fatal error: {ex.Message}");
        }
    }
}
