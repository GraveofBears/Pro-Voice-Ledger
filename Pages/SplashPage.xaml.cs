using System.Reflection;
using Microsoft.Maui.Controls;
using ProVoiceLedger.Core.Services;

namespace ProVoiceLedger.Pages;

public partial class SplashPage : ContentPage
{
    public string AppVersion { get; set; }

    public SplashPage()
    {
        InitializeComponent();

        AppVersion = $"Version: {Assembly.GetExecutingAssembly()?.GetName().Version?.ToString() ?? "unknown"}";
        BindingContext = this;

        StartLoginTransition();
    }

    private async void StartLoginTransition()
    {
        await Task.Delay(2000); // 🕒 Display splash for 2 seconds
        Application.Current.MainPage = new LoginPage(); // 🔐 Load login screen
    }
}
