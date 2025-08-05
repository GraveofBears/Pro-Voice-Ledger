using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProVoiceLedger.Core.Models;

namespace ProVoiceLedger.Core.Services
{
    public interface IAudioCaptureService
    {
        /// <summary>
        /// Starts a new audio recording session.
        /// </summary>
        /// <param name="sessionName">A user-defined label for the session.</param>
        /// <param name="metadata">Optional metadata to associate with the session.</param>
        /// <returns>True if recording successfully started.</returns>
        Task<bool> StartRecordingAsync(string sessionName, Dictionary<string, string>? metadata = null);

        /// <summary>
        /// Stops the current recording and returns recorded clip info.
        /// </summary>
        /// <returns>A RecordedClipInfo object with session data, or null if no recording was active.</returns>
        Task<RecordedClipInfo?> StopRecordingAsync();

        /// <summary>
        /// Indicates whether recording is currently in progress.
        /// </summary>
        bool IsRecording { get; }

        /// <summary>
        /// Emits waveform samples during recording, as a float array.
        /// </summary>
        event Action<float[]>? OnAudioSampleCaptured;
    }
}
