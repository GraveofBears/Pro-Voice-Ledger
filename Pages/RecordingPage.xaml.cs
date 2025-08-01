using Microsoft.Maui.Controls;
using ProVoiceLedger.Core.Services;
using ProVoiceLedger.Core.Models;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.IO;
using System.Threading.Tasks;

namespace ProVoiceLedger.Pages
{
    public partial class RecordingPage : ContentPage, INotifyPropertyChanged
    {
        private readonly IRecordingService _recordingService;
        private readonly SessionDatabase _sessionDb;

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

        public RecordingPage(IRecordingService recordingService, SessionDatabase sessionDb)
        {
            InitializeComponent();
            _recordingService = recordingService;
            _sessionDb = sessionDb;
            BindingContext = this;
        }

        private void OnStartRecordingClicked(object sender, EventArgs e)
        {
            var sessionName = SessionNameEntry?.Text?.Trim();
            if (string.IsNullOrEmpty(sessionName))
            {
                DisplayAlert("Missing Name", "Please enter a session name.", "OK");
                return;
            }

            _recordingService.StartRecording();
            IsRecording = true;

            AnimateRecordingIndicator();
        }

        private async void OnStopRecordingClicked(object sender, EventArgs e)
        {
            _recordingService.StopRecording();
            IsRecording = false;

            var timestamp = DateTime.Now;
            var clipPath = "MockPath/AudioClip.wav"; // Replace with actual output path
            var clipDuration = GetMockDuration();    // Replace with real method

            LastSessionInfo = $"📁 {clipPath}\n" +
                              $"🕒 Duration: {clipDuration.TotalSeconds:n1}s\n" +
                              $"🗓️ Timestamp: {timestamp:g}";

            var sessionName = SessionNameEntry?.Text?.Trim() ?? "Untitled Session";

            var session = new Session
            {
                Title = sessionName,
                StartTime = timestamp,
                FilePath = clipPath,
                DurationSeconds = clipDuration.TotalSeconds
            };

            await _sessionDb.SaveSessionAsync(session);
        }

        // Stub method – replace with real .wav duration logic
        private TimeSpan GetMockDuration()
        {
            return TimeSpan.FromSeconds(10.3);
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
}
