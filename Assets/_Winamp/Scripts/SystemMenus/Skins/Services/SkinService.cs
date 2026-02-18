using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using System;
using SoftAware.PocketAmp.SystemMenus.Core; // Added for IntPtr, Exception

namespace SoftAware.PocketAmp.SystemMenus.Skins
{
    public class SkinService : IService
    {
        private string SkinsDirectory => Path.Combine(Application.persistentDataPath, "skins");

        public SkinService()
        {
            if (!Directory.Exists(SkinsDirectory))
            {
                Directory.CreateDirectory(SkinsDirectory);
            }
        }

        public Task<List<string>> GetAvailableSkinsAsync()
        {
            if (!Directory.Exists(SkinsDirectory)) return Task.FromResult(new List<string>());

            // Get both .wsz and .zip files
            var files = Directory.GetFiles(SkinsDirectory, "*.*")
                .Where(s => s.ToLower().EndsWith(".wsz") || s.ToLower().EndsWith(".zip"))
                .Select(Path.GetFileName)
                .OrderBy(n => n)
                .ToList();
            
            return Task.FromResult(files);
        }

        public async Task ImportSkinAsync(string sourcePath)
        {
            var fileName = Path.GetFileName(sourcePath);
            // Fallback for content URIs if Path.GetFileName returns empty or bad name
            if (string.IsNullOrEmpty(fileName) || sourcePath.StartsWith("content://"))
            {
                if (Application.platform == RuntimePlatform.Android)
                {
                    var realName = GetFileNameFromContentUri(sourcePath);
                    if (!string.IsNullOrEmpty(realName))
                    {
                        fileName = realName;
                    }
                    else
                    {
                        fileName = "imported_skin_" + System.DateTime.Now.Ticks + ".wsz";
                    }
                }
                else
                {
                     fileName = "imported_skin_" + System.DateTime.Now.Ticks + ".wsz";
                }
            }
            
            var destPath = Path.Combine(SkinsDirectory, fileName);

            await CopyFileAsync(sourcePath, destPath);
        }

        private async Task CopyFileAsync(string sourcePath, string destPath)
        {
            // 1. Try standard File.Copy for local paths
            try
            {
                if (!sourcePath.Contains("://") && File.Exists(sourcePath))
                {
                     await Task.Run(() => File.Copy(sourcePath, destPath, true));
                     return;
                }
            }
            catch { }

            // 2. Android Content URI handling via JNI (UnityWebRequest fails on content://)
            if (Application.platform == RuntimePlatform.Android && sourcePath.StartsWith("content://"))
            {
                // JNI operations involving UnityPlayer.currentActivity MUST be on the main thread
                // or have the thread attached to the JVM. Simpler to just run on main thread for small skin files.
                CopyAndroidContentUri(sourcePath, destPath);
                return;
            }

            // 3. UnityWebRequest fallback (file://, jar:file://, http://)
            string uri = sourcePath;
            if (!uri.Contains("://") && !uri.StartsWith("/")) uri = "file://" + uri;

            using (var uwr = UnityEngine.Networking.UnityWebRequest.Get(uri))
            {
                var op = uwr.SendWebRequest();
                while (!op.isDone) await Task.Yield();

                if (uwr.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    await File.WriteAllBytesAsync(destPath, uwr.downloadHandler.data);
                }
                else
                {
                    throw new IOException($"Failed to copy skin from {sourcePath}: {uwr.error}");
                }
            }
        }

        private static void CopyAndroidContentUri(string uriString, string destPath)
        {
            // Use JNI to read from ContentResolver
            try
            {
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var contentResolver = currentActivity.Call<AndroidJavaObject>("getContentResolver"))
                using (var uriClass = new AndroidJavaClass("android.net.Uri"))
                using (var uriObject = uriClass.CallStatic<AndroidJavaObject>("parse", uriString))
                using (var inputStream = contentResolver.Call<AndroidJavaObject>("openInputStream", uriObject))
                {
                     if (inputStream == null) throw new IOException("Failed to open InputStream for URI: " + uriString);

                     using (var outputStream = File.OpenWrite(destPath))
                     {
                         // Buffer size
                         var bufferSize = 4096;
                         var bufferPtr = AndroidJNI.NewByteArray(bufferSize);
                         // byte[] managedBuffer = new byte[bufferSize]; // Not needed with FromByteArray
                         
                         try 
                         {
                             // Get read method ID: int read(byte[])
                             IntPtr readMethodId = AndroidJNIHelper.GetMethodID(inputStream.GetRawClass(), "read", "([B)I");
                             
                             while (true)
                             {
                                 // Call read(buffer)
                                 var args = new jvalue[1];
                                 args[0].l = bufferPtr;
                                 
                                 int bytesRead = AndroidJNI.CallIntMethod(inputStream.GetRawObject(), readMethodId, args);
                                 
                                 if (bytesRead < 0) break; // End of stream
                                 if (bytesRead == 0) continue;

                                 // Read data from Java array to new C# array
                                 // Using FromByteArray is safer than GetByteArrayRegion regarding sbyte[] vs byte[] signatures
                                 byte[] chunk = AndroidJNI.FromByteArray(bufferPtr);
                                 
                                 // Write only the bytes read
                                 outputStream.Write(chunk, 0, bytesRead);
                             }
                         }
                         finally
                         {
                             AndroidJNI.DeleteLocalRef(bufferPtr);
                         }
                     }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SkinService] JNI Error copying content URI: {ex.Message}\n{ex.StackTrace}");
                throw; // Re-throw to show error in UI
            }
        }

        private string GetFileNameFromContentUri(string uriString)
        {
            try
            {
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var contentResolver = currentActivity.Call<AndroidJavaObject>("getContentResolver"))
                using (var uriClass = new AndroidJavaClass("android.net.Uri"))
                using (var uriObject = uriClass.CallStatic<AndroidJavaObject>("parse", uriString))
                {
                    // Query columns: just _display_name
                    // Cursor query(Uri uri, String[] projection, String selection, String[] selectionArgs, String sortOrder)
                    // We need to pass projection as String[]
                    
                    // But creating String[] in JNI is annoying. Passing null returns all columns.
                    // Let's pass null and find the column index by name.
                    
                    using (var cursor = contentResolver.Call<AndroidJavaObject>("query", uriObject, null, null, null, null))
                    {
                        if (cursor != null && cursor.Call<bool>("moveToFirst"))
                        {
                            // OpenableColumns.DISPLAY_NAME is "_display_name"
                            var nameIndex = cursor.Call<int>("getColumnIndex", "_display_name");
                            if (nameIndex >= 0)
                            {
                                var name = cursor.Call<string>("getString", nameIndex);
                                if (!string.IsNullOrEmpty(name)) return name;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SkinService] Could not retrieve filename from URI: {ex.Message}");
            }
            return null;
        }

        public async Task DeleteSkinAsync(string skinName)
        {
            var path = Path.Combine(SkinsDirectory, skinName);
            if (File.Exists(path))
            {
                await Task.Run(() => File.Delete(path));
            }
        }

        public async Task<bool> LoadSkin(string skinName)
        {
            var path = Path.Combine(SkinsDirectory, skinName);
            if (File.Exists(path))
                return await Refs.SkinManager.LoadSkin(path);
            return false;
        }
    }
}
