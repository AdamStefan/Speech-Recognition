using System.Collections.Generic;
using System.Linq;
using NAudio.Wave;

namespace SpeechRecognition.Audio
{
    public class WavSoundSignalReader : SoundSignalReaderBase
    {
        #region Fields

        private WaveFileReader _reader;
        private readonly string _file;

        #endregion

        #region Instance

        public WavSoundSignalReader(string file)
        {
            _file = file;
            _reader = new WaveFileReader(file);
            SampleRate = _reader.WaveFormat.SampleRate;
            Channels = _reader.WaveFormat.Channels;
            Length = (int) (_reader.SampleCount/Channels);
        }

        #endregion

        #region Methods

        protected override bool ReadInternal(float[] buffer, int bufferStartIndex, int length,
             Dictionary<string, object> properties)
        {            
            for (int index = 0; index < length; index++)
            {
                float[] frameData;
                if ((frameData = _reader.ReadNextSampleFrame()) != null)
                {
                    float data = frameData.Sum();
                    buffer[bufferStartIndex + index] = data/frameData.Length;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        public override void Reset()
        {
            if (_reader != null)
            {
                _reader.Close();
            }
            _reader = new WaveFileReader(_file);
        }

        #endregion
    }
}
