using UnityEngine;
using UnityEngine.UI;

namespace SoftAware
{
    /// <summary>
    /// Central controller for all Winamp UI elements.
    /// Decouples UI updates from the core playback logic.
    /// </summary>
    public class WinampUIController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Main mainPanel;
        
        private AudioPlayer player;
        private bool isDraggingSlider = false;

        public void Initialize(AudioPlayer audioPlayer)
        {
            player = audioPlayer;
            
            if (mainPanel.ProgressSlider != null)
            {
                // Hook into slider drag events if needed, 
                // but AudioPlayer still manages the slider binding for now.
            }
        }

        public void SetDragging(bool dragging)
        {
            isDraggingSlider = dragging;
        }

        public void UpdateUI(float currentTime, float duration, bool isPlaying, bool isPaused)
        {
            UpdateProgress(currentTime, duration, isPlaying, isPaused);
            UpdateStatus(isPlaying, isPaused);
            UpdateAudioInfo(isPlaying, isPaused);
        }

        private void UpdateProgress(float currentTime, float duration, bool isPlaying, bool isPaused)
        {
            if (mainPanel.ProgressSlider == null) return;

            float progress = (duration > 0) ? currentTime / duration : 0f;

            // Knob Visibility
            if (mainPanel.ProgressSlider.handleRect != null)
                mainPanel.ProgressSlider.handleRect.gameObject.SetActive(isPlaying || isPaused || isDraggingSlider);

            // Slider Value
            if ((isPlaying || isPaused) && !isDraggingSlider)
            {
                mainPanel.ProgressSlider.value = progress;
            }

            // Time Display
            if (mainPanel.TimeDisplay != null)
            {
                if (isPlaying || isPaused)
                {
                    mainPanel.TimeDisplay.SetTime(currentTime, duration);
                    mainPanel.TimeDisplay.SetPaused(isPaused);
                }
                else
                {
                    mainPanel.TimeDisplay.Clear();
                }
            }
        }

        private void UpdateStatus(bool isPlaying, bool isPaused)
        {
            if (mainPanel.StatusDisplay == null) return;

            if (isPlaying || isPaused)
            {
                mainPanel.StatusDisplay.SetStatus(isPaused ? 
                    WinampStatusDisplay.WinampStatus.Paused : 
                    WinampStatusDisplay.WinampStatus.Playing);
            }
            else
            {
                mainPanel.StatusDisplay.SetStatus(WinampStatusDisplay.WinampStatus.Stop);
            }
        }

        public void ShowLoading()
        {
            if (mainPanel.StatusDisplay != null)
                mainPanel.StatusDisplay.SetStatus(WinampStatusDisplay.WinampStatus.Loading);
        }

        public void UpdateSongInfo(int index, string title, float duration)
        {
            if (mainPanel.SongTitleDisplay != null)
            {
                mainPanel.SongTitleDisplay.SetSongInfo(index, title, duration);
            }
        }

        public void ClearSongInfo()
        {
            if (mainPanel.SongTitleDisplay != null)
                mainPanel.SongTitleDisplay.Clear();
        }

        public void UpdateMetadata(int bitrateKbps, int sampleRateKHz, int channels, bool active)
        {
            if (mainPanel.BitrateDisplay != null)
            {
                if (active) mainPanel.BitrateDisplay.SetNumber(bitrateKbps);
                else mainPanel.BitrateDisplay.Clear();
            }

            if (mainPanel.SampleRateDisplay != null)
            {
                if (active) mainPanel.SampleRateDisplay.SetNumber(sampleRateKHz);
                else mainPanel.SampleRateDisplay.Clear();
            }

            if (mainPanel.ChannelsDisplay != null)
            {
                mainPanel.ChannelsDisplay.UpdateDisplay(active, channels);
            }
        }

        private void UpdateAudioInfo(bool isPlaying, bool isPaused)
        {
            // This is called per frame, but we only update metadata when it changes 
            // or through specific calls from AudioPlayer.
        }
    }
}
