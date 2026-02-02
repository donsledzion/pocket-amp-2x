using UnityEngine;
using System;

namespace SoftAware
{
    public class AndroidVisualizerBridge : MonoBehaviour
    {
        private static AndroidJavaObject javaVisualizer;
        public static bool TestMode = false;

        public static void Initialize(int sessionId)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                Release();
                
                Playlist.Log($"[Viz] Java Init: {sessionId}");
                javaVisualizer = new AndroidJavaObject("com.softaware.winamp.WinampVisualizer");
                bool success = javaVisualizer.Call<bool>("initialize", sessionId);
                
                Playlist.Log($"[Viz] Java Init Result: {success}");
            }
            catch (Exception e)
            {
                Playlist.Log($"[Viz] Java ERR: {e.Message}");
            }
#endif
        }

        public static void Release()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (javaVisualizer != null)
            {
                javaVisualizer.Call("release");
                javaVisualizer.Dispose();
                javaVisualizer = null;
            }
#endif
        }


        private static int silenceCounter = 0;
        private static float[] cachedFftData;

        public static float[] GetFFTData(int dataSize)
        {
            if (TestMode) return GetTestData(dataSize);

            float[] result = new float[dataSize];
#if UNITY_ANDROID && !UNITY_EDITOR
            if (javaVisualizer != null)
            {
                try
                {
                    float[] javaData = javaVisualizer.Call<float[]>("getFft", dataSize);
                    if (javaData != null && javaData.Length == dataSize) 
                    {
                        // Cache for waveform simulation if needed
                        cachedFftData = javaData;
                        return javaData;
                    }
                }
                catch {}
            }
#endif
            return result;
        }

        public static float[] GetWaveformData(int dataSize)
        {
            float[] result = new float[dataSize];
#if UNITY_ANDROID && !UNITY_EDITOR
            if (javaVisualizer != null)
            {
                try
                {
                    float[] javaData = javaVisualizer.Call<float[]>("getWaveform", dataSize);
                    if (javaData != null && javaData.Length == dataSize) 
                    {
                        // Check for silence
                        bool isSilent = true;
                        for(int i=0; i<dataSize; i+=10) { // Check every 10th sample for perf
                            if (Mathf.Abs(javaData[i]) > 0.001f) {
                                isSilent = false;
                                break;
                            }
                        }
                        
                        if (isSilent) {
                            silenceCounter++;
                             // If silent for > 20 frames, switch to simulation
                            if (silenceCounter > 20 && cachedFftData != null) {
                                return SimulateWaveformFromFFT(dataSize);
                            }
                        } else {
                            silenceCounter = 0;
                        }
                        
                        // If not simulating, return real data (even if silent for first few frames)
                        return javaData;
                    }
                }
                catch {}
            }
#endif
            return result;
        }

        // Generate a plausible waveform from FFT data (Sum of Sines approximation)
        private static float[] SimulateWaveformFromFFT(int size)
        {
             float[] simulated = new float[size];
             if (cachedFftData == null) return simulated;

             // Use first few FFT bins (bass/low-mids) to drive the main shape
             // This is a simplified reconstruction for visual effect
             float t = Time.time;
             int binsToUse = Mathf.Min(cachedFftData.Length, 16); 
             
             for (int i = 0; i < size; i++)
             {
                 float val = 0f;
                 float normalizedX = (float)i / size;
                 
                 for (int b = 0; b < binsToUse; b++)
                 {
                     // Frequency factor: higher bin = higher freq
                     float freq = (b + 1) * 2f * Mathf.PI;
                     // Amplitude from FFT
                     float amp = cachedFftData[b];
                     
                     // Add sine wave: Amp * Sin(Freq * x + PhaseShift)
                     // Phase shift moves with Time to create animation
                     val += amp * Mathf.Sin(freq * normalizedX + t * (b + 1));
                 }
                 // Scale down to keep within -1..1 roughly
                 simulated[i] = Mathf.Clamp(val * 0.5f, -1f, 1f);
             }
             return simulated;
        }

        private static float[] GetTestData(int dataSize)
        {
            float[] test = new float[dataSize];
            for (int i = 0; i < dataSize; i++) test[i] = UnityEngine.Random.value * 0.5f;
            return test;
        }

        private void OnDestroy()
        {
            Release();
        }
    }
}
