using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpeechRecognition;
using SpeechRecognition.Audio;
using SpeechRecognition.FeaturesProvider.LPC;
using SpeechRecognition.FeaturesProvider.MelFrequencySpectrum;

namespace SpeechRecognitionTests
{
    [TestClass]
    public class EngineTests
    {            

        [TestMethod]
        public void TestFeatureUtility()
        {

            var arraySoundSignalReader = new ArraySignalReader(new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
            var filteredSignal = new PreemphasisFilter(arraySoundSignalReader, 0.95f);

            VoiceActivitySignalReader fu = new VoiceActivitySignalReader(filteredSignal, 5);
            float[] frame1;
            float[] frame2;
            float[] frame3;
            bool isVoice;
            fu.Read( 5, 2, out frame1,out isVoice);
            fu.Read(5, 2, out frame2, out isVoice);
            fu.Read(5, 2, out frame3, out isVoice);


            var arraySoundSignalReader1 = new ArraySignalReader(new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
            var filteredSignal1 = new PreemphasisFilter(arraySoundSignalReader1, 0.95f);

            float[] frame4 = new float[5];
            float[] frame5 = new float[5];
            //float[] frame6 = new float[5];

            filteredSignal1.Read(frame4, 0, 5);
            filteredSignal1.Read(frame5, 0, 5);
        }                


        [TestMethod]
        public void TestMethodDiscrete()
        {
            Dictionary<string, IList<ISoundSignalReader>> learningWordSignals = new Dictionary<string, IList<ISoundSignalReader>>();
            Dictionary<string, IList<ISoundSignalReader>> testWordSignals = new Dictionary<string, IList<ISoundSignalReader>>();

            var learningDirectories = Directory.GetDirectories("Sounds\\Learning");
            var testDirectories = Directory.GetDirectories("Sounds\\test");


            foreach (var directory in learningDirectories.Where(item => !item.Contains("catalog")))
            {
                var word = new DirectoryInfo(directory).Name;
                learningWordSignals.Add(word, new List<ISoundSignalReader>());
                var wavFiles = Directory.GetFiles(directory).Select(item => new FileInfo(item)).Where(fItem => fItem.Extension.Contains("wav"));
                foreach (var file in wavFiles)
                {
                    learningWordSignals[word].Add(new WavSoundSignalReader(file.FullName));
                }
            }

            foreach (var directory in testDirectories)
            {
                var word = new DirectoryInfo(directory).Name;
                testWordSignals.Add(word, new List<ISoundSignalReader>());
                var wavFiles = Directory.GetFiles(directory).Select(item => new FileInfo(item)).Where(fItem => fItem.Extension.Contains("wav"));
                foreach (var file in wavFiles)
                {
                    testWordSignals[word].Add(new WavSoundSignalReader(file.FullName));
                }
            }            

            var catalogSignals = new List<ISoundSignalReader>();
            catalogSignals.AddRange(learningWordSignals.SelectMany(item => item.Value));

            var codeBook = CodeBookFactory.FromWaves(catalogSignals, EngineParameters.Default);

            var recognitionEngine = new RecognizeEngineDiscreteHmmLearning(codeBook);
            var result = recognitionEngine.TrainAll(learningWordSignals);
           

            foreach (var word in testWordSignals)
            {
                foreach (var signal in word.Value)
                {                    
                    string name;
                    var value = recognitionEngine.Recognize(signal, result.Models, out name);
                    Assert.AreEqual(word.Key, name);                   
                }
            }

        }



        [TestMethod]
        public void TestDetectionEngine()
        {
            Dictionary<string, IList<ISoundSignalReader>> learningWordSignals = new Dictionary<string, IList<ISoundSignalReader>>();
            Dictionary<string, IList<ISoundSignalReader>> testWordSignals = new Dictionary<string, IList<ISoundSignalReader>>();

            var learningDirectories = Directory.GetDirectories("Sounds\\Learning");
            var testDirectories = Directory.GetDirectories("Sounds\\test");

            foreach (var directory in learningDirectories.Where(item => !item.Contains("catalog")))
            {
                var word = new DirectoryInfo(directory).Name;
                learningWordSignals.Add(word, new List<ISoundSignalReader>());
                var wavFiles = Directory.GetFiles(directory).Select(item => new FileInfo(item)).Where(fItem => fItem.Extension.Contains("wav"));
                foreach (var file in wavFiles)
                {
                    learningWordSignals[word].Add(new WavSoundSignalReader(file.FullName));
                }
            }

            foreach (var directory in testDirectories)
            {
                var word = new DirectoryInfo(directory).Name;
                testWordSignals.Add(word, new List<ISoundSignalReader>());
                var wavFiles = Directory.GetFiles(directory).Select(item => new FileInfo(item)).Where(fItem => fItem.Extension.Contains("wav"));
                foreach (var file in wavFiles)
                {
                    testWordSignals[word].Add(new WavSoundSignalReader(file.FullName));
                }
            }

            var catalogSignals = new List<ISoundSignalReader>();
            catalogSignals.AddRange(learningWordSignals.SelectMany(item => item.Value));

            var codeBook = CodeBookFactory.FromWaves(catalogSignals, EngineParameters.Default);

            var recognitionEngine = new DetectionEngine(codeBook);
            var result = recognitionEngine.Train(learningWordSignals);


            foreach (var word in testWordSignals)
            {
                foreach (var signal in word.Value)
                {                    
                    string name;
                    var value = recognitionEngine.Recognize(signal, out name);                    
                    Assert.AreEqual(word.Key, name);
                    
                }
            }
        }



        [TestMethod]
        public void TestFilterBankComposion()
        {

            var filterBanks = Mfcc.ComputeMelFilterBank(512, 300, 8000, 16000, 12);

            var ret = new int[] { 9, 16, 25, 35, 47, 63, 81, 104, 132, 165, 206, 256 };

            for (int index = 0; index < 12; index++)
            {
                Assert.AreEqual(ret[index], filterBanks[index]);
            }

        }

        [TestMethod]
        public void TestMelFilter()
        {
            var fBins = new int[] { 9, 16, 25, 35, 47, 63, 81, 104, 132, 165, 206, 256 };
            var signals = new double[512];

            for (int index = 0; index < signals.Length; index++)
            {
                signals[index] = index;
            }

            var ret = Mfcc.ApplyFilterbankFilter(signals, fBins);
        }


        [TestMethod]
        public void TestPreFiltering()
        {
            var soundSignal = new TestSoundSignalReader(new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });

            var preemphasisFilter = new PreemphasisFilter(soundSignal, 0.95f);
            var buffer = new float[10];
            var items = preemphasisFilter.Read(buffer, 10);
            var expected = new[] { 1.0000f, 1.0500f, 1.1000f, 1.1500f, 1.2000f, 1.2500f, 1.3000f, 1.3500f, 1.4000f, 1.45f };

            for (var index = 0; index < buffer.Length; index++)
            {
                var delta = Math.Abs(expected[index] - buffer[index]);

                Assert.IsTrue(0.01 > delta);
            }

            preemphasisFilter = new PreemphasisFilter(soundSignal, 0.95f);
            buffer = new float[3];
            var startIndex = 0;
            while (preemphasisFilter.Read(buffer, startIndex, 3))
            {
                startIndex += 3;
            }


        }


        [TestMethod]
        public void TestHamming()
        {
            var hamming = new HammingWindowDef();
            var hammingWindow = new HammingWindowDef.HammingWindow(hamming, 5);
            var ret = new double[5];
            for (var index = 0; index < 5; index++)
            {
                ret[index] = hammingWindow[index];
            }
        }

        [TestMethod]
        public void TestVoiceActivationDetection()
        {
            Dictionary<string, IList<ISoundSignalReader>> learningWordSignals = new Dictionary<string, IList<ISoundSignalReader>>();
            Dictionary<string, IList<ISoundSignalReader>> testWordSignals = new Dictionary<string, IList<ISoundSignalReader>>();

            var learningDirectories = Directory.GetDirectories("Sounds\\Learning");
            var testDirectories = Directory.GetDirectories("Sounds\\test");


            foreach (var directory in learningDirectories.Where(item => !item.Contains("catalog")))
            {
                var word = new DirectoryInfo(directory).Name;
                learningWordSignals.Add(word, new List<ISoundSignalReader>());
                var wavFiles = Directory.GetFiles(directory).Select(item => new FileInfo(item)).Where(fItem => fItem.Extension.Contains("wav"));
                foreach (var file in wavFiles)
                {
                    learningWordSignals[word].Add(new WavSoundSignalReader(file.FullName));
                }
            }


        }


        [TestMethod]
        public void TestLevinsonDurbin()
        {
            var data = new[] { 1.0000, -0.7600, 0.1400 };
            double bo, energy;
            var ret = LinearPredictiveCoding.LevinsonDurbin(data, 1, out bo);
            var ret1 = LinearPredictiveCoding.Lpc(data, 1, out bo,out energy);
        }

        [TestMethod]
        public void TestFft()
        {
            var data = new double[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
            FourierTransform ft = new FourierTransform();
            ft.ComputeFft(data, 16);
            double energy;
            var result = ft.GetMagnitudeSquared(1, out energy);

            var dct = new DiscreteCosinusTransform(data.Length);
            var ret = dct.PerformDct(data);
        }


    }
}
