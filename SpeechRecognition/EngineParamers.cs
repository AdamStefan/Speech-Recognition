using SpeechRecognition.FeaturesProvider;
using SpeechRecognition.FeaturesProvider.LPC;
using SpeechRecognition.FeaturesProvider.MelFrequencySpectrum;

namespace SpeechRecognition
{
    public struct EngineParameters
    {
        //public const double FrameSizeMiliseconds = 23.22;
        public double FrameSizeMiliseconds;
        public double StepSizeMiliseconds;
        //public int NumberOfFilterBanks;
        //public int NumberOfExtractedMelItems;

        public FeatureProviderParameters ProviderParameters;

        public static EngineParameters Default = new EngineParameters
        {
            FrameSizeMiliseconds = 25,
            StepSizeMiliseconds = 12,
            //NumberOfFilterBanks = 24,
            //NumberOfFilterBanks = 36,
            //NumberOfExtractedMelItems = 12,

            ProviderParameters = new MellFreqProviderParameters
            {
                NumberOfExtractedMelItems = 12,
                NumberOfFilterBanks = 24
                //NumberOfFilterBanks = 36,
            }

            //ProviderParameters = new LpcCepstrumParameters
            //{
            //    LpcOrder = 12,
            //    MOrder = 80
            //    //NumberOfExtractedMelItems = 12,
            //    //NumberOfFilterBanks = 24
            //    //NumberOfFilterBanks = 36,
            //}
        };
    }
}
