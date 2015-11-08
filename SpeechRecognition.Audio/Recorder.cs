using NAudio.Mixer;
using NAudio.Wave;

namespace SpeechRecognition.Audio
{
    public class Recorder
    {

        #region Fields

        WaveIn _waveIn;
        WaveFileWriter _writer;

        #endregion

        #region Instance

        public void Record(string fileName, int volume = 100)
        {
            _waveIn = new WaveIn {WaveFormat = new WaveFormat()};
            _writer = new WaveFileWriter(fileName, _waveIn.WaveFormat);

            TrySetVolumeControl(_waveIn.GetMixerLine(), volume);            

            _waveIn.DataAvailable += new_dataAvailable;
            _waveIn.StartRecording();
        }

        #endregion

        #region Methods

        private void new_dataAvailable(object sender, WaveInEventArgs e)
        {
            for (int i = 0; i < e.BytesRecorded; i += 4)
            {
                short leftSample = (short)((e.Buffer[i + 1] << 8) | e.Buffer[i + 0]);
                float leftSample32 = leftSample / 32768f;

                short rightSample = (short)((e.Buffer[i + 3] << 8) | e.Buffer[i + 2]);
                float rightSample32 = rightSample / 32768f;

                _writer.WriteSample(leftSample32);
                _writer.WriteSample(rightSample32);

            }
        }

        public void Stop()
        {
            if (_waveIn != null)
            {
                _waveIn.StopRecording();
                _waveIn.Dispose();
                _waveIn = null;
                _writer.Close();
                _writer = null;
            }
        }



        private static void TrySetVolumeControl(MixerLine mixerLine, int value)
        {            
            foreach (var control in mixerLine.Controls)
            {
                if (control.ControlType == MixerControlType.Volume)
                {
                    var volumeControl = control as UnsignedMixerControl;

                    if (volumeControl != null) volumeControl.Percent = value;
                    break;
                }
            }
        }


        #endregion

    }

}
