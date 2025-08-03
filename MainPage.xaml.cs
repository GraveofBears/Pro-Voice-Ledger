using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ProVoiceLedger.Core.Models;
using ProVoiceLedger.Core.Services;
using ProVoiceLedger.Pages;

namespace ProVoiceLedger;

public partial class MainPage : ContentPage
{
    private readonly IAudioCaptureService _audioCaptureService;
    private readonly SessionDatabase _sessionDb;
    private readonly HttpClient _httpClient;

    private int _count = 0;
    private readonly List<string> _iconSequence = new()
    {
        "play.png", "pause.png", "record.png", "rewind.png", "finish.png"
    };

    public MainPage(IAudioCaptureService audioCaptureService, SessionDatabase sessionDb)
    {
        InitializeComponent();
        _audioCaptureService = audioCaptureService;
        _sessionDb = sessionDb;
        _httpClient = new HttpClient();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string username = UsernameEntry?.Text ?? string.Empty;
        string password = PasswordEntry?.Text ?? string.Empty;

        var loginUrl = "https://localhost:7071/api/auth/login";
        var request = new LoginRequest
        {
            Username = username,
            Password = password
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync(loginUrl, request);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (result?.Success == true)
                {
                    var user = new User
                    {
                        Username = username,
                        Role = result.Role,
                        IsSuspended = result.Role == "Suspended", // Optional logic
                        //Token = result.Token
                    };

                    var recordingPage = new RecordingPage(_audioCaptureService, _sessionDb, user);
                    Application.Current.MainPage = new NavigationPage(recordingPage);
                }
                else
                {
                    await DisplayAlert("Login Failed", result?.Message ?? "Invalid credentials", "Try Again");
                }
            }
            else
            {
                await DisplayAlert("Server Error", $"Code: {response.StatusCode}", "Close");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Network Error", ex.Message, "Close");
        }
    }

    private async void OnStartRecordingClicked(object sender, EventArgs e)
    {
        string sessionName = SessionNameEntry?.Text?.Trim();
        if (string.IsNullOrWhiteSpace(sessionName))
        {
            sessionName = $"Demo {DateTime.Now:yyyyMMdd_HHmmss}";
        }

        if (!_audioCaptureService.IsRecording)
        {
            ShowRecordingVisuals();

            var metadata = new Dictionary<string, string>
        {
            { "TriggeredBy", "MainPage" },
            { "Device", "MockMic" }
        };

            bool started = await _audioCaptureService.StartRecordingAsync(sessionName, metadata);
            if (started)
            {
                CounterBtn.Text = "Recording started…";
                SemanticScreenReader.Announce("Recording started");
                RitualPulse();
            }
            else
            {
                CounterBtn.Text = "Recording failed to start.";
                HideRecordingVisuals();
            }
        }
        else
        {
            var recordedClip = await _audioCaptureService.StopRecordingAsync();
            if (recordedClip != null)
            {
                var session = new Session
                {
                    Title = sessionName,
                    FilePath = recordedClip.FilePath,
                    StartTime = recordedClip.Timestamp,
                    Duration = recordedClip.Duration,
                    DeviceUsed = recordedClip.DeviceUsed
                };

                await _sessionDb.SaveSessionAsync(session);
                AnimateSessionSaved();
            }

            HideRecordingVisuals();
            CounterBtn.Text = "Recording stopped.";
        }
    }

    private void ShowRecordingVisuals()
    {
        RecordButton.IsEnabled = false;
        ShimmerOverlay.IsVisible = true;
    }

    private void HideRecordingVisuals()
    {
        RecordButton.IsEnabled = true;
        ShimmerOverlay.IsVisible = false;
    }

    private void AnimateSessionSaved()
    {
        ConfirmSaveOverlay.IsVisible = true;
        Task.Delay(1500).ContinueWith(_ =>
            MainThread.BeginInvokeOnMainThread(() =>
                ConfirmSaveOverlay.IsVisible = false));
    }

    private async void RitualPulse()
    {
        await CounterBtn.ScaleTo(1.1, 100, Easing.CubicIn);
        await CounterBtn.ScaleTo(1.0, 100, Easing.CubicOut);

        if (_iconOverlay != null)
        {
            await _iconOverlay.FadeTo(1, 400, Easing.CubicInOut);
            await _iconOverlay.FadeTo(0.6, 200);
        }

        int iconIndex = Math.Min(_count++, _iconSequence.Count - 1);
        string nextIcon = _iconSequence[iconIndex];
        await AnimateIconChange(nextIcon);
    }

    private async Task AnimateIconChange(string iconName)
    {
        if (_iconOverlay == null) return;

        await _iconOverlay.FadeTo(0.0, 200, Easing.CubicIn);
        _iconOverlay.Source = ImageSource.FromFile(iconName);
        await _iconOverlay.FadeTo(0.8, 300, Easing.CubicOut);

        await _iconOverlay.ScaleTo(1.15, 100, Easing.SinInOut);
        await _iconOverlay.ScaleTo(1.00, 100, Easing.SinInOut);
    }
}
