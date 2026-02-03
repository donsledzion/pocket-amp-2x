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
                
                // FORCE SESSION 0 (Global Output Mix)
                // This captures everything playing on the device and is often the only way 
                // to get valid Waveform data on some vendors (Samsung etc.)
                // Requires android.permission.MODIFY_AUDIO_SETTINGS (which we have)
                int targetSession = 0; 
                
                Playlist.Log($"[Viz] Java Init FORCE GLOBAL: {targetSession}");
                javaVisualizer = new AndroidJavaObject("com.softaware.winamp.WinampVisualizer");
                bool success = javaVisualizer.Call<bool>("initialize", targetSession);
                
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
            // ALWAYS Simulate from FFT.
            // Native GetWaveform is unreliable on many devices (Samsung, Pixel) returning silence/flatline.
            // Simulation provides a consistent, high-fidelity experience that looks "correct" to the user.
            return SimulateWaveformFromFFT(dataSize);
        }

        // Generate a jagged, reactive waveform from FFT data (Noise Modulation)
        private static float[] SimulateWaveformFromFFT(int size)
        {
             float[] simulated = new float[size];
             if (cachedFftData == null) return simulated;

             // 1. Analyze Energy for "Kick" detection
             float bassEnergy = 0f;
             float trebleEnergy = 0f;
             
             // Bass: bins 0-4
             for(int k=0; k<5 && k<cachedFftData.Length; k++) bassEnergy += cachedFftData[k];
             bassEnergy /= 5f; 
             
             // Treble: bins 10-32
             int trebleCount = 0;
             for(int k=10; k<32 && k<cachedFftData.Length; k++) {
                 trebleEnergy += cachedFftData[k];
                 trebleCount++;
             }
             if (trebleCount > 0) trebleEnergy /= trebleCount;

             // 2. Dynamic Gain (Expander)
             float kickRaw = bassEnergy * 8.0f; 
             float kick = kickRaw * kickRaw; // Square response
             kick = Mathf.Clamp(kick, 0.1f, 3.0f); 
             
             float fizzRaw = trebleEnergy * 4.0f;
             float fizz = fizzRaw * fizzRaw;

             float t = Time.time;
             
             // 3. Synthesize Raw Signal
             for (int i = 0; i < size; i++)
             {
                 float normalizedX = (float)i / size;
                 
                 // Sine carrier wobbles with Bass
                 float carrier = Mathf.Sin(normalizedX * (10f + kick * 2f) + t * 5f);
                 
                 // Noise modulated by Treble
                 float noise = (UnityEngine.Random.value * 2f - 1f) * fizz;

                 // Combine: Carrier provides shape, Noise provides jaggedness
                 // When bass kicks, the waveform expands vertically
                 float sample = (carrier * 0.4f * kick) + noise;
                 
                 simulated[i] = Mathf.Clamp(sample, -1f, 1f);
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
