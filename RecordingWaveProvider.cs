using NAudio.Wave;

namespace MornyMorse
{
    // Custom WaveProvider to record and pass audio
    public class RecordingWaveProvider : IWaveProvider
    {
        private readonly IWaveProvider sourceProvider;
        private readonly WaveFileWriter writer;

        public RecordingWaveProvider(IWaveProvider sourceProvider, WaveFileWriter writer)
        {
            this.sourceProvider = sourceProvider;
            this.writer = writer;
        }

        public WaveFormat WaveFormat => sourceProvider.WaveFormat;

        public int Read(byte[] buffer, int offset, int count)
        {
            // Read from the source provider
            int bytesRead = sourceProvider.Read(buffer, offset, count);

            // Write to the WaveFileWriter
            if (bytesRead > 0)
            {
                writer.Write(buffer, offset, bytesRead);
            }

            return bytesRead;
        }
    }
}
