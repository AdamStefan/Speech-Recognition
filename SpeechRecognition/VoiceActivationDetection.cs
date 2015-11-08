using System;
using SpeechRecognition.Audio;
using System.Collections.Generic;

namespace SpeechRecognition
{
    public class VoiceActivationDetection
    {

        #region Fields

        private readonly SoundSignalReader _signal;
        private readonly int _frameSize;
        private const int ScaleFactor = 1000;
        private double _thresholdsActivation;
        private double _standardDeviation;
        private double _mean;
        private readonly int _emptyFrames;
        private readonly double alphaCoeff =0.2;
        private VoiceActivationDetectionEnhancement _enhancements;       
        private FrameInfo? _lastFrameInfo;
        private bool _lastFrameWasVoice;

        

        #endregion

        #region Instance

        public VoiceActivationDetection(SoundSignalReader signal, int frameSize, int emptyFrames = 3, VoiceActivationDetectionEnhancement enhancements = VoiceActivationDetectionEnhancement.All)
        {
            _signal = signal;
            _frameSize = frameSize;
            _emptyFrames = emptyFrames;
            _enhancements = enhancements;
            Init();
        }

        #endregion


        #region Methods
      

        private FrameInfo ComputeThreshholds(float[] buffer)
        {            
            var energy = 0.0;
            var zeroCrossingRate = 0;
            var samplesWithSounds = 0;


            for (int iterator = 0; iterator < buffer.Length; iterator++)
            {
                energy += buffer[iterator] * buffer[iterator];
                if (iterator > 0)
                {
                    var temp = Math.Sign(buffer[iterator]) - Math.Sign(buffer[iterator - 1]);
                    zeroCrossingRate += Math.Abs(temp) / 2;
                }

                if (_enhancements.HasFlag(VoiceActivationDetectionEnhancement.MahalanobisDistance)
                    && _standardDeviation != 0.0 && (Math.Abs(buffer[iterator] - _mean) / _standardDeviation) > 2)
                {
                    samplesWithSounds++;
                }
            }

            return new FrameInfo
            {
                Energy = energy,
                Power = energy / buffer.Length,
                ZeroCrossingRate = zeroCrossingRate / buffer.Length - 1,
                VoicedSamples = samplesWithSounds
            };
        }       

        private double ComputeCaracteristicFunction(FrameInfo frameInfo)
        {            
            return frameInfo.Power * (1 - frameInfo.ZeroCrossingRate) * ScaleFactor;
        }

        private void Init()
        {            
            var values = new double[_emptyFrames];
            var temp = 0.0;
            List<float> voiceFreeData = new List<float>();
            for (int index = 0; index < _emptyFrames; index++)
            {                
                var itemsToRead =  _frameSize;
                var buffer = new float[itemsToRead];
                _signal.Read(buffer, 0, itemsToRead);
                voiceFreeData.AddRange(buffer);

                var frameInfo = ComputeThreshholds(buffer);
                values[index] = ComputeCaracteristicFunction(frameInfo);
                temp += values[index];
            }

            var meanCaracteristicFunction = temp / _emptyFrames;
            temp = 0.0;
            for (int index = 0; index < _emptyFrames; index++)
            {
                var val = values[index] - meanCaracteristicFunction;
                temp += val * val;
            }

            var varianceCaracteristicFunction = Math.Sqrt(temp);

            //var alpha = 0.2 * Math.Pow(varianceCaracteristicFunction, -0.8);
            //_thresholdsActivation = meanCaracteristicFunction + alpha * varianceCaracteristicFunction;
            _thresholdsActivation = meanCaracteristicFunction + alphaCoeff * Math.Pow(varianceCaracteristicFunction, 0.2);

            if (_enhancements.HasFlag(VoiceActivationDetectionEnhancement.MahalanobisDistance))
            {
                double sum = 0.0;
                // 1. calculation of mean
                for (int i = 0; i < voiceFreeData.Count; i++)
                {
                    sum += voiceFreeData[i];
                }
                _mean = sum / voiceFreeData.Count;// mean
                sum = 0;// reuse var for S.D.

                // 2. calculation of Standard Deviation
                for (int i = 0; i < voiceFreeData.Count; i++)
                {
                    sum += Math.Pow((voiceFreeData[i] - _mean), 2);
                }
                _standardDeviation = Math.Sqrt(sum / voiceFreeData.Count);
            }
        }


        public bool IsVoice(float[] data)
        {
            FrameInfo? previousFrameInfo = _lastFrameInfo;
            _lastFrameInfo = ComputeThreshholds(data);
            var value = ComputeCaracteristicFunction(_lastFrameInfo.Value);
            var ret = value > _thresholdsActivation;
            if (ret)
            {
                _lastFrameWasVoice = true;
                return true;
            }
            else if (_enhancements.HasFlag(VoiceActivationDetectionEnhancement.MahalanobisDistance) &&
                ((_lastFrameInfo.Value.VoicedSamples > data.Length - _lastFrameInfo.Value.VoicedSamples)))
            {
                _lastFrameWasVoice = true;
                return true;
            }
            else if (_enhancements.HasFlag(VoiceActivationDetectionEnhancement.DeltaVariance) && previousFrameInfo.HasValue)
            {
                var previousValue = ComputeCaracteristicFunction(previousFrameInfo.Value);
                var gainRatio = (value - previousValue) / previousValue;
                if (gainRatio > 0.5)
                {
                    _lastFrameWasVoice = true;
                    return true;
                }
                else if (_lastFrameWasVoice && ((value - _thresholdsActivation) / _thresholdsActivation) > -0.2)
                {
                    _lastFrameWasVoice = true;
                    return true;
                }
            }
            _lastFrameWasVoice = false;
            return false;
        }

        #endregion

        #region Properties

        #endregion

        struct FrameInfo
        {
            public double Energy;
            public double Power;
            public double ZeroCrossingRate;
            public int VoicedSamples;
        }
    }

    [Flags]
    public enum VoiceActivationDetectionEnhancement
    {
        MahalanobisDistance = 1,
        DeltaVariance = 2,
        All = MahalanobisDistance | DeltaVariance
    }
}
