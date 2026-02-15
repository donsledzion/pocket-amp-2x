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
                    
                    // Slice Title Bar
                    currentSkin.TitleBar = WinampSkinSlicer.SliceSprite(mainTex, WinampSkinSlicer.TitleBar);

                    // Slice Title Bar Buttons
                    currentSkin.MinimizeBtn_Normal = WinampSkinSlicer.SliceSprite(mainTex, WinampSkinSlicer.MinimizeButton);
                    currentSkin.MinimizeBtn_Pressed = WinampSkinSlicer.SliceSprite(mainTex, WinampSkinSlicer.MinimizeButtonPressed);
                    currentSkin.CloseBtn_Normal = WinampSkinSlicer.SliceSprite(mainTex, WinampSkinSlicer.CloseButton);
                    currentSkin.CloseBtn_Pressed = WinampSkinSlicer.SliceSprite(mainTex, WinampSkinSlicer.CloseButtonPressed);
                    
                    // Slice Shuffle Buttons
                    currentSkin.Shuffle_Off_Normal = WinampSkinSlicer.SliceSprite(mainTex, WinampSkinSlicer.ShuffleButtonOff);
                    currentSkin.Shuffle_Off_Pressed = WinampSkinSlicer.SliceSprite(mainTex, WinampSkinSlicer.ShuffleButtonOffPressed);
                    currentSkin.Shuffle_On_Normal = WinampSkinSlicer.SliceSprite(mainTex, WinampSkinSlicer.ShuffleButtonOn);
                    currentSkin.Shuffle_On_Pressed = WinampSkinSlicer.SliceSprite(mainTex, WinampSkinSlicer.ShuffleButtonOnPressed);

                    // Slice Repeat Buttons
                    currentSkin.Repeat_Off_Normal = WinampSkinSlicer.SliceSprite(mainTex, WinampSkinSlicer.RepeatButtonOff);
                    currentSkin.Repeat_Off_Pressed = WinampSkinSlicer.SliceSprite(mainTex, WinampSkinSlicer.RepeatButtonOffPressed);
                    currentSkin.Repeat_On_Normal = WinampSkinSlicer.SliceSprite(mainTex, WinampSkinSlicer.RepeatButtonOn);
                    currentSkin.Repeat_On_Pressed = WinampSkinSlicer.SliceSprite(mainTex, WinampSkinSlicer.RepeatButtonOnPressed);

                    // Now load CButtons
                    WinampSkinImporter.Instance.LoadCButtonsBmp((cbuttonsTex) =>
                    {
                        if (cbuttonsTex != null)
                        {
                             currentSkin.PlayBtn_Normal = WinampSkinSlicer.SliceSprite(cbuttonsTex, WinampSkinSlicer.CButtons.Play);
                             currentSkin.PlayBtn_Pressed = WinampSkinSlicer.SliceSprite(cbuttonsTex, WinampSkinSlicer.CButtons.PlayPressed);
                             
                             currentSkin.PauseBtn_Normal = WinampSkinSlicer.SliceSprite(cbuttonsTex, WinampSkinSlicer.CButtons.Pause);
                             currentSkin.PauseBtn_Pressed = WinampSkinSlicer.SliceSprite(cbuttonsTex, WinampSkinSlicer.CButtons.PausePressed);
                             
                             currentSkin.StopBtn_Normal = WinampSkinSlicer.SliceSprite(cbuttonsTex, WinampSkinSlicer.CButtons.Stop);
                             currentSkin.StopBtn_Pressed = WinampSkinSlicer.SliceSprite(cbuttonsTex, WinampSkinSlicer.CButtons.StopPressed);
                             
                             currentSkin.PrevBtn_Normal = WinampSkinSlicer.SliceSprite(cbuttonsTex, WinampSkinSlicer.CButtons.Previous);
                             currentSkin.PrevBtn_Pressed = WinampSkinSlicer.SliceSprite(cbuttonsTex, WinampSkinSlicer.CButtons.PreviousPressed);
                             
                             currentSkin.NextBtn_Normal = WinampSkinSlicer.SliceSprite(cbuttonsTex, WinampSkinSlicer.CButtons.Next);
                             currentSkin.NextBtn_Pressed = WinampSkinSlicer.SliceSprite(cbuttonsTex, WinampSkinSlicer.CButtons.NextPressed);
                             
                             currentSkin.EjectBtn_Normal = WinampSkinSlicer.SliceSprite(cbuttonsTex, WinampSkinSlicer.CButtons.Eject);
                             currentSkin.EjectBtn_Pressed = WinampSkinSlicer.SliceSprite(cbuttonsTex, WinampSkinSlicer.CButtons.EjectPressed);
                        }
                        
                        // Done loading main.bmp and cbuttons.bmp? Now apply!
                        ApplySkinToHierarchy();
                    });
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
