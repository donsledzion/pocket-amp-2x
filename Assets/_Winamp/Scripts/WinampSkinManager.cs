using System.Collections;
using UnityEngine;
using com.cyborgAssets.inspectorButtonPro;
using SoftAware.Winamp; // Required for Main class

namespace SoftAware
{
    public class WinampSkinManager : MonoBehaviour
    {
        [SerializeField] private bool loadOnStart;
        [Header("Settings")]
        [SerializeField] private string testSkinPath = "";
        [SerializeField] private string persistentSkinFileName = "skin.wsz";
        [SerializeField] private bool loadFromPersistentOnStart = true;
        
        [SerializeField] private string baseSkinFileName = "base.wsz";
        [SerializeField] private bool useBaseSkinFallback = true;
        
        [Header("Hierarchy References")]
        [SerializeField] private Main mainController;
        [SerializeField] private WinampPlaylistUI playlistUI;
        
        [Header("Runtime Data")]
        [SerializeField] private WinampSkin currentSkin;
        
        // Use the new SkinFileSystem directly for setup tasks
        private SkinFileSystem skinFileSystem;

        public static WinampSkinManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
            
            skinFileSystem = new SkinFileSystem();
        }

        private IEnumerator Start()
        {
            // Wait for systems to initialize
            yield return null;

            // 0. Ensure Base Skin Exists
            if (useBaseSkinFallback)
            {
                var copyTask = skinFileSystem.EnsureBaseSkinExists(baseSkinFileName);
                yield return new WaitUntil(() => copyTask.IsCompleted);
            }

            // 0.5 Ensure Demo Skins
            var demoTask = skinFileSystem.EnsureDemoSkinsExist();
            yield return new WaitUntil(() => demoTask.IsCompleted);

            string skinToLoad = "";
            bool foundSkin = false;

            // 1. Priority: Settings (Last used skin)
            if (SettingsManager.Instance != null && !string.IsNullOrEmpty(SettingsManager.Instance.LastSkinPath))
            {
                string savedPath = SettingsManager.Instance.LastSkinPath;
                if (System.IO.File.Exists(savedPath))
                {
                    Debug.Log($"[WinampSkinManager] Loading saved skin from Settings: {savedPath}");
                    skinToLoad = savedPath;
                    foundSkin = true;
                }
            }

            // 2. Fallback: Persistent Data Path
            if (!foundSkin && loadFromPersistentOnStart)
            {
                string pPath = System.IO.Path.Combine(Application.persistentDataPath, persistentSkinFileName);
                if (System.IO.File.Exists(pPath))
                {
                    Debug.Log($"[WinampSkinManager] Loading persistent fallback skin: {pPath}");
                    skinToLoad = pPath;
                    foundSkin = true;
                }
            }

            // 3. Fallback: Base Skin
            if (!foundSkin && useBaseSkinFallback)
            {
                string basePath = System.IO.Path.Combine(Application.persistentDataPath, "Skins", baseSkinFileName);
                if (System.IO.File.Exists(basePath))
                {
                    Debug.Log($"[WinampSkinManager] Loading BASE skin: {basePath}");
                    skinToLoad = basePath;
                    foundSkin = true;
                }
            }

            // 4. Fallback: Test Skin Path
            if (!foundSkin && loadOnStart && !string.IsNullOrEmpty(testSkinPath))
            {
                 Debug.Log($"[WinampSkinManager] Loading test skin: {testSkinPath}");
                 skinToLoad = testSkinPath;
                 foundSkin = true;
            }

            // Execute Load
            if (foundSkin && !string.IsNullOrEmpty(skinToLoad))
            {
                var loadTask = LoadSkin(skinToLoad);
            }
        }
        
        [ProPlayButton]
        public void LoadFromPersistentPath()
        {
            string pPath = System.IO.Path.Combine(Application.persistentDataPath, persistentSkinFileName);
            LoadSkin(pPath);
        }

        /// <summary>
        /// Loads a skin from the given path.
        /// </summary>
        public async System.Threading.Tasks.Task<bool> LoadSkin(string absolutePath)
        {
            if (System.IO.File.Exists(absolutePath))
            {
                Debug.Log($"[WinampSkinManager] Loading skin from: {absolutePath}");
                testSkinPath = absolutePath;
                
                try 
                {
                    await LoadAndApplyTestSkin(); // Renamed internally but keeps logic
                    if (SettingsManager.Instance != null)
                    {
                        SettingsManager.Instance.LastSkinPath = absolutePath;
                        Debug.Log("[WinampSkinManager] Saved LastSkinPath to Settings.");
                    }
                    return true;
                }
                catch (System.Exception ex)
                {
                     Debug.LogError($"[WinampSkinManager] Failed to load skin: {ex.Message}");
                     return false;
                }
            }
            else
            {
                Debug.LogWarning($"[WinampSkinManager] Skin file not found at: {absolutePath}");
                return false;
            }
        }

        [ProPlayButton]
        public async System.Threading.Tasks.Task LoadAndApplyTestSkin()
        {
            if (string.IsNullOrEmpty(testSkinPath))
            {
                Debug.LogError("Test Skin Path is empty!");
                return;
            }

            Debug.Log($"[WinampSkinManager] Starting Async Skin Loading from: {testSkinPath}");

            // 1. Unpack
            string unpackedDir = WinampSkinImporter.Instance.UnpackWsz(testSkinPath);
            if (unpackedDir == null) return;
            
            string skinName = System.IO.Path.GetFileNameWithoutExtension(testSkinPath);

            // 2. Load via Importer Facade
            try
            {
                this.currentSkin = await WinampSkinImporter.Instance.LoadSkinAsync(skinName);
            }
            catch (System.Exception ex)
            {
                 Debug.LogError($"[WinampSkinManager] CRITICAL: Failed to load skin assets: {ex.Message}\n{ex.StackTrace}");
                 return;
            }

            // 3. Apply
            ApplySkinToHierarchy();
        }

        private void ApplySkinToHierarchy()
        {
            if (currentSkin == null) return;
            
            Debug.Log("[WinampSkinManager] Applying skin to hierarchy...");

            // Update Global Font
            if (TextSpriteProvider.Instance != null && currentSkin.TextSprites != null)
            {
                TextSpriteProvider.Instance.ApplySkin(currentSkin.TextSprites);
            }

            // Distribute to known applicators
            if (mainController != null) 
            {
                mainController.ApplySkin(currentSkin);
            }
            if (playlistUI != null)
            {
                playlistUI.ApplySkin(currentSkin);
            }
        }
    }
}
