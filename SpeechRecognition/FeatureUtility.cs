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

        private int SilenceThreshHold { get; set; }
        private int MinWordLength { get; set; }



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

            SilenceThreshHold = Convert.ToInt32(Math.Ceiling((FrameSizeMiliseconds*10)/StepSizeMiliseconds));
            MinWordLength = Convert.ToInt32(Math.Ceiling((FrameSizeMiliseconds * 8) / StepSizeMiliseconds));
        }

        #endregion

       

        #region Methods

        private void ExtractFeaturesInternalUsingVad(ISoundSignalReader signal, Action<List<double[]>> featureExtracted,
            SignalVisitor voiceVisitor)
        {
            var featureProvider = FeaturesProviderFactory.GetProvider(ProviderParameters, signal);

            var frameSize = (int) Math.Floor(signal.SampleRate*FrameSizeMiliseconds/1000.0);
            var stepSize = (int) Math.Floor(signal.SampleRate*StepSizeMiliseconds/1000.0);
            var filteredSignal = new PreemphasisFilter(signal, 0.95f);
            float[] frame;

            var voiceStream = new VoiceActivitySignalReader(filteredSignal, frameSize, 8);
            if (voiceVisitor != null)
            {
                voiceStream.Accept(voiceVisitor);
            }

            int index = 0, silentSamples = 0, noOfItems = ProviderParameters.NumberOfCoeff - 1;
            var observables = new List<double[]>();

            bool isVoice;
            while (voiceStream.Read(frameSize, stepSize, out frame, out isVoice))
            {
                if (isVoice)
                {
                    bool isEmpty;
                    var features = featureProvider.Extract(frame, out isEmpty);

                    silentSamples = 0;
                    observables.Add(features);
                    if (featureProvider.ComputeDelta)
                    {
                        ComputeDelta(observables, index - 1, 1, noOfItems);
                        ComputeDelta(observables, index - 2, noOfItems + 1, noOfItems, true);
                    }

                    index++;
                }
                else if (observables.Count > 0 && silentSamples > SilenceThreshHold)
                {
                    if (index >= MinWordLength)
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
        }

        public IEnumerable<List<double[]>> ExtractFeatures(ISoundSignalReader signal, SignalVisitor voiceVisitor = null)
        {
            List<List<Double[]>> allObservables = new List<List<Double[]>>();
            Action<List<double[]>> addfeatures = features =>
            {
                allObservables.Add(features);
            };

            ExtractFeaturesInternalUsingVad(signal, addfeatures, voiceVisitor);

            return allObservables;
        }

        public void ExtractFeaturesAsync(ISoundSignalReader signal, Action<List<double[]>> action,
            SignalVisitor voiceVisitor = null)
        {
            Action<List<double[]>> addfeatures = features =>
            {
                action.BeginInvoke(features, null, null);
            };

            ExtractFeaturesInternalUsingVad(signal, addfeatures, voiceVisitor);
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
