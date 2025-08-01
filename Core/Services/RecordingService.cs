using ProVoiceLedger.Core.Models;
using ProVoiceLedger.Core.Services;
using SQLite;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace ProVoiceLedger.Core.Services
{
    public interface IRecordingService
    {
        void StartRecording();
        void StopRecording();
        bool IsRecording { get; }
    }

    public class RecordingService : IRecordingService
    {
        public bool IsRecording { get; private set; } = false;

        public void StartRecording()
        {
            if (IsRecording)
                return;

            IsRecording = true;

            // TODO: Initialize mic, start stream, log timestamp
            Console.WriteLine("🎤 Recording started");
        }

        public void StopRecording()
        {
            if (!IsRecording)
                return;

            IsRecording = false;

            // TODO: Stop stream, finalize file, tag timestamp
            Console.WriteLine("🛑 Recording stopped");
        }
    }
}
