using UnityEngine;
using System.Collections;
using System.IO;
using System.IO.Compression;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SoftAware
{
    /// <summary>
    /// Runtime importer for Winamp 2.x skins (.wsz files)
    /// Handles file picking, unpacking, and texture loading
    /// </summary>
    public class WinampSkinImporter : MonoBehaviour
    {
        [Header("Debug Settings")]
        [SerializeField] private bool verboseLogging = true;
        
        [Header("Test Settings (Editor Only)")]
        [SerializeField] private string testWszPath = "";
        
        private string lastUnpackedSkinPath = "";
        private string lastUnpackedSkinName = "";
        
        public static WinampSkinImporter Instance { get; private set; }
        
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
        
        #region File Picking
        
        /// <summary>
        /// Opens file picker to select a .wsz skin file
        /// Uses EditorUtility in Editor, NativeFilePicker on Android
        /// </summary>
        public void PickSkinFile()
        {
#if UNITY_EDITOR
            PickSkinFileEditor();
#else
            PickSkinFileAndroid();
#endif
        }
        
#if UNITY_EDITOR
        private void PickSkinFileEditor()
        {
            string path = EditorUtility.OpenFilePanel("Select Winamp Skin (.wsz)", "", "wsz");
            
            if (string.IsNullOrEmpty(path))
            {
                Log("File picking cancelled");
                return;
            }
            
            Log($"Selected file: {path}");
            UnpackWsz(path);
        }
#endif
        
        private void PickSkinFileAndroid()
        {
            // TODO: Integrate NativeFilePicker plugin when available
            // NativeFilePicker.PickFile((path) =>
            // {
            //     if (path == null)
            //     {
            //         Log("File picking cancelled");
            //         return;
            //     }
            //     
            //     Log($"Selected file: {path}");
            //     UnpackWsz(path);
            // }, new string[] { "wsz" });
            
            Debug.LogWarning("NativeFilePicker not implemented yet. Use testWszPath for testing.");
        }
        
        #endregion
        
        #region Unpacking
        
        /// <summary>
        /// Unpacks a .wsz (ZIP) file to persistentDataPath/Skins/[skinName]
        /// </summary>
        public void UnpackWsz(string wszPath)
        {
            if (!File.Exists(wszPath))
            {
                Debug.LogError($"File does not exist: {wszPath}");
                return;
            }
            
            string skinName = Path.GetFileNameWithoutExtension(wszPath);
            string outputDir = Path.Combine(Application.persistentDataPath, "Skins", skinName);
            
            Log($"Unpacking skin '{skinName}' to: {outputDir}");
            
            try
            {
                // Delete existing skin directory if it exists
                if (Directory.Exists(outputDir))
                {
                    Log($"Deleting existing skin directory: {outputDir}");
                    Directory.Delete(outputDir, true);
                }
                
                // Create output directory
                Directory.CreateDirectory(outputDir);
                
                // Extract ZIP file
                ZipFile.ExtractToDirectory(wszPath, outputDir);
                
                lastUnpackedSkinPath = outputDir;
                lastUnpackedSkinName = skinName;
                
                Log($"Successfully unpacked skin '{skinName}'");
                Log($"Files extracted to: {outputDir}");
                
                // List unpacked files for debugging
                if (verboseLogging)
                {
                    ListUnpackedFiles(skinName);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to unpack skin: {ex.Message}\n{ex.StackTrace}");
            }
        }
        
        #endregion
        
        #region Texture Loading
        
        private Texture2D lastLoadedTexture = null;
        
        /// <summary>
        /// Loads a BMP texture from the specified path
        /// Uses UnityWebRequest for runtime loading
        /// </summary>
        public void LoadTexture(string bmpPath, System.Action<Texture2D> onComplete)
        {
            if (!File.Exists(bmpPath))
            {
                Debug.LogError($"Texture file does not exist: {bmpPath}");
                onComplete?.Invoke(null);
                return;
            }
            
            StartCoroutine(LoadTextureCoroutine(bmpPath, onComplete));
        }
        
        private IEnumerator LoadTextureCoroutine(string path, System.Action<Texture2D> onComplete)
        {
            Log($"Loading texture: {path}");
            
            Texture2D tex = null;
            
            try
            {
                // Use custom BMP loader (ImageConversion doesn't support BMP)
                tex = BMPLoader.LoadBMP(path);
                
                if (tex != null)
                {
                    Log($"Texture loaded successfully: {tex.width}x{tex.height}, format: {tex.format}");
                    
                    // Remove magenta transparency
                    RemoveMagenta(tex);
                    
                    lastLoadedTexture = tex;
                    onComplete?.Invoke(tex);
                }
                else
                {
                    Debug.LogError($"Failed to decode BMP from: {path}");
                    onComplete?.Invoke(null);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to load texture: {ex.Message}\nPath: {path}\n{ex.StackTrace}");
                if (tex != null) Destroy(tex);
                onComplete?.Invoke(null);
            }
            
            yield return null;
        }
        
        /// <summary>
        /// Removes magenta color (#FF00FF) by making it transparent
        /// This is the standard transparency color in Winamp skins
        /// </summary>
        private void RemoveMagenta(Texture2D tex)
        {
            if (tex == null) return;
            
            Log("Removing magenta transparency...");
            
            Color[] pixels = tex.GetPixels();
            int magentaCount = 0;
            
            // Magenta color with some tolerance for compression artifacts
            const float tolerance = 0.05f;
            
            for (int i = 0; i < pixels.Length; i++)
            {
                Color pixel = pixels[i];
                
                // Check if pixel is close to magenta (R=1, G=0, B=1)
                if (Mathf.Abs(pixel.r - 1f) < tolerance && 
                    pixel.g < tolerance && 
                    Mathf.Abs(pixel.b - 1f) < tolerance)
                {
                    pixels[i] = new Color(pixel.r, pixel.g, pixel.b, 0f); // Make transparent
                    magentaCount++;
                }
            }
            
            tex.SetPixels(pixels);
            tex.Apply();
            
            Log($"Removed {magentaCount} magenta pixels ({(magentaCount * 100f / pixels.Length):F2}% of image)");
        }
        
        /// <summary>
        /// Loads main.bmp from the last unpacked skin
        /// </summary>
        public void LoadMainBmp(System.Action<Texture2D> onComplete)
        {
            if (string.IsNullOrEmpty(lastUnpackedSkinPath))
            {
                Debug.LogWarning("No skin has been unpacked yet.");
                onComplete?.Invoke(null);
                return;
            }
            
            string mainBmpPath = Path.Combine(lastUnpackedSkinPath, "main.bmp");
            LoadTexture(mainBmpPath, onComplete);
        }
        
        #endregion
        
        #region Debug Methods
        
        /// <summary>
        /// Lists all files in the unpacked skin directory
        /// </summary>
        public void ListUnpackedFiles(string skinName)
        {
            string skinDir = Path.Combine(Application.persistentDataPath, "Skins", skinName);
            
            if (!Directory.Exists(skinDir))
            {
                Debug.LogWarning($"Skin directory does not exist: {skinDir}");
                return;
            }
            
            string[] files = Directory.GetFiles(skinDir, "*.*", SearchOption.AllDirectories);
            
            Log($"=== Files in skin '{skinName}' ({files.Length} files) ===");
            foreach (string file in files)
            {
                string relativePath = file.Replace(skinDir, "").TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                FileInfo info = new FileInfo(file);
                Log($"  {relativePath} ({info.Length} bytes)");
            }
            Log("=== End of file list ===");
        }
        
        private void Log(string message)
        {
            if (verboseLogging)
            {
                Debug.Log($"[WinampSkinImporter] {message}");
            }
        }
        
        #endregion
        
        #region Context Menu (Testing)
        
        [ContextMenu("Test: Pick and Unpack Skin")]
        private void TestPickAndUnpack()
        {
            PickSkinFile();
        }
        
        [ContextMenu("Test: List Unpacked Files")]
        private void TestListUnpackedFiles()
        {
            if (string.IsNullOrEmpty(lastUnpackedSkinName))
            {
                Debug.LogWarning("No skin has been unpacked yet. Use 'Test: Pick and Unpack Skin' first.");
                return;
            }
            
            ListUnpackedFiles(lastUnpackedSkinName);
        }
        
        [ContextMenu("Test: Unpack Test WSZ Path")]
        private void TestUnpackTestPath()
        {
            if (string.IsNullOrEmpty(testWszPath))
            {
                Debug.LogWarning("testWszPath is empty. Set it in the Inspector first.");
                return;
            }
            
            UnpackWsz(testWszPath);
        }
        
        [ContextMenu("Test: Load Main BMP")]
        private void TestLoadMainBmp()
        {
            LoadMainBmp((tex) =>
            {
                if (tex != null)
                {
                    Debug.Log($"[TEST] Successfully loaded main.bmp: {tex.width}x{tex.height}");
                }
                else
                {
                    Debug.LogError("[TEST] Failed to load main.bmp");
                }
            });
        }
        
        [ContextMenu("Test: Show Texture Info")]
        private void TestShowTextureInfo()
        {
            if (lastLoadedTexture == null)
            {
                Debug.LogWarning("No texture loaded yet. Use 'Test: Load Main BMP' first.");
                return;
            }
            
            Debug.Log($"=== Texture Info ===");
            Debug.Log($"Size: {lastLoadedTexture.width}x{lastLoadedTexture.height}");
            Debug.Log($"Format: {lastLoadedTexture.format}");
            Debug.Log($"Mipmap: {lastLoadedTexture.mipmapCount}");
            Debug.Log($"Filter Mode: {lastLoadedTexture.filterMode}");
            Debug.Log($"Wrap Mode: {lastLoadedTexture.wrapMode}");
            Debug.Log($"===================");
        }
        
        #endregion
    }
}
