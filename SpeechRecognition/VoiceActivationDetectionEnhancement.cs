using System;

namespace SpeechRecognition
{
    [Flags]
    public enum VoiceActivationDetectionEnhancement
    {
        MahalanobisDistance = 1,
        DeltaVariance = 2,
        All = MahalanobisDistance | DeltaVariance
    }
}