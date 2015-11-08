using System;
using System.Collections.Generic;
using System.Linq;
using Accord.Statistics.Distributions.Fitting;
using Accord.Statistics.Distributions.Multivariate;
using Accord.Statistics.Models.Markov;
using Accord.Statistics.Models.Markov.Learning;
using Accord.Statistics.Models.Markov.Topology;
using SpeechRecognition.Audio;

namespace SpeechRecognition
{
    public class RecognitionEngine
    {
        #region Properties

        private readonly FeatureUtility _featureUtility;

        private readonly Dictionary<string, HiddenMarkovModel<MultivariateMixture<MultivariateNormalDistribution>>> _models = new Dictionary<string, HiddenMarkovModel<MultivariateMixture<MultivariateNormalDistribution>>>();

        private const int NumberOfHiddenStates = 15;

        private readonly int _numberOfCoeffcients;

        #endregion

        #region Instance

        public RecognitionEngine(EngineParameters parameters)
        {
            _numberOfCoeffcients = parameters.ProviderParameters.NumberOfCoeff;
            _featureUtility = new FeatureUtility(parameters);
        }

        public RecognitionEngine()
            : this(EngineParameters.Default)
        {

        }

        #endregion

        #region Methods

        public void Train(IList<SoundSignalReader> signals, string value)
        {
            int featureLength = _numberOfCoeffcients * 3;
            Array[] observationsInput = new Array[signals.Count()];
            var signalsCount = signals.Count();
            for (var signalIndex = 0; signalIndex < signalsCount; signalIndex++)
            {
                var signal = signals[signalIndex];
                var observables = _featureUtility.ExtractFeatures(signal).First();
                observationsInput[signalIndex] = observables.ToArray();
            }


            const int iterations = 1000;
            const double tolerance = 0.1;
            const bool rejection = true;            
            
            var distribution1 = new MultivariateNormalDistribution(featureLength);
            var distribution2 = new MultivariateNormalDistribution(featureLength);
            var distribution3 = new MultivariateNormalDistribution(featureLength);

            var mixture = new MultivariateMixture<MultivariateNormalDistribution>(distribution1, distribution2, distribution3);


            var hmm = new HiddenMarkovClassifier<MultivariateMixture<MultivariateNormalDistribution>>(1,
                 new Forward(NumberOfHiddenStates), mixture, new[] { value });

            // Create the learning algorithm for the ensemble classifier
            var teacher = new HiddenMarkovClassifierLearning<MultivariateMixture<MultivariateNormalDistribution>>(hmm,
                // Train each model using the selected convergence criteria
                i => new ViterbiLearning<MultivariateMixture<MultivariateNormalDistribution>>(hmm.Models[i])
                {
                    Tolerance = tolerance,
                    Iterations = iterations,
                    UseLaplaceRule = true,
                    FittingOptions = new MixtureOptions
                    {
                        Logarithm = true,
                        Iterations = iterations,
                        InnerOptions = new NormalOptions
                        {
                            Regularization = 1e-8
                        },
                    }
                }
                ) {Empirical = true, Rejection = rejection};


            // Run the learning algorithm
            teacher.Run(observationsInput, new int[observationsInput.Length]);            
        }

        public string Recognize(SoundSignalReader signal)
        {
            var observables = _featureUtility.ExtractFeatures(signal).First(); 

            var ret = string.Empty;
            var likelyHoodValue = Double.MinValue;
            var observations = observables.ToArray();
            foreach (var item in _models)
            {
                var currentValue = item.Value.Evaluate(observations);
                if (currentValue > likelyHoodValue)
                {
                    likelyHoodValue = currentValue;
                    ret = item.Key;
                }
            }

            return ret;
        }
        
        public int Recognize(SoundSignalReader signal, HiddenMarkovClassifier<MultivariateMixture<MultivariateNormalDistribution>> hmm)
        {
            var observables = _featureUtility.ExtractFeatures(signal).First(); 

            var observations = observables.ToArray();
            double[] responsabilities;
            var ret = hmm.Compute(observations, out responsabilities);

            return ret;
        }

        public HiddenMarkovClassifier<MultivariateMixture<MultivariateNormalDistribution>> TrainAll(Dictionary<string, IList<SoundSignalReader>> signalsDictionary)
        {
            int featureLength = ((_numberOfCoeffcients - 1) * 3) + 1;
            double[][][][] featuresInput = new Double[signalsDictionary.Count][][][];


            var models = new List<int>();
            var allSamples = new List<double[][]>();
            var modelIndex = 0;

            foreach (var item in signalsDictionary)
            {
                var signals = item.Value; // signals
                var signalsCount = signals.Count();
                List<List<double[]>> samples = new List<List<double[]>>();

                for (var signalIndex = 0; signalIndex < signalsCount; signalIndex++)
                {
                    var signal = signals[signalIndex];
                    var allSignalfeatures = _featureUtility.ExtractFeatures(signal).ToArray();
                    
                    samples.AddRange(allSignalfeatures);
                    for (var featuresIndex = 0; featuresIndex < allSignalfeatures.Length; featuresIndex++)
                    {
                        allSamples.Add(allSignalfeatures[featureLength].ToArray());
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


            //int iterations = 100000;
            const int iterations = 10000;
            const double tolerance = 0.001;
            const bool rejection = false;            

            var distribution1 = new MultivariateNormalDistribution(featureLength);
            var distribution2 = new MultivariateNormalDistribution(featureLength);
            var distribution3 = new MultivariateNormalDistribution(featureLength);

            var mixture = new MultivariateMixture<MultivariateNormalDistribution>(distribution1, distribution2, distribution3);

            var hmm = new HiddenMarkovClassifier<MultivariateMixture<MultivariateNormalDistribution>>(signalsDictionary.Count,
                 new Forward(NumberOfHiddenStates), mixture, signalsDictionary.Keys.ToArray());
            

            // Create the learning algorithm for the ensemble classifier

            var teacher = new HiddenMarkovClassifierLearning<MultivariateMixture<MultivariateNormalDistribution>>(hmm,

                // Train each model using the selected convergence criteria
                i => new ViterbiLearning<MultivariateMixture<MultivariateNormalDistribution>>(hmm.Models[i])
                {
                    Tolerance = tolerance,
                    Iterations = iterations,
                    FittingOptions = new MixtureOptions
                    {
                        Iterations = 200,
                        InnerOptions = new NormalOptions
                        {
                            Regularization = 1e-8
                        },
                    }
                }
                ) {Rejection = rejection};

            //teacher.Empirical = true;

            var data = allSamples.ToArray();
            // Run the learning algorithm
           // Array[][] asd = new double[3][];
           teacher.Run(data, models.ToArray());

            return hmm;

        }

        public void TrainAllSync(Dictionary<string, IList<SoundSignalReader>> signalsDictionary)
        {
            int featureLength = (_numberOfCoeffcients + 1) * 3;

            var observationsForModel = signalsDictionary.ToDictionary(item => item.Key, item => new Double[item.Value.Count][][]);

            foreach (var item in signalsDictionary)
            {
                var signals = item.Value;
                var signalsCount = signals.Count();
                for (var signalIndex = 0; signalIndex < signalsCount; signalIndex++)
                {
                    var signal = signals[signalIndex];
                    List<Double[]> observables = _featureUtility.ExtractFeatures(signal).First();                    
                    observationsForModel[item.Key][signalIndex] = observables.ToArray();
                }

            }


            foreach (var key in signalsDictionary)
            {
                var distribution1 = new MultivariateNormalDistribution(featureLength);
                var distribution2 = new MultivariateNormalDistribution(featureLength);
                var distribution3 = new MultivariateNormalDistribution(featureLength);

                var mixture = new MultivariateMixture<MultivariateNormalDistribution>(distribution1, distribution2, distribution3);                

                var model = new HiddenMarkovModel<MultivariateMixture<MultivariateNormalDistribution>>(NumberOfHiddenStates, mixture);


                var learning = new ViterbiLearning<MultivariateMixture<MultivariateNormalDistribution>>(model)
                    {                        
                        Iterations = 100,
                        Tolerance = 0.0001,

                        FittingOptions = new MixtureOptions
                        {                            

                            InnerOptions = new NormalOptions
                            {
                                Regularization = 1e-8,
                            },
                        }
                    };
                learning.Run(observationsForModel[key.Key]);
                _models.Add(key.Key, model);
            }
        }     
        #endregion
    }
}
