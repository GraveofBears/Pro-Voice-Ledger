using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProVoiceLedger.Core.Models;
using ProVoiceLedger.Core.Services;

namespace ProVoiceLedger.AudioBackup
{
    public class MockAudioCaptureService : IAudioCaptureService
    {
        private bool _isRecording;
        private DateTime _startTime;
        private string? _sessionName;
        private Dictionary<string, string>? _metadata;

        public bool IsRecording => _isRecording;

        public Task<bool> StartRecordingAsync(string sessionName, Dictionary<string, string>? metadata = null)
        {
            _isRecording = true;
            _startTime = DateTime.Now;
            _sessionName = sessionName;
            _metadata = metadata;

            Console.WriteLine($"🎙️ Mock recording started: {_sessionName}");

            return Task.FromResult(true);
        }

        public Task<RecordedClipInfo?> StopRecordingAsync()
        {
            if (!_isRecording)
                return Task.FromResult<RecordedClipInfo?>(null);

            _isRecording = false;

            var duration = DateTime.Now - _startTime;
            var timestamp = DateTime.Now;

            var clip = new RecordedClipInfo
            {
                FilePath = $"mock://audio/{_sessionName}_{timestamp:yyyyMMdd_HHmmss}.mp3",
                Duration = duration,
                SessionName = _sessionName ?? string.Empty,
                Timestamp = timestamp,
                Metadata = _metadata,
                RecordedAt = timestamp
            };

            Console.WriteLine($"🛑 Mock recording stopped: {_sessionName} ({duration})");

            return Task.FromResult<RecordedClipInfo?>(clip);
        }
    }
}
