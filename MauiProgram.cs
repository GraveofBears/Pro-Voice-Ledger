using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using System.IO;
using ProVoiceLedger.Core;
using ProVoiceLedger.Core.Models;
using ProVoiceLedger.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using ProVoiceLedger.AudioBackup;

namespace ProVoiceLedger;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp(services => services.GetRequiredService<App>())
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });


        // 🧪 Register development/testing audio services
        builder.Services.AddSingleton<IAudioCaptureService, MockAudioCaptureService>();
        builder.Services.AddSingleton<IRecordingService, RecordingService>();

        // 📁 Register SQLite session database
        string dbPath = Path.Combine(FileSystem.AppDataDirectory, "sessions.db");
        builder.Services.AddSingleton<SessionDatabase>(provider => new SessionDatabase(dbPath));

        // 🌱 Inject App with a runtime test page to isolate shell issues
        builder.Services.AddSingleton<App>(provider =>
        {
            var db = provider.GetRequiredService<SessionDatabase>();

            // ✅ Replace AppShell with minimal content
            var app = new App(db);
            app.MainPage = new ContentPage
            {
                Content = new Label
                {
                    Text = "Test page loaded successfully ✅",
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                }
            };
            return app;
        });

        return builder.Build();
    }
}
