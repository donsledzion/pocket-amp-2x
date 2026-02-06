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
                    SetWindowVisibility(eqWindow, isOn);
                });
                // Sync initial state
                SetWindowVisibility(eqWindow, eqButton.IsOn);
            }

            if (playlistButton != null)
            {
                playlistButton.OnValueChanged.AddListener((isOn) => {
                    SetWindowVisibility(playlistWindow, isOn);
                });
                // Sync initial state
                SetWindowVisibility(playlistWindow, playlistButton.IsOn);
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
