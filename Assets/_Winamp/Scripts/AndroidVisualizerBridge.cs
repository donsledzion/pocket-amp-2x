using UnityEngine;
using System;

namespace SoftAware
{
    public class AndroidVisualizerBridge : MonoBehaviour
    {
        private static AndroidJavaObject visualizer;
        private static IntPtr fftArrayPtr;
        private static IntPtr waveArrayPtr;
        private static int currentCaptureSize = 0;
        public static bool TestMode = false;

        public static void Initialize(int sessionId)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                Release();
                
                // If sessionId is -1, try global session 0 (requires MODIFY_AUDIO_SETTINGS)
                int targetSession = (sessionId == -1) ? 0 : sessionId;
                Playlist.Log($"[Viz] Creating for session {targetSession}");
                
                visualizer = new AndroidJavaObject("android.media.audiofx.Visualizer", targetSession);
                
                using (var visualizerClass = new AndroidJavaClass("android.media.audiofx.Visualizer"))
                {
                    int[] range = visualizerClass.CallStatic<int[]>("getCaptureSizeRange");
                    int finalSize = (range != null && range.Length > 1) ? range[1] : 1024;
                    // Limit to 1024 for performance if max is huge
                    if (finalSize > 1024) finalSize = 1024;

                    visualizer.Call<int>("setCaptureSize", finalSize);
                    int enableStatus = visualizer.Call<int>("setEnabled", true);
                    
                    currentCaptureSize = finalSize;
                    
                    // Create persistent JNI arrays to avoid GC pressure
                    fftArrayPtr = AndroidJNI.NewByteArray(finalSize);
                    fftArrayPtr = AndroidJNI.NewGlobalRef(fftArrayPtr);
                    
                    waveArrayPtr = AndroidJNI.NewByteArray(finalSize);
                    waveArrayPtr = AndroidJNI.NewGlobalRef(waveArrayPtr);

                    Playlist.Log($"[Viz] Init status: {enableStatus}, Size: {finalSize}");
                }
            }
            catch (Exception e)
            {
                Playlist.Log($"[Viz] ERR: {e.Message}");
            }
#endif
        }

        public static void Release()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (visualizer != null)
            {
                visualizer.Call<int>("setEnabled", false);
                visualizer.Call("release");
                visualizer = null;
            }
            if (fftArrayPtr != IntPtr.Zero)
            {
                AndroidJNI.DeleteGlobalRef(fftArrayPtr);
                fftArrayPtr = IntPtr.Zero;
            }
            if (waveArrayPtr != IntPtr.Zero)
            {
                AndroidJNI.DeleteGlobalRef(waveArrayPtr);
                waveArrayPtr = IntPtr.Zero;
            }
            currentCaptureSize = 0;
#endif
        }

        public static float[] GetFFTData(int dataSize)
        {
            if (TestMode)
            {
                float[] test = new float[dataSize];
                for (int i = 0; i < dataSize; i++) test[i] = UnityEngine.Random.value * 0.5f;
                return test;
            }

            float[] result = new float[dataSize];
#if UNITY_ANDROID && !UNITY_EDITOR
            if (visualizer == null || fftArrayPtr == IntPtr.Zero) return result;

            // Use direct JNI calls to fill our persistent Java array and copy it back
            int status = visualizer.Call<int>("getFft", new AndroidJavaObject(fftArrayPtr));
            if (status == 0)
            {
                sbyte[] rawData = AndroidJNI.FromSByteArray(fftArrayPtr);
                long sum = 0;

                for (int i = 0; i < dataSize; i++)
                {
                    int idx = i * 2 + 2; 
                    if (idx + 1 < rawData.Length)
                    {
                        float real = rawData[idx];
                        float imag = rawData[idx+1];
                        float magnitude = Mathf.Sqrt(real * real + imag * imag);
                        result[i] = magnitude / 1024; // Reduced sensitivity
                    }
                }
            }
#endif
            return result;
        }

        public static float[] GetWaveformData(int dataSize)
        {
            float[] result = new float[dataSize];
#if UNITY_ANDROID && !UNITY_EDITOR
            if (visualizer == null || waveArrayPtr == IntPtr.Zero) return result;

            int status = visualizer.Call<int>("getWaveform", new AndroidJavaObject(waveArrayPtr));
            if (status == 0)
            {
                sbyte[] rawData = AndroidJNI.FromSByteArray(waveArrayPtr);
                for (int i = 0; i < dataSize; i++)
                {
                    int idx = i * (rawData.Length / dataSize);
                    if (idx < rawData.Length)
                    {
                        // Waveform is 0..255 unsigned in Java, but in sbyte it's -128..127. 0 (Java 128) is silence.
                        // So we cast to byte to get 0..255 then subtract 128.
                        byte unsignedByte = (byte)rawData[idx];
                        result[i] = (unsignedByte - 128) / 128f;
                    }
                }
            }
#endif
            return result;
        }

        private void OnDestroy()
        {
            Release();
        }
    }
}
