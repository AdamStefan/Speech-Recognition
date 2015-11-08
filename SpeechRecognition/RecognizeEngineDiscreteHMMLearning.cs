using System;
using System.Collections.Generic;
using System.Linq;
using Accord.Statistics.Models.Markov;
using Accord.Statistics.Models.Markov.Learning;
using Accord.Statistics.Models.Markov.Topology;
using SpeechRecognition.Audio;
using SpeechRecognition.VectorQuantization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Serialization;

namespace SpeechRecognition
{
    public class RecognizeEngineDiscreteHmmLearning
    {
        #region Properties

        private readonly FeatureUtility _featureUtility;             
        private readonly int _numberOfHiddenStates;                
        private readonly Codebook _codeBook;

        #endregion

        #region Instance

        public RecognizeEngineDiscreteHmmLearning(int numberOfHiddenStates, EngineParameters parameters, Codebook codebook)
        {
            _numberOfHiddenStates = numberOfHiddenStates;            
            _featureUtility = new FeatureUtility(parameters);
            _codeBook = codebook;

        }

        public RecognizeEngineDiscreteHmmLearning(Codebook codeBook)
            : this(5, EngineParameters.Default, codeBook)
        {

        }

        #endregion

        public TrainResult TrainAll(Dictionary<string, IList<SoundSignalReader>> signalsDictionary)
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

            foreach (var item in signalsDictionary)
            {
                var signals = item.Value; // signals
                var signalsCount = signals.Count();                

                featuresInput[modelIndex] = new double[signalsCount][][];                

                for (var signalIndex = 0; signalIndex < signalsCount; signalIndex++)
                {
                    var signal = signals[signalIndex];
                    List<Double[]> features = _featureUtility.ExtractFeatures(signal).First();

                    
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

            var hmm = new HiddenMarkovClassifier(signalsDictionary.Count, new Forward(_numberOfHiddenStates), _codeBook.Size, signalsDictionary.Keys.ToArray());

            const int iterations = 200;
            const double tolerance = 0;

            var teacher = new HiddenMarkovClassifierLearning(hmm,
                i => new ViterbiLearning(hmm.Models[i]) { Iterations = iterations, Tolerance = tolerance }
                );

            teacher.Run(observables.ToArray(), models);

            return new TrainResult() { Catalog = _codeBook, Hmm = hmm };

        }


        public TrainResult Train(Dictionary<string, IList<SoundSignalReader>> signalsDictionary)
        {           
            double[][][][] featuresInput = new Double[signalsDictionary.Count][][][];
            
            var models = new List<int>();
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
                for (var signalIndex = 0; signalIndex < featuresInput[wordIndex].Length; signalIndex++) // foreach word signal
                {
                    var points = featuresInput[wordIndex][signalIndex].Select(item => new Point(item)); // convert feature to points

                    var codeItems = _codeBook.Quantize(points.ToArray());
                    observables.Add(codeItems);
                }
            }           

            var hmm = new HiddenMarkovClassifier(signalsDictionary.Count, new Forward(_numberOfHiddenStates), _codeBook.Size, signalsDictionary.Keys.ToArray());

            const int iterations = 2000;
            const double tolerance = 0;

            var teacher = new HiddenMarkovClassifierLearning(hmm,
                i => new ViterbiLearning(hmm.Models[i]) { Iterations = iterations, Tolerance = tolerance, Batches = 2 }
                );

            teacher.Run(observables.ToArray(), models.ToArray());

            return new TrainResult { Catalog = _codeBook, Hmm = hmm };
        }


        public int Recognize(SoundSignalReader signal, HiddenMarkovClassifier hmm, out string name)
        {
            signal.Reset();
            var features = _featureUtility.ExtractFeatures(signal).First();
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

        public void RecognizeAsync(SoundSignalReader signal, HiddenMarkovClassifier hmm, Action<string> handleMessage)
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
            _featureUtility.ExtractFeaturesAsync(signal,action);
        }


    }

    public class TrainResult
    {
        public HiddenMarkovClassifier Hmm { get; set; }
        public Codebook Catalog { get; set; }

        public void Save(string folder, string name)
        {
            var classifierPath = System.IO.Path.Combine(folder, name + "hmm.dat");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            Hmm.Save(classifierPath);

            var codebookPath = System.IO.Path.Combine(folder, name + "codebook.dat");
            FileStream fs = new FileStream(codebookPath, FileMode.Create);

            // Construct a BinaryFormatter and use it to serialize the data to the stream.
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                formatter.Serialize(fs, Catalog);
            }
            catch (SerializationException e)
            {
                Console.WriteLine("Failed to serialize. Reason: " + e.Message);
                throw;
            }
            finally
            {
                fs.Close();
            }

        }

        public static TrainResult Load(string folder, string name)
        {
            var classifierPath = System.IO.Path.Combine(folder, name + "hmm.dat");

            if (!File.Exists(classifierPath))
            {
                return null;
            }
            var hmm = HiddenMarkovClassifier.Load(classifierPath);
            var codebookPath = System.IO.Path.Combine(folder, name + "codebook.dat");
            FileStream fs = new FileStream(codebookPath, FileMode.Open);

            // Construct a BinaryFormatter and use it to serialize the data to the stream.
            BinaryFormatter formatter = new BinaryFormatter();
            Codebook cb = null;
            try
            {
                cb = (Codebook)formatter.Deserialize(fs);
            }
            catch (SerializationException e)
            {
                Console.WriteLine("Failed to serialize. Reason: " + e.Message);
                throw;
            }
            finally
            {
                fs.Close();
            }

            return new TrainResult
            {
                Hmm = hmm,
                Catalog = cb
            };

        }


    }
}

