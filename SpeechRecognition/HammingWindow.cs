using System;

namespace SpeechRecognition
{
    public class HammingWindowDef
    {
        #region Properties


        public double Alpha { get; private set; }
        public double Beta { get; private set; }


        public HammingWindowDef(double alpha = 0.54, double beta = 0.46)
        {
            Alpha = alpha;
            Beta = beta;
        }

        #endregion


        public class HammingWindow
        {
            #region Fields

            private readonly double[] _values;

            #endregion

            #region Instance

            public HammingWindow(HammingWindowDef definition, int signalLength)
            {
                var phaseHammingPart = 2 * Math.PI / (signalLength - 1);
                var definition1 = definition;

                _values = new double[signalLength];

                for (int index = 0; index < signalLength; index++)
                {
                    _values[index] = definition1.Alpha - (definition1.Beta * Math.Cos(phaseHammingPart * index));
                }
            }

            #endregion

            #region Properties

            public double this[int index]
            {
                get
                {
                    return _values[index];
                }
            }

            #endregion

            #region Methods

            public double[] Apply(double[] signal)
            {
                var ret = new double[signal.Length];
                for (int index = 0; index < signal.Length; index++)
                {
                    ret[index] = signal[index] * this[index];
                }

                return ret;
            }

            public double[] Apply(float[] signal)
            {
                var ret = new double[signal.Length];
                for (int index = 0; index < signal.Length; index++)
                {
                    ret[index] = signal[index] * this[index];
                }

                return ret;
            }

            #endregion
        }

        public static double[] ApplyHammingWindow(double[] signal, HammingWindowDef hammingWindowDef = null)
        {
            if (hammingWindowDef == null)
            {
                hammingWindowDef = new HammingWindowDef();
            }

            var hammingWindow = new HammingWindow(hammingWindowDef, signal.Length);

            return hammingWindow.Apply(signal);
        }

        public static double[] ApplyHammingWindow(float[] signal, HammingWindowDef hammingWindowDef = null)
        {
            if (hammingWindowDef == null)
            {
                hammingWindowDef = new HammingWindowDef();
            }
            var hammingWindow = new HammingWindow(hammingWindowDef, signal.Length);

            return hammingWindow.Apply(signal);
        }
    }
}
