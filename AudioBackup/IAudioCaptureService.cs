using ProVoiceLedger.Core.Models;
using ProVoiceLedger.Core.Services;
public interface IAudioCaptureService
{
    Task<bool> StartRecordingAsync(string sessionName, Dictionary<string, string>? metadata = null);
    Task<RecordedClipInfo?> StopRecordingAsync();
    bool IsRecording { get; }
}

