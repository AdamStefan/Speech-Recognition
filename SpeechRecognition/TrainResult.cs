using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Accord.Statistics.Models.Markov;
using SpeechRecognition.VectorQuantization;

namespace SpeechRecognition
{
    [Serializable]
    public class TrainResult
    {
        public HiddenMarkovModel[] Models { get; set; }

        public Codebook Catalog { get; set; }

        public void Save(string folder, string name)
        {
            var fileName = Path.Combine(folder, name + ".dat");
            using (FileStream fileStream = new FileStream(fileName, FileMode.Create))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(fileStream, this);
            }

        }

        public static TrainResult Load(string fileName)
        {
            TrainResult result;
            using (FileStream fileStream = new FileStream(fileName, FileMode.Open))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                result = (TrainResult)binaryFormatter.Deserialize(fileStream);
            }
            return result;
        }

        public static TrainResult Load(string folder, string name)
        {
            var fileName = Path.Combine(folder, name + ".dat");
            return Load(fileName);
        }

    }
}