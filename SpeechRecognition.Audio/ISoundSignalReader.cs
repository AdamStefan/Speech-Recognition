using System.Collections.Generic;

namespace SpeechRecognition.Audio
{
    public interface ISoundSignalReader
    {
        int SampleRate { get; set; }
        int Channels { get; set; }
        int Length { get; }

        bool Read(float[] buffer, int bufferStartIndex, int length);
        bool Read(float[] buffer, int length);

        bool Read(float[] buffer, int bufferStartIndex, int length, Dictionary<string, object> properties);
        bool Read(float[] buffer, int length, Dictionary<string, object> properties);
        void Reset();
        bool Accept(SignalVisitor visitor);
        IEnumerable<string>  SupportedProperties { get; }
    }
}
