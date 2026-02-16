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

        public static WinampSkinManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private IEnumerator Start()
        {
            // Wait for systems to initialize
            yield return null;

            // 0. Ensure Base Skin Exists (Copy from StreamingAssets if needed)
            if (useBaseSkinFallback)
            {
                var copyTask = EnsureBaseSkinExists();
                yield return new WaitUntil(() => copyTask.IsCompleted);
            }

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
                else
                {
                    Debug.LogWarning($"[WinampSkinManager] Saved skin not found at {savedPath}");
                }
            }

            // 2. Fallback: Persistent Data Path (default persistent skin)
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

            // 3. Fallback: Base Skin (from StreamingAssets -> Persistent)
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

            // 4. Fallback: Test Skin Path (Inspector) - only if explicitly enabled
            if (!foundSkin && loadOnStart && !string.IsNullOrEmpty(testSkinPath))
            {
                 Debug.Log($"[WinampSkinManager] Loading test skin: {testSkinPath}");
                 skinToLoad = testSkinPath;
                 foundSkin = true;
            }

            // Execute Load
            if (foundSkin && !string.IsNullOrEmpty(skinToLoad))
            {
                // We start the async task but don't await strictly in Start (it returns void/IEnumerator)
                // But we can fire and forget, or handle it properly.
                // Since this is Start coroutine, we can just call it.
                var loadTask =  LoadSkin(skinToLoad);
            }
        }
        
        private async System.Threading.Tasks.Task EnsureBaseSkinExists()
        {
            string destPath = System.IO.Path.Combine(Application.persistentDataPath, "Skins", baseSkinFileName);
            
            // If it already exists, we assume it's fine. 
            // Optional: Check hash/version? For now, just existence.
            if (System.IO.File.Exists(destPath)) return;

            Debug.Log($"[WinampSkinManager] Base skin missing at {destPath}. Copying from StreamingAssets...");
            
            string sourcePath = System.IO.Path.Combine(Application.streamingAssetsPath, baseSkinFileName);
            byte[] data = null;

            if (sourcePath.Contains("://") || Application.platform == RuntimePlatform.Android)
            {
                // Android / WebGL requires UnityWebRequest
                using (var wr = UnityEngine.Networking.UnityWebRequest.Get(sourcePath))
                {
                    var op = wr.SendWebRequest();
                    while (!op.isDone) await System.Threading.Tasks.Task.Yield();
                    
                    if (wr.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                    {
                        data = wr.downloadHandler.data;
                    }
                    else
                    {
                        Debug.LogError($"[WinampSkinManager] Failed to read Base Skin from StreamingAssets: {wr.error}");
                    }
                }
            }
            else
            {
                // PC / Editor
                if (System.IO.File.Exists(sourcePath))
                {
                    data = await System.IO.File.ReadAllBytesAsync(sourcePath);
                }
            }

            if (data != null)
            {
                try 
                {
                    string dir = System.IO.Path.GetDirectoryName(destPath);
                    if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);
                    
                    await System.IO.File.WriteAllBytesAsync(destPath, data);
                    Debug.Log("[WinampSkinManager] Base Skin copied successfully.");
                }
                catch (System.Exception ex)
                {
                     Debug.LogError($"[WinampSkinManager] Failed to write Base Skin to persistent path: {ex.Message}");
                }
            }
            else
            {
                Debug.LogWarning($"[WinampSkinManager] Base Skin '{baseSkinFileName}' not found in StreamingAssets.");
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
        /// <returns>True if loading initiated successfully and parsing started, False if file not found.</returns>
        public async System.Threading.Tasks.Task<bool> LoadSkin(string absolutePath)
        {
            if (System.IO.File.Exists(absolutePath))
            {
                Debug.Log($"[WinampSkinManager] Loading skin from: {absolutePath}");
                testSkinPath = absolutePath;
                
                try 
                {
                    await LoadAndApplyTestSkin();
                    // If we are here, it means no exception was thrown in await
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
                throw;
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
