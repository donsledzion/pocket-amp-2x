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
        internal ToggleButton ShuffleButton => shuffleButton;
        internal ToggleButton RepeatButton => repeatButton;

        [Header("Audio Info Displays")]
        [SerializeField] private SpriteTextDisplay bitrateDisplay;
        [SerializeField] private SpriteTextDisplay sampleRateDisplay;
        [SerializeField] private WinampTimeDisplay timeDisplay;
        internal SpriteTextDisplay BitrateDisplay => bitrateDisplay;
        internal SpriteTextDisplay SampleRateDisplay => sampleRateDisplay;
        internal WinampTimeDisplay TimeDisplay => timeDisplay;

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
