using System.Collections.Generic;
using SpeechRecognition.Audio;

namespace SpeechRecognition
{
    public class PreemphasisFilter : SoundSignalReaderBase
    {
        #region Fields

        private readonly ISoundSignalReader _signalReader;
        private readonly float _filterRatio;        
        private float? _lastFrameValue;        

        #endregion

        #region Instance

        public PreemphasisFilter(ISoundSignalReader signalReader, float filterRatio)
        {
            _signalReader = signalReader;
            _filterRatio = filterRatio;
            Length = signalReader.Length;            
        }

        #endregion

        protected override bool ReadInternal(float[] buffer, int bufferStartIndex, int length,
             Dictionary<string, object> properties)
        {            
            var bufferOffSetIndex = 0;
            bool start = true;
            float[] tmpbuffer;
            var startIndex = 0;
            var itemsLength = length + bufferOffSetIndex;
            if (_lastFrameValue.HasValue)
            {
                start = false;
                bufferOffSetIndex = 1;
                tmpbuffer = new float[length + bufferOffSetIndex];
                tmpbuffer[0] = _lastFrameValue.Value;
                startIndex = 1;
            }
            else
            {
                tmpbuffer = new float[length + bufferOffSetIndex];
            }

            var ret = _signalReader.Read(tmpbuffer, startIndex, itemsLength);           

            _lastFrameValue = tmpbuffer[tmpbuffer.Length - 1];

            for (int index = 1; index < tmpbuffer.Length; index++)
            {
                buffer[bufferStartIndex + index - bufferOffSetIndex] = tmpbuffer[index] -
                                                                       _filterRatio*tmpbuffer[index - 1];
            }
            if (start)
            {
                buffer[0] = tmpbuffer[0];
            }

            return ret;
        }

        public override void Reset()
        {
            _lastFrameValue = null;            
            _signalReader.Reset();
        }
    }
}
