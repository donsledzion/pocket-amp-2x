using UnityEngine;
using System.Threading.Tasks;

namespace SoftAware
{
    /// <summary>
    /// Facade class that coordinates the loading of Winamp skins.
    /// Delegates responsibility to SkinFileSystem, SkinTextureLoader, SkinParser, and SkinComposer.
    /// </summary>
    public class WinampSkinImporter : MonoBehaviour
    {
        public static WinampSkinImporter Instance { get; private set; }

        private SkinFileSystem fileSystem;
        private SkinTextureLoader textureLoader;
        private SkinParser parser;
        private SkinComposer composer;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeServices();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeServices()
        {
            fileSystem = new SkinFileSystem();
            textureLoader = new SkinTextureLoader();
            parser = new SkinParser();
            composer = new SkinComposer();
        }

        #region Public Methods

        public string UnpackWsz(string wszPath)
        {
           return fileSystem.UnpackWsz(wszPath);
        }

        public void PickSkinFile()
        {
            fileSystem.PickSkinFile(path => {
                string outputDir = UnpackWsz(path);
                // We could fire an event here or just let the caller handle it.
                // The original code called UnpackWsz then just logged. 
                // The Manager usually triggers load after this or monitors.
                // But for now, just replicate the Unpack call.
            });
        }
        
        /// <summary>
        /// Loads all assets for a skin from the file system.
        /// </summary>
        public async Task<WinampSkin> LoadSkinAsync(string skinName)
        {
             WinampSkin skin = new WinampSkin { SkinName = skinName };
             string skinDir = fileSystem.GetSkinDirectory(skinName);

             if (!System.IO.Directory.Exists(skinDir))
             {
                 Debug.LogError($"[WinampSkinImporter] Skin directory not found: {skinDir}");
                 return null;
             }

             await LoadFullSkinAssets(skin, skinDir);
             
             return skin;
        }

        #endregion

        #region Internal Loading

        private async Task LoadFullSkinAssets(WinampSkin skin, string skinDir)
        {
             // 1. Main
             var mainTex = await LoadTex(skinDir, "MAIN.BMP", "MAIN.PNG", "main.bmp", "main.png");
             composer.ComposeMain(skin, mainTex);

             // 2. TitleBar
             var titleTex = await LoadTex(skinDir, "TITLEBAR.BMP", "TITLEBAR.PNG", "titlebar.bmp", "titlebar.png", "TitleBar.bmp");
             if (titleTex != null) composer.ComposeTitleBar(skin, titleTex);

             // 3. ShufRep
             var shufTex = await LoadTex(skinDir, "SHUFREP.BMP", "SHUFREP.PNG", "shufrep.bmp", "shufrep.png");
             composer.ComposeShufRep(skin, shufTex, mainTex); 

             // 4. CButtons
             var cbtnTex = await LoadTex(skinDir, "CBUTTONS.BMP", "CBUTTONS.PNG", "cbuttons.bmp", "cbuttons.png");
             composer.ComposeCButtons(skin, cbtnTex);

             // 5. PosBar
             var posTex = await LoadTex(skinDir, "POSBAR.BMP", "posbar.bmp", "POSBAR.PNG", "posbar.png", "Posbar.bmp");
             composer.ComposePosBar(skin, posTex, mainTex);

             // 6. MonoSter
             var monoTex = await LoadTex(skinDir, "MONOSTER.BMP", "monoster.bmp", "MONOSTER.PNG", "monoster.png");
             composer.ComposeMonoSter(skin, monoTex);

             // 7. Volume
             var volTex = await LoadTex(skinDir, "VOLUME.BMP", "volume.bmp", "VOLUME.PNG", "volume.png");
             composer.ComposeVolume(skin, volTex);

             // 8. Balance
             var balTex = await LoadTex(skinDir, "BALANCE.BMP", "balance.bmp", "BALANCE.PNG", "balance.png");
             composer.ComposeBalance(skin, balTex);

             // 9. PlayPaus
             var ppTex = await LoadTex(skinDir, "PLAYPAUS.BMP", "playpaus.bmp", "PLAYPAUS.PNG", "playpaus.png", "PlayPaus.bmp");
             composer.ComposePlayPaus(skin, ppTex);

             // 10. Numbers
             var numTex = await LoadTex(skinDir, "NUMBERS.BMP", "numbers.bmp", "Numbers.bmp", "NUMBERS.PNG", "numbers.png", "Numbers.png");
             var numExTex = await LoadTex(skinDir, "NUMS_EX.BMP", "nums_ex.bmp", "Nums_ex.bmp", "NUMS_EX.PNG", "nums_ex.png", "Nums_ex.png");
             composer.ComposeNumbers(skin, numTex, numExTex);

             // 11. Text
             var textTex = await LoadTex(skinDir, "TEXT.BMP", "text.bmp", "Text.bmp", "TEXT.PNG", "text.png", "Text.png");
             composer.ComposeText(skin, textTex);

             // 12. EQ Main
             var eqTex = await LoadTex(skinDir, "EQMAIN.BMP", "eqmain.bmp", "EQMAIN.PNG", "eqmain.png");
             composer.ComposeEqualizer(skin, eqTex);

             // 13. Playlist
             var plTex = await LoadTex(skinDir, "PLEDIT.BMP", "pledit.bmp", "PLEDIT.PNG", "pledit.png");
             composer.ComposePlaylist(skin, plTex);

             // 14. Data Files (Text)
             await LoadSkinData(skin, skinDir);
        }

        private async Task LoadSkinData(WinampSkin skin, string skinDir)
        {
             // VISCOLOR
             string visPath = fileSystem.FindFile(skinDir, new[] { "VISCOLOR.TXT", "viscolor.txt" });
             if (visPath != null)
             {
                 string text = await System.IO.File.ReadAllTextAsync(visPath);
                 skin.VisColors = parser.ParseVisColor(text);
             }

             // PLEDIT.TXT
             string plTxtPath = fileSystem.FindFile(skinDir, new[] { "PLEDIT.TXT", "pledit.txt" });
             if (plTxtPath != null)
             {
                  string text = await System.IO.File.ReadAllTextAsync(plTxtPath);
                  parser.ParsePlEditTxt(text, skin);
             }
        }

        private async Task<Texture2D> LoadTex(string dir, params string[] candidates)
        {
            string path = fileSystem.FindFile(dir, candidates);
            if (path == null) return null;
            return await textureLoader.LoadTextureAsync(path);
        }

        #endregion
    }
}
