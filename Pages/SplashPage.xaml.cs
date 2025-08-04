using System.Reflection;
using Microsoft.Maui.Controls;

namespace ProVoiceLedger.Pages;

public partial class SplashPage : ContentPage
{
    public string AppVersion { get; set; }

    public SplashPage()
    {
        InitializeComponent();

        AppVersion = $"Version: {Assembly.GetExecutingAssembly()?.GetName().Version?.ToString() ?? "unknown"}";
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // ⬇️ Fade in
        await SplashLogo.FadeTo(1, 1000, Easing.CubicIn);
        await Task.Delay(600); // Let logo settle

        // 🧭 Animate move-up and scale-down
        var targetY = -200; // Roughly matches LoginPage logo position
        await Task.WhenAll(
            SplashLogo.ScaleTo(0.5, 1000, Easing.SinInOut),    // Scale to roughly 150px size
            SplashLogo.TranslateTo(0, targetY, 1000, Easing.SinInOut)
        );

        //await Task.Delay(200); // Optional pause

        // 🚀 Transition to login page
        Application.Current.MainPage = new LoginPage();
    }
}
