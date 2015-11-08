namespace SpeechRecognition.FeaturesProvider.MelFrequencySpectrum
{
    public class MellFreqProviderParameters : FeatureProviderParameters
    {
        public int NumberOfFilterBanks
        {
            get;
            set;
        }

        public int NumberOfExtractedMelItems
        {
            get;
            set;
        }

        public override int NumberOfCoeff
        {
            get { return NumberOfExtractedMelItems + 1; }
        }
    }
}
