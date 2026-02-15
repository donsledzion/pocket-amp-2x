using UnityEngine;
using com.cyborgAssets.inspectorButtonPro;
using SoftAware.Winamp; // Required for Main class

namespace SoftAware
{
    public class WinampSkinManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private string testSkinPath = "";
        
        [Header("Hierarchy References")]
        [SerializeField] private Main mainController;
        
        [Header("Runtime Data")]
        [SerializeField] private WinampSkin currentSkin;

        public static WinampSkinManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        [ProPlayButton]
        public void LoadAndApplyTestSkin()
        {
            if (string.IsNullOrEmpty(testSkinPath))
            {
                Debug.LogError("Test Skin Path is empty!");
                return;
            }

            Debug.Log($"[WinampSkinManager] Starting skin load from: {testSkinPath}");

            // 1. Unpack (or just use path if already unpacked folder? Importer handles logic)
            // Let's assume Importer is already set up to Unpack
            WinampSkinImporter.Instance.UnpackWsz(testSkinPath);
            
            // 2. Load Textures & Create Skin Data
            WinampSkinImporter.Instance.LoadMainBmp((mainTex) => 
            {
                if (mainTex != null)
                {
                    if (currentSkin == null) currentSkin = new WinampSkin();
                    currentSkin.SkinName = System.IO.Path.GetFileNameWithoutExtension(testSkinPath);

                    // Slice Main Background
                    currentSkin.MainBackground = WinampSkinSlicer.SliceSprite(mainTex, WinampSkinSlicer.MainPanel);
                    
                    // Done loading main.bmp? Now apply!
                    ApplySkinToHierarchy();
                }
            });
            
            // TODO: Load CButtons, etc.
        }

        private void ApplySkinToHierarchy()
        {
            if (currentSkin == null) return;
            
            Debug.Log("[WinampSkinManager] Applying skin to hierarchy...");

            // Distribute to known applicators
            if (mainController != null) 
            {
                mainController.ApplySkin(currentSkin);
            }
            else
            {
                Debug.LogWarning("[WinampSkinManager] MainController reference is missing!");
            }
        }
    }
}
