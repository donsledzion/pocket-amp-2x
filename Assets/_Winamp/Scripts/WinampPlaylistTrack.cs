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

        [Header("Colors (Initial Defaults)")]
        [SerializeField] private Color normalTextColor = Color.white;
        [SerializeField] private Color playingTextColor = new Color(0.9f, 0.9f, 0.7f); // Example Winamp yellow-ish
        [SerializeField] private Color selectedBgColor = new Color(0f, 0f, 0.5f); // Example Winamp dark blue

        private int index;
        private Action<int> onClick;
        private Action<int> onDoubleClick;
        private float lastClickTime;
        private const float DOUBLE_CLICK_TIME = 0.3f;

        public void Setup(int trackIndex, string title, Action<int> clickCallback, Action<int> doubleClickCallback)
        {
            index = trackIndex;
            trackText.text = $"{index + 1}. {title}";
            onClick = clickCallback;
            onDoubleClick = doubleClickCallback;
            
            SetSelected(false);
            SetPlaying(false);
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
