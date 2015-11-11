using System.Collections.Generic;
using System.Threading;
using NAudio.Wave;

namespace SpeechRecognition.Audio
{
    public class MicrophoneSoundSignalReader : SoundSignalReader
    {
        #region Fields

        private readonly WaveIn _waveIn;
        private readonly Queue<float> _buffer;
        private const int QueueLength = 10000;
        private bool _isRecording;

        #endregion

        #region Instance

        public MicrophoneSoundSignalReader(int volume = 100)
        {
            _waveIn = new WaveIn();
            _buffer = new Queue<float>();
            _waveIn.DataAvailable += WaveInDataAvailable;
            _waveIn.WaveFormat = new WaveFormat();
            SampleRate = _waveIn.WaveFormat.SampleRate;
            Channels = _waveIn.WaveFormat.Channels;

            Recorder.TrySetVolumeControl(_waveIn.GetMixerLine(), volume);
        }


        #endregion

        #region Methods        

        void WaveInDataAvailable(object sender, WaveInEventArgs e)
        {
            lock (_buffer)
            {
                var step = Channels * 2;// foreach channel there is a sample of 16 bits
                for (int i = 0; i < e.BytesRecorded; i += step)
                {
                    float sample32 = 0.0f;
                    for (var channelIndex = 0; channelIndex < Channels; channelIndex++)
                    {
                        var j = channelIndex * 2;
                        var sample = (short)((e.Buffer[i + j + 1] << 8) | e.Buffer[i + j + 0]);
                        sample32 += sample / 32768f;
                    }

                    _buffer.Enqueue(sample32 / Channels);

                    if (_buffer.Count > 3 * QueueLength)
                    {
                        while (_buffer.Count > QueueLength)
                        {
                            _buffer.Dequeue();
                        }
                    }
                }
            }
        }

        public override bool Read(float[] buffer, int bufferStartIndex, int length)
        {
            while (_isRecording)
            {
                lock (_buffer)
                {
                    if (_isRecording && _buffer.Count < length)
                    {
                    }
                    else
                    {
                        var currentBuferLength = _buffer.Count;
                        var itemsToLoad = currentBuferLength > length ? length : currentBuferLength;

                        for (int index = 0; index < itemsToLoad; index++)
                        {
                            buffer[bufferStartIndex + index] = _buffer.Dequeue();
                        }
                        break;
                    }
                }
                Thread.Sleep(100);
            }

            return _isRecording;
        }

        public void Close()
        {
            _waveIn.StopRecording();
            _waveIn.DataAvailable -= WaveInDataAvailable;
            _waveIn.Dispose();
            _isRecording = false;
        }

        public void Start()
        {
            _waveIn.StartRecording();
            _isRecording = true;
        }

        public override bool Read(float[] buffer, int length)
        {
            return Read(buffer, 0, length);
        }

        public override void Reset()
        {
        }

        #endregion
    }
}
