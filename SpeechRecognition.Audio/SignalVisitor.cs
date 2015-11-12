using System.Collections.Generic;

namespace SpeechRecognition.Audio
{
    public abstract class SignalVisitor
    {
        public abstract void Visit(float[] data, int startIndex, int length,
            Dictionary<string, object> properties);
    }
}
