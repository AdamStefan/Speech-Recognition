using System;
using System.Linq;

namespace SpeechRecognition
{
    public static class Utils
    {
        public static double Log(double value, double tolerance = 0.00000000001)
        {
            if (value >= 0.0 && value < tolerance)
            {
                return Math.Log(tolerance);
            }

            return Math.Log(value);
        }

        public static double[] Substract(double[] left, double[] right)
        {
            if (left.Length != right.Length)
            {
                throw new ArgumentException();
            }
            var length = left.Length;
            var ret = new double[length];
            for (var index = 0; index < length; index++)
            {
                ret[index] = left[index] - right[index];
            }

            return ret;
        }

        public static double[] Add(double[] left, double[] right)
        {
            if (left.Length != right.Length)
            {
                throw new ArgumentException();
            }
            var length = left.Length;
            var ret = new double[length];
            for (var index = 0; index < length; index++)
            {
                ret[index] = left[index] + right[index];
            }

            return ret;
        }

        public static bool IsPowerOfTwo(ulong x)
        {
            return (x & (x - 1)) == 0;
        }

        public static int ClosestPower(ulong x)
        {
            int power = 2;

            while ((x >>= 1) != 0)
            {
                power <<= 1;
            }

            return power;
        }

        public static void Normalize(double[] frame)
        {
            var mean = frame.Average();

            for (var index = 0; index < frame.Length; index++)
            {
                frame[index] = frame[index] - mean;
            }
        }


        

    }
}
