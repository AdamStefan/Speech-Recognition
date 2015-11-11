using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Accord.Statistics.Models.Markov;
using Accord.Statistics.Models.Markov.Learning;
using SpeechRecognition.Audio;
using SpeechRecognition.VectorQuantization;
using System.Threading.Tasks;

namespace SpeechRecognition
{
    public class DetectionEngine
    {
        #region Properties

        private readonly int _numberOfHiddenStates;
        private readonly Codebook _codeBook;
        private readonly EngineParameters _engineParameters;
        private readonly ConcurrentDictionary<string, HiddenMarkovModel> _models = new ConcurrentDictionary<string, HiddenMarkovModel>();

        #endregion

        #region Instance

        public DetectionEngine(int numberOfHiddenStates, EngineParameters parameters, Codebook codebook, IEnumerable<HiddenMarkovModel> models = null)
        {
            _numberOfHiddenStates = numberOfHiddenStates;
            _engineParameters = parameters;
            _codeBook = codebook;
            if (models != null)
            {
                foreach (var model in models)
                {
                    _models[model.Tag.ToString()] = model;
                }
            }

        }

        public DetectionEngine(Codebook codeBook, IEnumerable<HiddenMarkovModel> models = null)
            : this(5, EngineParameters.Default, codeBook, models)
        {

        }

        #endregion

        #region Methods


        public TrainResult Train(Dictionary<string, IList<SoundSignalReader>> signalsDictionary,
            SampleAggregator aggregator = null)
        {

            Parallel.ForEach(signalsDictionary, item =>
            {
                BuildModel(item.Value, item.Key, aggregator);
            });

            return new TrainResult { Catalog = _codeBook, Models = _models.Values.ToArray() };
        }


        public HiddenMarkovModel BuildModel(IList<SoundSignalReader> signalReaders, string tag,
            SampleAggregator aggregator)
        {
            var signals = signalReaders; // signals
            var signalsCount = signals.Count();
            List<List<double[]>> samples = new List<List<double[]>>();
            var featureUtility = new FeatureUtility(_engineParameters, aggregator);

            for (var signalIndex = 0; signalIndex < signalsCount; signalIndex++)
            {
                var signal = signals[signalIndex];
                signal.Reset();
                var allSignalfeatures = featureUtility.ExtractFeatures(signal).ToArray();
                samples.AddRange(allSignalfeatures);
            }

            var featuresInput = new double[samples.Count][][];

            for (var index = 0; index < samples.Count; index++)
            {
                featuresInput[index] = samples[index].ToArray();
            }
            var hmm = new HiddenMarkovModel(_numberOfHiddenStates, _codeBook.Size, false);

            List<int[]> observables = new List<int[]>();
            for (var signalIndex = 0; signalIndex < featuresInput.Length; signalIndex++) // foreach word signal
            {
                var points = featuresInput[signalIndex].Select(item => new Point(item)); // convert feature to points

                var codeItems = _codeBook.Quantize(points.ToArray());
                observables.Add(codeItems);
            }


            const int iterations = 20000;
            const double tolerance = 0.0;
            var viterbiLearning = new ViterbiLearning(hmm) { Iterations = iterations, Tolerance = tolerance };

            var error = viterbiLearning.Run(observables.ToArray());
            viterbiLearning.Model.Tag = tag;

            _models[tag] = viterbiLearning.Model;
            return viterbiLearning.Model;
        }

        public int Recognize(SoundSignalReader signal, out string name,
            SampleAggregator aggregator = null)
        {
            var featureUtility = new FeatureUtility(_engineParameters, aggregator);
            signal.Reset();
            var features = featureUtility.ExtractFeatures(signal).First();

            var observations = _codeBook.Quantize(features.Select(item => new Point(item)).ToArray());
            var likelyHoodValue = Double.MinValue;
            HiddenMarkovModel bestFit = null;
            var index = 0;
            var ret = 0;
            foreach (var model in _models.Values)
            {
                var val = model.Evaluate(observations);

                if (val > likelyHoodValue)
                {
                    likelyHoodValue = val;
                    bestFit = model;
                    ret = index;
                }
                index++;
            }
            name = bestFit != null ? bestFit.Tag.ToString() : string.Empty;
            return ret;
        }

        public void RecognizeAsync(SoundSignalReader signal, Action<string> handleMessage,
            SampleAggregator aggregator = null)
        {
            Action<List<double[]>> action = features =>
            {
                var observations = _codeBook.Quantize(features.Select(item => new Point(item)).ToArray());
                var likelyHoodValue = Double.MinValue;
                HiddenMarkovModel bestFit = null;
                foreach (var model in _models.Values)
                {
                    var val = model.Evaluate(observations);
                    if (val > likelyHoodValue)
                    {
                        likelyHoodValue = val;
                        bestFit = model;
                    }
                }

                if (bestFit != null)
                {
                    handleMessage(bestFit.Tag.ToString());
                }
            };

            var featureUtility = new FeatureUtility(_engineParameters, aggregator);
            featureUtility.ExtractFeaturesAsync(signal, action);
        }

        #endregion
    }
}
