using System;

namespace SpeechRecognition
{
    [Serializable]
    public class IdentificationProperties
    {
        public ClassType Class { get; set; }
        public string Label { get; set; }
        public double MeanFeaturesLength { get; set; }
    }

    public enum ClassType
    {
        Word,
        Phoneme
    }
}
