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
        _httpClient = new HttpClient(); // Consider DI later if needed
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string username = UsernameEntry?.Text ?? string.Empty;
        string password = PasswordEntry?.Text ?? string.Empty;

        var request = new LoginRequest
        {
            Username = username,
            Password = password
        };

        try
        {
            string loginUrl = "https://localhost:7071/api/auth/login";
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync(loginUrl, request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (result != null && result.Success)
                {
                    await DisplayAlert("Welcome", $"Role: {result.Role}", "OK");

                    // TODO: Store token, navigate to main page, or personalize experience
                    await Shell.Current.GoToAsync("main");
                }
                else
                {
                    await DisplayAlert("Login Failed", result?.Message ?? "Invalid credentials", "Retry");
                }
            }
            else
            {
                await DisplayAlert("Server Error", $"Status code: {response.StatusCode}", "Close");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Network Error", ex.Message, "Close");
        }
    }
}
