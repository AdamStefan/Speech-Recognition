using System;
using System.Collections.Generic;

namespace SpeechRecognition.Audio
{
    public class SampleAggregator : SignalVisitor
    {
        #region Fields

        public int SamplingRate { get; private set; }
        private double _currentMinValue = Double.MaxValue;
        private double _currentMaxValue = Double.MinValue;

        private double _sessionMaxValue = Double.MinValue;
        private int _currentProcessedItems;
        private int _voiceSample;
        private int _unvoicedSample;

        #endregion

        public event EventHandler<SamplePointEventArgs> SampleReady;

        #region Methods

        private void WriteData(float[] data, int startIndex, int length, bool isVoice)
        {
            for (int index = startIndex; index < length; index++)
            {
                _currentMinValue = Math.Min(data[index], _currentMinValue);
                _currentMaxValue = Math.Max(data[index], _currentMinValue);
                _sessionMaxValue = Math.Max(_sessionMaxValue, _currentMaxValue);
                if (isVoice)
                {
                    _voiceSample ++;
                }
                else
                {
                    _unvoicedSample ++;
                }
                _currentProcessedItems++;
                if (_currentProcessedItems == SamplingRate)
                {
                    if (SampleReady != null)
                    {
                        SampleReady(this,
                            new SamplePointEventArgs(new SamplePoint
                            {
                                IsVoice = _voiceSample > _unvoicedSample,
                                MaxValue = _currentMaxValue,
                                MinValue = _currentMinValue
                            }));

                    }
                    _currentProcessedItems = 0;
                    _currentMinValue = Double.MaxValue;
                    _currentMaxValue = Double.MinValue;
                    _unvoicedSample = _voiceSample = 0;
                }
            }
        }


        #endregion

        #region Instance

        public SampleAggregator(int samplingRate)
        {
            SamplingRate = samplingRate;
        }

        #endregion

        public struct SamplePoint
        {
            public double MaxValue;
            public double MinValue;
            public bool IsVoice;
        }

        public class SamplePointEventArgs : EventArgs
        {
            #region Fields

            public SamplePoint Point { get; private set; }

            #endregion


            #region Instance

            public SamplePointEventArgs(SamplePoint samplePoint)
            {
                Point = samplePoint;
            }

            #endregion
        }

        public override void Visit(float[] data, int startIndex, int length, Dictionary<string, object> properties)
        {
            var isVoice = (bool) properties["Voice"];
            WriteData(data, startIndex, length, isVoice);
        }
    }




}
