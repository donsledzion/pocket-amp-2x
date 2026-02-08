using UnityEngine;

namespace SoftAware
{
    public class SettingsManager : MonoBehaviour
    {
        public static SettingsManager Instance { get; private set; }

        private const string KEY_SHOW_EQ = "Winamp_ShowEQ";
        private const string KEY_SHOW_PLAYLIST = "Winamp_ShowPlaylist";
        private const string KEY_SHUFFLE = "Winamp_Shuffle";
        private const string KEY_REPEAT = "Winamp_Repeat";
        private const string KEY_VOLUME = "Winamp_Volume";
        private const string KEY_BALANCE = "Winamp_Balance";
        private const string KEY_VIS_MODE = "Winamp_VisMode";
        private const string KEY_TIME_MODE = "Winamp_TimeMode";
        private const string KEY_EQ_ON = "Winamp_EQ_On";
        private const string KEY_EQ_AUTO = "Winamp_EQ_Auto";
        private const string KEY_EQ_PREAMP = "Winamp_EQ_Preamp";
        private const string KEY_EQ_BAND_PREFIX = "Winamp_EQ_Band_";

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

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
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
