using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using ProVoiceLedger.Core.Models;
using ProVoiceLedger.Core.Services;

namespace ProVoiceLedger;

public partial class MainPage : ContentPage
{
    private int _count = 0;
    private readonly List<string> _iconSequence = new()
    {
        "play.png",
        "pause.png",
        "record.png",
        "rewind.png",
        "finish.png"
    };

    private readonly IAudioCaptureService _audioCaptureService;
    private readonly SessionDatabase _sessionDb;
    private Image _iconOverlay = null!;

    public MainPage(IAudioCaptureService audioCaptureService, SessionDatabase sessionDb)
    {
        InitializeComponent();
        _audioCaptureService = audioCaptureService;
        _sessionDb = sessionDb;
        SetupIconOverlay();
    }

    private async void OnViewHistoryClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("history");
    }

    private async void OnStartRecordingClicked(object sender, EventArgs e)
    {
        if (!_audioCaptureService.IsRecording)
        {
            ShowRecordingVisuals();

            var sessionName = SessionNameEntry?.Text ?? $"Session {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
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
                    Title = SessionNameEntry?.Text ?? $"Session {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
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

    private void SetupIconOverlay()
    {
        _iconOverlay = new Image
        {
            Source = "play.png",
            WidthRequest = 64,
            HeightRequest = 64,
            Opacity = 0.8,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 16, 0, 0),
            InputTransparent = true
        };

        MainLayout.Children.Add(_iconOverlay);
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

    private void OnCounterClicked(object sender, EventArgs e)
    {
        _count++;

        string text = _count switch
        {
            1 => "Clicked once",
            <= 5 => $"Clicked {_count} times",
            6 => "The ritual intensifies...",
            7 => "Prepare for ascension.",
            _ => $"Clicked {_count} times"
        };

        CounterBtn.Text = text;
        SemanticScreenReader.Announce(text);

        int iconIndex = Math.Min(_count - 1, _iconSequence.Count - 1);
        string nextIcon = _iconSequence[iconIndex];

        AnimateIconChange(nextIcon);
        if (nextIcon == "record.png" || _count >= 3)
        {
            RitualPulse();
        }
    }

    private async void AnimateIconChange(string iconName)
    {
        await _iconOverlay.FadeTo(0.0, 200, Easing.CubicIn);
        _iconOverlay.Source = ImageSource.FromFile(iconName);
        await _iconOverlay.FadeTo(0.8, 300, Easing.CubicOut);

        await _iconOverlay.ScaleTo(1.15, 100, Easing.SinInOut);
        await _iconOverlay.ScaleTo(1.00, 100, Easing.SinInOut);
    }

    private async void RitualPulse()
    {
        await CounterBtn.ScaleTo(1.1, 100, Easing.CubicIn);
        await CounterBtn.ScaleTo(1.0, 100, Easing.CubicOut);

        await _iconOverlay.FadeTo(1, 400, Easing.CubicInOut);
        await _iconOverlay.FadeTo(0.6, 200);
    }
}
