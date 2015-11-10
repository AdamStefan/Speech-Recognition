using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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
    public partial class PolygonWaveFormControl:IWaveFormRenderer
    {
        #region Fields

        int _renderPosition;
        double _yTranslate = 40;
        double _yScale = 40;
        readonly double _xScale = 2;
        readonly int _blankZone = 10;
        private SolidColorBrush _normalBrush;
        private SolidColorBrush _silenceBrush;
        private SolidColorBrush _voiceBrush;
        private List<Rectangle> _rectangles = new List<Rectangle>();

        Polygon _waveForm = new Polygon();        


        private ConcurrentQueue<SampleAggregator.SamplePoint> _queue = new ConcurrentQueue<SampleAggregator.SamplePoint>();

        #endregion

        #region Instance

        public PolygonWaveFormControl()
        {
            SizeChanged += OnSizeChanged;
            InitializeComponent();
            _waveForm.Stroke = Foreground;
            _waveForm.StrokeThickness = 1;
            _normalBrush = new SolidColorBrush(Colors.Bisque);
            _silenceBrush = new SolidColorBrush(Colors.Blue);
            _waveForm.Fill = _normalBrush;
            _voiceBrush = new SolidColorBrush(Colors.Red);
            mainCanvas.Children.Add(_waveForm);
        }

        #endregion

        #region Methods

        void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            // We will remove everything as we are going to rescale vertically
            _renderPosition = 0;
            ClearAllPoints();

            _yTranslate = ActualHeight / 2;
            _yScale = ActualHeight / 2;
        }

        private void ClearAllPoints()
        {
            _waveForm.Points.Clear();
        }

        private int Points
        {
            get { return _waveForm.Points.Count / 2; }
        }

        private void Refresh()
        {
            SampleAggregator.SamplePoint point;
            while (_queue.TryDequeue(out point))
            {                               
                var maxValue = point.MaxValue;
                var minValue = point.MinValue;

                int visiblePixels = (int)(ActualWidth / _xScale);
                if (visiblePixels > 0)
                {
                    CreatePoint(maxValue, minValue, point.IsVoice);

                    if (_renderPosition > visiblePixels)
                    {
                        _renderPosition = 0;
                    }
                    int erasePosition = (_renderPosition + _blankZone) % visiblePixels;
                    if (erasePosition < Points)
                    {
                        double yPos = SampleToYPosition(0);                       
                        _waveForm.Points[erasePosition] = new Point(erasePosition * _xScale, yPos);
                        _waveForm.Points[BottomPointIndex(erasePosition)] = new Point(erasePosition * _xScale, yPos);
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

        private int BottomPointIndex(int position)
        {
            return _waveForm.Points.Count - position - 1;
        }

        private double SampleToYPosition(double value)
        {
            return _yTranslate + value * _yScale;
        }

        private void CreatePoint(double topValue, double bottomValue, bool isVoice)
        {
            double topYPos = SampleToYPosition(topValue);
            double bottomYPos = SampleToYPosition(bottomValue);
            double xPos = _renderPosition*_xScale;
            if (_renderPosition >= Points)
            {
                int insertPos = Points;
                _waveForm.Points.Insert(insertPos, new Point(xPos, topYPos));
                _waveForm.Points.Insert(insertPos + 1, new Point(xPos, bottomYPos));
            }
            else
            {
                _waveForm.Points[_renderPosition] = new Point(xPos, topYPos);
                _waveForm.Points[BottomPointIndex(_renderPosition)] = new Point(xPos, bottomYPos);
            }

            AddRectangle(isVoice ? _voiceBrush : _silenceBrush);
            _renderPosition++;
        }

        private void AddRectangle(Brush brush)
        {
            Rectangle rectangle;

            if (_renderPosition >= _rectangles.Count)
            {
                rectangle = new Rectangle {Width = _xScale, Height = 2, Fill = brush};
                mainCanvas.Children.Add(rectangle);
            }
            else
            {
                rectangle = _rectangles[_renderPosition];
            }
            rectangle.Fill = brush;

            Canvas.SetLeft(rectangle, _renderPosition * _xScale);
            Canvas.SetTop(rectangle, _yTranslate - 1);
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
