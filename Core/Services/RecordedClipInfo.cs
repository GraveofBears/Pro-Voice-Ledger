using ProVoiceLedger.Core.Models;
using ProVoiceLedger.Core.Services;
using SQLite;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace ProVoiceLedger.Core.Services;

public class RecordedClipInfo
{
    public string DeviceUsed { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; } = TimeSpan.Zero;
    public DateTime RecordedAt { get; set; } = DateTime.Now;

    // ✨ New additions
    public string? SessionName { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public Dictionary<string, string>? Metadata { get; set; }

    public RecordedClipInfo() { }
}
