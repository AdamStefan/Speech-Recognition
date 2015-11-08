using System;

namespace SpeechRecognition
{
    public class HammingWindowDef
    {
        public double Alpha { get; private set; }
        public double Beta { get; private set; }


        public HammingWindowDef(double alpha = 0.54, double beta = 0.46)
        {
            Alpha = alpha;
            Beta = beta;
        }


        public class HammingWindow
        {
            private readonly double _phaseHammingPart;
            private readonly HammingWindowDef _definition;

            public HammingWindow(HammingWindowDef definition, int signalLength)
            {
                _phaseHammingPart = 2 * Math.PI / (signalLength - 1);
                _definition = definition;
            }

            public double GetValue(int value)
            {
                return _definition.Alpha - (_definition.Beta * Math.Cos(_phaseHammingPart * value));
            }
        }


        public static double[] ApplyHammingWindow(double[] signal, HammingWindowDef hammingWindowDef = null)
        {
            var ret = new double[signal.Length];
            if (hammingWindowDef == null)
            {
                hammingWindowDef = new HammingWindowDef();
            }
            //var phaseHammingPart = 2 * Math.PI / (signal.Length - 1);
            var hammingWindow = new HammingWindow(hammingWindowDef, signal.Length);

            for (int index = 0; index < signal.Length; index++)
            {
                ret[index] = signal[index] * hammingWindow.GetValue(index);
            }

            return ret;
        }

        public static double[] ApplyHammingWindow(float[] signal, HammingWindowDef hammingWindowDef = null)
        {
            var ret = new double[signal.Length];
            if (hammingWindowDef == null)
            {
                hammingWindowDef = new HammingWindowDef();
            }            
            var hammingWindow = new HammingWindow(hammingWindowDef, signal.Length);

            for (int index = 0; index < signal.Length; index++)
            {
                ret[index] = signal[index] * hammingWindow.GetValue(index);
            }

            return ret;
        }
    }
}
