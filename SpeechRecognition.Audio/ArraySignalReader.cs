namespace SpeechRecognition.Audio
{

    public class ArraySignalReader : SoundSignalReader
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

        public override bool Read(float[] buffer, int bufferStartIndex, int length)
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

        public override bool Read(float[] buffer, int length)
        {
            var ret = Read(buffer, _position, 0, length);
            _position += length;
            return ret;
        }

        #endregion
    }
}
