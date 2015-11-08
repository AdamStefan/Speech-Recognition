using System;
using System.Collections.Generic;
using System.Linq;
using SpeechRecognition.Audio;
using SpeechRecognition.FeaturesProvider;

namespace SpeechRecognition
{
    public class FeatureUtility
    {
        #region Properties

        public double StepSizeMiliseconds { get; private set; }

        public double FrameSizeMiliseconds { get; private set; }        

        private FeatureProviderParameters ProviderParameters { get; set; }

        private const int SilenceThreshHold = 10;

        private float[] _nextFrame;
        
        #endregion

        #region Instance

        public FeatureUtility()
            : this(EngineParameters.Default)
        {

        }

        public FeatureUtility(EngineParameters parameters)
        {
            FrameSizeMiliseconds = parameters.FrameSizeMiliseconds;
            StepSizeMiliseconds = parameters.StepSizeMiliseconds;
            ProviderParameters = parameters.ProviderParameters;            
        }

        #endregion

        #region Methods

        private void ExtractFeaturesInternalUsingVad(SoundSignalReader signal, Action<List<double[]>> featureExtracted)
        {
            var frameSize = (int)Math.Floor(signal.SampleRate * FrameSizeMiliseconds / 1000.0);
            var stepSize = (int)Math.Floor(signal.SampleRate * StepSizeMiliseconds / 1000.0);
            var filteredSignal = new PreemphasisFilter(signal, 0.95f);

            var featureProvider = FeaturesProviderFactory.GetProvider(ProviderParameters, signal);

            float[] frame;
            filteredSignal.Reset();
            var voiceActivationDetection = new VoiceActivationDetection(signal, frameSize, 8);
            int index = 0;
            var noOfItems = ProviderParameters.NumberOfCoeff - 1; ;


            var observables = new List<double[]>();
            var silentSamples = 0;
            _nextFrame = null;
            filteredSignal.Reset();

            //stepSize = frameSize; 
            var silenceThreshHold = (frameSize * SilenceThreshHold) / stepSize;
            var minWordLength = (frameSize * 8) / stepSize;
            while (Read(filteredSignal, frameSize, stepSize, out frame))
            {
                if (voiceActivationDetection.IsVoice(frame))
                {
                    bool isEmpty;
                    var features = featureProvider.Extract(frame, out isEmpty);
                  

                    //if (observables.Count < 3 && isEmpty)
                    //{
                    //    observables = new List<double[]>();
                    //    silentSamples++;
                    //    continue;
                    //}

                    silentSamples = 0;
                    observables.Add(features);
                    if (featureProvider.ComputeDelta)
                    {
                        ComputeDelta(observables, index - 1, 1, noOfItems);
                        ComputeDelta(observables, index - 2, noOfItems + 1, noOfItems, true);
                    }

                    index++;
                }
                else if (observables.Count > 0 && silentSamples > silenceThreshHold )
                {
                    if (index >= minWordLength)
                    {
                        if (featureProvider.ComputeDelta)
                        {
                            ComputeDelta(observables, index - 1, 1, noOfItems);
                            ComputeDelta(observables, index - 2, noOfItems + 1, noOfItems, true);
                            ComputeDelta(observables, index - 1, noOfItems + 1, noOfItems, true);
                        }

                        featureExtracted(observables);
                    }

                    observables = new List<double[]>();
                    index = 0;
                }
                else
                {
                    silentSamples++;
                }
            }

            if (observables.Count > minWordLength)
            {
                if (featureProvider.ComputeDelta)
                {
                    ComputeDelta(observables, index - 1, 1, noOfItems);
                    ComputeDelta(observables, index - 2, noOfItems + 1, noOfItems, true);
                    ComputeDelta(observables, index - 1, noOfItems + 1, noOfItems, true);
                }

                featureExtracted(observables);
            }
        }


        public IEnumerable<List<double[]>> ExtractFeatures(SoundSignalReader signal)
        {
            List<List<Double[]>> allObservables = new List<List<Double[]>>();
            Action<List<double[]>> addfeatures = features =>
            {
                allObservables.Add(features);
            };

            ExtractFeaturesInternalUsingVad(signal, addfeatures);

            return allObservables;
        }

        public void ExtractFeaturesAsync(SoundSignalReader signal, Action<List<double[]>> action)
        {            
            Action<List<double[]>> addfeatures = features =>
            {
                action.BeginInvoke(features, null, null);
            };

            ExtractFeaturesInternalUsingVad(signal, addfeatures);
        }
        

              

        public bool Read(SoundSignalReader signal, int frameSize, int stepSize, out float[] frame)
        {
            int numberOfBitsToRead = frameSize;
            int bufferStartIndex = 0;
            if (_nextFrame == null)
            {
                _nextFrame = new float[frameSize];
                frame = new float[frameSize];
            }
            else
            {
                frame = _nextFrame;
                _nextFrame = new float[frameSize];
                bufferStartIndex = frameSize - stepSize;
                numberOfBitsToRead = stepSize;
            }

            var ret = signal.Read(frame, bufferStartIndex, numberOfBitsToRead);

            for (int index = stepSize; index < frameSize; index++)
            {
                _nextFrame[index - stepSize] = frame[index];
            }

            return ret;
        }

        private void ComputeDelta(IList<double[]> values, int deltaIndex, int startIndex, int count,
            bool normalize = false)
        {
            if (deltaIndex < 0)
            {
                return;
            }
            var valuesItems = values[deltaIndex].ToList();
            var itemsCount = values.Count;
            for (var index = startIndex; index < startIndex + count; index++)
            {
                var leftValue = deltaIndex == 0 ? 0 : values[deltaIndex - 1][index];
                var rightValue = (deltaIndex + 1) >= itemsCount ? 0 : values[deltaIndex + 1][index];
                valuesItems.Add((rightValue - leftValue) / 2.0);
            }

            values[deltaIndex] = valuesItems.ToArray();

            if (normalize)
            {
                Utils.Normalize(values[deltaIndex]);
            }
        }

        #endregion
    }    
}
