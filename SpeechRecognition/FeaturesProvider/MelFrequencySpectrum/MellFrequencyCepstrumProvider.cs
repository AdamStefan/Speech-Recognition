using System.Linq;

namespace SpeechRecognition.FeaturesProvider.MelFrequencySpectrum
{
    public class MellFrequencyCepstrumProvider : IFeatureProvider
    {
        #region Fields

        private readonly Mfcc _mfcc;
        private readonly int? _numberOfCoeff;

        #endregion

        #region Instance

        public MellFrequencyCepstrumProvider(int numberOfFilterBanks = 46, int numberOfFftCoeff = 512, int lowerfrequency = 0, int samplingRate = 16000, int? numberOfCoef = null)
        {
            _numberOfCoeff = numberOfCoef;
            _mfcc = new Mfcc(numberOfFilterBanks, numberOfFftCoeff, lowerfrequency, samplingRate);
        }

        #endregion

        #region IFeature Provider

        public double[] Extract(float[] frame, out bool isEmpty)
        {
            var ret = _mfcc.Extract(frame, out  isEmpty);
            if (_numberOfCoeff.HasValue)
            {
                ret = ret.Take(_numberOfCoeff.Value).ToArray();
            }

            return ret;
        }

        public bool ComputeDelta
        {
            get { return true; }
        }

        #endregion
    }
}
