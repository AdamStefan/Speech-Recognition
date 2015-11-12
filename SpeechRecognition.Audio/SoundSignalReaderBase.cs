using System.Collections.Generic;

namespace SpeechRecognition.Audio
{
    public abstract class SoundSignalReaderBase : ISoundSignalReader
    {
        #region Properties

        public int SampleRate { get; set; }
        public int Channels { get; set; }
        public int Length { get; protected set; }
        public const string VoiceProperty = "Voice";

        #endregion

        #region Fields

        private readonly List<SignalVisitor> _visitors = new List<SignalVisitor>();
        protected readonly HashSet<string> SupportedPropertiesSet = new HashSet<string>();

        #endregion

        #region Methods

        #region Abstract

        protected abstract bool ReadInternal(float[] buffer, int bufferStartIndex, int length,
            Dictionary<string, object> properties);

     
        public abstract void Reset();

        #endregion

        protected virtual bool AcceptVisitor(SignalVisitor visitor)
        {
            _visitors.Add(visitor);
            return true;
        }

        private bool ReadData(float[] buffer, int bufferStartIndex, int length, Dictionary<string, object> properties)
        {            
            if (ReadInternal(buffer, bufferStartIndex, length, properties))
            {                
                foreach (var visitor in _visitors)
                {
                    visitor.Visit(buffer, bufferStartIndex, length, properties);
                }

                return true;
            }

            return false;
        }

        public bool Read(float[] buffer, int bufferStartIndex, int length)
        {            
            return ReadData(buffer, bufferStartIndex, length, null);
        }

        public bool Read(float[] buffer, int length)
        {            
            return ReadData(buffer, 0, length, null);
        }

        public bool Read(float[] buffer, int bufferStartIndex, int length, Dictionary<string, object> properties)
        {
            return ReadData(buffer, 0, length, properties);
        }

        public bool Read(float[] buffer, int length, Dictionary<string, object> properties)
        {
            return ReadData(buffer, 0, length,  properties);
        }

        public bool Accept(SignalVisitor visitor)
        {
            return AcceptVisitor(visitor);
        }

        public IEnumerable<string> SupportedProperties
        {
            get { return SupportedPropertiesSet; }
        }

        #endregion
    }
}