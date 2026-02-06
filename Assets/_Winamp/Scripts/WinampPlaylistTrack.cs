using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

namespace SoftAware
{
    public class WinampPlaylistTrack : MonoBehaviour, IPointerClickHandler
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI indexText;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI durationText;
        [SerializeField] private Image background;

        [Header("Settings")]
        [SerializeField] private Color normalTextColor = Color.white;
        [SerializeField] private Color playingTextColor = new Color(0.95f, 0.95f, 0.4f); // Winamp yellow
        [SerializeField] private Color selectedBgColor = new Color(0f, 0f, 0.5f); // Winamp blue

        private int index;
        private string originalTitle;
        private Action<int> onClick;
        private Action<int> onDoubleClick;
        private float lastClickTime;
        private const float DOUBLE_CLICK_TIME = 0.3f;

        public void SetColors(Color normal, Color playing, Color normalBg, Color selectedBg)
        {
            normalTextColor = normal;
            playingTextColor = playing;
            selectedBgColor = selectedBg;
            
            // Re-apply current playing color to all local labels
            SetPlaying(isTrackPlaying);
            
            // If currently selected, refresh background color
            if (background != null && background.enabled)
            {
                background.color = selectedBgColor;
            }
        }
        
        // Internal state trackers to help SetColors
        private bool isTrackPlaying = false;

        public void Setup(int trackIndex, string title, float duration, Action<int> clickCallback, Action<int> doubleClickCallback)
        {
            index = trackIndex;
            originalTitle = title;
            onClick = clickCallback;
            onDoubleClick = doubleClickCallback;

            RefreshDisplay(duration);
            
            SetSelected(false);
            SetPlaying(false);
        }

        public void RefreshDuration(string title, float duration)
        {
            originalTitle = title;
            RefreshDisplay(duration);
        }

        private void RefreshDisplay(float duration)
        {
            if (indexText != null) indexText.text = $"{index + 1}.";
            
            if (titleText != null) 
            {
                // Clean title (strip extensions)
                string cleanTitle = originalTitle;
                if (cleanTitle.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase) || 
                    cleanTitle.EndsWith(".wav", StringComparison.OrdinalIgnoreCase) || 
                    cleanTitle.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase))
                {
                    cleanTitle = System.IO.Path.GetFileNameWithoutExtension(cleanTitle);
                }
                
                // Use manual truncation helper
                TruncateWithEllipsis(titleText, cleanTitle.ToUpper());
            }

            if (durationText != null)
            {
                durationText.text = duration > 0 ? AudioMetadataUtils.FormatTime(duration) : "?:??";
            }
        }

        private void TruncateWithEllipsis(TextMeshProUGUI tmp, string text)
        {
            tmp.text = text;
            // Get the available width from the RectTransform
            float maxWidth = tmp.rectTransform.rect.width;

            // If the RectTransform hasn't been sized yet (e.g. 0), 
            // we can't truncate accurately, so we just set the text and hope for the best.
            if (maxWidth <= 0) 
            {
                tmp.text = text;
                return;
            }

            if (tmp.preferredWidth > maxWidth)
            {
                string t = text;
                while (t.Length > 0)
                {
                    t = t.Substring(0, t.Length - 1);
                    tmp.text = t + "...";
                    // TMP's preferredWidth is calculated based on the current text
                    if (tmp.preferredWidth <= maxWidth)
                        break;
                }
            }
        }

        public void SetPlaying(bool isPlaying)
        {
            isTrackPlaying = isPlaying;
            Color c = isPlaying ? playingTextColor : normalTextColor;
            if (indexText != null) indexText.color = c;
            if (titleText != null) titleText.color = c;
            if (durationText != null) durationText.color = c;
        }

        public void SetSelected(bool isSelected)
        {
            background.enabled = isSelected;
            background.color = selectedBgColor;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            float timeSinceLastClick = Time.time - lastClickTime;
            if (timeSinceLastClick < DOUBLE_CLICK_TIME)
            {
                onDoubleClick?.Invoke(index);
            }
            else
            {
                onClick?.Invoke(index);
            }
            lastClickTime = Time.time;
        }
    }
}
