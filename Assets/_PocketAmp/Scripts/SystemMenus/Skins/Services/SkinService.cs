using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using System;
using SoftAware.PocketAmp.SystemMenus.Core; // Added for IntPtr, Exception

namespace SoftAware.PocketAmp.SystemMenus.Skins
{
    public class SkinService : IService
    {
        private string skinsDirectory;
        private string BaseUrl => "https://skin-library.softaware.pl/api";

        public SkinService()
        {
            skinsDirectory = Path.Combine(Application.persistentDataPath, "skins");
            if (!Directory.Exists(skinsDirectory))
            {
                Directory.CreateDirectory(skinsDirectory);
            }
        }

        public async Awaitable<List<string>> GetAvailableSkinsAsync()
        {
            if (!Directory.Exists(skinsDirectory)) return new List<string>();

            // Get both .wsz and .zip files
            var files = await Task.Run(() => Directory.GetFiles(skinsDirectory, "*.*")
                .Where(s => s.ToLower().EndsWith(".wsz") || s.ToLower().EndsWith(".zip"))
                .Select(Path.GetFileName)
                .OrderBy(n => n)
                .ToList());
            
            return files;
        }

        public async Awaitable ImportSkinAsync(string sourcePath)
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
            
            var destPath = Path.Combine(skinsDirectory, fileName);

            await CopyFileAsync(sourcePath, destPath);
        }

        private async Awaitable CopyFileAsync(string sourcePath, string destPath)
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
            catch(Exception e)
            {
                Debug.LogError($"[SkinService] Exception: {e.Message}");
            }

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
                         var bufferPtr = AndroidJNI.NewSByteArray(bufferSize);
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
                                 
                                 var bytesRead = AndroidJNI.CallIntMethod(inputStream.GetRawObject(), readMethodId, args);
                                 
                                 if (bytesRead < 0) break; // End of stream
                                 if (bytesRead == 0) continue;

                                 // Read data from Java array to new C# array
                                 // Using FromByteArray is safer than GetByteArrayRegion regarding sbyte[] vs byte[] signatures
                                 var sChunk = AndroidJNI.FromSByteArray(bufferPtr);
                                 var chunk = ByteHelpers.ToByteArray(sChunk);
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

        public async Awaitable DeleteSkinAsync(string skinName)
        {
            var path = Path.Combine(skinsDirectory, skinName);
            if (File.Exists(path))
            {
                await Task.Run(() => File.Delete(path));
            }
        }

        public async Awaitable<bool> LoadSkin(string skinName)
        {
            var path = Path.Combine(skinsDirectory, skinName);
            if (File.Exists(path))
                return await Refs.SkinManager.LoadSkin(path);
            return false;
        }

        #region Web API

        public bool IsSkinDownloaded(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;
            var path = Path.Combine(skinsDirectory, id + ".wsz");
            return File.Exists(path);
        }

        public async Awaitable<SkinListResponse> GetWebSkinsAsync(string query = "", int page = 1)
        {
            var url = $"{BaseUrl}/skins?page={page}&limit=20";
            if (!string.IsNullOrEmpty(query))
            {
                url += $"&q={Uri.EscapeDataString(query)}";
            }

            using (var www = UnityWebRequest.Get(url))
            {
                var op = www.SendWebRequest();
                while (!op.isDone) await Task.Yield();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    return JsonUtility.FromJson<SkinListResponse>(www.downloadHandler.text);
                }
                
                Debug.LogError($"[SkinService] Failed to fetch web skins: {www.error}");
                return null;
            }
        }

        public async Awaitable<Texture2D> GetTextureAsync(string url, System.Threading.CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(url)) return null;

            using (var loader = UnityWebRequestTexture.GetTexture(url))
            {
                var op = loader.SendWebRequest();
                while (!op.isDone)
                {
                    if (token.IsCancellationRequested)
                    {
                        loader.Abort();
                        return null;
                    }
                    await Awaitable.NextFrameAsync();
                }

                if (loader.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    return DownloadHandlerTexture.GetContent(loader);
                }
                
                Debug.LogError($"[SkinService] Failed to load texture from {url}: {loader.error}");
                return null;
            }
        }

        public async Awaitable<string> DownloadWebSkinAsync(SkinData skin, System.Threading.CancellationToken token = default)
        {
            var fileName = $"{skin.id}.wsz";
            var destPath = Path.Combine(skinsDirectory, fileName);
            
            // Check if already downloaded
            if (File.Exists(destPath)) return fileName;

            var url = skin.download_url;
            if (string.IsNullOrEmpty(url))
            {
                url = $"{BaseUrl}/skins/{skin.id}/download";
            }

            using (var www = UnityWebRequest.Get(url))
            {
                www.downloadHandler = new DownloadHandlerFile(destPath);
                var op = www.SendWebRequest();
                while (!op.isDone)
                {
                    if (token.IsCancellationRequested)
                    {
                        www.Abort();
                        // Clean up partial file if needed (DownloadHandlerFile might leave it)
                        if (File.Exists(destPath)) File.Delete(destPath);
                        return null;
                    }
                    await Awaitable.NextFrameAsync();
                }

                if (www.result == UnityWebRequest.Result.Success)
                {
                    return fileName;
                }
                
                if (url.Contains("archive.org/download/"))
                {
                    try 
                    {
                        var uri = new Uri(url);
                        var parts = uri.AbsolutePath.Split('/');
                        if (parts.Length >= 3)
                        {
                            string itemId = parts[2];
                            string metaUrl = $"https://archive.org/metadata/{itemId}";
                            using (var metaWww = UnityWebRequest.Get(metaUrl))
                            {
                                var metaOp = metaWww.SendWebRequest();
                                while (!metaOp.isDone)
                                {
                                    if (token.IsCancellationRequested)
                                    {
                                        metaWww.Abort();
                                        if (File.Exists(destPath)) File.Delete(destPath);
                                        return null;
                                    }
                                    await Awaitable.NextFrameAsync();
                                }

                                if (metaWww.result == UnityWebRequest.Result.Success)
                                {
                                    var metaData = JsonUtility.FromJson<ArchiveMetadataResponse>(metaWww.downloadHandler.text);
                                    if (metaData != null && metaData.files != null)
                                    {
                                        var validFile = metaData.files.FirstOrDefault(f => f.name.EndsWith(".wsz", StringComparison.OrdinalIgnoreCase) || f.name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase));
                                        if (validFile != null && !string.IsNullOrEmpty(validFile.name))
                                        {
                                            string newUrl = $"https://archive.org/download/{itemId}/{validFile.name}";
                                            using (var retryWww = UnityWebRequest.Get(newUrl))
                                            {
                                                retryWww.downloadHandler = new DownloadHandlerFile(destPath);
                                                var retryOp = retryWww.SendWebRequest();
                                                while (!retryOp.isDone)
                                                {
                                                    if (token.IsCancellationRequested)
                                                    {
                                                        retryWww.Abort();
                                                        if (File.Exists(destPath)) File.Delete(destPath);
                                                        return null;
                                                    }
                                                    await Awaitable.NextFrameAsync();
                                                }

                                                if (retryWww.result == UnityWebRequest.Result.Success)
                                                {
                                                    Debug.Log($"[SkinService] Successfully downloaded using fallback URL: {newUrl}");
                                                    return fileName;
                                                }
                                                else
                                                {
                                                    Debug.LogError($"[SkinService] Fallback download failed for {newUrl}: {retryWww.error}");
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[SkinService] Fallback metadata fetch failed: {ex.Message}");
                    }
                }

                Debug.LogError($"[SkinService] Failed to download skin {skin.id} from {url}: {www.error}");
                if (File.Exists(destPath)) File.Delete(destPath);
                return null;
            }
        }

        #endregion
    }

    [Serializable]
    public class ArchiveMetadataResponse
    {
        public ArchiveFileInfo[] files;
    }

    [Serializable]
    public class ArchiveFileInfo
    {
        public string name;
    }
}
