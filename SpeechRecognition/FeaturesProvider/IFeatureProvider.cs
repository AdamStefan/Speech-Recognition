namespace SpeechRecognition.FeaturesProvider
{
    public interface IFeatureProvider
    {        
        double[] Extract(float[] frame, out bool isEmpty);
        bool ComputeDelta { get; }
    }


}
