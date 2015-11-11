using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using SpeechRecognition.Audio;

namespace SpeechRecognition.UI
{
    /// <summary>
    /// Interaction logic for PolygonWaveFormControl.xaml
    /// </summary>
    public partial class PolygonWaveFormControl : IWaveFormRenderer
    {
        #region Fields

        private int _renderPosition;
        private double _yTranslate = 40;
        private double _yScale = 40;
        private const double XScale = 2;
        private readonly SolidColorBrush _silenceBrush;
        private readonly SolidColorBrush _voiceBrush;
        private readonly List<Rectangle> _rectangles = new List<Rectangle>();

        private readonly ConcurrentQueue<SampleAggregator.SamplePoint> _queue =
            new ConcurrentQueue<SampleAggregator.SamplePoint>();

        #endregion

        #region Instance

        public PolygonWaveFormControl()
        {
            SizeChanged += OnSizeChanged;
            InitializeComponent();

            _silenceBrush = new SolidColorBrush(Colors.Blue);
            _voiceBrush = new SolidColorBrush(Colors.Red);
        }

        #endregion

        #region Methods

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            // We will remove everything as we are going to rescale vertically
            _renderPosition = 0;
            ClearAllPoints();

            _yTranslate = ActualHeight / 2;
            _yScale = ActualHeight / 4;
        }

        private void ClearAllPoints()
        {
            _rectangles.Clear();
        }

        private void Refresh()
        {
            SampleAggregator.SamplePoint point;
            while (_queue.TryDequeue(out point))
            {
                var maxValue = point.MaxValue;
                var minValue = point.MinValue;

                int visiblePixels = (int)(ActualWidth / XScale);
                if (visiblePixels > 0)
                {
                    CreatePoint(maxValue, minValue, point.IsVoice);

                    if (_renderPosition > visiblePixels)
                    {
                        _renderPosition = 0;
                    }
                }
            }
        }

        public void AddValue(SampleAggregator.SamplePoint samplePoint)
        {
            _queue.Enqueue(samplePoint);
            var action = new Action(Refresh);
            Dispatcher.BeginInvoke(action, DispatcherPriority.Render);
        }

        private double SampleToYPosition(double value)
        {
            var ret = _yTranslate - value * _yScale;
            return ret;
        }

        private void CreatePoint(double topValue, double bottomValue, bool isVoice)
        {
            double topYPos = SampleToYPosition(topValue);
            double bottomYPos = SampleToYPosition(bottomValue);

            AddRectangle(isVoice ? _voiceBrush : _silenceBrush, topYPos, bottomYPos);
            _renderPosition++;
        }

        private void AddRectangle(Brush brush, double topYPos, double bottomYpos)
        {
            Rectangle rectangle;

            if (_renderPosition == 0)
            {
                for (int index = 1; index < _rectangles.Count; index++)
                {
                    _rectangles[index].Fill = Brushes.Transparent;
                }
            }

            if (_renderPosition >= _rectangles.Count)
            {
                rectangle = new Rectangle { Width = XScale, Height = Math.Max(bottomYpos - topYPos, 1), Fill = brush };
                mainCanvas.Children.Add(rectangle);
                _rectangles.Add(rectangle);
            }
            else
            {
                rectangle = _rectangles[_renderPosition];
                rectangle.Width = XScale;
                rectangle.Height = Math.Max(bottomYpos - topYPos, 1);
                rectangle.Fill = brush;
            }

            if (rectangle.Height + topYPos > 100)
            {
            }


            Canvas.SetLeft(rectangle, _renderPosition * XScale);
            Canvas.SetTop(rectangle, topYPos);
        }

        /// <summary>
        /// Clears the waveform and repositions on the left
        /// </summary>
        public void Reset()
        {
            _renderPosition = 0;
            ClearAllPoints();
        }

        #endregion
    }
}
