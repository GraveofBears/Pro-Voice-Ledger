using System;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Maui.Controls;
using ProVoiceLedger.Core.Models;

namespace ProVoiceLedger.Pages;

public partial class LoginPage : ContentPage
{
    private readonly HttpClient _httpClient;

    public LoginPage()
    {
        InitializeComponent();
        _httpClient = new HttpClient(); // Optional: Use dependency injection for future flexibility
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        // 🧠 Safely get text input, trim spaces
        string username = UsernameEntry?.Text?.Trim() ?? string.Empty;
        string password = PasswordEntry?.Text ?? string.Empty;

        // 🚫 Validate fields
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Missing Input", "Please enter both a username and password.", "OK");
            return;
        }

        var loginRequest = new LoginRequest
        {
            Username = username,
            Password = password
        };

        try
        {
            string loginUrl = "https://localhost:7290/api/auth/login";

            // 📡 Send login request to server
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync(loginUrl, loginRequest);

            if (response.IsSuccessStatusCode)
            {
                var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

                if (loginResponse?.Success == true)
                {
                    await DisplayAlert("Welcome", $"Role: {loginResponse.Role}", "OK");

                    // 📝 TODO: Store token securely if returned
                    // Navigation to main page or user dashboard
                    await Shell.Current.GoToAsync("main");
                }
                else
                {
                    await DisplayAlert("Login Failed", loginResponse?.Message ?? "Invalid credentials", "Retry");
                }
            }
            else
            {
                await DisplayAlert("Server Error", $"Status code: {response.StatusCode}", "Close");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Network Error", $"Unable to reach server: {ex.Message}", "Close");
        }
    }
}
