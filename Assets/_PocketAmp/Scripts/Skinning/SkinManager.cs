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

            // 0.6 Ensure Default Skin Exists
            var defaultSkinTask = skinFileSystem.EnsureDefaultSkinExists(defaultSkinFileName);
            yield return new WaitUntil(() => defaultSkinTask.IsCompleted);

            var skinToLoad = "";
            var foundSkin = false;

            // 1. Priority: Settings (Last used skin)
            if (SettingsManager.Instance != null && !string.IsNullOrEmpty(SettingsManager.Instance.LastSkinPath))
            {
                var savedPath = SettingsManager.Instance.LastSkinPath;
                if (System.IO.File.Exists(savedPath))
                {
                    Debug.Log($"[PocketAmpSkinManager] Loading saved skin from Settings: {savedPath}");
                    skinToLoad = savedPath;
                    foundSkin = true;
                }
            }

            // 2. Fallback: Persistent Data Path
            if (!foundSkin && loadFromPersistentOnStart)
            {
                var pPath = System.IO.Path.Combine(Application.persistentDataPath, persistentSkinFileName);
                if (System.IO.File.Exists(pPath))
                {
                    Debug.Log($"[PocketAmpSkinManager] Loading persistent fallback skin: {pPath}");
                    skinToLoad = pPath;
                    foundSkin = true;
                }
            }

            // 2.5 Fallback: Default Skin
            if (!foundSkin)
            {
                var defaultSkinPath = DefaultSkinPath;
                if (System.IO.File.Exists(defaultSkinPath))
                {
                    Debug.Log($"[PocketAmpSkinManager] Loading DEFAULT skin: {defaultSkinPath}");
                    skinToLoad = defaultSkinPath;
                    foundSkin = true;
                }
            }

            // 3. Fallback: Base Skin
            if (!foundSkin && useBaseSkinFallback)
            {
                var basePath = System.IO.Path.Combine(Application.persistentDataPath, "Skins", baseSkinFileName);
                if (System.IO.File.Exists(basePath))
                {
                    Debug.Log($"[PocketAmpSkinManager] Loading BASE skin: {basePath}");
                    skinToLoad = basePath;
                    foundSkin = true;
                }
            }

            // 4. Fallback: Test Skin Path
            if (!foundSkin && loadOnStart && !string.IsNullOrEmpty(testSkinPath))
            {
                 Debug.Log($"[PocketAmpSkinManager] Loading test skin: {testSkinPath}");
                 skinToLoad = testSkinPath;
                 foundSkin = true;
            }

            // Execute Load
            if (foundSkin && !string.IsNullOrEmpty(skinToLoad))
            {
                var loadTask = LoadSkin(skinToLoad);
            }
            else
            {
                ShowUI();
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
                ShowUI();
                return;
            }

            Debug.Log($"[PocketAmpSkinManager] Starting Async Skin Loading from: {testSkinPath}");

            // 1. Unpack
            string unpackedDir = SkinImporter.Instance.UnpackWsz(testSkinPath);
            if (unpackedDir == null)
            {
                ShowUI();
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
                 ShowUI();
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

            ShowUI();
        }

        private void ShowUI()
        {
            if (playerUiCanvasGroup != null)
            {
                playerUiCanvasGroup.alpha = 1f;
            }
        }
    }
}
