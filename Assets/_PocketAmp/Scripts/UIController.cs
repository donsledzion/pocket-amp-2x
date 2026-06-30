using UnityEngine;

namespace SoftAware.PocketAmp
{
    /// <summary>
    /// Central controller for all UI elements.
    /// Decouples UI updates from the core playback logic.
    /// </summary>
    public class UIController : MonoBehaviour
    {
        private AudioPlayer player;
        private bool isDraggingSlider = false;
        private static Main main => Refs.Main;

        public void Initialize(AudioPlayer audioPlayer)
        {
            player = audioPlayer;

            if (main.PlaylistUI == null) return;
            main.PlaylistUI.Initialize();
            main.PlaylistUI.RefreshColors();
        }

        public void SetDragging(bool dragging)
        {
            isDraggingSlider = dragging;
            if (!dragging && main.SongTitleDisplay != null)
            {
                main.SongTitleDisplay.ClearOverrideText();
            }
        }

        [Header("Localization")]
        [SerializeField] private UnityEngine.Localization.LocalizedString seekToText;

        public void HandleSliderDrag(float normalizedValue, float duration)
        {
            if (main.SongTitleDisplay == null) return;

            int minutesTotal = (int)(duration / 60);
            int secondsTotal = (int)(duration % 60);
            string totalTimeStr = $"{minutesTotal}:{secondsTotal:D2}";

            float currentTime = normalizedValue * duration;
            int minutesCurrent = (int)(currentTime / 60);
            int secondsCurrent = (int)(currentTime % 60);
            string currentTimeStr = $"{minutesCurrent}:{secondsCurrent:D2}";

            int percent = Mathf.RoundToInt(normalizedValue * 100f);

            if (seekToText != null && !seekToText.IsEmpty)
            {
                seekToText.Arguments = new object[] { currentTimeStr, totalTimeStr, percent };
                main.SongTitleDisplay.SetOverrideText(seekToText.GetLocalizedString());
            }
            else
            {
                main.SongTitleDisplay.SetOverrideText($"SEEK TO: {currentTimeStr}/{totalTimeStr} ({percent}%)");
            }
        }

        public void UpdateUI(float currentTime, float duration, bool isPlaying, bool isPaused)
        {
            UpdateProgress(currentTime, duration, isPlaying, isPaused);
            UpdateStatus(isPlaying, isPaused);
            UpdateAudioInfo(isPlaying, isPaused);
        }

        private void UpdateProgress(float currentTime, float duration, bool isPlaying, bool isPaused)
        {
            if (!main.ProgressSlider) return;
            
            bool isStream = duration <= 0f;

            var progress = (!isStream) ? currentTime / duration : 0f;

            // Knob Visibility
            if (main.ProgressSlider.handleRect)
                main.ProgressSlider.handleRect.gameObject.SetActive((isPlaying || isPaused || isDraggingSlider) && !isStream);

            // Slider Value
            if ((isPlaying || isPaused) && !isDraggingSlider)
            {
                main.ProgressSlider.value = progress;
            }

            // Time Display
            if (main.TimeDisplay)
            {
                if ((isPlaying || isPaused) && !isStream)
                {
                    main.TimeDisplay.SetTime(currentTime, duration);
                    main.TimeDisplay.SetPaused(isPaused);
                }
                else
                {
                    main.TimeDisplay.Clear();
                }
            }
        }
        private void UpdateStatus(bool isPlaying, bool isPaused)
        {
            if (!main.StatusDisplay) return;

            if (isPlaying || isPaused)
            {
                main.StatusDisplay.SetStatus(isPaused ? 
                    StatusDisplay.PocketAmpStatus.Paused : 
                    StatusDisplay.PocketAmpStatus.Playing);
            }
            else
            {
                main.StatusDisplay.SetStatus(StatusDisplay.PocketAmpStatus.Stop);
            }
        }

        public void ShowLoading()
        {
            if (main.StatusDisplay)
                main.StatusDisplay.SetStatus(StatusDisplay.PocketAmpStatus.Loading);
        }

        public void HideLoading()
        {
            if (main.StatusDisplay)
                main.StatusDisplay.SetStatus(StatusDisplay.PocketAmpStatus.Stop);
        }

        public void UpdateSongInfo(int index, string title, float duration)
        {
            if (main.SongTitleDisplay)
            {
                if (duration <= 0f) duration = 0f; // Prevent weird -1 display
                main.SongTitleDisplay.SetSongInfo(index, title, duration);
            }

            if (main.PlaylistUI)
            {
                // index here is 1-based from AudioPlayer, PlaylistUI expects 0-based
                main.PlaylistUI.UpdateTrackDuration(index - 1, title, duration);
            }
        }

        public void ClearSongInfo()
        {
            if (main.SongTitleDisplay)
                main.SongTitleDisplay.Clear();
        }

        public void UpdateMetadata(int bitrateKbps, int sampleRateKHz, int channels, bool active)
        {
            if (main.BitrateDisplay)
            {
                if (active) main.BitrateDisplay.SetNumber(bitrateKbps);
                else main.BitrateDisplay.Clear();
            }

            if (main.SampleRateDisplay)
            {
                if (active) main.SampleRateDisplay.SetNumber(sampleRateKHz);
                else main.SampleRateDisplay.Clear();
            }

            if (main.ChannelsDisplay)
            {
                main.ChannelsDisplay.UpdateDisplay(active, channels);
            }
        }

        private void UpdateAudioInfo(bool isPlaying, bool isPaused)
        {
            // This is called per frame, but we only update metadata when it changes 
            // or through specific calls from AudioPlayer.
        }
    }
}
