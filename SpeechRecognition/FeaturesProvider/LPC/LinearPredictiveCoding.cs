using System;

namespace SpeechRecognition.FeaturesProvider.LPC
{
    public class LinearPredictiveCoding
    {
        #region  Methods

        public static double[] Apply(float[] frame, int order = 80, int lpcOrder = 12)
        {
            var windowedSample = HammingWindowDef.ApplyHammingWindow(frame); // 1. windowing
            double b0 = 0.0;
            double energy = 0.0;
            var lpc = Lpc(windowedSample, lpcOrder, out b0, out energy);
            var p = lpc.Length - 1;
            var ret = new double[order];

            for (int m = 1; m <= order; m++)
            {
                if (m <= p)
                {
                    ret[m] = lpc[m - 1];
                    for (var k = 1; k <= m - 1; k++)
                    {
                        ret[m] += (k / ((double)(m))) * ret[k] * lpc[m - k - 1];
                    }
                }
                else
                {
                    for (var k = m - p; k <= m - 1; k++)
                    {
                        ret[m] += (k / ((double)(m))) * ret[k] * lpc[m - k - 1];
                    }
                }
            }

            ret[0] = b0;
            return ret;
        }

        public static double[] LpcAndHamming(float[] frame, int lpcOrder)
        {
            var windowedSample = HammingWindowDef.ApplyHammingWindow(frame); // 1. windowing   
            double b0;
            double energy = 0.0;
            var lpc = Lpc(windowedSample, lpcOrder, out b0, out energy);
            var ret = new double[lpc.Length + 1];


            Array.Copy(lpc, 0, ret, 1, lpc.Length);
            ret[1] = b0;
            ret[0] = Math.Log(energy);

            return ret;
        }

        public static double[] Lpc(double[] data, int p, out double b0, out double energy)
        {
            double[] r = new double[p + 1];
            energy = 0.0;

            for (int m = 0; m <= p; m++)
            {
                r[m] = 0.0;
                for (int n = 0; n <= data.Length - m - 1; n++)
                {
                    r[m] += data[n] * data[n + m];

                    if (m == 0)
                    {
                        energy += (data[n] * data[n]);
                    }
                }
            }
            energy = energy / data.Length;
            return LevinsonDurbin(r, p, out b0);
        }

        public static double[] LevinsonDurbin(double[] r, int p, out double b0)
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

            b0 = ei1;
            //b0 = Math.Sqrt(ei1);
            return ret;
        }

        #endregion
    }
}
