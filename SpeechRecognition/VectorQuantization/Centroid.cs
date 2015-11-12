using System;
using System.Collections.Generic;

namespace SpeechRecognition.VectorQuantization
{
    [Serializable]
    public class Centroid : Point
    {
        #region Fields

        protected double _distortion = 0;
        protected List<Point> _points = new List<Point>();

        #endregion

        #region Instance

        public Centroid(double[] coordinates)
            : base(coordinates)
        {
        }

        #endregion

        #region Properties

        public int Length
        {
            get
            {
                return _points.Count;
            }
        }

        public double Distortion
        {
            get
            {
                return _distortion;
            }
        }

        #endregion

        #region Methods

        public Point GetPoint(int index)
        {
            return _points[index];
        }

        public void Remove(Point point, double distance)
        {
            for (int index = 0; index < _points.Count; index++)
            {
                if (point.Equals(_points[index]))
                {
                    _points.RemoveAt(index);
                    _distortion -= distance;
                    break;
                }
            }
        }

        public void Add(Point pt, double dist)
        {            
            _points.Add(pt);                        
            _distortion += dist;
        }


        public void Update()
        {
            double[] sumCoordinates = new double[Dimension];

            for (int index = 0; index < _points.Count; index++)
            {
                for (int dimIndex = 0; dimIndex < Dimension; dimIndex++)
                {
                    sumCoordinates[dimIndex] += _points[index][dimIndex];
                }
            }

            // divide sum of coordinates by total number points to get average
            for (int k = 0; k < Dimension; k++)
            {
                this[k] = sumCoordinates[k] / _points.Count;
            }

            _points.Clear();


            // reset distortion measure
            _distortion = 0;
        }

        #endregion
    }
}
