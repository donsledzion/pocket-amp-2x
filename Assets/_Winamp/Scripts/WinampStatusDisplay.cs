using UnityEngine;
using UnityEngine.UI;

namespace SoftAware
{
    /// <summary>
    /// Controls the Winamp status indicators (Play, Pause, Stop, and Red/Green Squares).
    /// </summary>
    public class WinampStatusDisplay : MonoBehaviour
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
    }
}
