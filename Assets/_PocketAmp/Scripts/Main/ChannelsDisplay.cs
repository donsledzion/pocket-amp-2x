using UnityEngine;
using UnityEngine.UI;

namespace SoftAware
{
    public class ChannelsDisplay : MonoBehaviour, ISkinApplicator
    {
        [Header("References")]
        [SerializeField] private Image monoImage;
        [SerializeField] private Image stereoImage;

        [Header("Sprites")]
        [SerializeField] private Sprite monoOn;
        [SerializeField] private Sprite monoOff;
        [SerializeField] private Sprite stereoOn;
        [SerializeField] private Sprite stereoOff;

        // Current state tracking
        private bool isPlaying = false;
        private int currentChannels = 2; // Default stereo

        public void ApplySkin(Skin skin)
        {
            if (skin == null) return;
            
            // Assign skin sprites if available (e.g. from MONOSTER.BMP)
            // If skin sprites are null (classic skin), keep current ones (default or previous skin) 
            // OR should we clear them? 
            // Let's assume if skin has them, we use them.
            if (skin.Mono_Active != null) monoOn = skin.Mono_Active;
            if (skin.Mono_Inactive != null) monoOff = skin.Mono_Inactive;
            if (skin.Stereo_Active != null) stereoOn = skin.Stereo_Active;
            if (skin.Stereo_Inactive != null) stereoOff = skin.Stereo_Inactive;

            // Force update display with new sprites
            UpdateDisplay(isPlaying, currentChannels);
        }

        public void UpdateDisplay(bool isPlaying, int channels)
        {
            this.isPlaying = isPlaying;
            this.currentChannels = channels;

            if (!isPlaying)
            {
                // Stopped: All OFF
                // Use override sprites if set, otherwise fallback to serialized
                monoImage.sprite = monoOff;
                stereoImage.sprite = stereoOff;
                
                // Ensure visibility (in case they were disabled by MainIndicators previously)
                monoImage.enabled = monoImage.sprite;
                stereoImage.enabled = stereoImage.sprite;
                return;
            }

            // Playing: Check channels
            // 1 = Mono, 2 (or more) = Stereo
            var isMono = (channels == 1);

            monoImage.sprite = isMono ? monoOn : monoOff;
            stereoImage.sprite = !isMono ? stereoOn : stereoOff;
            
            monoImage.enabled = monoImage.sprite;
            stereoImage.enabled = stereoImage.sprite;
        }
    }
}
