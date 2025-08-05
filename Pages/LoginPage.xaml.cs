using Microsoft.Maui.Controls;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using ProVoiceLedger.Core.Models;
using ProVoiceLedger.Core.Services;
using ProVoiceLedger.Pages;

namespace ProVoiceLedger.Pages
{
    public partial class LoginPage : ContentPage
    {
        private readonly LoginService _loginService;

        public LoginPage()
        {
            InitializeComponent();

            // 🔧 HttpClient setup
            var httpClient = new HttpClient();
            _loginService = new LoginService(httpClient);
        }

        private async void OnLoginButtonClicked(object sender, EventArgs e)
        {
            // ✅ These match named controls in the XAML
            string username = UsernameEntry?.Text?.Trim() ?? string.Empty;
            string password = PasswordEntry?.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Missing Info", "Please enter both username and password.", "OK");
                return;
            }

            var request = new LoginRequest
            {
                Username = username,
                Password = password
            };

            try
            {
                var result = await _loginService.AttemptLoginAsync(request);
                if (result?.Success == true)
                {
                    var user = new User
                    {
                        Username = username,
                        Role = result.Role ?? "User", // 🔧 Avoid null assignment
                        IsSuspended = result.Role == "Suspended"
                    };

                    var recordingPage = new RecordingPage(App.AudioService, App.SessionDb, user);
                    await Navigation.PushAsync(recordingPage);
                }
                else
                {
                    await DisplayAlert("Login Failed", result?.Message ?? "Invalid credentials", "Try Again");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Login error: {ex.Message}", "Close");
            }
        }
    }
}
