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
        
        [Header("Hierarchy References")]
        [SerializeField] private Main mainController;
        [SerializeField] private WinampPlaylistUI playlistUI;
        
        [Header("Runtime Data")]
        [SerializeField] private WinampSkin currentSkin;

        public static WinampSkinManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private IEnumerator Start()
        {
            if (!loadOnStart) yield break;
            yield return new WaitForSeconds(1f);
            LoadAndApplyTestSkin();
        }

        [ProPlayButton]
        public async void LoadAndApplyTestSkin()
        {
            if (string.IsNullOrEmpty(testSkinPath))
            {
                Debug.LogError("Test Skin Path is empty!");
                return;
            }

            if (currentSkin == null) currentSkin = new WinampSkin();
            currentSkin.SkinName = System.IO.Path.GetFileNameWithoutExtension(testSkinPath);

            Debug.Log($"[WinampSkinManager] Starting Async Skin Loading from: {testSkinPath}");

            // 1. Unpack
            WinampSkinImporter.Instance.UnpackWsz(testSkinPath);
            
            // 2. Load Main BMP
            Texture2D mainTex = await WinampSkinImporter.Instance.LoadMainBmpAsync();
            if (mainTex != null)
            {
                Debug.Log($"[WinampSkinManager] Slicing Main from {mainTex.name}");
                currentSkin.MainBackground = WinampSkinSlicer.SliceSprite(mainTex, WinampSkinSlicer.MainPanel);
                currentSkin.TitleBar = WinampSkinSlicer.SliceSprite(mainTex, WinampSkinSlicer.TitleBar);

                // Title bar buttons
                currentSkin.MinimizeBtn_Normal = WinampSkinSlicer.SliceSprite(mainTex, WinampSkinSlicer.MinimizeButton);
                currentSkin.MinimizeBtn_Pressed = WinampSkinSlicer.SliceSprite(mainTex, WinampSkinSlicer.MinimizeButtonPressed);
                currentSkin.CloseBtn_Normal = WinampSkinSlicer.SliceSprite(mainTex, WinampSkinSlicer.CloseButton);
                currentSkin.CloseBtn_Pressed = WinampSkinSlicer.SliceSprite(mainTex, WinampSkinSlicer.CloseButtonPressed);
            }

            // 3. Load ShufRep
            Texture2D shufrepTex = await WinampSkinImporter.Instance.LoadShufRepBmpAsync();
            if (shufrepTex != null)
            {
                Debug.Log("[WinampSkinManager] Slicing Shuffle/Repeat from SHUFREP.BMP");
                currentSkin.Shuffle_Off_Normal = WinampSkinSlicer.SliceSprite(shufrepTex, WinampSkinSlicer.ShufRep.ShuffleOffNormal);
                currentSkin.Shuffle_Off_Pressed = WinampSkinSlicer.SliceSprite(shufrepTex, WinampSkinSlicer.ShufRep.ShuffleOffPressed);
                currentSkin.Shuffle_On_Normal = WinampSkinSlicer.SliceSprite(shufrepTex, WinampSkinSlicer.ShufRep.ShuffleOnNormal);
                currentSkin.Shuffle_On_Pressed = WinampSkinSlicer.SliceSprite(shufrepTex, WinampSkinSlicer.ShufRep.ShuffleOnPressed);

                currentSkin.Repeat_Off_Normal = WinampSkinSlicer.SliceSprite(shufrepTex, WinampSkinSlicer.ShufRep.RepeatOffNormal);
                currentSkin.Repeat_Off_Pressed = WinampSkinSlicer.SliceSprite(shufrepTex, WinampSkinSlicer.ShufRep.RepeatOffPressed);
                currentSkin.Repeat_On_Normal = WinampSkinSlicer.SliceSprite(shufrepTex, WinampSkinSlicer.ShufRep.RepeatOnNormal);
                currentSkin.Repeat_On_Pressed = WinampSkinSlicer.SliceSprite(shufrepTex, WinampSkinSlicer.ShufRep.RepeatOnPressed);

                currentSkin.EQ_Off_Normal = WinampSkinSlicer.SliceSprite(shufrepTex, WinampSkinSlicer.ShufRep.EqOffNormal);
                currentSkin.EQ_Off_Pressed = WinampSkinSlicer.SliceSprite(shufrepTex, WinampSkinSlicer.ShufRep.EqOffPressed);
                currentSkin.EQ_On_Normal = WinampSkinSlicer.SliceSprite(shufrepTex, WinampSkinSlicer.ShufRep.EqOnNormal);
                currentSkin.EQ_On_Pressed = WinampSkinSlicer.SliceSprite(shufrepTex, WinampSkinSlicer.ShufRep.EqOnPressed);

                currentSkin.PL_Off_Normal = WinampSkinSlicer.SliceSprite(shufrepTex, WinampSkinSlicer.ShufRep.PlOffNormal);
                currentSkin.PL_Off_Pressed = WinampSkinSlicer.SliceSprite(shufrepTex, WinampSkinSlicer.ShufRep.PlOffPressed);
                currentSkin.PL_On_Normal = WinampSkinSlicer.SliceSprite(shufrepTex, WinampSkinSlicer.ShufRep.PlOnNormal);
                currentSkin.PL_On_Pressed = WinampSkinSlicer.SliceSprite(shufrepTex, WinampSkinSlicer.ShufRep.PlOnPressed);
            }
            else if (mainTex != null)
            {
                Debug.Log("[WinampSkinManager] Slicing Shuffle/Repeat from MAIN.BMP (Fallback)");
                currentSkin.Shuffle_Off_Normal = WinampSkinSlicer.SliceSprite(mainTex, WinampSkinSlicer.ShuffleButtonOff);
                currentSkin.Shuffle_Off_Pressed = WinampSkinSlicer.SliceSprite(mainTex, WinampSkinSlicer.ShuffleButtonOffPressed);
                currentSkin.Shuffle_On_Normal = WinampSkinSlicer.SliceSprite(mainTex, WinampSkinSlicer.ShuffleButtonOn);
                currentSkin.Shuffle_On_Pressed = WinampSkinSlicer.SliceSprite(mainTex, WinampSkinSlicer.ShuffleButtonOnPressed);

                currentSkin.Repeat_Off_Normal = WinampSkinSlicer.SliceSprite(mainTex, WinampSkinSlicer.RepeatButtonOff);
                currentSkin.Repeat_Off_Pressed = WinampSkinSlicer.SliceSprite(mainTex, WinampSkinSlicer.RepeatButtonOffPressed);
                currentSkin.Repeat_On_Normal = WinampSkinSlicer.SliceSprite(mainTex, WinampSkinSlicer.RepeatButtonOn);
                currentSkin.Repeat_On_Pressed = WinampSkinSlicer.SliceSprite(mainTex, WinampSkinSlicer.RepeatButtonOnPressed);

                currentSkin.EQ_Off_Normal = WinampSkinSlicer.SliceSprite(mainTex, WinampSkinSlicer.EqualizerButton); 
                currentSkin.EQ_On_Normal = currentSkin.EQ_Off_Normal; 
                currentSkin.PL_Off_Normal = WinampSkinSlicer.SliceSprite(mainTex, WinampSkinSlicer.PlaylistButton);
                currentSkin.PL_On_Normal = currentSkin.PL_Off_Normal;
            }

            // 4. Load CButtons
            Texture2D cbuttonsTex = await WinampSkinImporter.Instance.LoadCButtonsBmpAsync();
            if (cbuttonsTex != null)
            {
                Debug.Log("[WinampSkinManager] Slicing CButtons");
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

            // 5. Load Posbar (with fallback to Main)
            Texture2D posbarTex = await WinampSkinImporter.Instance.LoadPosbarBmpAsync();
            Texture2D posSource = posbarTex != null ? posbarTex : mainTex;
            if (posSource != null)
            {
                Debug.Log($"[WinampSkinManager] Slicing Posbar Knob from {(posbarTex != null ? "POSBAR.BMP" : "MAIN.BMP")}");
                currentSkin.PosKnobNormal = WinampSkinSlicer.SliceSprite(posSource, new Rect(248, 0, 29, 10));
                currentSkin.PosKnobPressed = WinampSkinSlicer.SliceSprite(posSource, new Rect(278, 0, 29, 10));
            }

            // 6. Load MonoSter
            Texture2D monosterTex = await WinampSkinImporter.Instance.LoadMonoSterBmpAsync();
            if (monosterTex != null)
            {
                Debug.Log("[WinampSkinManager] Slicing Mono/Stereo");
                currentSkin.Stereo_Active = WinampSkinSlicer.SliceSprite(monosterTex, WinampSkinSlicer.MonoSter.StereoOn);
                currentSkin.Stereo_Inactive = WinampSkinSlicer.SliceSprite(monosterTex, WinampSkinSlicer.MonoSter.StereoOff);
                currentSkin.Mono_Inactive = WinampSkinSlicer.SliceSprite(monosterTex, WinampSkinSlicer.MonoSter.MonoOff);
            }

            // 7. Load Volume
            Texture2D volumeTex = await WinampSkinImporter.Instance.LoadVolumeBmpAsync();
            if (volumeTex != null)
            {
                Debug.Log("[WinampSkinManager] Slicing Volume");
                float h = volumeTex.height;
                currentSkin.VolumeKnobNormal = WinampSkinSlicer.SliceSprite(volumeTex, new Rect(WinampSkinSlicer.Volume.KnobNormal.x, h - WinampSkinSlicer.Volume.KnobNormal.y - WinampSkinSlicer.Volume.KnobNormal.height, WinampSkinSlicer.Volume.KnobNormal.width, WinampSkinSlicer.Volume.KnobNormal.height));
                currentSkin.VolumeKnobPressed = WinampSkinSlicer.SliceSprite(volumeTex, new Rect(WinampSkinSlicer.Volume.KnobPressed.x, h - WinampSkinSlicer.Volume.KnobPressed.y - WinampSkinSlicer.Volume.KnobPressed.height, WinampSkinSlicer.Volume.KnobPressed.width, WinampSkinSlicer.Volume.KnobPressed.height));
                currentSkin.VolumeAnimation = new Sprite[WinampSkinSlicer.Volume.FrameCount];
                for (int i = 0; i < WinampSkinSlicer.Volume.FrameCount; i++)
                {
                    float yBottom = 420 - (i * WinampSkinSlicer.Volume.FrameStride);
                    float yTop = h - yBottom - WinampSkinSlicer.Volume.FrameHeight;
                    currentSkin.VolumeAnimation[i] = WinampSkinSlicer.SliceSprite(volumeTex, new Rect(0, yTop, WinampSkinSlicer.Volume.FrameWidth, WinampSkinSlicer.Volume.FrameHeight));
                }
            }

            // 8. Load Balance
            Texture2D balanceTex = await WinampSkinImporter.Instance.LoadBalanceBmpAsync();
            if (balanceTex != null)
            {
                Debug.Log("[WinampSkinManager] Slicing Balance");
                float h = balanceTex.height;
                currentSkin.BalanceKnobNormal = WinampSkinSlicer.SliceSprite(balanceTex, new Rect(WinampSkinSlicer.Balance.KnobNormal.x, h - WinampSkinSlicer.Balance.KnobNormal.y - WinampSkinSlicer.Balance.KnobNormal.height, WinampSkinSlicer.Balance.KnobNormal.width, WinampSkinSlicer.Balance.KnobNormal.height));
                currentSkin.BalanceKnobPressed = WinampSkinSlicer.SliceSprite(balanceTex, new Rect(WinampSkinSlicer.Balance.KnobPressed.x, h - WinampSkinSlicer.Balance.KnobPressed.y - WinampSkinSlicer.Balance.KnobPressed.height, WinampSkinSlicer.Balance.KnobPressed.width, WinampSkinSlicer.Balance.KnobPressed.height));
                currentSkin.BalanceAnimation = new Sprite[WinampSkinSlicer.Balance.FrameCount];
                for (int i = 0; i < WinampSkinSlicer.Balance.FrameCount; i++)
                {
                    float yBottom = 420 - (i * WinampSkinSlicer.Balance.FrameStride);
                    float yTop = h - yBottom - WinampSkinSlicer.Balance.FrameHeight;
                    currentSkin.BalanceAnimation[i] = WinampSkinSlicer.SliceSprite(balanceTex, new Rect(9, yTop, WinampSkinSlicer.Balance.FrameWidth, WinampSkinSlicer.Balance.FrameHeight));
                }
            }

            // 9. Load PlayPaus
            Texture2D playpausTex = await WinampSkinImporter.Instance.LoadPlayPausBmpAsync();
            if (playpausTex != null)
            {
                Debug.Log("[WinampSkinManager] Slicing PlayPaus");
                currentSkin.Status_Play = WinampSkinSlicer.SliceSprite(playpausTex, WinampSkinSlicer.PlayPaus.PlayIcon);
                currentSkin.Status_Pause = WinampSkinSlicer.SliceSprite(playpausTex, WinampSkinSlicer.PlayPaus.PauseIcon);
                currentSkin.Status_Stop = WinampSkinSlicer.SliceSprite(playpausTex, WinampSkinSlicer.PlayPaus.StopIcon);
                currentSkin.Status_Indicator_Play = WinampSkinSlicer.SliceSprite(playpausTex, WinampSkinSlicer.PlayPaus.PlayingIndicator);
                currentSkin.Status_Indicator_Load = WinampSkinSlicer.SliceSprite(playpausTex, WinampSkinSlicer.PlayPaus.LoadingIndicator);
            }

            // 10. Load Numbers & Fallback
            Texture2D numbersTex = await WinampSkinImporter.Instance.LoadNumbersBmpAsync();
            bool numbersFound = false;
            if (numbersTex != null)
            {
                Debug.Log("[WinampSkinManager] Slicing Numbers");
                currentSkin.TimeDigits = new Sprite[10];
                for (int i = 0; i < 10; i++) currentSkin.TimeDigits[i] = WinampSkinSlicer.SliceSprite(numbersTex, WinampSkinSlicer.Numbers.GetDigitRect(i));
                numbersFound = true;
            }

            Texture2D numsExTex = await WinampSkinImporter.Instance.LoadNumsExBmpAsync();
            if (numsExTex != null)
            {
                Debug.Log($"[WinampSkinManager] Slicing NumsEx (Fallback: {!numbersFound})");
                if (!numbersFound && numsExTex.width >= 90)
                {
                    currentSkin.TimeDigits = new Sprite[10];
                    for (int i = 0; i < 10; i++) currentSkin.TimeDigits[i] = WinampSkinSlicer.SliceSprite(numsExTex, WinampSkinSlicer.Numbers.GetDigitRect(i));
                }
                currentSkin.TimeMinus = WinampSkinSlicer.SliceSprite(numsExTex, WinampSkinSlicer.NumsEx.MinusSign);
            }

            try
            {
                // 11. Load Font
                Texture2D textTex = await WinampSkinImporter.Instance.LoadTextBmpAsync();
                if (textTex != null)
                {
                    Debug.Log("[WinampSkinManager] Slicing Font");
                    string allChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ\"@0123456789.:()-'!_+\\/[]^&%,=$#?* ";
                    var fontSprites = new System.Collections.Generic.List<Sprite>();
                    foreach (char c in allChars)
                    {
                        Rect r = WinampSkinSlicer.Font.GetCharRect(c);
                        if (r != Rect.zero)
                        {
                            Sprite s = WinampSkinSlicer.SliceSprite(textTex, r);
                            if (s != null)
                            {
                                s.name = WinampSkinSlicer.Font.GetSpriteName(c);
                                fontSprites.Add(s);
                            }
                        }
                    }
                    currentSkin.TextSprites = fontSprites.ToArray();
                }

                // 12. Load EqMain (EQMAIN.BMP)
                Debug.Log("[WinampSkinManager] Step 12: Loading Equalizer skin...");
                await WinampSkinImporter.Instance.LoadEqMainAsync(currentSkin);
                Debug.Log("[WinampSkinManager] Step 12: Done.");

                // 13. Load VisColors (VISCOLOR.TXT)
                Debug.Log("[WinampSkinManager] Step 13: Loading VisColors...");
                currentSkin.VisColors = await WinampSkinImporter.Instance.LoadVisColorAsync();
                Debug.Log($"[WinampSkinManager] Step 13: Done. Colors: {(currentSkin.VisColors != null ? currentSkin.VisColors.Length.ToString() : "NULL")}");

                // 14. Load Playlist skin (PLEDIT)
                Debug.Log("[WinampSkinManager] Step 14: Loading Playlist skin...");
                await WinampSkinImporter.Instance.LoadPlEditAsync(currentSkin);
                await WinampSkinImporter.Instance.LoadPlEditTxtAsync(currentSkin);
                Debug.Log("[WinampSkinManager] Step 14: Done.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[WinampSkinManager] CRITICAL ERROR during skin loading: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                Debug.Log("[WinampSkinManager] Finishing skin loading sequence.");
                ApplySkinToHierarchy();
            }
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
