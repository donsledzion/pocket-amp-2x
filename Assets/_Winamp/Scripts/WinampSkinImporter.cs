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
    /// Runtime importer for Winamp 2.x skins (.wsz files)
    /// Handles file picking, unpacking, and texture loading
    /// </summary>
    public class WinampSkinImporter : MonoBehaviour
    {
        [Header("Debug Settings")]
        [SerializeField] private bool verboseLogging = true;
        
        [Header("Test Settings (Editor Only)")]
        [SerializeField] private string testWszPath = "";
        [SerializeField] private UnityEngine.UI.Image testImage = null;
        [SerializeField] private bool setNativeSizeOnApply = true;
        
        private string lastUnpackedSkinPath = "";
        private string lastUnpackedSkinName = "";
        private Sprite lastSlicedSprite = null;
        
        // Keep track of specific textures
        private Texture2D mainTexture = null;
        private Texture2D cbuttonsTexture = null;
        private Texture2D shufrepTexture = null;
        
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
                byte[] fileData = File.ReadAllBytes(path);
                string extension = Path.GetExtension(path).ToLower();

                if (extension == ".bmp")
                {
                    // Use custom BMP loader
                    tex = BMPLoader.LoadBMP(path);
                }
                else
                {
                    // Use Unity's native loader for PNG/JPG
                    // Create texture without mipmaps (false)
                    tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    if (!ImageConversion.LoadImage(tex, fileData))
                    {
                        Debug.LogError($"[WinampSkinImporter] Failed to load non-BMP image: {path}");
                        Destroy(tex);
                        tex = null;
                    }
                }
                
                if (tex != null)
                {
                    // Set pixel-perfect settings
                    tex.filterMode = FilterMode.Point;
                    tex.wrapMode = TextureWrapMode.Clamp;

                    Log($"Texture loaded successfully: {path} ({tex.width}x{tex.height}), format: {tex.format}");
                    
                    // Remove magenta transparency (standard for Winamp)
                    RemoveMagenta(tex);
                    
                    lastLoadedTexture = tex;
                    onComplete?.Invoke(tex);
                }
                else
                {
                    Debug.LogError($"Failed to decode image from: {path}");
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
            LoadTexture(mainBmpPath, (tex) => {
                if (tex != null) mainTexture = tex;
                lastLoadedTexture = tex;
                onComplete?.Invoke(tex);
            });
        }

        /// <summary>
        /// Loads cbuttons.bmp from the last unpacked skin
        /// </summary>
        public void LoadCButtonsBmp(System.Action<Texture2D> onComplete)
        {
            if (string.IsNullOrEmpty(lastUnpackedSkinPath))
            {
                Debug.LogWarning("No skin has been unpacked yet.");
                onComplete?.Invoke(null);
                return;
            }
            
            string bmpPath = Path.Combine(lastUnpackedSkinPath, "CBUTTONS.BMP");
            if (!File.Exists(bmpPath)) bmpPath = Path.Combine(lastUnpackedSkinPath, "cbuttons.bmp");
            
            LoadTexture(bmpPath, (tex) => {
                if (tex != null) cbuttonsTexture = tex;
                lastLoadedTexture = tex;
                onComplete?.Invoke(tex);
            });
        }

        public void LoadShufRepBmp(System.Action<Texture2D> onComplete)
        {
            if (string.IsNullOrEmpty(lastUnpackedSkinPath))
            {
                Debug.LogWarning("No skin has been unpacked yet.");
                onComplete?.Invoke(null);
                return;
            }

            // Try explicit SHUFREP.BMP / shufrep.bmp / SHUFREP.PNG / shufrep.png
            string[] candidates = { "SHUFREP.BMP", "shufrep.bmp", "SHUFREP.PNG", "shufrep.png" };
            string foundPath = null;
            
            foreach (var cand in candidates)
            {
                string p = Path.Combine(lastUnpackedSkinPath, cand);
                if (File.Exists(p))
                {
                    foundPath = p;
                    break;
                }
            }
            
            if (foundPath != null)
            {
                LoadTexture(foundPath, (tex) => {
                    if (tex != null) shufrepTexture = tex;
                    onComplete?.Invoke(tex);
                });
            }
            else
            {
                Debug.LogWarning("[WinampSkinImporter] SHUFREP.BMP not found.");
                onComplete?.Invoke(null);
            }
        }
        
        public void LoadMonoSterBmp(System.Action<Texture2D> onComplete)
        {
            if (string.IsNullOrEmpty(lastUnpackedSkinPath))
            {
                onComplete?.Invoke(null);
                return;
            }

            string[] candidates = { "MONOSTER.BMP", "monoster.bmp", "MONOSTER.PNG", "monoster.png" };
            string foundPath = null;
            
            foreach (var cand in candidates)
            {
                string p = Path.Combine(lastUnpackedSkinPath, cand);
                if (File.Exists(p))
                {
                    foundPath = p;
                    break;
                }
            }
            
            if (foundPath != null)
            {
                LoadTexture(foundPath, onComplete);
            }
            else
            {
                onComplete?.Invoke(null);
            }
        }

        public void LoadVolumeBmp(System.Action<Texture2D> onComplete)
        {
            LoadSkinFile(new[] { "VOLUME.BMP", "volume.bmp", "VOLUME.PNG", "volume.png" }, onComplete);
        }

        public void LoadNumbersBmp(System.Action<Texture2D> onComplete)
        {
            LoadSkinFile(new[] { "NUMBERS.BMP", "numbers.bmp", "Numbers.bmp", "NUMBERS.PNG", "numbers.png", "Numbers.png" }, onComplete);
        }

        public void LoadNumsExBmp(System.Action<Texture2D> onComplete)
        {
            LoadSkinFile(new[] { "NUMS_EX.BMP", "nums_ex.bmp", "Nums_ex.bmp", "NUMS_EX.PNG", "nums_ex.png", "Nums_ex.png" }, onComplete);
        }

        public void LoadBalanceBmp(System.Action<Texture2D> onComplete)
        {
            LoadSkinFile(new[] { "BALANCE.BMP", "balance.bmp", "BALANCE.PNG", "balance.png" }, onComplete);
        }

        public void LoadPlayPausBmp(System.Action<Texture2D> onComplete)
        {
            LoadSkinFile(new[] { "PLAYPAUS.BMP", "playpaus.bmp", "PlayPaus.bmp", "PLAYPAUS.PNG", "playpaus.png", "PlayPaus.png" }, onComplete);
        }

        public void LoadPosbarBmp(System.Action<Texture2D> onComplete)
        {
            LoadSkinFile(new[] { "POSBAR.BMP", "posbar.bmp", "Posbar.bmp", "POSBAR.PNG", "posbar.png" }, onComplete);
        }

        public void LoadTextBmp(System.Action<Texture2D> onComplete)
        {
            LoadSkinFile(new[] { "TEXT.BMP", "text.bmp", "Text.bmp", "TEXT.PNG", "text.png", "Text.png" }, onComplete);
        }

        public Task<Texture2D> LoadTextBmpAsync() => LoadSkinFileAsync(new[] { "TEXT.BMP", "text.bmp", "Text.bmp", "TEXT.PNG", "text.png", "Text.png" });
        public Task<Texture2D> LoadTitleBarBmpAsync() => LoadSkinFileAsync(new[] { "TITLEBAR.BMP", "titlebar.bmp", "TitleBar.bmp", "TITLEBAR.PNG", "titlebar.png", "TitleBar.png" });

        public Task<Texture2D> LoadMainBmpAsync() => LoadSkinFileAsync(new[] { "MAIN.BMP", "main.bmp", "MAIN.PNG", "main.png" });
        public Task<Texture2D> LoadCButtonsBmpAsync() => LoadSkinFileAsync(new[] { "CBUTTONS.BMP", "cbuttons.bmp", "CBUTTONS.PNG", "cbuttons.png" });
        public Task<Texture2D> LoadShufRepBmpAsync() => LoadSkinFileAsync(new[] { "SHUFREP.BMP", "shufrep.bmp", "SHUFREP.PNG", "shufrep.png" });
        public Task<Texture2D> LoadVolumeBmpAsync() => LoadSkinFileAsync(new[] { "VOLUME.BMP", "volume.bmp", "VOLUME.PNG", "volume.png" });
        public Task<Texture2D> LoadBalanceBmpAsync() => LoadSkinFileAsync(new[] { "BALANCE.BMP", "balance.bmp", "BALANCE.PNG", "balance.png" });
        public Task<Texture2D> LoadNumbersBmpAsync() => LoadSkinFileAsync(new[] { "NUMBERS.BMP", "numbers.bmp", "Numbers.bmp", "NUMBERS.PNG", "numbers.png", "Numbers.png" });
        public Task<Texture2D> LoadNumsExBmpAsync() => LoadSkinFileAsync(new[] { "NUMS_EX.BMP", "nums_ex.bmp", "Nums_ex.bmp", "NUMS_EX.PNG", "nums_ex.png", "Nums_ex.png" });
        public Task<Texture2D> LoadPlayPausBmpAsync() => LoadSkinFileAsync(new[] { "PLAYPAUS.BMP", "playpaus.bmp", "PlayPaus.bmp", "PLAYPAUS.PNG", "playpaus.png", "PlayPaus.png" });
        public Task<Texture2D> LoadPosbarBmpAsync() => LoadSkinFileAsync(new[] { "POSBAR.BMP", "posbar.bmp", "Posbar.bmp", "POSBAR.PNG", "posbar.png" });
        public Task<Texture2D> LoadMonoSterBmpAsync() => LoadSkinFileAsync(new[] { "MONOSTER.BMP", "MONOSTER.PNG" });
        // Duplicate LoadTextBmpAsync removed

        public async Task<Color[]> LoadVisColorAsync()
        {
            if (string.IsNullOrEmpty(lastUnpackedSkinPath)) return null;

            string foundPath = null;
            // Try root first
            string directPath = Path.Combine(lastUnpackedSkinPath, "VISCOLOR.TXT");
            if (File.Exists(directPath)) foundPath = directPath;
            else
            {
                // Recursive search
                try
                {
                    string[] files = Directory.GetFiles(lastUnpackedSkinPath, "VISCOLOR.TXT", SearchOption.AllDirectories);
                    if (files != null && files.Length > 0) foundPath = files[0];
                }
                catch { }
            }

            if (foundPath == null) 
            {
                Log("VISCOLOR.TXT not found (even recursively)");
                return null;
            }

            try
            {
                Log($"Reading VISCOLOR.TXT from: {foundPath}");
                string text = await File.ReadAllTextAsync(foundPath);
                var colors = ParseVisColor(text);
                Log(colors != null ? $"Successfully parsed {colors.Length} colors" : "Failed to parse any colors from VISCOLOR.TXT");
                return colors;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[WinampSkinImporter] Failed to read VISCOLOR.TXT: {ex.Message}");
                return null;
            }
        }

        public async Task LoadEqMainAsync(WinampSkin skin)
        {
            if (skin == null) return;

            Texture2D eqMainTex = await LoadSkinFileAsync(new[] { "EQMAIN.BMP", "eqmain.bmp", "EQMAIN.PNG", "eqmain.png" });

            if (eqMainTex != null)
            {
                Log($"Slicing Equalizer components from {eqMainTex.name} ({eqMainTex.width}x{eqMainTex.height})");
                
                // Background & Title
                skin.EqBackground = WinampSkinSlicer.SliceSprite(eqMainTex, WinampSkinSlicer.Equalizer.Background);
                skin.EqTitleBar = WinampSkinSlicer.SliceSprite(eqMainTex, WinampSkinSlicer.Equalizer.TitleBar);
                
                // Close button
                skin.EqCloseNormal = WinampSkinSlicer.SliceSprite(eqMainTex, WinampSkinSlicer.Equalizer.CloseNormal);
                skin.EqClosePressed = WinampSkinSlicer.SliceSprite(eqMainTex, WinampSkinSlicer.Equalizer.ClosePressed);
                
                // Toggles (On/Auto)
                skin.EqOn_Off_Normal = WinampSkinSlicer.SliceSprite(eqMainTex, WinampSkinSlicer.Equalizer.On_Off_Normal);
                skin.EqOn_On_Normal = WinampSkinSlicer.SliceSprite(eqMainTex, WinampSkinSlicer.Equalizer.On_On_Normal);
                skin.EqOn_Off_Pressed = WinampSkinSlicer.SliceSprite(eqMainTex, WinampSkinSlicer.Equalizer.On_Off_Pressed);
                skin.EqOn_On_Pressed = WinampSkinSlicer.SliceSprite(eqMainTex, WinampSkinSlicer.Equalizer.On_On_Pressed);
                
                skin.EqAuto_Off_Normal = WinampSkinSlicer.SliceSprite(eqMainTex, WinampSkinSlicer.Equalizer.Auto_Off_Normal);
                skin.EqAuto_On_Normal = WinampSkinSlicer.SliceSprite(eqMainTex, WinampSkinSlicer.Equalizer.Auto_On_Normal);
                skin.EqAuto_Off_Pressed = WinampSkinSlicer.SliceSprite(eqMainTex, WinampSkinSlicer.Equalizer.Auto_Off_Pressed);
                skin.EqAuto_On_Pressed = WinampSkinSlicer.SliceSprite(eqMainTex, WinampSkinSlicer.Equalizer.Auto_On_Pressed);
                
                // Presets
                skin.EqPresetsNormal = WinampSkinSlicer.SliceSprite(eqMainTex, WinampSkinSlicer.Equalizer.PresetsNormal);
                skin.EqPresetsPressed = WinampSkinSlicer.SliceSprite(eqMainTex, WinampSkinSlicer.Equalizer.PresetsPressed);
                
                // Knob
                skin.EqKnobNormal = WinampSkinSlicer.SliceSprite(eqMainTex, WinampSkinSlicer.Equalizer.KnobNormal);
                skin.EqKnobPressed = WinampSkinSlicer.SliceSprite(eqMainTex, WinampSkinSlicer.Equalizer.KnobPressed);
                
                // Slider Backgrounds (28 frames)
                var sliderFames = new List<Sprite>();
                Rect first = WinampSkinSlicer.Equalizer.SliderFirstFrame;
                for (int row = 0; row < 2; row++)
                {
                    for (int col = 0; col < 14; col++)
                    {
                        Rect frameRect = new Rect(
                            first.x + (col * 15), 
                            first.y + (row * 65), 
                            first.width, 
                            first.height);
                        sliderFames.Add(WinampSkinSlicer.SliceSprite(eqMainTex, frameRect));
                    }
                }
                skin.EqSliderBackgrounds = sliderFames.ToArray();
                
                // Graph Elements
                skin.EqGraphBackground = WinampSkinSlicer.SliceSprite(eqMainTex, WinampSkinSlicer.Equalizer.GraphBG);
                skin.EqGraphColors = WinampSkinSlicer.SliceSprite(eqMainTex, WinampSkinSlicer.Equalizer.GraphColors);
                skin.EqGraphPreampLine = WinampSkinSlicer.SliceSprite(eqMainTex, WinampSkinSlicer.Equalizer.PreampLine);
            }
        }

        public async Task LoadPlEditAsync(WinampSkin skin)
        {
            if (skin == null) return;

            Texture2D plEditTex = await LoadSkinFileAsync(new[] { "PLEDIT.BMP", "pledit.bmp", "PLEDIT.PNG", "pledit.png" });

            if (plEditTex != null)
            {
                Log($"Slicing Playlist components from {plEditTex.name} ({plEditTex.width}x{plEditTex.height})");
                
                // Borders & Title
                skin.PlTopLeft = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.TopLeft);
                skin.PlTopTitle = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.TopTitle);
                skin.PlTopStretch = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.TopStretch);
                skin.PlTopLeftStretch = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.TopLeftStretch);
                skin.PlTopRightStretch = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.TopRightStretch);
                skin.PlTopRight = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.TopRight);
                
                skin.PlBottomLeft = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.BottomLeft);
                skin.PlBottomRight = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.BottomRight);
                skin.PlBottomStretch = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.BottomStretch);
                
                skin.PlLeftEdge = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.LeftEdge);
                skin.PlRightEdge = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.RightEdge);
                // Background is handled by color from PLEDIT.TXT

                // Buttons Add
                skin.PlAddUrlNormal = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.AddUrlNormal);
                skin.PlAddUrlPressed = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.AddUrlPressed);
                skin.PlAddDirNormal = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.AddDirNormal);
                skin.PlAddDirPressed = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.AddDirPressed);
                skin.PlAddFileNormal = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.AddFileNormal);
                skin.PlAddFilePressed = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.AddFilePressed);

                // Buttons Remove
                skin.PlRemoveAllNormal = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.RemAllNormal);
                skin.PlRemoveAllPressed = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.RemAllPressed);
                skin.PlRemoveSelNormal = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.RemSelNormal);
                skin.PlRemoveSelPressed = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.RemSelPressed);
                skin.PlRemoveCropNormal = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.RemCropNormal);
                skin.PlRemoveCropPressed = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.RemCropPressed);
                skin.PlRemoveOptNormal = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.RemMiscNormal);
                skin.PlRemoveOptPressed = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.RemMiscPressed);

                // Buttons Select
                skin.PlSelectAllNormal = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.SelAllNormal);
                skin.PlSelectAllPressed = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.SelAllPressed);
                skin.PlSelectNoneNormal = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.SelNoneNormal);
                skin.PlSelectNonePressed = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.SelNonePressed);
                skin.PlSelectInvNormal = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.SelInvNormal);
                skin.PlSelectInvPressed = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.SelInvPressed);

                // Buttons Sort/Misc
                skin.PlSortNormal = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.SortNormal);
                skin.PlSortPressed = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.SortPressed);
                skin.PlFileInfoNormal = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.FileInfoNormal);
                skin.PlFileInfoPressed = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.FileInfoPressed);
                skin.PlMiscNormal = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.MiscNormal);
                skin.PlMiscPressed = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.MiscPressed);

                // Buttons List
                skin.PlNewListNormal = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.NewListNormal);
                skin.PlNewListPressed = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.NewListPressed);
                skin.PlSaveListNormal = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.SaveListNormal);
                skin.PlSaveListPressed = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.SaveListPressed);
                skin.PlLoadListNormal = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.LoadListNormal);
                skin.PlLoadListPressed = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.LoadListPressed);

                // Scrollbar
                skin.PlScrollHandleNormal = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.SliderHandleNormal);
                skin.PlScrollHandlePressed = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.SliderHandlePressed);

                // Close Button
                skin.PlCloseNormal = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.CloseNormal);
                skin.PlClosePressed = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.ClosePressed);

                // Clippers
                skin.PlAddClipper = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.AddClipper);
                skin.PlRemoveClipper = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.RemoveClipper);
                skin.PlSelectClipper = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.SelectClipper);
                skin.PlMiscClipper = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.MiscClipper);
                skin.PlListOptionsClipper = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.ListOptionsClipper);
            }
        }

        public async Task LoadPlEditTxtAsync(WinampSkin skin)
        {
            if (skin == null || string.IsNullOrEmpty(lastUnpackedSkinPath)) return;

            // Search for PLEDIT.TXT (recursive)
            string foundPath = null;
            try
            {
                string[] files = Directory.GetFiles(lastUnpackedSkinPath, "PLEDIT.TXT", SearchOption.AllDirectories);
                if (files.Length == 0) files = Directory.GetFiles(lastUnpackedSkinPath, "pledit.txt", SearchOption.AllDirectories);
                if (files.Length > 0) foundPath = files[0];
            }
            catch { }

            if (foundPath == null) return;

            try
            {
                string text = await File.ReadAllTextAsync(foundPath);
                string[] lines = text.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines)
                {
                    string trimmed = line.Trim();
                    if (trimmed.Contains('='))
                    {
                        string[] parts = trimmed.Split('=');
                        if (parts.Length == 2)
                        {
                            string key = parts[0].Trim().ToLower();
                            string val = parts[1].Trim();
                            
                            // Remove comments after value (e.g. #FFFFFF ; white)
                            if (val.Contains(';')) val = val.Split(';')[0].Trim();
                            if (val.Contains("//")) val = val.Split(new[] { "//" }, System.StringSplitOptions.None)[0].Trim();

                            if (!val.StartsWith("#")) val = "#" + val;

                            if (ColorUtility.TryParseHtmlString(val, out Color col))
                            {
                                switch (key)
                                {
                                    case "normal": skin.PlNormalColor = col; break;
                                    case "current": skin.PlCurrentColor = col; break;
                                    case "normalbg": skin.PlNormalBGColor = col; break;
                                    case "selectedbg": skin.PlSelectedBGColor = col; break;
                                    case "mbfg": skin.PlMbFGColor = col; break;
                                    case "mbbg": skin.PlMbBGColor = col; break;
                                }
                            }
                        }
                    }
                }
                Log($"Parsed PLEDIT.TXT and updated skin colors.");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[WinampSkinImporter] Failed to read PLEDIT.TXT: {ex.Message}");
            }
        }

        private Color[] ParseVisColor(string text)
        {
            if (string.IsNullOrEmpty(text)) return null;

            // Remove Byte Order Mark (BOM) if present
            text = text.Trim('\uFEFF', '\u200B');
            
            var colors = new List<Color>();
            string[] lines = text.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
            
            Log($"Parsing VISCOLOR.TXT: {lines.Length} raw lines found");
            if (lines.Length > 0) Log($"First line preview: [{lines[0].Trim()}]");

            foreach (string line in lines)
            {
                string trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("//") || trimmed.StartsWith(";") || trimmed.StartsWith("#")) continue;

                // Expected format: R,G,B or R G B
                string[] parts = trimmed.Split(new[] { ',', ' ', '\t' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3)
                {
                    if (int.TryParse(parts[0], out int r) && 
                        int.TryParse(parts[1], out int g) && 
                        int.TryParse(parts[2], out int b))
                    {
                        colors.Add(new Color(r / 255f, g / 255f, b / 255f, 1f));
                    }
                    else
                    {
                        Log($"Failed to parse RGB values from line: {trimmed}");
                    }
                }
                else
                {
                    // Skip lines that don't look like color data (e.g. headers or comments I missed)
                    if (trimmed.Length > 0 && !char.IsDigit(trimmed[0]))
                    {
                        Log($"Skipping non-digit line: {trimmed}");
                    }
                }

                if (colors.Count >= 24) break; 
            }

            Log($"Final palette size: {colors.Count} colors");
            return colors.Count > 0 ? colors.ToArray() : null;
        }

        private Task<Texture2D> LoadSkinFileAsync(string[] candidates)
        {
            var tcs = new TaskCompletionSource<Texture2D>();
            LoadSkinFile(candidates, (tex) => tcs.SetResult(tex));
            return tcs.Task;
        }

        private void LoadSkinFile(string[] candidates, System.Action<Texture2D> onComplete)
        {
            if (string.IsNullOrEmpty(lastUnpackedSkinPath))
            {
                onComplete?.Invoke(null);
                return;
            }

            string foundPath = null;
            foreach (var cand in candidates)
            {
                // Try direct path first (optimal)
                string directPath = Path.Combine(lastUnpackedSkinPath, cand);
                if (File.Exists(directPath))
                {
                    foundPath = directPath;
                    break;
                }

                // Fallback: Recursive search for skins with subdirectories
                try
                {
                    string[] files = Directory.GetFiles(lastUnpackedSkinPath, cand, SearchOption.AllDirectories);
                    if (files != null && files.Length > 0)
                    {
                        foundPath = files[0];
                        Log($"Found nested skin file: {cand} -> {foundPath}");
                        break;
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[WinampSkinImporter] Recursive search failed for {cand}: {ex.Message}");
                }
            }

            if (foundPath != null)
            {
                LoadTexture(foundPath, onComplete);
            }
            else
            {
                onComplete?.Invoke(null);
            }
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

        [ContextMenu("Test: Load CButtons BMP")]
        private void TestLoadCButtonsBmp()
        {
            LoadCButtonsBmp((tex) =>
            {
                if (tex != null)
                {
                    Debug.Log($"[TEST] Successfully loaded cbuttons.bmp: {tex.width}x{tex.height}");
                }
                else
                {
                    Debug.LogError("[TEST] Failed to load cbuttons.bmp");
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
        
        [ContextMenu("Test: Slice Play Button (CButtons)")]
        private void TestSlicePlayButton()
        {
            // Prefer cbuttonsTexture if available, otherwise fallback to lastLoaded with warning
            Texture2D sourceTex = cbuttonsTexture != null ? cbuttonsTexture : lastLoadedTexture;

            if (sourceTex == null)
            {
                Debug.LogWarning("No texture loaded yet. Use 'Test: Load CButtons BMP' first.");
                return;
            }
            
            // Check dimensions to warn user if they loaded wrong file
            if (sourceTex.width != 136 || sourceTex.height != 34)
            {
                Debug.LogWarning($"Current texture size ({sourceTex.width}x{sourceTex.height}) doesn't match standard CBUTTONS.BMP (136x34). Correct file loaded?");
            }
            
            Sprite sprite = WinampSkinSlicer.SlicePlayButton(sourceTex);
            
            if (sprite != null)
            {
                lastSlicedSprite = sprite;
                Debug.Log($"[TEST] Successfully sliced Play button sprite: {sprite.rect}");
                Debug.Log($"Sprite size: {sprite.rect.width}x{sprite.rect.height}");
                Debug.Log($"Use 'Test: Apply to Test Image' to visualize it in the scene");
            }
            else
            {
                Debug.LogError("[TEST] Failed to slice Play button sprite");
            }
        }
        
        [ContextMenu("Test: Apply to Test Image")]
        private void TestApplyToTestImage()
        {
            if (testImage == null)
            {
                Debug.LogWarning("testImage is not assigned. Assign a UI Image in the Inspector first.");
                return;
            }
            
            if (lastSlicedSprite == null)
            {
                Debug.LogWarning("No sprite sliced yet. Use 'Test: Slice Play Button' first.");
                return;
            }
            
            testImage.sprite = lastSlicedSprite;
            
            if (setNativeSizeOnApply)
            {
                testImage.SetNativeSize();
                Debug.Log($"[TEST] Applied sprite and SetNativeSize: {testImage.rectTransform.sizeDelta}");
            }
            else
            {
                Debug.Log($"[TEST] Applied sprite (kept original Image size: {testImage.rectTransform.sizeDelta})");
            }
        }
        
        #endregion
    }
}
