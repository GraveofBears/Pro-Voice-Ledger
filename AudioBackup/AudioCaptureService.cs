using Microsoft.VisualBasic;
using NAudio.Wave;
using ProVoiceLedger.Core.Models;
using ProVoiceLedger.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ProVoiceLedger.AudioBackup
{
    public class AudioCaptureService : IAudioCaptureService
    {
        private bool _isRecording;
        private DateTime _startTime;
        private string? _sessionName;
        private Dictionary<string, string>? _metadata;
        private WaveInEvent? _waveIn;
        private WaveFileWriter? _writer;
        private string? _currentFilePath;

        public bool IsRecording => _isRecording;

        public event Action<float[]>? OnAudioSampleCaptured;

        public Task<bool> StartRecordingAsync(string sessionName, Dictionary<string, string>? metadata = null)
        {
            if (_isRecording)
            {
                Console.WriteLine("⚠️ Recording already in progress.");
                return Task.FromResult(false);
            }

            _isRecording = true;
            _startTime = DateTime.Now;
            _sessionName = sessionName;
            _metadata = metadata;

            string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "VoiceRecordings");
            Directory.CreateDirectory(directory);

            _currentFilePath = Path.Combine(directory, $"{sessionName}_{_startTime:yyyyMMdd_HHmmss}.wav");

            var format = new WaveFormat(8000, 16, 1); // 8kHz, 16-bit, mono
            _waveIn = new WaveInEvent
            {
                WaveFormat = format,
                BufferMilliseconds = 100
            };

            _writer = new WaveFileWriter(_currentFilePath, format);

            _waveIn.DataAvailable += (s, e) =>
            {
                _writer?.Write(e.Buffer, 0, e.BytesRecorded);

                if (OnAudioSampleCaptured != null)
                {
                    float[] samples = ConvertToSamples(e.Buffer, e.BytesRecorded, format);
                    OnAudioSampleCaptured.Invoke(samples);
                }
            };

            _waveIn.RecordingStopped += (s, e) =>
            {
                _writer?.Dispose();
                _waveIn?.Dispose();
            };

            _waveIn.StartRecording();

            Console.WriteLine($"🎙️ Recording started: {_currentFilePath}");
            return Task.FromResult(true);
        }

        public Task<RecordedClipInfo?> StopRecordingAsync()
        {
            if (!_isRecording)
            {
                Console.WriteLine("⚠️ Stop called with no active recording.");
                return Task.FromResult<RecordedClipInfo?>(null);
            }

            _isRecording = false;

            _waveIn?.StopRecording();

            var duration = DateTime.Now - _startTime;
            var timestamp = DateTime.Now;

            Console.WriteLine($"🛑 Recording stopped: {_currentFilePath} ({duration})");

            var clip = new RecordedClipInfo
            {
                FilePath = _currentFilePath ?? "",
                Duration = duration,
                SessionName = _sessionName ?? "",
                Timestamp = timestamp,
                Metadata = _metadata,
                RecordedAt = timestamp
            };

            return Task.FromResult<RecordedClipInfo?>(clip);
        }

        private float[] ConvertToSamples(byte[] buffer, int bytesRecorded, WaveFormat format)
        {
            int sampleCount = bytesRecorded / 2; // 16-bit = 2 bytes per sample
            float[] samples = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                short sample = BitConverter.ToInt16(buffer, i * 2);
                samples[i] = sample / 32768f;
            }
            return samples;
        }
    }
}
