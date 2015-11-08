using System;

namespace SpeechRecognition.VectorQuantization
{
    [Serializable]
    public class Codebook
    {

        #region Fields

        private readonly Point[] _points;
        private Centroid[] _centroids;

        private const double SplitValue = 0.01;
        private const double MinDistortion = 0.1;
        private readonly int _codebookSize;
        private readonly int _dimension;

        #endregion

        #region Instance

        public Codebook(Point[] points)
            : this(points, 256)
        {

        }

        public Codebook(Point[] points, int codebookSize)
        {
            _points = points;
            _codebookSize = codebookSize;
            _dimension = points[0].Dimension;
            Initialize();
        }

        #endregion

        #region Properties

        public int Size
        {
            get
            {
                return _codebookSize;
            }
        }

        #endregion

        #region Methods

        void Initialize()
        {
            double distortionBeforeUpdate = 0; // distortion measure before
            // updating centroids
            double distortionAfterUpdate = 0; // distortion measure after update
            // centroids

            // design a 1-vector Codebook
            _centroids = new Centroid[1];

            // then initialize it with (0, 0) coordinates
            double[] origin = new double[_dimension];
            _centroids[0] = new Centroid(origin);

            // initially, all training points will belong to 1 single cell
            for (int i = 0; i < _points.Length; i++)
            {
                _centroids[0].Add(_points[i], 0);
            }

            // calls update to set the initial codevector as the average of all
            // points
            _centroids[0].Update();



            // Iteration 1: repeat splitting step and K-means until required number
            // of codewords is reached
            while (_centroids.Length < _codebookSize)
            {
                // split codevectors by a binary splitting method
                Split();

                // group training points to centroids closest to them
                GroupPtoC();

                // Iteration 2: perform K-means algorithm
                do
                {
                    for (int i = 0; i < _centroids.Length; i++)
                    {
                        distortionBeforeUpdate += _centroids[i].Distortion;
                        _centroids[i].Update();
                    }

                    // regroup
                    GroupPtoC();

                    for (int i = 0; i < _centroids.Length; i++)
                    {
                        distortionAfterUpdate += _centroids[i].Distortion;
                    }

                } while (Math.Abs(distortionAfterUpdate - distortionBeforeUpdate) < MinDistortion);
            }
        }

        private void Split()
        {
            // "Centroids length now becomes " + centroids.length + 2

            Centroid[] temp = new Centroid[_centroids.Length * 2];
            double[][] tCo = new double[2][];
            for (int i = 0; i < temp.Length; i += 2)
            {
                tCo[0] = new double[_dimension];
                tCo[1] = new double[_dimension];
                for (int j = 0; j < _dimension; j++)
                {
                    tCo[0][j] = _centroids[i / 2][j] * (1 + SplitValue);
                    tCo[1][j] = _centroids[i / 2][j] * (1 - SplitValue);
                }
                temp[i] = new Centroid(tCo[0]);
                temp[i + 1] = new Centroid(tCo[1]);
            }

            // replace old centroids array with new one		
            _centroids = temp;
        }

        public int[] Quantize(Point[] points)
        {
            int[] output = new int[points.Length];
            var index = 0;
            foreach (var point in points)
            {
                output[index] = ClosestCentroidToPoint(point);
                index++;
            }            
            return output;
        }

        private int ClosestCentroidToPoint(Point pt)
        {
            double lowestDist = 0; // = getDistance(pt, centroids[0]);
            int lowestIndex = 0;

            for (int i = 0; i < _centroids.Length; i++)
            {
                var tmpDist = GetDistance(pt, _centroids[i]);
                if (tmpDist < lowestDist || i == 0)
                {
                    lowestDist = tmpDist;
                    lowestIndex = i;
                }
            }
            return lowestIndex;
        }

        private int ClosestCentroidToCentroid(Centroid c)
        {
            double lowestDist = Double.MaxValue;
            int lowestIndex = 0;
            for (int i = 0; i < _centroids.Length; i++)
            {
                var tmpDist = GetDistance(c, _centroids[i]);
                if (tmpDist < lowestDist && _centroids[i].Length > 1)
                {
                    lowestDist = tmpDist;
                    lowestIndex = i;
                }
            }
            return lowestIndex;
        }

        private int ClosestPoint(Centroid c1, Centroid c2)
        {
            double lowestDist = GetDistance(c2.GetPoint(0), c1);
            int lowestIndex = 0;
            for (int i = 1; i < c2.Length; i++)
            {
                var tmpDist = GetDistance(c2.GetPoint(i), c1);
                if (tmpDist < lowestDist)
                {
                    lowestDist = tmpDist;
                    lowestIndex = i;
                }
            }
            return lowestIndex;
        }

        private void GroupPtoC()
        {
            // find closest Centroid and assign Points to it
            for (int i = 0; i < _points.Length; i++)
            {
                int index = ClosestCentroidToPoint(_points[i]);
                _centroids[index].Add(_points[i], GetDistance(_points[i], _centroids[index]));
            }
            // make sure that all centroids have at least one Points assigned to it
            // no cell should be empty or else NaN error will occur due to division
            // of 0 by 0
            for (int i = 0; i < _centroids.Length; i++)
            {
                if (_centroids[i].Length == 0)
                {
                    // find the closest Centroid with more than one points assigned
                    // to it
                    int index = ClosestCentroidToCentroid(_centroids[i]);
                    // find the closest Points in the closest Centroid's cell
                    int closestIndex = ClosestPoint(_centroids[i], _centroids[index]);
                    Point closestPt = _centroids[index].GetPoint(closestIndex);
                    _centroids[index].Remove(closestPt, GetDistance(closestPt, _centroids[index]));
                    _centroids[i].Add(closestPt, GetDistance(closestPt, _centroids[i]));
                }
            }
        }

        private double GetDistance(Point point, Centroid tC)
        {
            double distance = 0;
            for (int i = 0; i < _dimension; i++)
            {
                var temp = point[i] - tC[i];
                distance += temp * temp;
            }
            distance = Math.Sqrt(distance);
            return distance;
        }

        #endregion


    }
}
