using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Accord.Statistics.Models.Markov;
using SpeechRecognition.VectorQuantization;

namespace SpeechRecognition
{
    public class TrainResult
    {
        public HiddenMarkovClassifier Hmm { get; set; }
        public Codebook Catalog { get; set; }

        public void Save(string folder, string name)
        {
            var classifierPath = Path.Combine(folder, name + "hmm.dat");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            Hmm.Save(classifierPath);

            var codebookPath = Path.Combine(folder, name + "codebook.dat");
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