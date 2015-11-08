using SpeechRecognition.Audio;
using SpeechRecognition.FeaturesProvider.LPC;
using SpeechRecognition.FeaturesProvider.MelFrequencySpectrum;

namespace SpeechRecognition.FeaturesProvider
{
    public static class FeaturesProviderFactory
    {
        public static IFeatureProvider GetProvider(FeatureProviderParameters parameters, SoundSignalReader signal)
        {
            var lpcParam = parameters as LpcCepstrumParameters;
            if (lpcParam!= null)
            {
                return new LpcCepstrumProvider(lpcParam.LpcOrder, lpcParam.NumberOfCoeff);
            }

            var melParam = parameters as MellFreqProviderParameters;
            if (melParam != null)
            {
                //var mfcc = new MFCC(NumberOfFilterBanks, samplingRate: signal.SampleRate, lowerfrequency: 50);
                return new MellFrequencyCepstrumProvider(melParam.NumberOfFilterBanks, samplingRate: signal.SampleRate, lowerfrequency: 50, numberOfCoef: melParam.NumberOfExtractedMelItems + 1);
            }

            return null;
        }
    }

   
}
