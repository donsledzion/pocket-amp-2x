using UnityEngine;

namespace SoftAware.PocketAmp
{
    public static class AndroidStatusBar
    {
        private const int SYSTEM_UI_FLAG_FULLSCREEN = 0x00000004; // Hides status bar

        public static void SetVisible(bool visible)
        {
            if (Application.platform != RuntimePlatform.Android) return;

            try
            {
                // Retrieve activity JUST to run on UI thread
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                    {
                        try
                        {
                            // RE-RETRIEVE Activity inside the runnable to ensure it's valid in this thread context
                            // The outer 'activity' might be disposed by the time this runs
                            using (var innerUnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                            using (var innerActivity = innerUnityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                            using (var window = innerActivity.Call<AndroidJavaObject>("getWindow"))
                            using (var view = window.Call<AndroidJavaObject>("getDecorView"))
                            using (var buildVersion = new AndroidJavaClass("android.os.Build$VERSION"))
                            {
                                int sdkInt = buildVersion.GetStatic<int>("SDK_INT");
                                Debug.Log($"[AndroidStatusBar] SDK Version: {sdkInt}, Setting Visible: {visible}");

                                if (sdkInt >= 30)
                                {
                                    // API 30+: Use WindowInsetsController
                                    using (var controller = window.Call<AndroidJavaObject>("getInsetsController"))
                                    {
                                        if (controller != null)
                                        {
                                            // Dynamically get the mask for statusBars()
                                            int statusBarsMask;
                                            using (var typeClass = new AndroidJavaClass("android.view.WindowInsets$Type"))
                                            {
                                                statusBarsMask = typeClass.CallStatic<int>("statusBars");
                                            }
                                            
                                            Debug.Log($"[AndroidStatusBar] WindowInsetsController found. StatusBars Mask: {statusBarsMask}");

                                            if (visible)
                                            {
                                                controller.Call("show", statusBarsMask);
                                                Debug.Log("[AndroidStatusBar] Called controller.show(statusBars)");
                                            }
                                            else
                                            {
                                                controller.Call("hide", statusBarsMask);
                                                Debug.Log("[AndroidStatusBar] Called controller.hide(statusBars)");
                                            }
                                        }
                                        else
                                        {
                                            Debug.LogError("[AndroidStatusBar] WindowInsetsController is null!");
                                        }
                                    }
                                }
                                else
                                {
                                    // Legacy: Use system UI flags
                                    int flags = view.Call<int>("getSystemUiVisibility");
                                    Debug.Log($"[AndroidStatusBar] Legacy Mode. Current Flags: {flags}");

                                    if (visible)
                                    {
                                        flags &= ~SYSTEM_UI_FLAG_FULLSCREEN;
                                    }
                                    else
                                    {
                                        flags |= SYSTEM_UI_FLAG_FULLSCREEN;
                                    }

                                    view.Call("setSystemUiVisibility", flags);
                                    Debug.Log($"[AndroidStatusBar] Set Flags: {flags}");
                                }
                            }
                        }
                        catch (System.Exception innerEx)
                        {
                            Debug.LogError($"[AndroidStatusBar] Error inside UI Thread: {innerEx}");
                        }
                    }));
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[AndroidStatusBar] Failed to set visibility: {e.Message}");
            }
        }
    }
}
