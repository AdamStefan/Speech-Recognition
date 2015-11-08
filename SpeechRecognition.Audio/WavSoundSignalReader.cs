using System.IO;

namespace SpeechRecognition.Audio
{
    public class WavSoundSignalReader : SoundSignalReader
    {
        #region Fields
             
        NAudio.Wave.WaveFileReader _reader;
        private string _file;

        #endregion

        #region Instance

        public WavSoundSignalReader(string file)
        {
            _file = file;
                                          
            NAudio.Wave.WaveFileReader _reader = new NAudio.Wave.WaveFileReader(file);
            SampleRate = _reader.WaveFormat.SampleRate;
            Channels = _reader.WaveFormat.Channels;
            Length = (int)(_reader.SampleCount / Channels);      
        }

        #endregion

        #region Methods

        public override bool Read(float[] buffer, int bufferStartIndex, int length)
        {            
            for (int index = 0; index < length; index++)
            {
                float[] frameData;
                if ((frameData = _reader.ReadNextSampleFrame()) != null)
                {
                    float data = 0.0f;
                    for (int channelIndex = 0; channelIndex < frameData.Length; channelIndex++)
                    {
                        data += frameData[channelIndex];
                    }
                    if (buffer[bufferStartIndex + index] != data)
                    {
                        buffer[bufferStartIndex + index] = data;
                    }
                }
                else
                {
                    return false;
                }
            }
            
            return true;
        }        

        public override bool Read(float[] buffer, int length)
        {
            var ret = Read(buffer, 0, length);            
            return ret;
        }

        public override void Reset()
        {          
            if (_reader!=null)
            {
                _reader.Close();
            }
            _reader = new NAudio.Wave.WaveFileReader(_file);
        }

        #endregion
    }


    
}
