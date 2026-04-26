using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SoftAware
{
    /// <summary>
    /// Handles visual updates for a Old-style slider, 
    /// changing the background sprite based on the slider's value.
    /// Used for Volume, Preamp, and EQ Bands.
    /// </summary>
    public class SliderVisuals : MonoBehaviour, ISkinApplicator
    {
        private enum SliderType { Volume, Balance, Preamp, EQBand }

        [Header("References")]
        [SerializeField] private Slider slider;
        [SerializeField] private Image targetImage;
        [SerializeField] private SliderType sliderType;

        [Header("Data")]
        [SerializeField] private List<Sprite> sprites = new List<Sprite>();

        [Header("Editor Tools")]
        [SerializeField] private Texture2D spriteSheet;
        [SerializeField] private string spriteNamePrefix = ""; // e.g. "VOLUME_BG" or "EQ_SLIDER_BG"

        private void Start()
        {
            if (slider == null && !TryGetComponent(out slider))
                throw new("Missing SliderComponent!");

            slider.onValueChanged.AddListener(UpdateVisuals);
            // Initialize visual state
            UpdateVisuals(slider.value);
        }

        private void UpdateVisuals(float value)
        {
            if (targetImage == null || sprites == null || sprites.Count == 0) return;

            var range = slider != null ? (slider.maxValue - slider.minValue) : 0f;
            value = Mathf.Clamp(range > 0.0001f ? (value - slider.minValue) / range : value, 0f, 1f);
            
            // Calculate index: 0 to Count-1
            var maxIndex = sprites.Count - 1;
            var index = Mathf.RoundToInt(value * maxIndex);

            if (index >= 0 && index < sprites.Count)
                targetImage.sprite = sprites[index];
        }

        public void ApplySkin(Skin skin)
        {
            if (skin == null) return;

            // 1. Update Slider Background Sprites
            switch (sliderType)
            {
                case SliderType.Volume:
                    if (skin.VolumeAnimation is { Length: > 0 })
                        sprites = new List<Sprite>(skin.VolumeAnimation);
                    
                    if (slider != null && slider.targetGraphic is Image volKnob)
                    {
                        if (skin.VolumeKnobNormal != null) volKnob.sprite = skin.VolumeKnobNormal;
                        SpriteState ss = slider.spriteState;
                        if (skin.VolumeKnobPressed != null) ss.pressedSprite = skin.VolumeKnobPressed;
                        slider.spriteState = ss;
                    }
                    break;

                case SliderType.Balance:
                    if (skin.BalanceAnimation is { Length: > 0 })
                        sprites = new List<Sprite>(skin.BalanceAnimation);
                    
                    if (slider != null && slider.targetGraphic is Image balKnob)
                    {
                        if (skin.BalanceKnobNormal != null) balKnob.sprite = skin.BalanceKnobNormal;
                        SpriteState ss = slider.spriteState;
                        if (skin.BalanceKnobPressed != null) ss.pressedSprite = skin.BalanceKnobPressed;
                        slider.spriteState = ss;
                    }
                    break;

                case SliderType.Preamp:
                case SliderType.EQBand:
                    if (skin.EqSliderBackgrounds is { Length: > 0 })
                        sprites = new List<Sprite>(skin.EqSliderBackgrounds);
                    
                    if (slider != null && slider.targetGraphic is Image eqKnob)
                    {
                        if (skin.EqKnobNormal != null) eqKnob.sprite = skin.EqKnobNormal;
                        SpriteState ss = slider.spriteState;
                        if (skin.EqKnobPressed != null) ss.pressedSprite = skin.EqKnobPressed;
                        slider.spriteState = ss;
                    }
                    break;
            }

            // 2. Refresh visuals immediately
            if (slider != null) UpdateVisuals(slider.value);
        }
    }
}
