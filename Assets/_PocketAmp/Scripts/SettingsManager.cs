using UnityEngine;

namespace SoftAware.PocketAmp
{
    public class SettingsManager : MonoBehaviour
    {
        public static SettingsManager Instance { get; private set; }

        private const string KEY_SHOW_EQ = "PocketAmp_ShowEQ";
        private const string KEY_SHOW_PLAYLIST = "PocketAmp_ShowPlaylist";
        private const string KEY_SHUFFLE = "PocketAmp_Shuffle";
        private const string KEY_REPEAT = "PocketAmp_Repeat";
        private const string KEY_VOLUME = "PocketAmp_Volume";
        private const string KEY_BALANCE = "PocketAmp_Balance";
        private const string KEY_VIS_MODE = "PocketAmp_VisMode";
        private const string KEY_TIME_MODE = "PocketAmp_TimeMode";
        private const string KEY_EQ_ON = "PocketAmp_EQ_On";
        private const string KEY_EQ_AUTO = "PocketAmp_EQ_Auto";
        private const string KEY_EQ_PREAMP = "PocketAmp_EQ_Preamp";
        private const string KEY_EQ_BAND_PREFIX = "PocketAmp_EQ_Band_";
        private const string KEY_LAST_INDEX = "PocketAmp_LastIndex";
        private const string KEY_IS_FIRST_RUN = "PocketAmp_IsFirstRun";
        private const string KEY_EQ_PRESETS_BEHAVIOR = "PocketAmp_EQ_PresetsBehavior";

        [Header("Default Values")]
        [SerializeField] private bool defaultShowEQ = true;
        [SerializeField] private bool defaultShowPlaylist = true;
        [SerializeField] private bool defaultShuffle = false;
        [SerializeField] private bool defaultRepeat = false;
        [SerializeField, Range(0, 1)] private float defaultVolume = 0.5f;
        [SerializeField, Range(0, 1)] private float defaultBalance = 0.5f;

        public bool ShowEQ
        {
            get => PlayerPrefs.GetInt(KEY_SHOW_EQ, defaultShowEQ ? 1 : 0) == 1;
            set { if (value != ShowEQ) { PlayerPrefs.SetInt(KEY_SHOW_EQ, value ? 1 : 0); PlayerPrefs.Save(); } }
        }

        public bool ShowPlaylist
        {
            get => PlayerPrefs.GetInt(KEY_SHOW_PLAYLIST, defaultShowPlaylist ? 1 : 0) == 1;
            set { if (value != ShowPlaylist) { PlayerPrefs.SetInt(KEY_SHOW_PLAYLIST, value ? 1 : 0); PlayerPrefs.Save(); } }
        }

        public bool Shuffle
        {
            get => PlayerPrefs.GetInt(KEY_SHUFFLE, defaultShuffle ? 1 : 0) == 1;
            set { if (value != Shuffle) { PlayerPrefs.SetInt(KEY_SHUFFLE, value ? 1 : 0); PlayerPrefs.Save(); } }
        }

        public bool Repeat
        {
            get => PlayerPrefs.GetInt(KEY_REPEAT, defaultRepeat ? 1 : 0) == 1;
            set { if (value != Repeat) { PlayerPrefs.SetInt(KEY_REPEAT, value ? 1 : 0); PlayerPrefs.Save(); } }
        }

        public float Volume
        {
            get => PlayerPrefs.GetFloat(KEY_VOLUME, defaultVolume);
            set { if (!Mathf.Approximately(value, Volume)) { PlayerPrefs.SetFloat(KEY_VOLUME, value); PlayerPrefs.Save(); } }
        }

        public float Balance
        {
            get => PlayerPrefs.GetFloat(KEY_BALANCE, defaultBalance);
            set { if (!Mathf.Approximately(value, Balance)) { PlayerPrefs.SetFloat(KEY_BALANCE, value); PlayerPrefs.Save(); } }
        }

        public SpectrumVisualizer.VisMode VisualizerMode
        {
            get => (SpectrumVisualizer.VisMode)PlayerPrefs.GetInt(KEY_VIS_MODE, (int)SpectrumVisualizer.VisMode.Spectrum);
            set { if (value != VisualizerMode) { PlayerPrefs.SetInt(KEY_VIS_MODE, (int)value); PlayerPrefs.Save(); } }
        }

        public bool IsRemainingMode
        {
            get => PlayerPrefs.GetInt(KEY_TIME_MODE, 0) == 1;
            set { if (value != IsRemainingMode) { PlayerPrefs.SetInt(KEY_TIME_MODE, value ? 1 : 0); PlayerPrefs.Save(); } }
        }

        public bool EQOn
        {
            get => PlayerPrefs.GetInt(KEY_EQ_ON, 1) == 1;
            set { if (value != EQOn) { PlayerPrefs.SetInt(KEY_EQ_ON, value ? 1 : 0); PlayerPrefs.Save(); } }
        }

        public bool EQAuto
        {
            get => PlayerPrefs.GetInt(KEY_EQ_AUTO, 0) == 1;
            set { if (value != EQAuto) { PlayerPrefs.SetInt(KEY_EQ_AUTO, value ? 1 : 0); PlayerPrefs.Save(); } }
        }

        public float EQPreamp
        {
            get => PlayerPrefs.GetFloat(KEY_EQ_PREAMP, 0f);
            set { if (!Mathf.Approximately(value, EQPreamp)) { PlayerPrefs.SetFloat(KEY_EQ_PREAMP, value); PlayerPrefs.Save(); } }
        }

        public float GetEQBand(int index)
        {
            return PlayerPrefs.GetFloat(KEY_EQ_BAND_PREFIX + index, 0f);
        }

        public void SetEQBand(int index, float value)
        {
            if (!Mathf.Approximately(value, GetEQBand(index)))
            {
                PlayerPrefs.SetFloat(KEY_EQ_BAND_PREFIX + index, value);
                PlayerPrefs.Save();
            }
        }

        public int LastPlaylistIndex
        {
            get => PlayerPrefs.GetInt(KEY_LAST_INDEX, -1);
            set { if (value != LastPlaylistIndex) { PlayerPrefs.SetInt(KEY_LAST_INDEX, value); PlayerPrefs.Save(); } }
        }

        public bool IsFirstRun
        {
            get => PlayerPrefs.GetInt(KEY_IS_FIRST_RUN, 1) == 1;
            set { if (value != IsFirstRun) { PlayerPrefs.SetInt(KEY_IS_FIRST_RUN, value ? 1 : 0); PlayerPrefs.Save(); } }
        }

        private const string KEY_LAST_SKIN = "PocketAmp_LastSkin";
        public string LastSkinPath
        {
            get => PlayerPrefs.GetString(KEY_LAST_SKIN, "");
            set { if (value != LastSkinPath) { PlayerPrefs.SetString(KEY_LAST_SKIN, value); PlayerPrefs.Save(); } }
        }

        private const string KEY_IS_FULLSCREEN = "PocketAmp_IsFullscreen";
        public bool IsFullscreen
        {
            get => PlayerPrefs.GetInt(KEY_IS_FULLSCREEN, 1) == 1; // Default true (Immersive)
            set { if (value != IsFullscreen) { PlayerPrefs.SetInt(KEY_IS_FULLSCREEN, value ? 1 : 0); PlayerPrefs.Save(); } }
        }

        private const string KEY_IS_NAV_VISIBLE = "PocketAmp_IsNavVisible";
        public bool IsNavigationBarVisible
        {
            get => PlayerPrefs.GetInt(KEY_IS_NAV_VISIBLE, 1) == 1; // Default true (Visible)
            set { if (value != IsNavigationBarVisible) { PlayerPrefs.SetInt(KEY_IS_NAV_VISIBLE, value ? 1 : 0); PlayerPrefs.Save(); } }
        }

        public int EQPresetsLoadBehavior
        {
            get => PlayerPrefs.GetInt(KEY_EQ_PRESETS_BEHAVIOR, 0); // 0 = RequireLoadButton, 1 = LoadOnSelection
            set { if (value != EQPresetsLoadBehavior) { PlayerPrefs.SetInt(KEY_EQ_PRESETS_BEHAVIOR, value); PlayerPrefs.Save(); } }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Apply saved fullscreen setting on startup
            if (Application.platform == RuntimePlatform.Android)
            {
                ResolveSystemBars();
            }
            else
            {
                Screen.fullScreen = IsFullscreen;
            }
        }

        public void ResolveSystemBars()
        {
            if (Application.platform != RuntimePlatform.Android) return;

            // Navigation Bar Logic:
            // Visible -> Screen.fullScreen = false (Windowed mode, shows bars, resizes viewport)
            // Hidden -> Screen.fullScreen = true (Immersive mode, hides bars)
            bool desiredFullScreen = !IsNavigationBarVisible;
            
            // Only change screen mode if necessary to avoid unnecessary flickers
            if (Screen.fullScreen != desiredFullScreen)
            {
                Screen.fullScreen = desiredFullScreen;
            }

            // Status Bar Logic:
            // IsFullscreen (true) -> Status Bar Hidden
            // IsFullscreen (false) -> Status Bar Visible
            // We start a coroutine to enforce this state multiple times because Unity/Android 
            // love to reset these flags during orientation changes or window resizing.
            StopAllCoroutines();
            StartCoroutine(EnforceSystemBarsCoroutine());
        }

        private System.Collections.IEnumerator EnforceSystemBarsCoroutine()
        {
            // Initial forced set
            AndroidStatusBar.SetVisible(!IsFullscreen);

            // "Brute Force" Loop: 
            // Re-apply settings 5 times with 0.2s delay to overwrite any Unity/System resets.
            for (int i = 0; i < 5; i++)
            {
                yield return new WaitForSeconds(0.2f);
                AndroidStatusBar.SetVisible(!IsFullscreen);
            }
        }

        public void SaveSettings()
        {
            PlayerPrefs.Save();
        }

        private void OnApplicationQuit()
        {
            SaveSettings();
        }
    }
}
