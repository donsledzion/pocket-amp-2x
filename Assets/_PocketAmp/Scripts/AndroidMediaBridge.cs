using UnityEngine;
using System;

namespace SoftAware
{
    public class AndroidMediaBridge
    {
        public delegate void RemoteControlAction();
        public delegate void RemoteControlSeekAction(long positionMs);
        
        private class AndroidMediaCallbackProxy : AndroidJavaProxy
        {
            private readonly RemoteControlAction _onPlay, _onPause, _onNext, _onPrev;
            private readonly RemoteControlSeekAction _onSeek;

            public AndroidMediaCallbackProxy(RemoteControlAction play, RemoteControlAction pause, RemoteControlAction next, RemoteControlAction prev, RemoteControlSeekAction seek) 
                : base("com.softaware.pocketamp.PocketAmpAudioService$RemoteControlListener")
            {
                _onPlay = play;
                _onPause = pause;
                _onNext = next;
                _onPrev = prev;
                _onSeek = seek;
            }

            // IMPORTANT: These callbacks are executed on the Android Background Java Thread.
            // DO NOT call Unity APIs or touch UI elements directly here!
            // Enqueue actions to the main thread instead.
            void onPlay() => _onPlay?.Invoke();
            void onPause() => _onPause?.Invoke();
            void onNext() => _onNext?.Invoke();
            void onPrev() => _onPrev?.Invoke();
            void onSeekTo(long pos) => _onSeek?.Invoke(pos);
        }

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
            serviceIntent = new AndroidJavaObject("android.content.Intent", context, new AndroidJavaClass("com.softaware.pocketamp.PocketAmpAudioService"));
            #endif
        }

        public static void UpdateMetadata(string title, string artist, int duration, int position, bool isPlaying)
        {
            Initialize();

            #if UNITY_ANDROID && !UNITY_EDITOR
            serviceIntent.Call<AndroidJavaObject>("setAction", "UPDATE_METADATA");
            serviceIntent.Call<AndroidJavaObject>("putExtra", "title", title);
            serviceIntent.Call<AndroidJavaObject>("putExtra", "artist", artist);
            serviceIntent.Call<AndroidJavaObject>("putExtra", "duration", (long)duration); // in ms
            serviceIntent.Call<AndroidJavaObject>("putExtra", "position", (long)position); // in ms
            serviceIntent.Call<AndroidJavaObject>("putExtra", "isPlaying", isPlaying);

            if (UnityEngine.Device.Application.platform == RuntimePlatform.Android)
            {
                if (isPlaying)
                {
                    if (BuildVersion() >= 26) // Android 8.0+
                        context.Call<AndroidJavaObject>("startForegroundService", serviceIntent);
                    else
                        context.Call<AndroidJavaObject>("startService", serviceIntent);
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
            context.Call<AndroidJavaObject>("startService", serviceIntent);
            #endif
        }

        public static void RegisterRemoteControlListener(RemoteControlAction onPlay, RemoteControlAction onPause, RemoteControlAction onNext, RemoteControlAction onPrev, RemoteControlSeekAction onSeek)
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
            var proxy = new AndroidMediaCallbackProxy(onPlay, onPause, onNext, onPrev, onSeek);
            using (var serviceClass = new AndroidJavaClass("com.softaware.pocketamp.PocketAmpAudioService"))
            {
                serviceClass.CallStatic("setListener", proxy);
            }
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
