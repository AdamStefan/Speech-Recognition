using System;
using System.Numerics;

namespace SpeechRecognition
{
    public class FourierTransform
    {
        static private int BitReverse(int j, int nu)
        {
            int j2;
            int j1 = j;
            int k = 0;
            for (int i = 1; i <= nu; i++)
            {
                j2 = j1 / 2;
                k = 2 * k + j1 - 2 * j2;
                j1 = j2;
            }
            return k;
        }

        public static Complex[] Dft(Complex[] signal, uint samplingRation, HammingWindowDef hammingWindow = null)
        {
            var signalLength = signal.Length;
            var ret = new Complex[signalLength];
            var phasePart = -2 * Math.PI / signalLength;
            var phaseHammingPart = 2 * Math.PI / (signalLength - 1);

            for (int frequency = 0; frequency < signalLength; frequency++)
            {
                ret[frequency] = new Complex();
                var phasePartFrequency = phasePart * frequency;
                for (int step = 1; step <= signal.Length; step++)
                {
                    var phase = phasePartFrequency * (step);
                    var temp = signal[step - 1] * Complex.FromPolarCoordinates(1, phase);
                    if (hammingWindow != null)
                    {
                        temp = temp * (hammingWindow.Alpha - hammingWindow.Beta * Math.Cos(phaseHammingPart * (step)));
                    }
                    ret[frequency] += temp;
                }
            }

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="numberOfCoefficients">the minimum number of coefficients, should be a power of 2. If the number of coefficents is greater than signal length the signal is padded with zeros</param>
        /// <returns></returns>
        static public double[] FFTAmplitudeSquared(double[] signal, int numberOfCoefficients)
        {
            // Assume n is a power of 2

            double[] x = signal;
            if (!Utils.IsPowerOfTwo((uint)signal.Length))
            {

                if (numberOfCoefficients < signal.Length)
                {
                    numberOfCoefficients = Utils.ClosestPower((uint)signal.Length);
                }

                x = new double[numberOfCoefficients];
                for (int index = 0; index < signal.Length; index++)
                {
                    x[index] = signal[index];
                }
            }

            var n = x.Length;
            var nu = (int)(Math.Log(n) / Math.Log(2));
            int n2 = n / 2;
            int nu1 = nu - 1;
            double[] xre = new double[n];
            double[] xim = new double[n];
            //double[] magnitude = new double[n2];
            double[] magnitude = new double[n];
            double[] decibel = new double[n2];
            double tr, ti, p, arg, c, s;
            for (int i = 0; i < n; i++)
            {
                xre[i] = x[i];
                xim[i] = 0.0f;
            }
            int k = 0;
            for (int l = 1; l <= nu; l++)
            {
                while (k < n)
                {
                    for (int i = 1; i <= n2; i++)
                    {
                        p = BitReverse(k >> nu1, nu);
                        arg = 2 * (double)Math.PI * p / n;
                        c = (double)Math.Cos(arg);
                        s = (double)Math.Sin(arg);
                        tr = xre[k + n2] * c + xim[k + n2] * s;
                        ti = xim[k + n2] * c - xre[k + n2] * s;
                        xre[k + n2] = xre[k] - tr;
                        xim[k + n2] = xim[k] - ti;
                        xre[k] += tr;
                        xim[k] += ti;
                        k++;
                    }
                    k += n2;
                }
                k = 0;
                nu1--;
                n2 = n2 / 2;
            }
            k = 0;
            int r;
            while (k < n)
            {
                r = BitReverse(k, nu);
                if (r > k)
                {
                    tr = xre[k];
                    ti = xim[k];
                    xre[k] = xre[r];
                    xim[k] = xim[r];
                    xre[r] = tr;
                    xim[r] = ti;
                }
                k++;
            }
            //for (int i = 0; i < n / 2; i++)
            //    //magnitude[i] = (float)(Math.Sqrt((xre[i] * xre[i]) + (xim[i] * xim[i])));
            //    decibel[i] = 10.0 * Math.Log10((float)(Math.Sqrt((xre[i] * xre[i]) + (xim[i] * xim[i]))));
            ////return magnitude;
            //return decibel;

            double endValue = 0;
            var sign = 1;
            for (int i = 0; i < n / 2; i++)
            {
                magnitude[i] = (xre[i] * xre[i]) + (xim[i] * xim[i]);
                endValue += sign * x[i];
                sign = -sign;
            }
            var startIndex = n / 2;
            for (int i = startIndex; i < n; i++)
            {
                endValue += sign * x[i];
                sign = -sign;

                if (i == startIndex)
                {
                    continue;
                }
                magnitude[i] = magnitude[startIndex - i + startIndex];
            }

            magnitude[startIndex] = endValue * endValue;

            return magnitude;
        }


        /**
	 * number of points
	 */
        protected int _numPoints;
        /**
         * real part
         */
        private double[] _real;
        /**
         * imaginary part
         */
        private double[] _imag;


        public int computeFFT(double[] signal, int numberOfCoefficients)
        {
            double[] x = signal;
            if (!Utils.IsPowerOfTwo((uint)signal.Length))
            {

                if (numberOfCoefficients < signal.Length)
                {
                    numberOfCoefficients = Utils.ClosestPower((uint)signal.Length);
                }

                x = new double[numberOfCoefficients];
                for (int index = 0; index < signal.Length; index++)
                {
                    x[index] = signal[index];
                }
            }

            signal = x;

            _numPoints = signal.Length;
            // initialize real & imag array
            _real = new double[_numPoints];
            _imag = new double[_numPoints];
            // move the N point signal into the real part of the complex DFT's time
            // domain
            _real = signal;
            // set all of the samples in the imaginary part to zero
            for (int i = 0; i < _imag.Length; i++)
            {
                _imag[i] = 0;
            }
            // perform FFT using the real & imag array
            FFT();
            return numberOfCoefficients;
        }

        public double[] GetMagnitude()
        {
            var ret = new double[_real.Length];

            for (int index = 0; index < _real.Length; index++)
            {
                ret[index] = Math.Sqrt(_real[index] * _real[index] + _imag[index] * _imag[index]);
            }

            return ret;
        }

        public double[] GetMagnitudeSquared(out double energy) 
        {
            var ret = new double[_real.Length];
            energy = 0.0;

            for (int index = 0; index < _real.Length; index++)
            {
                ret[index] = _real[index] * _real[index] + _imag[index] * _imag[index];
                energy += ret[index];
            }
            energy = energy / _real.Length;
            return ret;
        }

        private void FFT()
        {
            if (_numPoints == 1) { return; }
            const double pi = Math.PI;
            int numStages = (int)(Math.Log(_numPoints) / Math.Log(2));
            int halfNumPoints = _numPoints >> 1;
            int j = halfNumPoints;

            // FFT time domain decomposition carried out by "bit reversal sorting"
            // algorithm
            int k = 0;
            for (int i = 1; i < _numPoints - 2; i++)
            {
                if (i < j)
                {
                    // swap
                    double tempReal = _real[j];
                    double tempImag = _imag[j];
                    _real[j] = _real[i];
                    _imag[j] = _imag[i];
                    _real[i] = tempReal;
                    _imag[i] = tempImag;
                }
                k = halfNumPoints;
                while (k <= j)
                {
                    j -= k;
                    k >>= 1;
                }
                j += k;
            }

            // loop for each stage
            for (int stage = 1; stage <= numStages; stage++)
            {
                int LE = 1;
                for (int i = 0; i < stage; i++)
                {
                    LE <<= 1;
                }
                int LE2 = LE >> 1;
                double UR = 1;
                double UI = 0;
                // calculate sine & cosine values
                double SR = Math.Cos(pi / LE2);
                double SI = -Math.Sin(pi / LE2);
                // loop for each sub DFT
                for (int subDFT = 1; subDFT <= LE2; subDFT++)
                {
                    // loop for each butterfly
                    for (int butterfly = subDFT - 1; butterfly <= _numPoints - 1; butterfly += LE)
                    {
                        int ip = butterfly + LE2;
                        // butterfly calculation
                        double tempReal = (double)(_real[ip] * UR - _imag[ip] * UI);
                        double tempImag = (double)(_real[ip] * UI + _imag[ip] * UR);
                        _real[ip] = _real[butterfly] - tempReal;
                        _imag[ip] = _imag[butterfly] - tempImag;
                        _real[butterfly] += tempReal;
                        _imag[butterfly] += tempImag;
                    }

                    double tempUR = UR;
                    UR = tempUR * SR - UI * SI;
                    UI = tempUR * SI + UI * SR;
                }
            }
        }
    }
}
