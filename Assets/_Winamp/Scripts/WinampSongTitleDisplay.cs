using UnityEngine;
using System.Collections;

namespace SoftAware
{
    /// <summary>
    /// Manages the scrolling song title display in the Winamp main window.
    /// Format: {index}. {Artist} - {Title} ({mm:ss})
    /// </summary>
    public class WinampSongTitleDisplay : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SpriteTextDisplay textDisplay;
        
        [Header("Settings")]
        [SerializeField] private float scrollSpeed = 0.25f; // Seconds per character shift
        [SerializeField] private int maxVisibleChars = 30; // Capacity of the UI display
        [SerializeField] private string scrollerSeparator = "  ***  "; // Two spaces, three stars, two spaces
        private static string defaultText => "Winamp " + Application.version;

        private string fullText;
        private string scrollBuffer;
        private int scrollOffset;
        private float scrollTimer;
        private bool isScrolling;

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
                textDisplay.SetText(fullText);
            }
        }

        public void Clear()
        {
            ResetScroll();
            isScrolling = false;
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
            if (!isScrolling || textDisplay == null) return;

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
    }
}
