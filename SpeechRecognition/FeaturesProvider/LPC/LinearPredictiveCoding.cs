using System;

namespace SpeechRecognition.FeaturesProvider.LPC
{
    public class LinearPredictiveCoding
    {
        #region  Methods

        public static double[] Apply(float[] frame, int order = 80, int lpcOrder = 12)
        {
            var windowedSample = HammingWindowDef.ApplyHammingWindow(frame); // 1. windowing

            var lpc = Lpc(windowedSample, lpcOrder);
            var p = lpc.Length - 1;
            var ret = new double[order];

            for (int m = 1; m <= order; m++)
            {
                if (m <= p)
                {
                    ret[m - 1] = lpc[m - 1];
                    for (var k = 1; k <= m - 1; k++)
                    {
                        ret[m - 1] += k / ((double)(m)) * ret[k - 1] * lpc[m - k - 1];
                    }
                }
                else
                {
                    for (var k = m - p; k <= m - 1; k++)
                    {
                        ret[m - 1] += k / ((double)(m)) * ret[k - 1] * lpc[m - k - 1];
                    }
                }
            }

            return ret;
        }

        public static double[] Lpc(double[] data, int p)
        {
            double[] r = new double[p + 1];

            for (int m = 0; m <= p; m++)
            {
                r[m] = 0.0;
                for (int n = 0; n <= data.Length - m - 1; n++)
                {
                    r[m] += data[n] * data[n + m];
                }
            }

            return LevinsonDurbin(r, p);
        }

        public static double[] LevinsonDurbin(double[] r, int p)
        {
            double ei1;
            double[] ai = new double[p + 1];
            double[] ret = null;
            ei1 = r[0];


            for (int i = 0; i < p; i++)
            {
                var ai1 = new double[i + 2];
                ai1[0] = 1;

                var gammai = r[i + 1];

                for (int j = 1; j <= i; j++)
                {
                    gammai = gammai + ai[j] * r[i - j + 1];
                }

                var lambdai1 = -gammai / ei1;

                for (int j = 1; j <= i; j++)
                {
                    ai1[j] = ai[j] + lambdai1 * (ai[i - j + 1]);
                }

                ai1[i + 1] = lambdai1;
                var ki = Math.Abs(lambdai1);
                ei1 = ei1 * (1 - (ki * ki));
                ai = ai1;
                ret = ai1;
            }

            return ret;
        }

        #endregion
    }
}
