using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using ProVoiceLedger.AudioBackup;
using ProVoiceLedger.Core.Models;
using ProVoiceLedger.Core.Services;
using ProVoiceLedger.Pages;

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

        // 🔧 Register core services
        builder.Services.AddSingleton<UserRepository>(); // Required by AuthService
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<FileStorageService>();
        builder.Services.AddSingleton<CommunicationService>();
        builder.Services.AddSingleton<PipeServerService>();

        // 🎙️ Audio and Recording
        builder.Services.AddSingleton<IAudioCaptureService, MockAudioCaptureService>();
        builder.Services.AddSingleton<IRecordingService, RecordingService>();
        builder.Services.AddSingleton<RecordingUploadService>();

        // 🗂️ SQLite-backed session database
        string dbPath = Path.Combine(FileSystem.AppDataDirectory, "sessions.db");
        builder.Services.AddSingleton(provider => new SessionDatabase(dbPath));

        // 📄 Register pages
        builder.Services.AddTransient<RecordingPage>();

        // 🌿 Root app setup
        builder.Services.AddSingleton<App>(provider =>
        {
            var db = provider.GetRequiredService<SessionDatabase>();
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

        var app = builder.Build();

        // 🚀 Launch background listener
        var pipeServer = app.Services.GetRequiredService<PipeServerService>();
        Task.Run(() => pipeServer.StartListenerAsync());

        return app;
    }
}
