using UnityEngine;
using UnityEngine.UI;

namespace SoftAware.PocketAmp
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
            
            if (mainPanel.PlaylistUI != null)
            {
                mainPanel.PlaylistUI.Initialize();
                mainPanel.PlaylistUI.RefreshColors();
            }
        }

        public void SetDragging(bool dragging)
        {
            isDraggingSlider = dragging;
            if (!dragging && mainPanel.SongTitleDisplay != null)
            {
                mainPanel.SongTitleDisplay.ClearOverrideText();
            }
        }

        public void HandleSliderDrag(float normalizedValue, float duration)
        {
            if (mainPanel.SongTitleDisplay == null) return;

            int minutesTotal = (int)(duration / 60);
            int secondsTotal = (int)(duration % 60);
            string totalTimeStr = $"{minutesTotal}:{secondsTotal:D2}";

            float currentTime = normalizedValue * duration;
            int minutesCurrent = (int)(currentTime / 60);
            int secondsCurrent = (int)(currentTime % 60);
            string currentTimeStr = $"{minutesCurrent}:{secondsCurrent:D2}";

            int percent = Mathf.RoundToInt(normalizedValue * 100f);

            mainPanel.SongTitleDisplay.SetOverrideText($"SEEK TO: {currentTimeStr}/{totalTimeStr} ({percent}%)");
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

            if (mainPanel.PlaylistUI != null)
            {
                // index here is 1-based from AudioPlayer, PlaylistUI expects 0-based
                mainPanel.PlaylistUI.UpdateTrackDuration(index - 1, title, duration);
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
