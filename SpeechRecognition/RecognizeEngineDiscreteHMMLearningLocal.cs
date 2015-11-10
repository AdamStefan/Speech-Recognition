using System;
using System.Collections.Generic;
using System.Linq;
using SpeechRecognition.Audio;
using SpeechRecognition.HMM;
using SpeechRecognition.VectorQuantization;

namespace SpeechRecognition
{
    public class RecognizeEngineDiscreteHmmLearningLocal
    {
        #region Properties
        
        private readonly Dictionary<string, HiddenMarkovModel> _models = new Dictionary<string, HiddenMarkovModel>();        
        private readonly int _numberOfHiddenStates;                
        private readonly Codebook _codeBook;
        private readonly EngineParameters _engineParameters;

        #endregion

        #region Instance

        public RecognizeEngineDiscreteHmmLearningLocal(int numberOfHiddenStates, EngineParameters parameters, Codebook codebook)
        {
            _numberOfHiddenStates = numberOfHiddenStates;
            _engineParameters = parameters;            
            _codeBook = codebook;
        }

        public RecognizeEngineDiscreteHmmLearningLocal(Codebook codeBook)
            : this(5, EngineParameters.Default, codeBook)
        {

        }

        #endregion

        public void TrainAll(Dictionary<string, IList<SoundSignalReader>> signalsDictionary)
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

            var featureUtility = new FeatureUtility(_engineParameters, null);

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
                        

            var words = signalsDictionary.Keys.ToList();

            for (int wordIndex = 0; wordIndex < featuresInput.Length; wordIndex++) // foreach word
            {
                List<int[]> observables = new List<int[]>();
                var word = words[wordIndex];
                for (var signalIndex = 0; signalIndex < featuresInput[wordIndex].Length; signalIndex++) // foreach word signal
                {
                    var points = featuresInput[wordIndex][signalIndex].Select(item => new Point(item)); // convert feature to points

                    var codeItems = _codeBook.Quantize(points.ToArray());
                    observables.Add(codeItems);
                }
                var hmm = new HiddenMarkovModel(_numberOfHiddenStates, _codeBook.Size);
                hmm.SetTrainSeq(observables.ToArray());
                hmm.Train();
                _models.Add(word, hmm);
            }
           

        }


        public string Recognize(SoundSignalReader signal)
        {

            var featureUtility = new FeatureUtility(_engineParameters, null);
            var features = featureUtility.ExtractFeatures(signal).First();            

            var observations = _codeBook.Quantize(features.Select(item => new Point(item)).ToArray());

            var likelyHoodValue = Double.MinValue;
            string ret = null;

            foreach (var model in _models)
            {
                var val = model.Value.Viterbi(observations);
                if (val > likelyHoodValue)
                {
                    likelyHoodValue = val;
                    ret = model.Key;
                }
            }

            return ret;
        }


    }

   
}
