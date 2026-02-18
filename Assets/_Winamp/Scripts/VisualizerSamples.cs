namespace SoftAware.PocketAmp.Visualizers
{
    [System.Serializable]
    public class VisualizerSamples
    {
        public float[] FFT;
        public float[] Waveform;
        
        public VisualizerSamples(int fftSize, int waveformSize)
        {
            FFT = new float[fftSize];
            Waveform = new float[waveformSize];
        }
    }
}
