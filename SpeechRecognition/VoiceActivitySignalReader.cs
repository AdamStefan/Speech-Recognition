using System.Collections.Generic;
using SpeechRecognition.Audio;

namespace SpeechRecognition
{
    public class VoiceActivitySignalReader : SoundSignalReaderBase
    {
        #region Fields

        private readonly VoiceActivityDetection _voiceActivityDetection;
        private readonly ISoundSignalReader _soundSignal;
        private readonly Dictionary<string, object> _properties;
        private float[] _nextFrame;

        #endregion

        #region Instance

        public VoiceActivitySignalReader(ISoundSignalReader signal, int frameSize, int emptyFrames = 3,
            VoiceActivationDetectionEnhancement enhancements = VoiceActivationDetectionEnhancement.All)
        {
            _voiceActivityDetection = new VoiceActivityDetection(signal, frameSize, emptyFrames, enhancements);
            signal.Reset();
            _soundSignal = signal;
            _properties = new Dictionary<string, object> {{VoiceProperty, false}};
            SupportedPropertiesSet.Add(VoiceProperty);
        }

        #endregion

        #region Override Methods

        protected override bool ReadInternal(float[] buffer, int bufferStartIndex, int length,
            Dictionary<string, object> properties)
        {
            if (_soundSignal.Read(buffer, bufferStartIndex, length))
            {
                if (_voiceActivityDetection.IsVoice(buffer) && properties != null)
                {
                    properties[VoiceProperty] = true;
                }
                else if (properties != null)
                {
                    properties[VoiceProperty] = false;
                }
                return true;
            }
            return false;
        }

        public override void Reset()
        {
            _soundSignal.Reset();
        }


        public bool Read(int frameSize, int stepSize, out float[] frame, out bool isVoice)
        {
            isVoice = false;
            int numberOfBitsToRead = frameSize;
            int bufferStartIndex = 0;
            if (_nextFrame == null)
            {
                _nextFrame = new float[frameSize];
                frame = new float[frameSize];
            }
            else
            {
                frame = _nextFrame;
                _nextFrame = new float[frameSize];
                bufferStartIndex = frameSize - stepSize;
                numberOfBitsToRead = stepSize;
            }

            var ret = Read(frame, bufferStartIndex, numberOfBitsToRead, _properties);
            if (ret)
            {
                isVoice = (bool) _properties[VoiceProperty];
            }

            for (int index = stepSize; index < frameSize; index++)
            {
                _nextFrame[index - stepSize] = frame[index];
            }

            return ret;
        }

        #endregion
    }
}