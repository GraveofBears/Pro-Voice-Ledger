using Microsoft.Maui.Controls;
using ProVoiceLedger.Core.Models;
using ProVoiceLedger.Core.Services;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ProVoiceLedger.Pages;

public partial class RecordingPage : ContentPage, INotifyPropertyChanged
{
    private readonly IAudioCaptureService _audioCaptureService;
    private readonly SessionDatabase _sessionDb;
    private readonly RecordingUploadService _uploadService;
    private readonly User _currentUser;

    private bool _isRecording;
    public bool IsRecording
    {
        get => _isRecording;
        set
        {
            if (_isRecording != value)
            {
                _isRecording = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(IsNotRecording));
            }
        }
    }

    public bool IsNotRecording => !_isRecording;

    private string _lastSessionInfo = string.Empty;
    public string LastSessionInfo
    {
        get => _lastSessionInfo;
        set
        {
            if (_lastSessionInfo != value)
            {
                _lastSessionInfo = value;
                NotifyPropertyChanged();
            }
        }
    }

    private float[] _audioSamples = Array.Empty<float>();

    public RecordingPage(IAudioCaptureService audioCaptureService, SessionDatabase sessionDb, User loggedInUser)
    {
        InitializeComponent();
        _audioCaptureService = audioCaptureService;
        _sessionDb = sessionDb;
        _uploadService = new RecordingUploadService();
        _currentUser = loggedInUser;
        BindingContext = this;

        _audioCaptureService.OnAudioSampleCaptured += UpdateWaveform;
    }

    private void UpdateWaveform(float[] samples)
    {
        _audioSamples = samples;
        MainThread.BeginInvokeOnMainThread(() => WaveformView?.InvalidateSurface());
    }

    private void OnWaveformPaint(object sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.LightGray);

        if (_audioSamples.Length == 0) return;

        var paint = new SKPaint
        {
            Color = SKColors.Blue,
            StrokeWidth = 2,
            IsAntialias = true
        };

        int width = e.Info.Width;
        int height = e.Info.Height;
        float midY = height / 2;

        for (int i = 0; i < _audioSamples.Length; i++)
        {
            float x = i * (width / (float)_audioSamples.Length);
            float y = _audioSamples[i] * midY;
            canvas.DrawLine(x, midY - y, x, midY + y, paint);
        }
    }

    private async void OnStartRecordingClicked(object sender, EventArgs e)
    {
        if (_currentUser.IsSuspended)
        {
            await DisplayAlert("Access Denied", "Your account is suspended. You cannot record sessions.", "OK");
            return;
        }

        var sessionName = SessionNameEntry?.Text?.Trim();
        if (string.IsNullOrEmpty(sessionName))
        {
            await DisplayAlert("Missing Name", "Please enter a session name.", "OK");
            return;
        }

        await _audioCaptureService.StartRecordingAsync(sessionName);
        IsRecording = true;
        AnimateRecordingIndicator();
    }

    private async void OnStopRecordingClicked(object sender, EventArgs e)
    {
        var recordedClip = await _audioCaptureService.StopRecordingAsync();
        IsRecording = false;

        var timestamp = DateTime.UtcNow;
        var sessionName = SessionNameEntry?.Text?.Trim() ?? "Untitled";
        var clipDuration = recordedClip?.Duration ?? TimeSpan.FromSeconds(10.3);

        bool saved = false;
        if (recordedClip != null)
        {
            byte[] audioBuffer = Array.Empty<byte>();
            if (!string.IsNullOrEmpty(recordedClip.FilePath) && File.Exists(recordedClip.FilePath))
            {
                audioBuffer = await File.ReadAllBytesAsync(recordedClip.FilePath);
            }

            saved = _uploadService.SaveRecording(_currentUser, audioBuffer, sessionName);
        }

        LastSessionInfo = saved
            ? $"✅ Saved to: /Recordings/{_currentUser.Username}/\n🕒 {clipDuration.TotalSeconds:n1}s — {timestamp:g}"
            : $"❌ Upload failed. Check folder or access rights.";

        if (saved && recordedClip != null)
        {
            var session = new Session
            {
                Title = sessionName,
                StartTime = timestamp,
                FilePath = recordedClip.FilePath ?? $"Recordings/{_currentUser.Username}/{sessionName}_{timestamp:yyyyMMdd_HHmmss}.wav",
                DurationSeconds = clipDuration.TotalSeconds,
                DeviceUsed = recordedClip.DeviceUsed
            };

            await _sessionDb.SaveSessionAsync(session);
        }
    }

    private async void AnimateRecordingIndicator()
    {
        while (IsRecording)
        {
            await RecordingIndicator.ScaleTo(1.2, 400, Easing.CubicInOut);
            await RecordingIndicator.ScaleTo(1.0, 400, Easing.CubicInOut);
        }
        await RecordingIndicator.ScaleTo(1.0, 200);
    }

    public new event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void NotifyPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
