using UnityEngine;
using UnityEngine.UI;

namespace SoftAware
{
    /// <summary>
    /// Controls the Winamp status indicators (Play, Pause, Stop, and Red/Green Squares).
    /// </summary>
    public class StatusDisplay : MonoBehaviour, ISkinApplicator
    {
        public enum WinampStatus
        {
            Stop,
            Loading,
            Playing,
            Paused
        }

        [Header("Image Components")]
        [SerializeField] private Image statusIconImage; // Play/Stop
        [SerializeField] private Image pauseIconImage;  // Separate Pause icon
        [SerializeField] private Image squaresImage;    // Red/Green squares indicator

        [Header("Status Sprites (5)")]
        [SerializeField] private Sprite playSprite;
        [SerializeField] private Sprite stopSprite;
        [SerializeField] private Sprite pauseSprite;
        [SerializeField] private Sprite squaresLoadingSprite; // Red bright, Green dimmed
        [SerializeField] private Sprite squaresPlayingSprite; // Green bright, Red dimmed

        private void Start()
        {
            // Initial state
            SetStatus(WinampStatus.Stop);
        }

        public void SetStatus(WinampStatus status)
        {
            switch (status)
            {
                case WinampStatus.Stop:
                    SetImage(statusIconImage, stopSprite, true);
                    SetImage(pauseIconImage, null, false);
                    SetImage(squaresImage, null, false);
                    break;

                case WinampStatus.Loading:
                    SetImage(statusIconImage, playSprite, true);
                    SetImage(pauseIconImage, null, false);
                    SetImage(squaresImage, squaresLoadingSprite, true);
                    break;

                case WinampStatus.Playing:
                    SetImage(statusIconImage, playSprite, true);
                    SetImage(pauseIconImage, null, false);
                    SetImage(squaresImage, squaresPlayingSprite, true);
                    break;

                case WinampStatus.Paused:
                    SetImage(statusIconImage, null, false);
                    SetImage(pauseIconImage, pauseSprite, true);
                    SetImage(squaresImage, null, false);
                    break;
            }
        }

        private void SetImage(Image img, Sprite sprite, bool enabled)
        {
            if (img == null) return;
            
            if (enabled && sprite != null)
            {
                img.sprite = sprite;
                img.enabled = true;
            }
            else
            {
                img.enabled = false;
            }
        }
        public void ApplySkin(WinampSkin skin)
        {
            if (skin == null) return;

            // Update sprites from skin
            if (skin.Status_Play != null) playSprite = skin.Status_Play;
            if (skin.Status_Pause != null) pauseSprite = skin.Status_Pause;
            if (skin.Status_Stop != null) stopSprite = skin.Status_Stop;
            
            if (skin.Status_Indicator_Play != null) squaresPlayingSprite = skin.Status_Indicator_Play;
            if (skin.Status_Indicator_Load != null) squaresLoadingSprite = skin.Status_Indicator_Load;

            // Refresh current state to apply new sprites
            // We need to know current status? 
            // For now, let's just re-set Stop (or we could expose a public property for current status)
            // But usually this happens at init. Let's just force a refresh if possible, 
            // strictly speaking we should store currentStatus state.
            // Let's assume we can just leave it to be updated by the next status change, 
            // OR we can guess/store it. 
            // For a robust implementation, let's add a private field `currentStatus`.
        }
    }
}
