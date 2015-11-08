using System;

namespace SpeechRecognition.VectorQuantization
{

    [Serializable]
    public class Point
    {
        #region Fields

        private  double[] _coordinates;

        #endregion

        #region Instance

        public Point(double[] coordinates)
        {
            Update(coordinates);
        }

        #endregion

        public void Update(double[] coordinates)
        {
            _coordinates = coordinates;
        }

        public int Dimension
        {
            get
            {
                return _coordinates.Length;
            }
        }

        public double this[int index]
        {
            get
            {
                return _coordinates[index];
            }
            set
            {
                _coordinates[index] = value;
            }
        }

        public override bool Equals(Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            Point p = obj as Point;
            if (p == null)
            {
                return false;
            }

            return Equals(p);
            
        }

        public bool Equals(Point p)
        {
            // If parameter is null return false:
            if ((object)p == null)
            {
                return false;
            }

            if (this.Dimension != p.Dimension)
            {
                return false;
            }

            for (int index = 0; index < Dimension; index++)
            {
                if (_coordinates[index] != p._coordinates[index])
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            var hash = _coordinates[0].GetHashCode();

            for (int index = 1; index < Dimension; index++)
            {
                hash = hash ^ _coordinates[index].GetHashCode();
            }
            return hash;
        }
    }
}
