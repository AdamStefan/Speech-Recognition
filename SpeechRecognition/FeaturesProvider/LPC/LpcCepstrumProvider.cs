namespace SpeechRecognition.FeaturesProvider.LPC
{
    public class LpcCepstrumProvider : IFeatureProvider
    {
        #region Fields

        private int _lpcOrder;
        private int _numberOfCoefficients;

        #endregion

        #region Instance

        public LpcCepstrumProvider(int lpcOrder, int numberOfCoefficients)
        {
            _lpcOrder = lpcOrder;
            _numberOfCoefficients = numberOfCoefficients;
        }

        #endregion

        #region Methods

        public double[] Extract(float[] frame, out bool isEmpty)
        {
            isEmpty = false;
            return LinearPredictiveCoding.Apply(frame, _numberOfCoefficients, _lpcOrder);
        }

        #endregion

        public bool ComputeDelta
        {
            get
            {
                return false;
            }
        }
    }
}
