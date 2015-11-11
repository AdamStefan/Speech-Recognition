using System;
using System.Collections.Generic;
using System.Linq;
using Accord.Statistics.Models.Markov;
using Accord.Statistics.Models.Markov.Learning;
using Accord.Statistics.Models.Markov.Topology;
using SpeechRecognition.Audio;
using SpeechRecognition.VectorQuantization;

namespace SpeechRecognition
{
    public class RecognizeEngineDiscreteHmmLearning
    {
        #region Properties

        private readonly int _numberOfHiddenStates;
        private readonly Codebook _codeBook;
        private readonly EngineParameters _engineParameters;

        #endregion

        #region Instance

        public RecognizeEngineDiscreteHmmLearning(int numberOfHiddenStates, EngineParameters parameters, Codebook codebook)
        {
            _numberOfHiddenStates = numberOfHiddenStates;
            _engineParameters = parameters;
            _codeBook = codebook;

        }

        public RecognizeEngineDiscreteHmmLearning(Codebook codeBook)
            : this(5, EngineParameters.Default, codeBook)
        {

        }

        #endregion

        #region Methods

        public TrainResult TrainAll(Dictionary<string, IList<SoundSignalReader>> signalsDictionary, SampleAggregator aggregator = null)
        {
            var numberOfItems = 0;
            foreach (var item in signalsDictionary)
            {
                numberOfItems += item.Value.Count;
            }

            double[][][][] featuresInput = new Double[signalsDictionary.Count][][][];


            int[] models = new int[numberOfItems];
            var allSignalIndex = 0;
            var modelIndex = 0;

            var featureUtility = new FeatureUtility(_engineParameters, aggregator);

            foreach (var item in signalsDictionary)
            {
                var signals = item.Value; // signals
                var signalsCount = signals.Count();

                featuresInput[modelIndex] = new double[signalsCount][][];

                for (var signalIndex = 0; signalIndex < signalsCount; signalIndex++)
                {
                    var signal = signals[signalIndex];
                    List<Double[]> features = featureUtility.ExtractFeatures(signal).First();


                    featuresInput[modelIndex][signalIndex] = features.ToArray();
                    models[allSignalIndex] = modelIndex;
                    allSignalIndex++;
                }
                modelIndex++;
            }


            List<int[]> observables = new List<int[]>();


            for (int wordIndex = 0; wordIndex < featuresInput.Length; wordIndex++) // foreach word
            {
                for (var signalIndex = 0; signalIndex < featuresInput[wordIndex].Length; signalIndex++) // foreach word signal
                {
                    var points = featuresInput[wordIndex][signalIndex].Select(item => new Point(item)); // convert feature to points

                    var codeItems = _codeBook.Quantize(points.ToArray());
                    observables.Add(codeItems);
                }
            }
            //HiddenMarkovModel hmm = new HiddenMarkovModel(5, _codeBook.Size, true);
            //var Bauc


            var hmm = new HiddenMarkovClassifier(signalsDictionary.Count, new Forward(_numberOfHiddenStates), _codeBook.Size, signalsDictionary.Keys.ToArray());

            const int iterations = 200;
            const double tolerance = 0;

            var teacher = new HiddenMarkovClassifierLearning(hmm,
                i => new ViterbiLearning(hmm.Models[i]) { Iterations = iterations, Tolerance = tolerance }
                );

            teacher.Run(observables.ToArray(), models);

            return new TrainResult { Catalog = _codeBook, Models = hmm.Models.ToArray() };

        }


        public TrainResult Train(Dictionary<string, IList<SoundSignalReader>> signalsDictionary,
            SampleAggregator aggregator = null)
        {
            double[][][][] featuresInput = new Double[signalsDictionary.Count][][][];

            var models = new List<int>();
            var modelIndex = 0;

            var featureUtility = new FeatureUtility(_engineParameters, aggregator);

            foreach (var item in signalsDictionary)
            {
                var signals = item.Value; // signals
                var signalsCount = signals.Count();
                List<List<double[]>> samples = new List<List<double[]>>();

                for (var signalIndex = 0; signalIndex < signalsCount; signalIndex++)
                {
                    var signal = signals[signalIndex];
                    var allSignalfeatures = featureUtility.ExtractFeatures(signal).ToArray();
                    samples.AddRange(allSignalfeatures);
                    for (var featuresIndex = 0; featuresIndex < allSignalfeatures.Length; featuresIndex++)
                    {
                        models.Add(modelIndex);
                    }
                }

                featuresInput[modelIndex] = new double[samples.Count][][];

                for (var index = 0; index < samples.Count; index++)
                {
                    featuresInput[modelIndex][index] = samples[index].ToArray();
                }
                modelIndex++;
            }


            List<int[]> observables = new List<int[]>();

            for (int wordIndex = 0; wordIndex < featuresInput.Length; wordIndex++) // foreach word
            {
                for (var signalIndex = 0; signalIndex < featuresInput[wordIndex].Length; signalIndex++)
                // foreach word signal
                {
                    var points = featuresInput[wordIndex][signalIndex].Select(item => new Point(item));
                    // convert feature to points

                    var codeItems = _codeBook.Quantize(points.ToArray());
                    observables.Add(codeItems);
                }
            }


            var hmm = new HiddenMarkovClassifier(signalsDictionary.Count, new Forward(_numberOfHiddenStates),
                _codeBook.Size, signalsDictionary.Keys.ToArray());

            const int iterations = 2000;
            const double tolerance = 0;

            var teacher = new HiddenMarkovClassifierLearning(hmm,
                i => new ViterbiLearning(hmm.Models[i]) { Iterations = iterations, Tolerance = tolerance, Batches = 2 }
                );

            teacher.Run(observables.ToArray(), models.ToArray());

            return new TrainResult { Catalog = _codeBook, Models = hmm.Models.ToArray() };
        }


        public int Recognize(SoundSignalReader signal, HiddenMarkovClassifier hmm, out string name, SampleAggregator aggregator = null)
        {

            var featureUtility = new FeatureUtility(_engineParameters, aggregator);
            signal.Reset();
            var features = featureUtility.ExtractFeatures(signal).First();
            var observations = _codeBook.Quantize(features.Select(item => new Point(item)).ToArray());

            double[] responsabilities;
            var ret = hmm.Compute(observations, out responsabilities);

            var models = hmm.Models;
            var likelyHoodValue = Double.MinValue;
            name = string.Empty;

            foreach (var model in models)
            {
                var val = model.Evaluate(observations);
                if (val > likelyHoodValue)
                {
                    likelyHoodValue = val;
                    name = model.Tag.ToString();
                }
            }

            return ret;
        }

        public int Recognize(SoundSignalReader signal, HiddenMarkovModel[] models, out string name,
            SampleAggregator aggregator = null)
        {

            var featureUtility = new FeatureUtility(_engineParameters, aggregator);
            signal.Reset();
            var features = featureUtility.ExtractFeatures(signal).First();
            var observations = _codeBook.Quantize(features.Select(item => new Point(item)).ToArray());


            var likelyHoodValue = Double.MinValue;
            name = string.Empty;
            var index = 0;
            var ret = 0;
            foreach (var model in models)
            {
                var val = model.Evaluate(observations);
                if (val > likelyHoodValue)
                {
                    likelyHoodValue = val;
                    name = model.Tag.ToString();
                    ret = index;
                }
                index++;
            }

            return ret;
        }


        public void RecognizeAsync(SoundSignalReader signal, HiddenMarkovClassifier hmm, Action<string> handleMessage,
            SampleAggregator aggregator = null)
        {
            Action<List<double[]>> action = features =>
            {
                var observations = _codeBook.Quantize(features.Select(item => new Point(item)).ToArray());
                double[] responsabilities;
                var ret = hmm.Compute(observations, out responsabilities);

                var models = hmm.Models;
                var likelyHoodValue = Double.MinValue;

                foreach (var model in models)
                {
                    var val = model.Evaluate(observations);
                    if (val > likelyHoodValue)
                    {
                        likelyHoodValue = val;
                    }
                }

                handleMessage(hmm[ret].Tag.ToString());
            };

            var featureUtility = new FeatureUtility(_engineParameters, aggregator);
            featureUtility.ExtractFeaturesAsync(signal, action);
        }

        #endregion
    }
}

