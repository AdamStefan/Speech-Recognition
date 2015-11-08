namespace SpeechRecognition.HMM
{
    public class HmmDefinition
    {
        public int NumStates { get; set; }

        public int NumSymbols { get; set; }

        protected int NumObSeq { get; set; }

        protected double[][] Transition { get; set; }

        protected double[][] Output { get; set; }

        protected double[] Pi { get; set; }
    }
}
