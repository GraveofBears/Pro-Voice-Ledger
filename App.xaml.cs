using Microsoft.Maui.Controls;
using ProVoiceLedger.Core.Models;
using ProVoiceLedger.Core.Services;
using ProVoiceLedger.Pages;
using System.Diagnostics;
using System.IO;

namespace ProVoiceLedger;

public partial class App : Application
{
    private readonly SessionDatabase _sessionDb;
    private readonly IAudioCaptureService _audioService;

    public App(SessionDatabase db, IAudioCaptureService audioCaptureService)
    {
        _sessionDb = db;
        _audioService = audioCaptureService;

        try
        {
            LogMessage("🪵 Begin App constructor");

            InitializeComponent();
            LogMessage("✅ InitializeComponent succeeded");

            // 🚀 Load SplashPage to show logo + version before redirecting to LoginPage
            MainPage = new SplashPage();

            LogMessage("🖼️ SplashPage assigned to MainPage");
        }
        catch (Exception ex)
        {
            var fatalPath = Path.Combine(FileSystem.AppDataDirectory, "fatal.txt");
            File.WriteAllText(fatalPath, $"⛔ Crash at {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n{ex}\n");

            Debug.WriteLine($"Fatal error: {ex}");

            MainPage = new ContentPage
            {
                Content = new Label
                {
                    Text = "Startup failed ❌ Check fatal.txt",
                    TextColor = Colors.Red,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                }
            };
        }

        LogMessage("🏁 End of App constructor");
    }

    public SessionDatabase SessionDb => _sessionDb;
    public IAudioCaptureService AudioService => _audioService;

    private void LogMessage(string message)
    {
        try
        {
            string logPath = Path.Combine(FileSystem.AppDataDirectory, "launchlog.txt");
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            File.AppendAllText(logPath, $"{timestamp} - {message}{Environment.NewLine}");

            Debug.WriteLine($"{timestamp}: {message}");
            Console.WriteLine(message);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Logging failed: {ex.Message}");
        }
    }
}
