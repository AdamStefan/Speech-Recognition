using System;
using System.Collections.Generic;
using SpeechRecognition.Audio;

namespace SpeechRecognitionTests
{
    public class TestSoundSignalReader : SoundSignalReaderBase
    {
        private readonly float[] _signal;
        private static readonly Random Random = new Random(123123);
        private int _position;

        #region Instance

        public TestSoundSignalReader(float[] arraySound)
        {
            _signal = arraySound;
            Channels = 1;
            SampleRate = 16000;
        }

        #endregion

        protected override bool ReadInternal(float[] buffer, int bufferStartIndex, int length, Dictionary<string, object> properties)
        {            
            var ret = Read(buffer, _position, bufferStartIndex, length);
            _position += length;

            return ret;
        }

        public  bool Read(float[] buffer, int position, int bufferStartIndex, int length)
        {
            var ret = true;
            var numberOfItems = length;
            if (position + length >= _signal.Length)
            {
                numberOfItems = _signal.Length - position;
                ret = false;
            }
            for (int index = 0; index < numberOfItems; index++)
            {
                buffer[bufferStartIndex + index] = _signal[position + index];
            }

            return ret;
        }

        public static TestSoundSignalReader GenerateSignal(int length)
        {
            var seed = Random.Next();
            var random = new Random(seed);
            var ret = new float[length];
            for (int index = 0; index < length; index++)
            {
                var floatBytes = new Byte[4];
                random.NextBytes(floatBytes);
                var value = BitConverter.ToSingle(floatBytes, 0);
                if (float.IsNaN(value))
                {
                    value = 0;
                }

                else if (value > 10000000)
                {
                    value = 10000000;
                }

                else if (value < -10000000)
                {
                    value = -10000000;
                }


                ret[index] = value;

            }

            return new TestSoundSignalReader(ret);
        }
       
        public override void Reset()
        {
            _position = 0;
        }
    }

    
}
