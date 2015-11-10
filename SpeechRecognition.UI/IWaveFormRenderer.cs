using SpeechRecognition.Audio;

namespace SpeechRecognition.UI
{
    public interface IWaveFormRenderer
    {
        void AddValue(SampleAggregator.SamplePoint samplePoint);
    }
}