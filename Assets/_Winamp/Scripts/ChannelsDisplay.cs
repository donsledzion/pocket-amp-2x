using UnityEngine;
using UnityEngine.UI;

namespace SoftAware
{
    public class ChannelsDisplay : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image monoImage;
        [SerializeField] private Image stereoImage;

        [Header("Sprites")]
        [SerializeField] private Sprite monoOn;
        [SerializeField] private Sprite monoOff;
        [SerializeField] private Sprite stereoOn;
        [SerializeField] private Sprite stereoOff;

        public void UpdateDisplay(bool isPlaying, int channels)
        {
            if (monoImage == null || stereoImage == null) return;

            if (!isPlaying)
            {
                // Stopped: All OFF
                monoImage.sprite = monoOff;
                stereoImage.sprite = stereoOff;
                return;
            }

            // Playing: Check channels
            // 1 = Mono, 2 (or more) = Stereo
            bool isMono = (channels == 1);

            monoImage.sprite = isMono ? monoOn : monoOff;
            stereoImage.sprite = !isMono ? stereoOn : stereoOff;
        }
    }
}
