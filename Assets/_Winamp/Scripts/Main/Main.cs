using UnityEngine;
using UnityEngine.UI;
using SoftAware; // For IWinampSkinApplicator and WinampSkin

namespace SoftAware.PocketAmp
{
    public class Main : MonoBehaviour, ISkinApplicator
    {
        [Header("Component References")]
        [SerializeField] private MainTitleBar mainTitleBar;
        [SerializeField] private MainControls mainControls;
        // [SerializeField] private MainIndicators mainIndicators; // Replaced by ChannelsDisplay
        
        [Header("Main Window Elements (Legacy/Direct)")]
        [SerializeField] private Image mainBackgroundImage;

        public void ApplySkin(WinampSkin skin)
        {
            if (skin == null) return;
            
            // Apply Main Window Background
            if (mainBackgroundImage != null && skin.MainBackground != null)
            {
                mainBackgroundImage.sprite = skin.MainBackground;
            }

            if (statusDisplay != null)
            {
                statusDisplay.ApplySkin(skin);
            }
            // Apply Title Bar (via component)
            if (mainTitleBar != null)
            {
                mainTitleBar.ApplySkin(skin);
            }

            // Apply Control Buttons & Toggles
            if (mainControls != null)
            {
                mainControls.ApplySkin(skin);
            }
            
            // Apply Indicators (Mono/Stereo via ChannelsDisplay)
            if (channelsDisplay != null)
            {
                channelsDisplay.ApplySkin(skin);
            }
            
            // Apply Volume & Balance
            if (volumeController != null)
            {
                volumeController.ApplySkin(skin);
            }
            if (balanceController != null)
            {
                balanceController.ApplySkin(skin);
            }
            
            // Apply Position Bar (Progress Slider)
            if (progressSlider != null && progressSlider.targetGraphic is Image posHandle)
            {
                if (skin.PosKnobNormal != null)
                {
                    posHandle.sprite = skin.PosKnobNormal;
                }
                
                SpriteState ss = progressSlider.spriteState;
                if (skin.PosKnobPressed != null)
                {
                    ss.pressedSprite = skin.PosKnobPressed;
                }
                progressSlider.spriteState = ss;
            }

            // Apply Time Display
            if (timeDisplay != null)
            {
                timeDisplay.ApplySkin(skin);
            }

            // Apply Visualizer Colors (VISCOLOR.TXT)
            if (spectrumVisualizer != null)
            {
                Debug.Log($"[Main] Propagating skin to SpectrumVisualizer. Colors: {(skin.VisColors != null ? skin.VisColors.Length.ToString() : "NULL")}");
                spectrumVisualizer.ApplySkin(skin);
            }
            else
            {
                Debug.LogWarning("[Main] spectrumVisualizer reference is NULL during ApplySkin!");
            }

            // Apply to Equalizer Window
            if (eqWindow != null)
            {
                var eqController = eqWindow.GetComponent<EqualizerController>();
                if (eqController != null)
                {
                    Debug.Log("[Main] Propagating skin to EqualizerController");
                    eqController.ApplySkin(skin);
                }
            }

            // Apply Font to Text Displays (Bitrate, Samplerate, Song Title)
            if (bitrateDisplay != null) bitrateDisplay.ApplySkin(skin);
            if (sampleRateDisplay != null) sampleRateDisplay.ApplySkin(skin);
            if (songTitleDisplay != null) songTitleDisplay.ApplySkin(skin);
        }

        internal Button PrevButton => mainControls != null ? mainControls.PrevButton : null;
        internal Button PlayButton => mainControls != null ? mainControls.PlayButton : null;
        internal Button PauseButton => mainControls != null ? mainControls.PauseButton : null;
        internal Button StopButton => mainControls != null ? mainControls.StopButton : null;
        internal Button NextButton => mainControls != null ? mainControls.NextButton : null;
        internal Button EjectButton => mainControls != null ? mainControls.EjectButton : null;

        [Header("Progress")]
        [SerializeField] private Slider progressSlider;
        internal Slider ProgressSlider => progressSlider;

        [Header("Volume")]
        [SerializeField] private VolumeController volumeController;
        internal VolumeController VolumeController => volumeController;

        [Header("Balance")]
        [SerializeField] private BalanceController balanceController;
        internal BalanceController BalanceController => balanceController;

        [Header("Channels")]
        [SerializeField] private ChannelsDisplay channelsDisplay;
        internal ChannelsDisplay ChannelsDisplay => channelsDisplay;

        // Toggles redirected to MainControls
        internal ToggleButton ShuffleButton => mainControls != null ? mainControls.ShuffleButton : null;
        internal ToggleButton RepeatButton => mainControls != null ? mainControls.RepeatButton : null;
        
        // These toggle buttons (EQ/Playlist) are separate from MainControls (they are layout toggles/windows toggles, often separate in skin)
        // But in default skin they are also buttons. If you want them in MainControls, we can move them too.
        // For now, let's keep EQ/Playlist separate as they might be handled differently or just added to MainControls later if requested.
        // Wait, user said: "buttons controls: play, next, prev, pause, stop eject, shuffle, repeat". It didn't list EQ/PL.
        // So I will keep EQ/PL as is for now.
        
        [Header("Layout Toggles")]
        // [SerializeField] private ToggleButton eqButton; // Moved to MainControls
        // [SerializeField] private ToggleButton playlistButton; // Moved to MainControls
        
        internal ToggleButton EqButton => mainControls != null ? mainControls.EqButton : null;
        internal ToggleButton PlaylistButton => mainControls != null ? mainControls.PlaylistButton : null;

        [Header("Windows")]
        [SerializeField] private GameObject eqWindow;
        [SerializeField] private GameObject playlistWindow;
        [SerializeField] private GameObject visWindow; 
        internal GameObject EqWindow => eqWindow;
        internal GameObject PlaylistWindow => playlistWindow;
        internal GameObject VisWindow => visWindow;

        [Header("System Windows")]
        [SerializeField] private GameObject skinsLibraryWindow; 
        [SerializeField] private GameObject miscOptionsMenu; 
        [SerializeField] private GameObject addUrlWindow;

        [Header("Window App Controls")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button minimizeButton;
        private bool isVisWindowOpen = false;

        private void Start()
        {
            // VisWindow zawsze startuje jako wyłączony (nie zapamiętujemy stanu)
            if (visWindow != null)
            {
                isVisWindowOpen = false;
                SetWindowVisibility(visWindow, false);
            }

            // Fallback for spectrumVisualizer if not assigned in Inspector
            if (spectrumVisualizer == null)
            {
                spectrumVisualizer = FindFirstObjectByType<SpectrumVisualizer>();
                if (spectrumVisualizer != null) Debug.Log("[Main] Found SpectrumVisualizer via Fallback.");
            }

            // Load and Apply Settings
            if (SettingsManager.Instance != null)
            {
                if (EqButton != null) EqButton.SetState(SettingsManager.Instance.ShowEQ);
                if (PlaylistButton != null) PlaylistButton.SetState(SettingsManager.Instance.ShowPlaylist);
                
                // Shuffle/Repeat via MainControls
                if (ShuffleButton != null) ShuffleButton.SetState(SettingsManager.Instance.Shuffle);
                if (RepeatButton != null) RepeatButton.SetState(SettingsManager.Instance.Repeat);
                
                if (volumeController != null && volumeController.Slider != null)
                    volumeController.Slider.value = SettingsManager.Instance.Volume;
                
                if (balanceController != null && balanceController.Slider != null)
                    balanceController.Slider.value = SettingsManager.Instance.Balance;

                if (spectrumVisualizer != null)
                    spectrumVisualizer.SetMode(SettingsManager.Instance.VisualizerMode);

                if (timeDisplay != null)
                    timeDisplay.SetMode(SettingsManager.Instance.IsRemainingMode);
            }

            if (EqButton != null)
            {
                EqButton.OnValueChanged.AddListener((isOn) => {
                    if (SettingsManager.Instance != null) SettingsManager.Instance.ShowEQ = isOn;
                    SetWindowVisibility(eqWindow, isOn);
                });
                SetWindowVisibility(eqWindow, EqButton.IsOn);
            }

            if (PlaylistButton != null)
            {
                PlaylistButton.OnValueChanged.AddListener((isOn) => {
                    if (SettingsManager.Instance != null) SettingsManager.Instance.ShowPlaylist = isOn;
                    SetWindowVisibility(playlistWindow, isOn);
                });
                SetWindowVisibility(playlistWindow, PlaylistButton.IsOn);
            }

            if (ShuffleButton != null)
            {
                ShuffleButton.OnValueChanged.AddListener((isOn) => {
                    if (SettingsManager.Instance != null) SettingsManager.Instance.Shuffle = isOn;
                });
            }

            if (RepeatButton != null)
            {
                RepeatButton.OnValueChanged.AddListener((isOn) => {
                    if (SettingsManager.Instance != null) SettingsManager.Instance.Repeat = isOn;
                });
            }

            if (volumeController != null && volumeController.Slider != null)
            {
                volumeController.Slider.onValueChanged.AddListener((val) => {
                    if (SettingsManager.Instance != null) SettingsManager.Instance.Volume = val;
                });
            }

            if (balanceController != null && balanceController.Slider != null)
            {
                balanceController.Slider.onValueChanged.AddListener((val) => {
                    if (SettingsManager.Instance != null) SettingsManager.Instance.Balance = val;
                });
            }

            if (spectrumVisualizer != null)
            {
                Debug.Log("[Main] Subscribing to SpectrumVisualizer DoubleClick.");
                spectrumVisualizer.OnModeChanged += (mode) => {
                    if (SettingsManager.Instance != null) SettingsManager.Instance.VisualizerMode = mode;
                };

                // Subscribing to the DoubleClick event to toggle the window
                spectrumVisualizer.OnDoubleClick += ToggleVisWindow;
            }
            else
            {
                Debug.Log("[Main] ERR: SpectrumVisualizer NOT FOUND!");
            }

            if (timeDisplay != null)
            {
                timeDisplay.OnModeChanged += (remaining) => {
                    if (SettingsManager.Instance != null) SettingsManager.Instance.IsRemainingMode = remaining;
                };
            }

            if (closeButton != null) closeButton.onClick.AddListener(CloseApplication);
            if (minimizeButton != null) minimizeButton.onClick.AddListener(MinimizeApplication);
            
            // Bind Title Bar buttons if available through new component
            if (mainTitleBar != null)
            {
                if (mainTitleBar.CloseButton != null) mainTitleBar.CloseButton.onClick.AddListener(CloseApplication);
                if (mainTitleBar.MinimizeButton != null) mainTitleBar.MinimizeButton.onClick.AddListener(MinimizeApplication);
            }
        }

        private void SetWindowVisibility(GameObject window, bool visible)
        {
            if (window == null) return;

            // 1. Handle Visuals (alpha) and Interaction
            CanvasGroup group = window.GetComponent<CanvasGroup>();
            if (group == null) group = window.AddComponent<CanvasGroup>();

            group.alpha = visible ? 1f : 0f;
            group.interactable = visible;
            group.blocksRaycasts = visible;

            // 2. Handle Layout (remove from LayoutGroup if hidden)
            LayoutElement layout = window.GetComponent<LayoutElement>();
            if (layout == null) layout = window.AddComponent<LayoutElement>();
            layout.ignoreLayout = !visible;

            // 3. Ensure the object itself is active so coroutines can run
            if (!window.activeSelf) window.SetActive(true);
        }

        [Header("Audio Info Displays")]
        [SerializeField] private SpectrumVisualizer spectrumVisualizer;
        [SerializeField] private SpriteTextDisplay bitrateDisplay;
        [SerializeField] private SpriteTextDisplay sampleRateDisplay;
        [SerializeField] private TimeDisplay timeDisplay;
        [SerializeField] private StatusDisplay statusDisplay;
        [SerializeField] private WinampSongTitleDisplay songTitleDisplay;
        internal SpriteTextDisplay BitrateDisplay => bitrateDisplay;
        internal SpriteTextDisplay SampleRateDisplay => sampleRateDisplay;
        internal TimeDisplay TimeDisplay => timeDisplay;
        internal StatusDisplay StatusDisplay => statusDisplay;
        internal WinampSongTitleDisplay SongTitleDisplay => songTitleDisplay;

        [SerializeField] private PlaylistUI playlistUI;
        internal PlaylistUI PlaylistUI => playlistUI;

        // Windows Minimize Support
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ShowWindow(System.IntPtr hwnd, int nCmdShow);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern System.IntPtr GetActiveWindow();
        const int SW_MINIMIZE = 6;
#endif

        public void CloseApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void MinimizeApplication()
        {
#if UNITY_EDITOR
            Debug.Log("[Main] Minimize requested (Editor)");
#elif UNITY_ANDROID
            try
            {
                using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                    {
                        activity.Call<bool>("moveTaskToBack", true);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[Main] Failed to minimize on Android: {e.Message}");
            }
#elif UNITY_STANDALONE_WIN
            ShowWindow(GetActiveWindow(), SW_MINIMIZE);
#endif
        }

        public void CloseEqualizerWindow()
        {
            if (EqButton != null && EqButton.IsOn)
            {
                EqButton.Toggle(); // This will trigger the OnValueChanged listener which hides the window
            }
        }

        public void ClosePlaylistWindow()
        {
            if (PlaylistButton != null && PlaylistButton.IsOn)
            {
                PlaylistButton.Toggle(); // This will trigger the OnValueChanged listener which hides the window
            }
        }

        public void OpenSkinsLibrary()
        {
            Debug.Log($"[Main] OpenSkinsLibrary called. Window ref: {skinsLibraryWindow}");
            if (skinsLibraryWindow == null) Debug.LogError("[Main] SkinsLibraryWindow reference is NULL!");
            
            SetWindowVisibility(skinsLibraryWindow, true);
        }

        public void CloseSkinsLibrary()
        {
            SetWindowVisibility(skinsLibraryWindow, false);
        }

        private void ToggleVisWindow()
        {
            isVisWindowOpen = !isVisWindowOpen;
            SetWindowVisibility(visWindow, isVisWindowOpen);
            Debug.Log($"[Main] Visualizer Window Toggled: {isVisWindowOpen}");
        }

        private void OnDestroy()
        {
            PrevButton.onClick.RemoveAllListeners();
            PlayButton.onClick.RemoveAllListeners();
            PauseButton.onClick.RemoveAllListeners();
            StopButton.onClick.RemoveAllListeners();
            NextButton.onClick.RemoveAllListeners();
            EjectButton.onClick.RemoveAllListeners();
        }

        internal void OpenMiscOptionsMenu() => SetWindowVisibility(miscOptionsMenu, true);
        internal void CloseMiscOptionsMenu() => SetWindowVisibility(miscOptionsMenu, false);

        internal void OpenAddUrlWindow() => SetWindowVisibility(addUrlWindow, true);
        internal void CloseAddUrlWindow() => SetWindowVisibility(addUrlWindow, false);
    }
}
