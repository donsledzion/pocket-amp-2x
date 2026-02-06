using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
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
        private float currentDuration;
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
            currentDuration = duration;
            onClick = clickCallback;
            onDoubleClick = doubleClickCallback;

            RefreshDisplay(duration);
            
            SetSelected(false);
            SetPlaying(false);
        }

        public void RefreshDuration(string title, float duration)
        {
            originalTitle = title;
            currentDuration = duration;
            RefreshDisplay(duration);
        }

        protected void OnRectTransformDimensionsChange()
        {
            // Optional: Also retry if dimensions changed significantly
            if (gameObject.activeInHierarchy)
            {
                RefreshDisplay(currentDuration);
            }
        }

        private void RefreshDisplay(float duration)
        {
            // Start a coroutine to handle display, because we might need to wait for layout/size
            StartCoroutine(RefreshDisplayCoroutine(duration));
        }

        private IEnumerator RefreshDisplayCoroutine(float duration)
        {
            if (indexText != null) indexText.text = $"{index + 1}.";
            if (durationText != null)
            {
                durationText.text = duration > 0 ? AudioMetadataUtils.FormatTime(duration) : "?:??";
            }

            if (titleText == null) yield break;

            // Wait until the end of the frame and for width to be valid
            // Items are often instantiated with 0 width until layout runs
            int failsafe = 0;
            while (titleText.rectTransform.rect.width <= 0 && failsafe < 5)
            {
                yield return null;
                failsafe++;
            }

            // Clean title (strip extensions)
            string cleanTitle = originalTitle;
            if (cleanTitle.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase) || 
                cleanTitle.EndsWith(".wav", StringComparison.OrdinalIgnoreCase) || 
                cleanTitle.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase))
            {
                cleanTitle = System.IO.Path.GetFileNameWithoutExtension(cleanTitle);
            }
            
            // Use manual truncation helper
            TruncateWithEllipsis(titleText, cleanTitle);
        }

        private void TruncateWithEllipsis(TextMeshProUGUI tmp, string text)
        {
            // Get the available width from the RectTransform
            float maxWidth = tmp.rectTransform.rect.width;

            // Jeśli layout jeszcze nie jest gotowy, ustawiamy tekst i wychodzimy
            if (maxWidth < 5f) 
            {
                tmp.text = text;
                return;
            }

            // Sprawdzamy, ile miejsca potrzebuje pełny tekst (matematycznie)
            Vector2 preferredSize = tmp.GetPreferredValues(text);

            if (preferredSize.x <= maxWidth)
            {
                tmp.text = text;
            }
            else
            {
                string t = text;
                // Ucinamy znak po znaku, sprawdzając szerokość z wielokropkiem
                while (t.Length > 0)
                {
                    t = t.Substring(0, t.Length - 1);
                    string candidate = t + "...";
                    
                    // GetPreferredValues jest błyskawiczne i nie wymaga ForceMeshUpdate
                    if (tmp.GetPreferredValues(candidate).x <= maxWidth)
                    {
                        tmp.text = candidate;
                        return;
                    }
                }
                
                // Jeśli nawet same kropki się nie mieszczą, wyświetl je mimo wszystko
                tmp.text = "...";
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
