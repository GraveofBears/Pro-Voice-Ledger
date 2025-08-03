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
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // 🔧 Core services
        builder.Services.AddSingleton<UserRepository>();
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<FileStorageService>();
        builder.Services.AddSingleton<CommunicationService>();
        builder.Services.AddSingleton<PipeServerService>();

        // 🎙️ Audio capture and recording
        builder.Services.AddSingleton<IAudioCaptureService, MockAudioCaptureService>();
        builder.Services.AddSingleton<IRecordingService, RecordingService>();
        builder.Services.AddSingleton<RecordingUploadService>();

        // 🗂️ SQLite database
        string dbPath = Path.Combine(FileSystem.AppDataDirectory, "sessions.db");
        builder.Services.AddSingleton(provider => new SessionDatabase(dbPath));

        // 📄 Pages
        builder.Services.AddTransient<RecordingPage>();
        builder.Services.AddTransient<LoginPage>(); // Ensure your LoginPage exists

        // 🌿 App with dependencies
        builder.Services.AddSingleton<App>(provider =>
        {
            var db = provider.GetRequiredService<SessionDatabase>();
            var audioService = provider.GetRequiredService<IAudioCaptureService>();

            var app = new App(db, audioService);

            // 👀 Initial splash page with logo
            app.MainPage = new ContentPage
            {
                Content = new Image
                {
                    Source = "logo.png", // Make sure logo.png is in Resources/Images and marked as MauiImage
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    WidthRequest = 200,
                    HeightRequest = 200
                },
                BackgroundColor = Colors.White
            };

            // ⏳ Transition to LoginPage after a short delay
            Task.Run(async () =>
            {
                await Task.Delay(2000); // 2 seconds splash
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    app.MainPage = new LoginPage();
                });
            });

            return app;
        });

        var app = builder.Build();

        // 🚀 Background listener
        var pipeServer = app.Services.GetRequiredService<PipeServerService>();
        Task.Run(() => pipeServer.StartListenerAsync());

        return app;
    }
}
