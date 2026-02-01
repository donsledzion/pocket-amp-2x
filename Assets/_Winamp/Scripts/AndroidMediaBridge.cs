using UnityEngine;
using System;

namespace SoftAware
{
    public class AndroidMediaBridge
    {
        private static AndroidJavaObject serviceIntent;
        private static AndroidJavaObject context;

        private static void Initialize()
        {
            if (context != null) return;

            #if UNITY_ANDROID && !UNITY_EDITOR
            using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                context = player.GetStatic<AndroidJavaObject>("currentActivity");
            }
            serviceIntent = new AndroidJavaObject("android.content.Intent", context, new AndroidJavaClass("com.softaware.winamp.WinampAudioService"));
            #endif
        }

        public static void UpdateMetadata(string title, string artist, bool isPlaying)
        {
            Initialize();

            #if UNITY_ANDROID && !UNITY_EDITOR
            serviceIntent.Call<AndroidJavaObject>("setAction", "UPDATE_METADATA");
            serviceIntent.Call<AndroidJavaObject>("putExtra", "title", title);
            serviceIntent.Call<AndroidJavaObject>("putExtra", "artist", artist);
            serviceIntent.Call<AndroidJavaObject>("putExtra", "isPlaying", isPlaying);

            if (UnityEngine.Device.Application.platform == RuntimePlatform.Android)
            {
                if (isPlaying)
                {
                    if (BuildVersion() >= 26) // Android 8.0+
                        context.Call<AndroidJavaObject>("startForegroundService", serviceIntent);
                    else
                        context.Call("startService", serviceIntent);
                }
                else
                {
                    // Even when paused, we keep the service for controls, 
                    // but onStartCommand handles UI update.
                    context.Call<AndroidJavaObject>("startService", serviceIntent);
                }
            }
            #endif
        }

        public static void StopService()
        {
            Initialize();
            #if UNITY_ANDROID && !UNITY_EDITOR
            serviceIntent.Call<AndroidJavaObject>("setAction", "STOP_SERVICE");
            context.Call("startService", serviceIntent);
            #endif
        }

        private static int BuildVersion()
        {
            using (var build = new AndroidJavaClass("android.os.Build$VERSION"))
            {
                return build.GetStatic<int>("SDK_INT");
            }
        }
    }
}
