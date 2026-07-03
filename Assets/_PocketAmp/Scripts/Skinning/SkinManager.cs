using System.Collections;
using UnityEngine;
using com.cyborgAssets.inspectorButtonPro;
using SoftAware.PocketAmp; // Required for Main class

namespace SoftAware
{
    public class SkinManager : MonoBehaviour
    {
        [SerializeField] private bool loadOnStart;
        [Header("Settings")]
        [SerializeField] private string testSkinPath = "";
        [SerializeField] private string persistentSkinFileName = "skin.wsz";
        [SerializeField] private bool loadFromPersistentOnStart = true;
        
        [SerializeField] private string baseSkinFileName = "base.wsz";
        [SerializeField] private bool useBaseSkinFallback = true;
        
        [SerializeField] private string defaultSkinFileName = "Simplicity.wsz";
        public string DefaultSkinPath => System.IO.Path.Combine(Application.persistentDataPath, "skins", defaultSkinFileName);
        
        [Header("Runtime Data")]
        [SerializeField] private Skin currentSkin;
        
        // Use the new SkinFileSystem directly for setup tasks
        private SkinFileSystem skinFileSystem;
        private CanvasGroup playerUiCanvasGroup;

        private static Main main => Refs.Main;
        private static PlaylistUI playlistUI => Refs.PlaylistUI;

        //public static SkinManager Instance { get; private set; }

        private void Awake()
        {
            skinFileSystem = new SkinFileSystem();
            
            var mainInstance = Object.FindAnyObjectByType<Main>();
            if (mainInstance != null)
            {
                var canvas = mainInstance.GetComponentInParent<Canvas>();
                if (canvas != null)
                {
                    playerUiCanvasGroup = canvas.GetComponent<CanvasGroup>();
                    if (playerUiCanvasGroup == null)
                    {
                        playerUiCanvasGroup = canvas.gameObject.AddComponent<CanvasGroup>();
                    }
                    playerUiCanvasGroup.alpha = 0f;
                }
            }
        }

        public async System.Threading.Tasks.Task InitializeAsync()
        {
            Debug.Log("[DIAG] SkinManager.InitializeAsync() - Oczekuje na inicjalizacje...");
            // Wait for systems to initialize
            await System.Threading.Tasks.Task.Yield();
            Debug.Log("[DIAG] SkinManager.InitializeAsync() - Po yield. Sprawdzam Base Skin...");

            // 0. Ensure Base Skin Exists
            if (useBaseSkinFallback)
            {
                await skinFileSystem.EnsureBaseSkinExists(baseSkinFileName);
            }

            var demoTask = skinFileSystem.EnsureDemoSkinsExist();
            await demoTask;
            Debug.Log("[DIAG] SkinManager.InitializeAsync() - Demo skins done");

            // 1. Ensure Default Skin
            await skinFileSystem.EnsureDefaultSkinExists(defaultSkinFileName);

            var skinToLoad = "";
            var foundSkin = false;

            // 1.5 Check Last Skin
            var lastSkin = SettingsManager.Instance?.LastSkinPath;
            if (!string.IsNullOrEmpty(lastSkin) && System.IO.File.Exists(lastSkin))
            {
                skinToLoad = lastSkin;
                foundSkin = true;
            }

            if (!foundSkin)
            {
                var defaultSkinPath = DefaultSkinPath;
                if (System.IO.File.Exists(defaultSkinPath))
                {
                    skinToLoad = defaultSkinPath;
                    foundSkin = true;
                }
            }

            if (foundSkin && !string.IsNullOrEmpty(skinToLoad))
            {
                Debug.Log($"[DIAG] SkinManager.InitializeAsync() - Wywoluje LoadSkin dla: {skinToLoad}");
                await LoadSkin(skinToLoad);
            }
            else
            {
                Debug.LogWarning("[PocketAmpSkinManager] No skins found to load at startup.");
            }
        }

        /// <summary>
        /// Loads a skin from the given path.
        /// </summary>
        internal async System.Threading.Tasks.Task<bool> LoadSkin(string absolutePath)
        {
            if (System.IO.File.Exists(absolutePath))
            {
                Debug.Log($"[PocketAmpSkinManager] Loading skin from: {absolutePath}");
                testSkinPath = absolutePath;
                
                try 
                {
                    await LoadAndApplyTestSkin(); // Renamed internally but keeps logic
                    if (SettingsManager.Instance == null) return true;
                    SettingsManager.Instance.LastSkinPath = absolutePath;
                    Debug.Log("[PocketAmpSkinManager] Saved LastSkinPath to Settings.");
                    return true;
                }
                catch (System.Exception ex)
                {
                     Debug.LogError($"[PocketAmpSkinManager] Failed to load skin: {ex.Message}");
                     return false;
                }
            }
            else
            {
                Debug.LogWarning($"[PocketAmpSkinManager] Skin file not found at: {absolutePath}");
                return false;
            }
        }

        [ProPlayButton]
        private async System.Threading.Tasks.Task LoadAndApplyTestSkin()
        {
            if (string.IsNullOrEmpty(testSkinPath))
            {
                Debug.LogError("Test Skin Path is empty!");
                return;
            }

            Debug.Log($"[PocketAmpSkinManager] Starting Async Skin Loading from: {testSkinPath}");

            // 1. Unpack
            string unpackedDir = SkinImporter.Instance.UnpackWsz(testSkinPath);
            if (unpackedDir == null)
            {
                return;
            }
            
            string skinName = System.IO.Path.GetFileNameWithoutExtension(testSkinPath);

            // 2. Load via Importer Facade
            try
            {
                this.currentSkin = await SkinImporter.Instance.LoadSkinAsync(skinName);
                ApplySkinToHierarchy();
            }
            catch (System.Exception ex)
            {
                 Debug.LogError($"[PocketAmpSkinManager] CRITICAL: Failed to load skin assets: {ex.Message}\n{ex.StackTrace}");
                 return;
            }
        }

        private void ApplySkinToHierarchy()
        {
            if (currentSkin == null) return;
            
            Debug.Log("[PocketAmpSkinManager] Applying skin to hierarchy...");

            // Update Global Font
            if (TextSpriteProvider.Instance != null && currentSkin.TextSprites != null)
            {
                TextSpriteProvider.Instance.ApplySkin(currentSkin.TextSprites);
            }

            // Distribute to known applicators
            main.ApplySkin(currentSkin);
            playlistUI.ApplySkin(currentSkin);
        }
    }
}
