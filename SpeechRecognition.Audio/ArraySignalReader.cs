using System.Collections.Generic;

namespace SpeechRecognition.Audio
{

    public class ArraySignalReader : SoundSignalReaderBase
    {
        #region Fields

        private readonly float[] _signal;
        private int _position;

        #endregion

        #region Instance

        public ArraySignalReader(float[] arraySound)
        {
            _signal = arraySound;
            Channels = 1;
            SampleRate = 16000;
        }

        #endregion

        #region Methods

        protected override bool ReadInternal(float[] buffer, int bufferStartIndex, int length,
             Dictionary<string, object> properties)
        {            
            var ret = Read(buffer, _position, bufferStartIndex, length);
            _position += length;
            return ret;
        }


        public bool Read(float[] buffer, int position, int bufferStartIndex, int length)
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


        public override void Reset()
        {
            _position = 0;
        }


        #endregion
    }
}
