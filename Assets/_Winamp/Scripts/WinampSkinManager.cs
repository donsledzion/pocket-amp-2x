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
                    
                    // Try Load SHUFREP first
                    WinampSkinImporter.Instance.LoadShufRepBmp((shufrepTex) => 
                    {
                        if (shufrepTex != null)
                        {
                            // Slice from SHUFREP
                            Debug.Log("[WinampSkinManager] Slicing Shuffle/Repeat from SHUFREP.BMP");
                            currentSkin.Shuffle_Off_Normal = WinampSkinSlicer.SliceSprite(shufrepTex, WinampSkinSlicer.ShufRep.ShuffleOffNormal);
                            currentSkin.Shuffle_Off_Pressed = WinampSkinSlicer.SliceSprite(shufrepTex, WinampSkinSlicer.ShufRep.ShuffleOffPressed);
                            currentSkin.Shuffle_On_Normal = WinampSkinSlicer.SliceSprite(shufrepTex, WinampSkinSlicer.ShufRep.ShuffleOnNormal);
                            currentSkin.Shuffle_On_Pressed = WinampSkinSlicer.SliceSprite(shufrepTex, WinampSkinSlicer.ShufRep.ShuffleOnPressed);

                            currentSkin.Repeat_Off_Normal = WinampSkinSlicer.SliceSprite(shufrepTex, WinampSkinSlicer.ShufRep.RepeatOffNormal);
                            currentSkin.Repeat_Off_Pressed = WinampSkinSlicer.SliceSprite(shufrepTex, WinampSkinSlicer.ShufRep.RepeatOffPressed);
                            currentSkin.Repeat_On_Normal = WinampSkinSlicer.SliceSprite(shufrepTex, WinampSkinSlicer.ShufRep.RepeatOnNormal);
                            currentSkin.Repeat_On_Pressed = WinampSkinSlicer.SliceSprite(shufrepTex, WinampSkinSlicer.ShufRep.RepeatOnPressed);

                            // Slice EQ/PL from ShufRep
                            currentSkin.EQ_Off_Normal = WinampSkinSlicer.SliceSprite(shufrepTex, WinampSkinSlicer.ShufRep.EqOffNormal);
                            currentSkin.EQ_Off_Pressed = WinampSkinSlicer.SliceSprite(shufrepTex, WinampSkinSlicer.ShufRep.EqOffPressed);
                            currentSkin.EQ_On_Normal = WinampSkinSlicer.SliceSprite(shufrepTex, WinampSkinSlicer.ShufRep.EqOnNormal);
                            currentSkin.EQ_On_Pressed = WinampSkinSlicer.SliceSprite(shufrepTex, WinampSkinSlicer.ShufRep.EqOnPressed);

                            currentSkin.PL_Off_Normal = WinampSkinSlicer.SliceSprite(shufrepTex, WinampSkinSlicer.ShufRep.PlOffNormal);
                            currentSkin.PL_Off_Pressed = WinampSkinSlicer.SliceSprite(shufrepTex, WinampSkinSlicer.ShufRep.PlOffPressed);
                            currentSkin.PL_On_Normal = WinampSkinSlicer.SliceSprite(shufrepTex, WinampSkinSlicer.ShufRep.PlOnNormal);
                            currentSkin.PL_On_Pressed = WinampSkinSlicer.SliceSprite(shufrepTex, WinampSkinSlicer.ShufRep.PlOnPressed);
                        }
                        else
                        {
                            // Fallback to MAIN.BMP
                            Debug.Log("[WinampSkinManager] Slicing Shuffle/Repeat from MAIN.BMP (Fallback)");
                            currentSkin.Shuffle_Off_Normal = WinampSkinSlicer.SliceSprite(mainTex, WinampSkinSlicer.ShuffleButtonOff);
                            currentSkin.Shuffle_Off_Pressed = WinampSkinSlicer.SliceSprite(mainTex, WinampSkinSlicer.ShuffleButtonOffPressed);
                            currentSkin.Shuffle_On_Normal = WinampSkinSlicer.SliceSprite(mainTex, WinampSkinSlicer.ShuffleButtonOn);
                            currentSkin.Shuffle_On_Pressed = WinampSkinSlicer.SliceSprite(mainTex, WinampSkinSlicer.ShuffleButtonOnPressed);

                            currentSkin.Repeat_Off_Normal = WinampSkinSlicer.SliceSprite(mainTex, WinampSkinSlicer.RepeatButtonOff);
                            currentSkin.Repeat_Off_Pressed = WinampSkinSlicer.SliceSprite(mainTex, WinampSkinSlicer.RepeatButtonOffPressed);
                            currentSkin.Repeat_On_Normal = WinampSkinSlicer.SliceSprite(mainTex, WinampSkinSlicer.RepeatButtonOn);
                            currentSkin.Repeat_On_Pressed = WinampSkinSlicer.SliceSprite(mainTex, WinampSkinSlicer.RepeatButtonOnPressed);

                            // Fallback EQ/PL from MAIN.BMP
                            // Note: Classic Winamp Main.bmp only has "Off" and "On" states visually represented as simple toggles or specific regions?
                            // WinampSkinSlicer defines EqualizerButton (219, 58) 23x12.
                            // In standard skin, these might not have 4 distinct states available in Main.bmp easily accessible exactly like ShufRep.
                            // But usually they just toggle. Let's use the basic rects we have or reuse them.
                            // For now, let's use the defined EqualizerButton for Normal state, and maybe offset for Pressed if standard exists?
                            // Actually, standard Winamp 2.x Main.bmp has EQ/PL buttons at (219,58) and (242,58).
                            // They are simple toggles. 
                            // Let's just map them simply for fallback.
                            currentSkin.EQ_Off_Normal = WinampSkinSlicer.SliceSprite(mainTex, WinampSkinSlicer.EqualizerButton); 
                            currentSkin.EQ_On_Normal = currentSkin.EQ_Off_Normal; // Reuse or find correct on state?
                            // Actually properly implementing fallback for EQ/PL requires more research on Main.bmp states. 
                            // Usually they just light up.
                            // For safety, let's map what we have.
                            currentSkin.PL_Off_Normal = WinampSkinSlicer.SliceSprite(mainTex, WinampSkinSlicer.PlaylistButton);
                            currentSkin.PL_On_Normal = currentSkin.PL_Off_Normal;
                        }

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
                            
                            // Done loading main, shufrep, and cbuttons? Now MONOSTER
                            WinampSkinImporter.Instance.LoadMonoSterBmp((monosterTex) => 
                            {
                                if (monosterTex != null)
                                {
                                    Debug.Log("[WinampSkinManager] Slicing Mono/Stereo from MONOSTER.BMP");
                                    currentSkin.Stereo_Active = WinampSkinSlicer.SliceSprite(monosterTex, WinampSkinSlicer.MonoSter.StereoOn);
                                    currentSkin.Stereo_Inactive = WinampSkinSlicer.SliceSprite(monosterTex, WinampSkinSlicer.MonoSter.StereoOff);
                                    currentSkin.Mono_Inactive = WinampSkinSlicer.SliceSprite(monosterTex, WinampSkinSlicer.MonoSter.MonoOff);
                                }
                                
                                // Load Volume
                                WinampSkinImporter.Instance.LoadVolumeBmp((volumeTex) => {
                                    if (volumeTex != null)
                                    {
                                        Debug.Log("[WinampSkinManager] Slicing Volume from VOLUME.BMP");
                                        // Knobs at bottom (Y=0 in meta, so Y=Height-11 in slicer logic?)
                                        // Wait, SliceSprite expects Y from Top.
                                        // If VolumeTex provided, calculate Y.
                                        // Meta: Knob Y=0 (bottom).
                                        // Rect.y (top) = H - 0 - 11.
                                        float h = volumeTex.height;
                                        
                                        currentSkin.VolumeKnobNormal = WinampSkinSlicer.SliceSprite(volumeTex, new Rect(
                                            WinampSkinSlicer.Volume.KnobNormal.x, 
                                            h - WinampSkinSlicer.Volume.KnobNormal.y - WinampSkinSlicer.Volume.KnobNormal.height,
                                            WinampSkinSlicer.Volume.KnobNormal.width,
                                            WinampSkinSlicer.Volume.KnobNormal.height));

                                        currentSkin.VolumeKnobPressed = WinampSkinSlicer.SliceSprite(volumeTex, new Rect(
                                            WinampSkinSlicer.Volume.KnobPressed.x, 
                                            h - WinampSkinSlicer.Volume.KnobPressed.y - WinampSkinSlicer.Volume.KnobPressed.height,
                                            WinampSkinSlicer.Volume.KnobPressed.width,
                                            WinampSkinSlicer.Volume.KnobPressed.height));

                                        // Animation Frames
                                        // Meta: Frame 0 at Y=420. H=13. 
                                        // Top of Frame 0 = H - 420 - 13.
                                        // Meta: Frame 27 at Y=15.
                                        // Stride is 15.
                                        // Assuming Frame 0 is at offset 0 from "Top of Animation Block".
                                        // Let's assume the texture layout is standard.
                                        // Frame 0 Y (bottom) = 420.
                                        // Loop 0 to 27.
                                        currentSkin.VolumeAnimation = new Sprite[WinampSkinSlicer.Volume.FrameCount];
                                        for (int i = 0; i < WinampSkinSlicer.Volume.FrameCount; i++)
                                        {
                                            // Y from bottom for current frame: 420 - (i * 15) ?
                                            // Frame 0: 420. Frame 1: 405. ... Frame 27: 15. Correct.
                                            float yBottom = 420 - (i * WinampSkinSlicer.Volume.FrameStride);
                                            // Convert to Top-Down Y for SliceSprite
                                            float yTop = h - yBottom - WinampSkinSlicer.Volume.FrameHeight;
                                            
                                            currentSkin.VolumeAnimation[i] = WinampSkinSlicer.SliceSprite(volumeTex, new Rect(
                                                0, // Frame X is always 0
                                                yTop,
                                                WinampSkinSlicer.Volume.FrameWidth,
                                                WinampSkinSlicer.Volume.FrameHeight
                                            ));
                                        }
                                    }
                                    
                                    // Load Balance
                                    WinampSkinImporter.Instance.LoadBalanceBmp((balanceTex) => {
                                        if (balanceTex != null)
                                        {
                                            Debug.Log("[WinampSkinManager] Slicing Balance from BALANCE.BMP");
                                            float h = balanceTex.height;
                                            
                                            currentSkin.BalanceKnobNormal = WinampSkinSlicer.SliceSprite(balanceTex, new Rect(
                                                WinampSkinSlicer.Balance.KnobNormal.x,
                                                h - WinampSkinSlicer.Balance.KnobNormal.y - WinampSkinSlicer.Balance.KnobNormal.height,
                                                WinampSkinSlicer.Balance.KnobNormal.width,
                                                WinampSkinSlicer.Balance.KnobNormal.height));

                                            currentSkin.BalanceKnobPressed = WinampSkinSlicer.SliceSprite(balanceTex, new Rect(
                                                WinampSkinSlicer.Balance.KnobPressed.x,
                                                h - WinampSkinSlicer.Balance.KnobPressed.y - WinampSkinSlicer.Balance.KnobPressed.height,
                                                WinampSkinSlicer.Balance.KnobPressed.width,
                                                WinampSkinSlicer.Balance.KnobPressed.height));

                                            currentSkin.BalanceAnimation = new Sprite[WinampSkinSlicer.Balance.FrameCount];
                                            for (int i = 0; i < WinampSkinSlicer.Balance.FrameCount; i++)
                                            {
                                                float yBottom = 420 - (i * WinampSkinSlicer.Balance.FrameStride);
                                                float yTop = h - yBottom - WinampSkinSlicer.Balance.FrameHeight;
                                                
                                                currentSkin.BalanceAnimation[i] = WinampSkinSlicer.SliceSprite(balanceTex, new Rect(
                                                    9, // Frame X is 9 (meta says x=9 for BG sprites)
                                                    yTop,
                                                    WinampSkinSlicer.Balance.FrameWidth,
                                                    WinampSkinSlicer.Balance.FrameHeight
                                                ));
                                            }
                                        }
                                        
                                        // Finally apply!
                                        ApplySkinToHierarchy();
                                    });
                                });
                            });
                        });
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
