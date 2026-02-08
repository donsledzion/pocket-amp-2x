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

        [Header("Default Values")]
        [SerializeField] private bool defaultShowEQ = true;
        [SerializeField] private bool defaultShowPlaylist = true;
        [SerializeField] private bool defaultShuffle = false;
        [SerializeField] private bool defaultRepeat = false;

        public bool ShowEQ
        {
            get => PlayerPrefs.GetInt(KEY_SHOW_EQ, defaultShowEQ ? 1 : 0) == 1;
            set => PlayerPrefs.SetInt(KEY_SHOW_EQ, value ? 1 : 0);
        }

        public bool ShowPlaylist
        {
            get => PlayerPrefs.GetInt(KEY_SHOW_PLAYLIST, defaultShowPlaylist ? 1 : 0) == 1;
            set => PlayerPrefs.SetInt(KEY_SHOW_PLAYLIST, value ? 1 : 0);
        }

        public bool Shuffle
        {
            get => PlayerPrefs.GetInt(KEY_SHUFFLE, defaultShuffle ? 1 : 0) == 1;
            set => PlayerPrefs.SetInt(KEY_SHUFFLE, value ? 1 : 0);
        }

        public bool Repeat
        {
            get => PlayerPrefs.GetInt(KEY_REPEAT, defaultRepeat ? 1 : 0) == 1;
            set => PlayerPrefs.SetInt(KEY_REPEAT, value ? 1 : 0);
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
