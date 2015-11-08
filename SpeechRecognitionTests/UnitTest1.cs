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
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var signals = new TestSoundSignalReader[3];
            for (int index = 0; index < 3; index++)
            {
                signals[index] = TestSoundSignalReader.GenerateSignal(5120);
            }

            var recognitionEngine = new RecognitionEngine();
            recognitionEngine.Train(new SoundSignalReader[] { signals[0] }, "a");
            recognitionEngine.Train(new SoundSignalReader[] { signals[1] }, "b");
            recognitionEngine.Train(new SoundSignalReader[] { signals[2] }, "c");

            var value = recognitionEngine.Recognize(signals[1]);

            Assert.AreEqual("b", value);
        }


        [TestMethod]
        public void TestMethodAll()
        {
            Dictionary<string, IList<SoundSignalReader>> learningWordSignals = new Dictionary<string, IList<SoundSignalReader>>();
            Dictionary<string, IList<SoundSignalReader>> testWordSignals = new Dictionary<string, IList<SoundSignalReader>>();

            var learningDirectories = Directory.GetDirectories("Sounds\\Learning");
            var testDirectories = Directory.GetDirectories("Sounds\\test");


            foreach (var directory in learningDirectories.Where(item => !item.Contains("catalog")))
            {
                var word = new DirectoryInfo(directory).Name;
                learningWordSignals.Add(word, new List<SoundSignalReader>());
                var wavFiles = Directory.GetFiles(directory).Select(item => new FileInfo(item)).Where(fItem => fItem.Extension.Contains("wav"));
                foreach (var file in wavFiles)
                {
                    learningWordSignals[word].Add(new WavSoundSignalReader(file.FullName));
                }
            }

            foreach (var directory in testDirectories)
            {
                var word = new DirectoryInfo(directory).Name;
                testWordSignals.Add(word, new List<SoundSignalReader>());
                var wavFiles = Directory.GetFiles(directory).Select(item => new FileInfo(item)).Where(fItem => fItem.Extension.Contains("wav"));
                foreach (var file in wavFiles)
                {
                    testWordSignals[word].Add(new WavSoundSignalReader(file.FullName));
                }
            }

            var recognitionEngine = new RecognitionEngine();

            foreach (var word in learningWordSignals)
            {
                recognitionEngine.Train(word.Value, word.Key);
            }

            foreach (var word in testWordSignals)
            {
                foreach (var signal in word.Value)
                {
                    var value = recognitionEngine.Recognize(signal);
                }
            }

        }

        [TestMethod]
        public void TestFeatureUtility()
        {

            var arraySoundSignalReader = new ArraySignalReader(new float[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10});
            var filteredSignal = new PreemphasisFilter(arraySoundSignalReader, 0.95f);

            FeatureUtility fu = new FeatureUtility();
            float[] frame1;
            float[] frame2;
            float[] frame3;
            fu.Read(filteredSignal, 5, 2, out frame1);
            fu.Read(filteredSignal, 5, 2, out frame2);
            fu.Read(filteredSignal, 5, 2, out frame3);


            var arraySoundSignalReader1 = new ArraySignalReader(new float[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10});
            var filteredSignal1 = new PreemphasisFilter(arraySoundSignalReader1, 0.95f);

            float[] frame4 = new float[5];
            float[] frame5 = new float[5];
            //float[] frame6 = new float[5];
            
            filteredSignal1.Read(frame4, 0, 5);
            filteredSignal1.Read(frame5, 0, 5);
        }

        [TestMethod]
        public void TestMethod4()
        {
            Dictionary<string, IList<SoundSignalReader>> learningWordSignals = new Dictionary<string, IList<SoundSignalReader>>();
            Dictionary<string, IList<SoundSignalReader>> testWordSignals = new Dictionary<string, IList<SoundSignalReader>>();

            var learningDirectories = Directory.GetDirectories("Sounds\\Learning");
            var testDirectories = Directory.GetDirectories("Sounds\\test");


            foreach (var directory in learningDirectories.Where(item => !item.Contains("catalog")))
            {
                var word = new DirectoryInfo(directory).Name;
                learningWordSignals.Add(word, new List<SoundSignalReader>());
                var wavFiles = Directory.GetFiles(directory).Select(item => new FileInfo(item)).Where(fItem => fItem.Extension.Contains("wav"));
                foreach (var file in wavFiles)
                {
                    learningWordSignals[word].Add(new WavSoundSignalReader(file.FullName));
                }
            }

            foreach (var directory in testDirectories)
            {
                var word = new DirectoryInfo(directory).Name;
                testWordSignals.Add(word, new List<SoundSignalReader>());
                var wavFiles = Directory.GetFiles(directory).Select(item => new FileInfo(item)).Where(fItem => fItem.Extension.Contains("wav"));
                foreach (var file in wavFiles)
                {
                    testWordSignals[word].Add(new WavSoundSignalReader(file.FullName));
                }
            }

            var recognitionEngine = new RecognitionEngine();

            recognitionEngine.TrainAllSync(learningWordSignals);


            foreach (var word in testWordSignals)
            {
                foreach (var signal in word.Value)
                {
                    var value = recognitionEngine.Recognize(signal);
                }
            }

        }


        [TestMethod]
        public void TestMethodDiscrete()
        {
            Dictionary<string, IList<SoundSignalReader>> learningWordSignals = new Dictionary<string, IList<SoundSignalReader>>();
            Dictionary<string, IList<SoundSignalReader>> testWordSignals = new Dictionary<string, IList<SoundSignalReader>>();

            var learningDirectories = System.IO.Directory.GetDirectories("Sounds\\Learning");
            var testDirectories = System.IO.Directory.GetDirectories("Sounds\\test");

            //"C:\Personal\Speech Recognition\Speech Recognition\SpeechRecognitionTests\bin\Debug\Sounds\learning"
            //"C:\Personal\Speech Recognition\Speech Recognition\SpeechRecognitionTests\bin\Debug\Sounds\test"

            //var learningDirectories = Directory.GetDirectories(@"C:\Personal\Speech Recognition\Speech Recognition\SpeechRecognitionTests\bin\Debug\Sounds\learning");
            //var testDirectories = Directory.GetDirectories(@"C:\Personal\Speech Recognition\Speech Recognition\SpeechRecognitionTests\bin\Debug\Sounds\test");
            


            foreach (var directory in learningDirectories.Where(item => !item.Contains("catalog")))
            {
                var word = new DirectoryInfo(directory).Name;
                learningWordSignals.Add(word, new List<SoundSignalReader>());
                var wavFiles = Directory.GetFiles(directory).Select(item => new FileInfo(item)).Where(fItem => fItem.Extension.Contains("wav"));
                foreach (var file in wavFiles)
                {
                    learningWordSignals[word].Add(new WavSoundSignalReader(file.FullName));
                }
            }

            foreach (var directory in testDirectories)
            {
                var word = new DirectoryInfo(directory).Name;
                testWordSignals.Add(word, new List<SoundSignalReader>());
                var wavFiles = Directory.GetFiles(directory).Select(item => new FileInfo(item)).Where(fItem => fItem.Extension.Contains("wav"));
                foreach (var file in wavFiles)
                {
                    testWordSignals[word].Add(new WavSoundSignalReader(file.FullName));
                }
            }

            //var catalogSignals = learningWordSignals.SelectMany(item=>item.Value).ToList();            

            var catalogSignals = new List<SoundSignalReader>();
            catalogSignals.AddRange(learningWordSignals.SelectMany(item => item.Value));            
            
            var codeBook = CodeBookFactory.FromWaves(catalogSignals, EngineParameters.Default);

            var recognitionEngine = new RecognizeEngineDiscreteHmmLearning(codeBook);
            var result = recognitionEngine.TrainAll(learningWordSignals);

            //var recognitionEngine = new RecognizeEngineDiscreteHMMLearning2(codeBook);
            //recognitionEngine.TrainAll(learningWordSignals);


            foreach (var word in testWordSignals)
            {
                foreach (var signal in word.Value)
                {
                    //var value = recognitionEngine.Recognize(signal);
                    string name;
                    var value = recognitionEngine.Recognize(signal, result.Hmm, out name);
                    Assert.AreEqual(word.Key, name);
                    if (name != word.Key)
                    {
                        
                    }
                    else
                    {
                        
                    }                    
                }
            }

        }


        [TestMethod]
        public void TestMethodDiscrete2()
        {
            Dictionary<string, IList<SoundSignalReader>> learningWordSignals = new Dictionary<string, IList<SoundSignalReader>>();
            Dictionary<string, IList<SoundSignalReader>> testWordSignals = new Dictionary<string, IList<SoundSignalReader>>();

            var learningDirectories = Directory.GetDirectories("Sounds\\Learning");
            var testDirectories = Directory.GetDirectories("Sounds\\test");



            foreach (var directory in learningDirectories.Where(item => !item.Contains("catalog")))
            {
                var word = new DirectoryInfo(directory).Name;
                learningWordSignals.Add(word, new List<SoundSignalReader>());
                var wavFiles = Directory.GetFiles(directory).Select(item => new FileInfo(item)).Where(fItem => fItem.Extension.Contains("wav"));
                foreach (var file in wavFiles)
                {
                    learningWordSignals[word].Add(new WavSoundSignalReader(file.FullName));
                }
            }

            foreach (var directory in testDirectories)
            {
                var word = new DirectoryInfo(directory).Name;
                testWordSignals.Add(word, new List<SoundSignalReader>());
                var wavFiles = Directory.GetFiles(directory).Select(item => new FileInfo(item)).Where(fItem => fItem.Extension.Contains("wav"));
                foreach (var file in wavFiles)
                {
                    testWordSignals[word].Add(new WavSoundSignalReader(file.FullName));
                }
            }

            //var catalogSignals = learningWordSignals.SelectMany(item=>item.Value).ToList();            

            var catalogSignals = new List<SoundSignalReader>();
            catalogSignals.AddRange(learningWordSignals.SelectMany(item => item.Value));

            var codeBook = CodeBookFactory.FromWaves(catalogSignals, EngineParameters.Default);

            //var recognitionEngine = new RecognizeEngineDiscreteHMMLearning(codeBook);
            //var result = recognitionEngine.TrainAll(learningWordSignals);

            var recognitionEngine = new RecognizeEngineDiscreteHmmLearningLocal(codeBook);
            recognitionEngine.TrainAll(learningWordSignals);


            foreach (var word in testWordSignals)
            {
                foreach (var signal in word.Value)
                {
                    var value = recognitionEngine.Recognize(signal);
                    //var test = recognitionEngine.Recognize(learningWordSignals.Values.ToList()[3][0], result.HMM, result.Catalog);
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
                ret[index] = hammingWindow.GetValue(index);
            }
        }

        [TestMethod]
        public void TestVoiceActivationDetection()
        {
            Dictionary<string, IList<SoundSignalReader>> learningWordSignals = new Dictionary<string, IList<SoundSignalReader>>();
            Dictionary<string, IList<SoundSignalReader>> testWordSignals = new Dictionary<string, IList<SoundSignalReader>>();

            var learningDirectories = Directory.GetDirectories("Sounds\\Learning");
            var testDirectories = Directory.GetDirectories("Sounds\\test");


            foreach (var directory in learningDirectories.Where(item => !item.Contains("catalog")))
            {
                var word = new DirectoryInfo(directory).Name;
                learningWordSignals.Add(word, new List<SoundSignalReader>());
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
            var ret = LinearPredictiveCoding.LevinsonDurbin(data, 1);
            var ret1 = LinearPredictiveCoding.Lpc(data, 1);
        }

        [TestMethod]
        public void TestFFT()
        {
            var data = new double[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
            FourierTransform ft = new FourierTransform();
            ft.computeFFT(data, 16);
            double energy;
            var result = ft.GetMagnitudeSquared(out energy);

            var dct = new DiscreteCosinusTransform(data.Length);
            var ret = dct.PerformDCT(data);
        }


    }
}
