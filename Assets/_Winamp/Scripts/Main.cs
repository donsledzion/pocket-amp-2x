using System;
using UnityEngine;
using UnityEngine.UI;

namespace SoftAware
{
    public class Main : MonoBehaviour
    {
        [Header("Controls Buttons")]
        [SerializeField] private Button prevButton;
        [SerializeField] private Button playButton;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button stopButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button ejectButton;

        internal Button PrevButton => prevButton;
        internal Button PlayButton => playButton;
        internal Button PauseButton => pauseButton;
        internal Button StopButton => stopButton;
        internal Button NextButton => nextButton;
        internal Button EjectButton => ejectButton;

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

        [Header("Toggles")]
        [SerializeField] private ToggleButton shuffleButton;
        [SerializeField] private ToggleButton repeatButton;
        [SerializeField] private ToggleButton eqButton;
        [SerializeField] private ToggleButton playlistButton;
        internal ToggleButton ShuffleButton => shuffleButton;
        internal ToggleButton RepeatButton => repeatButton;
        internal ToggleButton EqButton => eqButton;
        internal ToggleButton PlaylistButton => playlistButton;

        [Header("Windows")]
        [SerializeField] private GameObject eqWindow;
        [SerializeField] private GameObject playlistWindow;
        internal GameObject EqWindow => eqWindow;
        internal GameObject PlaylistWindow => playlistWindow;

        private void Start()
        {
            if (eqButton != null)
            {
                eqButton.OnValueChanged.AddListener((isOn) => {
                    if (eqWindow != null) eqWindow.SetActive(isOn);
                });
                // Sync initial state
                if (eqWindow != null) eqWindow.SetActive(eqButton.IsOn);
            }

            if (playlistButton != null)
            {
                playlistButton.OnValueChanged.AddListener((isOn) => {
                    if (playlistWindow != null) playlistWindow.SetActive(isOn);
                });
                // Sync initial state
                if (playlistWindow != null) playlistWindow.SetActive(playlistButton.IsOn);
            }
        }

        [Header("Audio Info Displays")]
        [SerializeField] private SpriteTextDisplay bitrateDisplay;
        [SerializeField] private SpriteTextDisplay sampleRateDisplay;
        [SerializeField] private WinampTimeDisplay timeDisplay;
        [SerializeField] private WinampStatusDisplay statusDisplay;
        [SerializeField] private WinampSongTitleDisplay songTitleDisplay;
        internal SpriteTextDisplay BitrateDisplay => bitrateDisplay;
        internal SpriteTextDisplay SampleRateDisplay => sampleRateDisplay;
        internal WinampTimeDisplay TimeDisplay => timeDisplay;
        internal WinampStatusDisplay StatusDisplay => statusDisplay;
        internal WinampSongTitleDisplay SongTitleDisplay => songTitleDisplay;

        [SerializeField] private WinampPlaylistUI playlistUI;
        internal WinampPlaylistUI PlaylistUI => playlistUI;

        private void OnDestroy()
        {
            PrevButton.onClick.RemoveAllListeners();
            PlayButton.onClick.RemoveAllListeners();
            PauseButton.onClick.RemoveAllListeners();
            StopButton.onClick.RemoveAllListeners();
            NextButton.onClick.RemoveAllListeners();
            EjectButton.onClick.RemoveAllListeners();
        }
    }
}
