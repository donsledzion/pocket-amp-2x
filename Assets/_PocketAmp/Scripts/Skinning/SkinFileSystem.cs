using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SoftAware
{
    /// <summary>
    /// Handles all file system operations for Skins:
    /// - Finding files
    /// - Unpacking .wsz archives
    /// - Managing persistent directories
    /// - Ensuring default assets exist
    /// </summary>
    public class SkinFileSystem
    {
        private const string SKINS_DIR = "Skins";
        private const string DEMO_SKINS_DIR = "demo_skins";

        public string LastUnpackedSkinPath { get; private set; }
        public string LastUnpackedSkinName { get; private set; }

        public string GetSkinDirectory(string skinName)
        {
            return Path.Combine(Application.persistentDataPath, SKINS_DIR, skinName);
        }

        #region File Picking

        public void PickSkinFile(System.Action<string> onFileSelected)
        {
#if UNITY_EDITOR
            string path = EditorUtility.OpenFilePanel("Select Skin (.wsz)", "", "wsz");
            if (!string.IsNullOrEmpty(path))
            {
                onFileSelected?.Invoke(path);
            }
#else
            // Placeholder for Android/Native picker
            Debug.LogWarning("NativeFilePicker not implemented. Use a test path.");
#endif
        }

        #endregion

        #region Unpacking

        public string UnpackWsz(string wszPath)
        {
            if (!File.Exists(wszPath))
            {
                Debug.LogError($"[SkinFileSystem] File does not exist: {wszPath}");
                return null;
            }

            string skinName = Path.GetFileNameWithoutExtension(wszPath);
            string outputDir = GetSkinDirectory(skinName);

            Debug.Log($"[SkinFileSystem] Unpacking skin '{skinName}' to: {outputDir}");

            try
            {
                if (Directory.Exists(outputDir))
                {
                    Directory.Delete(outputDir, true);
                }

                Directory.CreateDirectory(outputDir);
                ZipFile.ExtractToDirectory(wszPath, outputDir);

                LastUnpackedSkinPath = outputDir;
                LastUnpackedSkinName = skinName;

                return outputDir;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SkinFileSystem] Failed to unpack skin: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region File Finding

        /// <summary>
        /// Finds a file in the given directory or subdirectories.
        /// Returns the first match or null.
        /// Handles case-insensitivity manually for Android/Linux support.
        /// </summary>
        public string FindFile(string directory, string[] candidates)
        {
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory)) return null;

            // 1. Fast Path: Exact Match
            foreach (var cand in candidates)
            {
                string direct = Path.Combine(directory, cand);
                if (File.Exists(direct)) return direct;
            }

            // 2. Slow Path: Case-Insensitive Search
            // Android/Linux file systems are case-sensitive, so "File.Exists" fails if case mismatches.
            // We must iterate and check manually.
            
            try
            {
                // Check top-level files first (optimization)
                var topLevelFiles = Directory.EnumerateFiles(directory);
                foreach (var filePath in topLevelFiles)
                {
                    string fileName = Path.GetFileName(filePath);
                    foreach (var cand in candidates)
                    {
                        if (fileName.Equals(cand, System.StringComparison.OrdinalIgnoreCase))
                        {
                            return filePath;
                        }
                    }
                }

                // If not found, check recursively (deep scan)
                var allFiles = Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories);
                foreach (var filePath in allFiles)
                {
                    string fileName = Path.GetFileName(filePath);
                    foreach (var cand in candidates)
                    {
                        if (fileName.Equals(cand, System.StringComparison.OrdinalIgnoreCase))
                        {
                            return filePath;
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[SkinFileSystem] Error during case-insensitive search: {ex.Message}");
            }

            return null;
        }

        #endregion

        #region Defaults Management

        public async Task EnsureBaseSkinExists(string baseSkinFileName)
        {
            string destPath = Path.Combine(Application.persistentDataPath, SKINS_DIR, baseSkinFileName);
            if (File.Exists(destPath)) return;

            Debug.Log($"[SkinFileSystem] Base skin missing. Copying from StreamingAssets...");
            string sourcePath = Path.Combine(Application.streamingAssetsPath, baseSkinFileName);
            
            byte[] data = await ReadStreamingAssetAsync(sourcePath);
            if (data != null)
            {
                EnsureDirectory(Path.GetDirectoryName(destPath));
                await File.WriteAllBytesAsync(destPath, data);
            }
        }

        public async Task EnsureDemoSkinsExist()
        {
            if (PlayerPrefs.GetInt("DemoSkinsCopied", 0) == 1) return;

            Debug.Log("[SkinFileSystem] First run: Copying demo skins...");
            
            string manifestPath = Path.Combine(Application.streamingAssetsPath, DEMO_SKINS_DIR, "manifest.txt").Replace("\\", "/");
            string manifestText = await ReadStreamingAssetTextAsync(manifestPath);

            if (!string.IsNullOrEmpty(manifestText))
            {
                string[] files = manifestText.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
                foreach (var file in files)
                {
                    string cleanName = file.Trim();
                    if (string.IsNullOrEmpty(cleanName)) continue;

                    string source = Path.Combine(Application.streamingAssetsPath, DEMO_SKINS_DIR, cleanName).Replace("\\", "/");
                    string dest = Path.Combine(Application.persistentDataPath, "skins", cleanName); // strict lowercase 'skins' in original code, keeping consistent? Original used "skins" (lowercase) in one place and "Skins" (Capital) in another.
                    // Actually Manager used "skins" for demos and "Skins" for base. Let's stick to "skins" (lowercase) for demos as per Manager code, OR unify. 
                    // To be safe I'll use Path.Combine(Application.persistentDataPath, "skins", cleanName) as per original Manager logic.
                    
                    if (File.Exists(dest)) continue;

                    byte[] data = await ReadStreamingAssetAsync(source);
                    if (data != null)
                    {
                        EnsureDirectory(Path.GetDirectoryName(dest));
                        await File.WriteAllBytesAsync(dest, data);
                    }
                }
            }

            PlayerPrefs.SetInt("DemoSkinsCopied", 1);
            PlayerPrefs.Save();
        }

        public async Task EnsureDefaultSkinExists()
        {
            var dest = Path.Combine(Application.persistentDataPath, "skins", "Simplicity.wsz");
            if (!File.Exists(dest))
            {
                Debug.Log("[SkinFileSystem] Default skin missing. Copying from StreamingAssets...");
                var source = Path.Combine(Application.streamingAssetsPath, DEMO_SKINS_DIR, "Simplicity.wsz").Replace("\\", "/");
                var data = await ReadStreamingAssetAsync(source);
                if (data != null)
                {
                    EnsureDirectory(Path.GetDirectoryName(dest));
                    await File.WriteAllBytesAsync(dest, data);
                }
            }
        }

        #endregion

        #region Helpers

        private void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        }

        private async Task<byte[]> ReadStreamingAssetAsync(string path)
        {
            if (path.Contains("://") || Application.platform == RuntimePlatform.Android)
            {
                using (var wr = UnityEngine.Networking.UnityWebRequest.Get(path))
                {
                    var op = wr.SendWebRequest();
                    while (!op.isDone) await Task.Yield();

                    if (wr.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                        return wr.downloadHandler.data;
                    
                    Debug.LogWarning($"[SkinFileSystem] Failed to read streaming asset: {path} ({wr.error})");
                    return null;
                }
            }
            else
            {
                if (File.Exists(path)) return await File.ReadAllBytesAsync(path);
                return null;
            }
        }

        private async Task<string> ReadStreamingAssetTextAsync(string path)
        {
             byte[] data = await ReadStreamingAssetAsync(path);
             return data != null ? System.Text.Encoding.UTF8.GetString(data) : null;
        }

        #endregion
    }
}
