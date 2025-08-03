using Microsoft.Maui.Controls;
using ProVoiceLedger.Core.Models;
using ProVoiceLedger.Core.Services;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ProVoiceLedger.Pages
{
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

        public RecordingPage(IAudioCaptureService audioCaptureService, SessionDatabase sessionDb, User loggedInUser)
        {
            InitializeComponent();
            _audioCaptureService = audioCaptureService;
            _sessionDb = sessionDb;
            _uploadService = new RecordingUploadService();
            _currentUser = loggedInUser;
            BindingContext = this;
        }

        private async void OnStartRecordingClicked(object sender, EventArgs e)
        {
            if (_currentUser.IsSuspended)
            {
#pragma warning disable CA1416
                await DisplayAlert("Access Denied", "Your account is suspended. You cannot record sessions.", "OK");
#pragma warning restore CA1416
                return;
            }

#pragma warning disable CA1416
            var sessionName = SessionNameEntry?.Text?.Trim();
#pragma warning restore CA1416
            if (string.IsNullOrEmpty(sessionName))
            {
#pragma warning disable CA1416
                await DisplayAlert("Missing Name", "Please enter a session name.", "OK");
#pragma warning restore CA1416
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
#pragma warning disable CA1416
            var sessionName = SessionNameEntry?.Text?.Trim() ?? "Untitled";
#pragma warning restore CA1416
            var clipDuration = recordedClip?.Duration ?? GetMockDuration();

            bool saved = false;
            if (recordedClip != null)
            {
                // If you want to upload the actual audio, load the file as bytes here
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

        private TimeSpan GetMockDuration()
        {
            return TimeSpan.FromSeconds(10.3); // Replace with actual duration logic
        }

        private async void AnimateRecordingIndicator()
        {
            while (IsRecording)
            {
#pragma warning disable CA1416
                await RecordingIndicator.ScaleTo(1.2, 400, Easing.CubicInOut);
                await RecordingIndicator.ScaleTo(1.0, 400, Easing.CubicInOut);
#pragma warning restore CA1416
            }
#pragma warning disable CA1416
            await RecordingIndicator.ScaleTo(1.0, 200);
#pragma warning restore CA1416
        }

        public new event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void NotifyPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
