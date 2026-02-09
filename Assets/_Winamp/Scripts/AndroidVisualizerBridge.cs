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

                // Ensure permission is granted (required for Visualizer API on many devices)
                if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Microphone))
                {
                    UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Microphone);
                }
                
                // Use the provided sessionId (e.g. from MediaPlayer)
                // Fallback to 0 (Global Mix) only if sessionId is -1
                int targetSession = (sessionId == -1) ? 0 : sessionId; 
                
                Playlist.Log($"[Viz] Java Init Session: {targetSession}");
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
                    Playlist.Log($"[Viz] GetFFT ERR: {e.Message}");
                }
            }
#endif
            return new float[dataSize];
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
                    Playlist.Log($"[Viz] GetWave fatal: {e.Message}");
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
