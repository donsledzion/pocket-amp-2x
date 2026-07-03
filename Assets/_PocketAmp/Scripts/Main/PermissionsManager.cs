using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

namespace SoftAware.PocketAmp
{
    public class PermissionsManager : MonoBehaviour
    {
        public static PermissionsManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public async Task RequestStartupPermissionsAsync()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            // Wait for application focus just in case
            while (!Application.isFocused)
            {
                await Task.Yield();
            }

            string[] permissionsToRequest = new string[]
            {
                "android.permission.READ_MEDIA_AUDIO",
                "android.permission.READ_EXTERNAL_STORAGE",
                "android.permission.RECORD_AUDIO"
            };

            foreach (var perm in permissionsToRequest)
            {
                if (!Permission.HasUserAuthorizedPermission(perm))
                {
                    await RequestPermissionAsync(perm);
                }
            }
#else
            await Task.CompletedTask;
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private async Task RequestPermissionAsync(string permission)
        {
            var tcs = new TaskCompletionSource<bool>();
            var callbacks = new PermissionCallbacks();
            
            callbacks.PermissionGranted += (perm) => { tcs.TrySetResult(true); };
            callbacks.PermissionDenied += (perm) => { tcs.TrySetResult(false); };
            callbacks.PermissionDeniedAndDontAskAgain += (perm) => { tcs.TrySetResult(false); };

            Permission.RequestUserPermission(permission, callbacks);

            // Wait for user to interact with the dialog
            await tcs.Task;

            // Optional: Give the OS a frame to restore focus
            await Task.Yield();
        }
#endif
    }
}
