using System;
using System.Numerics;

namespace SpeechRecognition.FeaturesProvider.MelFrequencySpectrum
{
    public class Mfcc
    {
        #region Fields

        private readonly int _numberOfFilterBanks;
        private readonly DiscreteCosinusTransform _dct;
        private readonly int _numberOfFftCoeff;
        private int[] _filterBankCoefficients;
        private static readonly Func<double, double> MelScale = frequency => 2595.0 * Math.Log10(1.0 + frequency / 700.0);
        private static readonly Func<double, double> MelScaleInverse = scale => 700 * (Math.Pow(10, scale / 2595.0) - 1);
        private readonly int _lowerfrequency;
        private readonly int _higherfrequency;
        private readonly int _samplingRate;

        #endregion

        #region Instance

        public Mfcc(int numberOfFilterBanks = 46, int numberOfFftCoeff = 512, int lowerfrequency = 0, int samplingRate = 16000, int higherfrequency = 3400)
        {
            _numberOfFilterBanks = numberOfFilterBanks;

            _dct = new DiscreteCosinusTransform(_numberOfFilterBanks);
            _numberOfFftCoeff = numberOfFftCoeff;
            _lowerfrequency = lowerfrequency;
            _samplingRate = samplingRate;
            _higherfrequency = higherfrequency;
        }

        #endregion

        #region Methods

        public double[] Extract(float[] frame, out bool isEmpty)
        {
            var windowedSample = HammingWindowDef.ApplyHammingWindow(frame);

            //var result = FourierTransform.FFTAmplitudeSquared(windowedSample, (int)_numberOfFFTCoeff);
            FourierTransform ft = new FourierTransform();
            ft.computeFFT(windowedSample, _numberOfFftCoeff);
            var frameEnergy = 0.0;
            var result = ft.GetMagnitudeSquared(out frameEnergy);

            if (_filterBankCoefficients == null)
            {
                _filterBankCoefficients = ComputeMelFilterBank(result.Length, _lowerfrequency, _higherfrequency, _samplingRate, _numberOfFilterBanks + 2);
            }

            #region compute PowerSpectralEstimates
            //var mean = 0.0;
            //for (var index = 0; index < _filterBankCoefficients[_filterBankCoefficients.Length - 1]; index++)
            //{
            //    mean += result[index];
            //}
            //mean = mean / _filterBankCoefficients[_filterBankCoefficients.Length - 1];

                     
            for (int index = 0; index < result.Length; index++)
            {                
                result[index] = result[index] / result.Length;                
            }
            //variance = variance / _filterBankCoefficients[_filterBankCoefficients.Length - 1];

            #endregion

            var cepstra = ApplyFilterbankFilter(result, _filterBankCoefficients);            
            var ret = _dct.Apply(cepstra);
            ret[0] = Utils.Log(frameEnergy);
            isEmpty = false;
            return ret;
        }

        public double[] PowerSpectralEstimates(Complex[] inputSignal)
        {
            var ret = new Double[inputSignal.Length];
            for (int index = 0; index < inputSignal.Length; index++)
            {
                var magnitude = inputSignal[index].Magnitude;
                ret[index] = magnitude * magnitude / inputSignal.Length;
            }

            return ret;
        }

        public static double[] Preemphasis(double[] signal, double filterRatio = 0.95)
        {
            var ret = new double[signal.Length];
            ret[0] = signal[0];
            for (int index = 1; index < signal.Length; index++)
            {
                ret[index] = signal[index] - filterRatio * signal[index - 1];
            }

            return ret;
        }

        public static void Lifter(double[] signal, int level = 22)
        {
            if (level > 22)
            {
                var length = signal.Length;
                for (int index = 0; index < length; index++)
                {
                    var lift = 1.0 + ((level / 2.0) * Math.Sin(Math.PI * index / level));
                    signal[index] = signal[index] * lift;
                }
            }
        }

        public static int[] ComputeMelFilterBank(int size, int lowerFrequency = 0, int higherFrequency = 8000, int sampligRate = 16000, int numberOfCoefficients = 48)
        {
            var minValue = MelScale(lowerFrequency);
            var maxValue = MelScale(higherFrequency);

            var deltaScale = (maxValue - minValue) / (numberOfCoefficients - 1);

            double[] hValue = new double[numberOfCoefficients];
            hValue[0] = lowerFrequency;
            hValue[numberOfCoefficients - 1] = higherFrequency;

            int[] fvalues = new int[numberOfCoefficients];

            for (int index = 0; index < numberOfCoefficients; index++)
            {
                if (index > 0 && index < numberOfCoefficients - 1)
                {
                    hValue[index] = MelScaleInverse(minValue + (deltaScale * index));
                }
                fvalues[index] = (int)Math.Floor((size + 1) * hValue[index] / sampligRate);
            }

            return fvalues;
        }        

        public static double[] ApplyFilterbankFilter(double[] powerSpectralEstimates, int[] filterBankCoeficient)
        {
            var ret = new double[filterBankCoeficient.Length - 2];

            Func<int, int, double> filterBankFunction = (filterBankfunctionIndex, frequency) =>
            {
                if (frequency < filterBankCoeficient[filterBankfunctionIndex - 1])
                {
                    return 0;
                }
                if (frequency <= filterBankCoeficient[filterBankfunctionIndex])
                {
                    var nominator = (double)(frequency - filterBankCoeficient[filterBankfunctionIndex - 1]);
                    var denominator = (double)(filterBankCoeficient[filterBankfunctionIndex] - filterBankCoeficient[filterBankfunctionIndex - 1]);

                    return nominator / denominator;
                }
                if (frequency <= filterBankCoeficient[filterBankfunctionIndex + 1])
                {
                    var nominator = (double)(filterBankCoeficient[filterBankfunctionIndex + 1] - frequency);
                    var denominator = (double)(filterBankCoeficient[filterBankfunctionIndex + 1] - filterBankCoeficient[filterBankfunctionIndex]);


                    return nominator / denominator;
                }
                return 0;

            };

            for (int fbIndex = 0; fbIndex < filterBankCoeficient.Length - 2; fbIndex++)
            {
                ret[fbIndex] = 0;
                for (int index = 0; index < powerSpectralEstimates.Length; index++)
                {
                    var fbank = filterBankFunction(fbIndex + 1, index);
                    ret[fbIndex] += powerSpectralEstimates[index] * fbank * fbank;
                }

                var value = Utils.Log(ret[fbIndex]);

                ret[fbIndex] = value;
            }

            return ret;

        }
        
        #endregion
    }
}
