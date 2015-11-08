namespace SpeechRecognition.FeaturesProvider.LPC
{
    public class LpcCepstrumParameters : FeatureProviderParameters
    {
        public int LpcOrder
        {
            get;
            set;
        }

        public int MOrder { get; set; }

        public override int NumberOfCoeff
        {
            get { return MOrder; }
        }
    }
}
