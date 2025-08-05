using System;
using System.IO;

public static class WavEncoder
{
    public static void ConvertPcmToWav(string pcmPath, string wavPath, int sampleRate = 44100, int bitsPerSample = 16, int channels = 1)
    {
        using var pcmStream = File.OpenRead(pcmPath);
        using var wavStream = File.Create(wavPath);

        int byteRate = sampleRate * channels * (bitsPerSample / 8);
        int dataLength = (int)pcmStream.Length;
        int totalLength = 36 + dataLength;

        using var writer = new BinaryWriter(wavStream);

        // RIFF header
        writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
        writer.Write(totalLength);
        writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));

        // fmt chunk
        writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
        writer.Write(16); // PCM chunk size
        writer.Write((short)1); // Audio format = PCM
        writer.Write((short)channels);
        writer.Write(sampleRate);
        writer.Write(byteRate);
        writer.Write((short)(channels * bitsPerSample / 8)); // Block align
        writer.Write((short)bitsPerSample);

        // data chunk
        writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
        writer.Write(dataLength);

        // Copy raw PCM
        pcmStream.CopyTo(wavStream);
    }
}
