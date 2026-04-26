using UnityEngine;
using System;

namespace SoftAware.PocketAmp
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

                // Ensure permission is granted (required for Visualizer API on many devices)
                if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Microphone))
                {
                    UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Microphone);
                }
                
                // Use the provided sessionId (e.g. from MediaPlayer)
                // Fallback to 0 (Global Mix) only if sessionId is -1
                int targetSession = (sessionId == -1) ? 0 : sessionId; 
                                
                javaVisualizer = new AndroidJavaObject("com.softaware.pocketamp.PocketAmpVisualizer");
                bool success = javaVisualizer.Call<bool>("initialize", targetSession);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Viz] Java ERR: {e.Message}");
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

        // New Plugin-Friendly API (Non-invasive)
        // ---------------------------------------------------------
        private static float[] _latestFft;
        private static float[] _latestWave;
        private static int _lastFrameUpdate = -1;

        /// <summary>
        /// Returns the latest FFT data captured in the current frame.
        /// This allows multiple plugins to share the same capture without additional JNI overhead.
        /// </summary>
        public static float[] GetSharedFFT(int size)
        {
            UpdateSharedData(size);
            return _latestFft;
        }

        public static float[] GetSharedWaveform(int size)
        {
            UpdateSharedData(size);
            return _latestWave;
        }

        private static void UpdateSharedData(int size)
        {
            if (Time.frameCount == _lastFrameUpdate) return;
            
            _latestFft = GetFFTData(size);
            _latestWave = GetWaveformData(size);
            _lastFrameUpdate = Time.frameCount;
        }
        // ---------------------------------------------------------


        //private static int silenceCounter = 0;
        private static float[] cachedFftData;

        public static float[] GetFFTData(int dataSize)
        {
            if (TestMode) return GetTestData(dataSize);

#if UNITY_ANDROID && !UNITY_EDITOR
            if (javaVisualizer != null)
            {
                try
                {
                    float[] javaData = javaVisualizer.Call<float[]>("getFft", dataSize);
                    if (javaData != null && javaData.Length == dataSize) 
                    {
                        return javaData;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[Viz] GetFFT ERR: {e.Message}");
                }
            }
#endif
            return new float[dataSize];
        }

        public static float[] GetPocketAmpFFT()
        {
            if (TestMode) return GetTestData(19);

#if UNITY_ANDROID && !UNITY_EDITOR
            if (javaVisualizer != null)
            {
                try
                {
                    float[] javaData = javaVisualizer.Call<float[]>("getPocketAmpFft");
                    if (javaData != null && javaData.Length == 19) 
                    {
                        return javaData;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[Viz] GetPocketAmpFFT ERR: {e.Message}");
                }
            }
#endif
            return new float[19];
        }

        public static float[] GetWaveformData(int dataSize)
        {
            if (TestMode) return GetTestData(dataSize);

#if UNITY_ANDROID && !UNITY_EDITOR
            if (javaVisualizer != null)
            {
                try
                {
                    float[] javaData = javaVisualizer.Call<float[]>("getWaveformPCM", dataSize);
                    if (javaData != null && javaData.Length == dataSize) 
                    {
                        return javaData;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[Viz] GetWave fatal: {e.Message}");
                }
            }
#endif
            return new float[dataSize];
        }

        private static float[] GetTestData(int dataSize)
        {
            float[] test = new float[dataSize];
            float t = Time.time * 5f;
            for (int i = 0; i < dataSize; i++) 
            {
                test[i] = Mathf.Sin(t + i * 0.1f) * 0.5f + (UnityEngine.Random.value * 0.1f);
            }
            return test;
        }

        private void OnDestroy()
        {
            Release();
        }
    }
}
