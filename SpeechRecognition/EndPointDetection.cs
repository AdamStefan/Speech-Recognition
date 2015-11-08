using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechRecognition
{
    public class EndPointDetection
    {
        #region Fields

        private float[] _originalSignal; // input
        private float[] _silenceRemovedSignal;// output
        private int _samplingRate;
        private int _firstSamples;
        private int _samplePerFrame;

        #endregion

        public EndPointDetection(float[] originalSignal, int samplingRate)
        {
            this._originalSignal = originalSignal;
            this._samplingRate = samplingRate;
            _samplePerFrame = _samplingRate / 1000;
            _firstSamples = _samplePerFrame * 200;// according to formula
        }




        public float[] doEndPointDetection()
        {
            float[] voiced = new float[_originalSignal.Length];// for identifying
            // each
            // sample whether it is
            // voiced or unvoiced
            double sum = 0;
            double sd = 0.0;
            double m = 0.0;

            // 1. calculation of mean
            for (int i = 0; i < _firstSamples; i++)
            {
                sum += _originalSignal[i];
            }
            // System.err.println("total sum :" + sum);
            m = sum / _firstSamples;// mean
            sum = 0;// reuse var for S.D.

            // 2. calculation of Standard Deviation
            for (int i = 0; i < _firstSamples; i++)
            {
                sum += Math.Pow((_originalSignal[i] - m), 2);
            }
            sd = Math.Sqrt(sum / _firstSamples);
          

            // 3. identifying whether one-dimensional Mahalanobis distance function
            // i.e. |x-u|/s greater than ####3 or not,
            for (int i = 0; i < _originalSignal.Length; i++)
            {
                // System.out.println("x-u/SD  ="+(Math.abs(originalSignal[i] -u ) /
                // sd));
                if ((Math.Abs(_originalSignal[i] - m) / sd) > 2)
                {
                    voiced[i] = 1;
                }
                else
                {
                    voiced[i] = 0;
                }
            }

            // 4. calculation of voiced and unvoiced signals
            // mark each frame to be voiced or unvoiced frame
            int frameCount = 0;
            int usefulFramesCount = 1;
            int count_voiced = 0;
            int count_unvoiced = 0;
            int[] voicedFrame = new int[_originalSignal.Length / _samplePerFrame];
            int loopCount = _originalSignal.Length - (_originalSignal.Length % _samplePerFrame);// skip
            // the
            // last
            for (int i = 0; i < loopCount; i += _samplePerFrame)
            {
                count_voiced = 0;
                count_unvoiced = 0;
                for (int j = i; j < i + _samplePerFrame; j++)
                {
                    if (voiced[j] == 1)
                    {
                        count_voiced++;
                    }
                    else
                    {
                        count_unvoiced++;
                    }
                }
                if (count_voiced > count_unvoiced)
                {
                    usefulFramesCount++;
                    voicedFrame[frameCount++] = 1;
                }
                else
                {
                    voicedFrame[frameCount++] = 0;
                }
            }

            // 5. silence removal
            _silenceRemovedSignal = new float[usefulFramesCount * _samplePerFrame];
            int k = 0;
            for (int i = 0; i < frameCount; i++)
            {
                if (voicedFrame[i] == 1)
                {
                    for (int j = i * _samplePerFrame; j < i * _samplePerFrame + _samplePerFrame; j++)
                    {
                        _silenceRemovedSignal[k++] = _originalSignal[j];
                    }
                }
            }
            // end
            return _silenceRemovedSignal;
        }
    }
}
