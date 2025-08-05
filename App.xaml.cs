using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
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

            // 🚀 Set SplashPage first, then try restoring user session
            MainPage = new SplashPage();
            LogMessage("🖼️ SplashPage assigned to MainPage");

            // 💾 Attempt restore after short delay
            Task.Run(async () =>
            {
                await Task.Delay(1500); // Optional delay to show splash

                string? token = null;
                try
                {
                    token = await SecureStorage.GetAsync("auth_token");
                    LogMessage(token != null ? "🔓 Token retrieved from SecureStorage" : "🔐 No token found");
                }
                catch (Exception ex)
                {
                    LogMessage($"⚠️ Token restore failed: {ex.Message}");
                }

                // ⛳ Show appropriate page based on token
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (!string.IsNullOrEmpty(token))
                    {
                        var user = new User
                        {
                            Username = "RestoredUser",
                            Role = "User",
                            IsSuspended = false
                        };

                        MainPage = new NavigationPage(new RecordingPage(_audioService, _sessionDb, user));
                        LogMessage("📼 RecordingPage assigned (session restored)");
                    }
                    else
                    {
                        MainPage = new MainPage(_audioService, _sessionDb);
                        LogMessage("🔑 MainPage assigned (manual login required)");
                    }
                });
            });
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
