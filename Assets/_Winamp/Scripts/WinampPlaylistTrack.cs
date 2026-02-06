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
        [SerializeField] private TextMeshProUGUI trackText;
        [SerializeField] private Image background;

        [Header("Settings")]
        [SerializeField] private int maxChars = 48; // Typical Winamp PLEDIT width approx
        [SerializeField] private float mspaceValue = 11f; // Adjust this to match your font size for perfect spacing
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
            // Note: normalBg can be applied to the overall container if needed, 
            // but usually individual tracks just toggle the selectedBg image.
        }

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
            if (trackText == null) return;
            
            string durationStr = duration > 0 ? AudioMetadataUtils.FormatTime(duration) : "?:??";
            
            // 1. Clean title (strip extensions)
            string cleanTitle = originalTitle;
            if (cleanTitle.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase) || 
                cleanTitle.EndsWith(".wav", StringComparison.OrdinalIgnoreCase) || 
                cleanTitle.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase))
            {
                cleanTitle = System.IO.Path.GetFileNameWithoutExtension(cleanTitle);
            }

            // 2. Format parts
            string prefix = $"{index + 1}. ";
            string suffix = durationStr; // We want space before duration in the alignment logic

            // 3. Alignment Logic (Fixed Width)
            // Layout: [Prefix][Title].......[Suffix]
            // We need: Length([Prefix]) + Length([Title]) + 1 (space) + Length([Suffix]) <= maxChars
            
            int reservedSpace = prefix.Length + suffix.Length + 1; // 1 for a mandatory space before duration
            int titleBudget = maxChars - reservedSpace;

            string displayTitle = cleanTitle;
            if (displayTitle.Length > titleBudget)
            {
                displayTitle = displayTitle.Substring(0, Mathf.Max(0, titleBudget - 3)) + "...";
            }

            // Calculate total line
            int currentLen = prefix.Length + displayTitle.Length + suffix.Length;
            string padding = "";
            if (currentLen < maxChars)
            {
                padding = new string('.', maxChars - currentLen); // Winamp often uses dots or spaces
                // Actually, let's use spaces for that clean Winamp look, or dots if the user prefers.
                // Classic Winamp uses a space-filled gap but with fixed positions.
                padding = new string(' ', maxChars - currentLen);
            }

            // Wrap in <mspace> to force monospacing even with proportional fonts
            string combined = $"{prefix}{displayTitle}{padding} {suffix}";
            
            // TextMeshPro specific monospacing
            trackText.text = $"<mspace={mspaceValue}>{combined.ToUpper()}</mspace>";
        }

        public void SetPlaying(bool isPlaying)
        {
            trackText.color = isPlaying ? playingTextColor : normalTextColor;
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
