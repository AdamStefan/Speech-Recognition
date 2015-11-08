using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechRecognition
{
    public class DiscreteCosinusTransform
    {
        #region Fields

        private int _signalSize;
        private double[,] _coeff;

        #endregion
        #region Instance

        public DiscreteCosinusTransform(int signalSize)
        {
            _signalSize = signalSize;
            InitCoeff();
        }

        #endregion

        public void InitCoeff()
        {
            _coeff = new double[_signalSize, _signalSize];
            for (int k = 0; k < _signalSize; k++)
            {
                for (int n = 0; n < _signalSize; n++)
                {
                    _coeff[k, n] = Math.Cos((Math.PI * (n + 0.5) * k) / (double)(_signalSize));
                }
            }
        }

        public double[] PerformDCT(double[] signalInput, int numCepstra, int melFiters)
        {
            double[] ret = new double[numCepstra];
            // perform DCT
            for (int n = 1; n <= numCepstra; n++)
            {
                for (int i = 1; i <= melFiters; i++)
                {
                    ret[n - 1] += signalInput[i - 1] * Math.Cos(Math.PI * (n - 1) / melFiters * (i - 0.5));
                }
            }
            return ret;
        }

        public double[] PerformDCT(double[] signalInput)
        {
            double[] ret = new double[signalInput.Length];
            var N = signalInput.Length;
            // perform DCT
            for (int k = 1; k <= N; k++)
            {
                var prod = k == 1 ? 1 / Math.Sqrt(N) : Math.Sqrt(2.0 / N);
                for (int i = 1; i <= signalInput.Length; i++)
                {
                    var phase = (Math.PI * (2 * i - 1) * (k - 1)) / (2.0 * N);
                    ret[k - 1] += signalInput[i - 1] * Math.Cos(phase);
                }
                ret[k - 1] = ret[k - 1] * prod;
            }
            return ret;
        }

        public double[] Apply(double[] signalInput)
        {
            if (signalInput.Length != _signalSize)
            {
                throw new ArgumentException("signalinput lenght is invalid");
            }

            var ret = new double[_signalSize];
            for (int k = 0; k < _signalSize; k++)
            {
                ret[k] = 0.0;
                for (int n = 0; n < _signalSize; n++)
                {
                    //_coeff[k, n] = Math.Cos((Math.PI * (n + 0.5) * k) / (double)(_signalSize));
                    ret[k] += signalInput[n] * _coeff[k, n];
                }
            }

            return ret;
        }

        public void Apply(double[] signalInput, double[] signalOutput, int startIndex)
        {
            if (signalInput.Length != _signalSize)
            {
                throw new ArgumentException("signalinput lenght is invalid");
            }

            for (int k = 0; k < _signalSize; k++)
            {
                signalOutput[startIndex + k] = 0.0;
                for (int n = 0; n < _signalSize; n++)
                {
                    //_coeff[k, n] = Math.Cos((Math.PI * (n + 0.5) * k) / (double)(_signalSize));
                    signalOutput[startIndex + k] += signalInput[n] * _coeff[k, n];
                }
            }

            
        }
    }
}
