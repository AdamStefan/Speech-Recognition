using System.Collections.Generic;
using System.Linq;
using SpeechRecognition.Audio;
using SpeechRecognition.VectorQuantization;

namespace SpeechRecognition
{
    public class CodeBookFactory
    {
        public static Codebook FromWaves(IList<ISoundSignalReader> sounds, EngineParameters parameters, int codeBookSize = 256)
        {
            var featureUtility = new FeatureUtility(parameters);
            var features = new List<double[][]>();
            foreach (var signal in sounds)
            {
                signal.Reset();
                var items = featureUtility.ExtractFeatures(signal).Select(item => item.ToArray());
                features.AddRange(items);
            }


            var codeBook = new Codebook(features.SelectMany(item => item).Select(item => new Point(item)).ToArray(), codeBookSize);

            return codeBook;
        }
    }
}
