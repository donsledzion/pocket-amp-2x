using UnityEngine;
using System.Collections;

namespace SoftAware
{
    /// <summary>
    /// Manages the scrolling song title display in the PocketAmp main window.
    /// Format: {index}. {Artist} - {Title} ({mm:ss})
    /// </summary>
    public class SongTitleDisplay : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SpriteTextDisplay textDisplay;
        
        [Header("Settings")]
        [SerializeField] private float scrollSpeed = 0.25f; // Seconds per character shift
        [SerializeField] private int maxVisibleChars = 30; // Capacity of the UI display
        [SerializeField] private string scrollerSeparator = "  ***  "; // Two spaces, three stars, two spaces
        
        [Header("Localization")]
        [SerializeField] private UnityEngine.Localization.LocalizedString defaultTickerText;
        [SerializeField] private UnityEngine.Localization.LocalizedString notReadyText;

        private string defaultText 
        {
            get 
            {
                if (defaultTickerText != null && !defaultTickerText.IsEmpty)
                {
                    defaultTickerText.Arguments = new object[] { Application.version };
                    return defaultTickerText.GetLocalizedString();
                }
                return "PocketAmp " + Application.version;
            }
        }

        private string fullText;
        private string scrollBuffer;
        private int scrollOffset;
        private float scrollTimer;
        private bool isScrolling;

        private string overrideText;

        private void Start()
        {
            if (textDisplay != null)
                textDisplay.SetText(defaultText);
        }

        public void SetSongInfo(int index, string title, float durationSeconds)
        {
            // Format duration mm:ss without leading zeros on minutes
            int minutes = (int)(durationSeconds / 60);
            int seconds = (int)(durationSeconds % 60);
            string timeStr = $"{minutes}:{seconds:D2}";

            // Strip common extensions from title
            string cleanTitle = title;
            if (cleanTitle.EndsWith(".mp3", System.StringComparison.OrdinalIgnoreCase) || 
                cleanTitle.EndsWith(".wav", System.StringComparison.OrdinalIgnoreCase) || 
                cleanTitle.EndsWith(".ogg", System.StringComparison.OrdinalIgnoreCase))
            {
                cleanTitle = System.IO.Path.GetFileNameWithoutExtension(cleanTitle);
            }

            fullText = $"{index}. {cleanTitle} ({timeStr})";
            
            ResetScroll();

            if (fullText.Length > maxVisibleChars)
            {
                scrollBuffer = fullText + scrollerSeparator;
                isScrolling = true;
            }
            else
            {
                isScrolling = false;
                // Only update text immediately if not overridden
                if (string.IsNullOrEmpty(overrideText))
                {
                    textDisplay.SetText(fullText);
                }
            }
        }

        public void SetOverrideText(string text)
        {
            overrideText = text;
            if (textDisplay != null)
            {
                textDisplay.SetText(overrideText);
            }
        }

        public void ClearOverrideText()
        {
            overrideText = null;
            // Restore current state
            if (isScrolling) return;
            if (textDisplay)
                textDisplay.SetText(string.IsNullOrEmpty(fullText) ? defaultText : fullText);
            // If scrolling, the Update loop will pick it up immediately
        }

        /// <summary>
        /// Shows a temporary message for a specified duration, then reverts to normal display.
        /// </summary>
        /// <param name="message">The message to display (e.g., "Not Implemented Yet", "Coming Soon")</param>
        /// <param name="duration">Duration in seconds to show the message (default: 2 seconds)</param>
        public void ShowTemporaryMessage(string message, float duration = 2f)
        {
            StopAllCoroutines(); // Stop any existing temporary message
            StartCoroutine(TemporaryMessageCoroutine(message, duration));
        }

        public void ShowNotReadyYetMessage() 
        {
            if (notReadyText != null && !notReadyText.IsEmpty)
            {
                ShowTemporaryMessage(notReadyText.GetLocalizedString());
            }
            else
            {
                ShowTemporaryMessage("Not ready yet! :(");
            }
        }

        private IEnumerator TemporaryMessageCoroutine(string message, float duration)
        {
            SetOverrideText(message);
            yield return new WaitForSeconds(duration);
            ClearOverrideText();
        }

        public void Clear()
        {
            ResetScroll();
            isScrolling = false;
            fullText = null; // Clear implementation detail
            if (textDisplay != null)
                textDisplay.SetText(defaultText);
        }

        private void ResetScroll()
        {
            scrollOffset = 0;
            scrollTimer = 0;
        }

        private void Update()
        {
            if (textDisplay == null) return;

            // If overridden, just show that static text and do NOT scroll
            if (!string.IsNullOrEmpty(overrideText))
            {
                // Ensure text is set (redundant but safe)
                // textDisplay.SetText(overrideText); 
                return;
            }

            if (!isScrolling) return;

            scrollTimer += Time.deltaTime;
            if (scrollTimer >= scrollSpeed)
            {
                scrollTimer -= scrollSpeed;
                
                string visiblePortion = "";
                int bufferLength = scrollBuffer.Length;

                for (int i = 0; i < maxVisibleChars; i++)
                {
                    int idx = (scrollOffset + i) % bufferLength;
                    visiblePortion += scrollBuffer[idx];
                }

                textDisplay.SetText(visiblePortion);
                scrollOffset = (scrollOffset + 1) % bufferLength;
            }
        }

        public void ApplySkin(Skin skin)
        {
            if (textDisplay != null)
            {
                textDisplay.ApplySkin(skin);
                
                // Force immediate refresh of current content to use new font
                if (!string.IsNullOrEmpty(overrideText))
                {
                    textDisplay.SetText(overrideText);
                }
                else if (!isScrolling)
                {
                    textDisplay.SetText(string.IsNullOrEmpty(fullText) ? defaultText : fullText);
                }
                // If scrolling, the Update loop will refresh it naturally on next tick, 
                // but its textDisplay already knows to use new skin.
            }
        }
    }
}
