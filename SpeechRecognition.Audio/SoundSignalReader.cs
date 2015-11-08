namespace SpeechRecognition.Audio
{
    public abstract class SoundSignalReader
    {
        public int SampleRate { get; set; }
        public int Channels { get; set; }
        public int Length { get; protected set; }
        public abstract bool Read(float[] buffer, int bufferStartIndex, int length);
        public abstract bool Read(float[] buffer, int length);
        public abstract void Reset();
    }
}
