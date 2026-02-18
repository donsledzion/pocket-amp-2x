using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SoftAware
{
    /// <summary>
    /// Handles visual updates for a Winamp-style slider, 
    /// changing the background sprite based on the slider's value.
    /// Used for Volume, Preamp, and EQ Bands.
    /// </summary>
    public class WinampSliderVisuals : MonoBehaviour, IWinampSkinApplicator
    {
        public enum SliderType { Volume, Balance, Preamp, EQBand }

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
            if (slider == null) slider = GetComponent<Slider>();
            
            if (slider != null)
            {
                slider.onValueChanged.AddListener(UpdateVisuals);
                // Initialize visual state
                UpdateVisuals(slider.value);
            }
        }

        public void UpdateVisuals(float value)
        {
            if (targetImage == null || sprites == null || sprites.Count == 0) return;

            float range = slider != null ? (slider.maxValue - slider.minValue) : 0f;
            value = Mathf.Clamp(range > 0.0001f ? (value - slider.minValue) / range : value, 0f, 1f);
            
            // Calculate index: 0 to Count-1
            int maxIndex = sprites.Count - 1;
            int index = Mathf.RoundToInt(value * maxIndex);

            if (index >= 0 && index < sprites.Count)
            {
                targetImage.sprite = sprites[index];
            }
        }

        public void ApplySkin(WinampSkin skin)
        {
            if (skin == null) return;

            // 1. Update Slider Background Sprites
            switch (sliderType)
            {
                case SliderType.Volume:
                    if (skin.VolumeAnimation != null && skin.VolumeAnimation.Length > 0)
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
                    if (skin.BalanceAnimation != null && skin.BalanceAnimation.Length > 0)
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
                    if (skin.EqSliderBackgrounds != null && skin.EqSliderBackgrounds.Length > 0)
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

#if UNITY_EDITOR
        [ContextMenu("Load Sprites From Texture")]
        private void LoadSpritesFromTexture()
        {
            if (spriteSheet == null)
            {
                Debug.LogError("No SpriteSheet assigned!");
                return;
            }

            string path = AssetDatabase.GetAssetPath(spriteSheet);
            var assets = AssetDatabase.LoadAllAssetsAtPath(path);

            sprites.Clear();
            foreach (var asset in assets)
            {
                if (asset is Sprite s)
                {
                    if (string.IsNullOrEmpty(spriteNamePrefix) || s.name.StartsWith(spriteNamePrefix))
                    {
                        sprites.Add(s);
                    }
                }
            }

            // Sort by name to ensure correct order
            // Note: Winamp sprites often have numbers, we might need natural sort if they are not padded
            sprites = sprites.OrderBy(s => s.name, new NaturalStringComparer()).ToList();

            Debug.Log($"Loaded {sprites.Count} sprites from {spriteSheet.name} (Prefix: '{spriteNamePrefix}')");
            EditorUtility.SetDirty(this);
        }

        private class NaturalStringComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                if (x == y) return 0;
                if (x == null) return -1;
                if (y == null) return 1;

                string[] xParts = System.Text.RegularExpressions.Regex.Split(x.Replace(" ", ""), "([0-9]+)");
                string[] yParts = System.Text.RegularExpressions.Regex.Split(y.Replace(" ", ""), "([0-9]+)");

                for (int i = 0; i < Mathf.Min(xParts.Length, yParts.Length); i++)
                {
                    if (xParts[i] != yParts[i])
                    {
                        int xInt, yInt;
                        if (int.TryParse(xParts[i], out xInt) && int.TryParse(yParts[i], out yInt))
                            return xInt.CompareTo(yInt);
                        return xParts[i].CompareTo(yParts[i]);
                    }
                }

                return xParts.Length.CompareTo(yParts.Length);
            }
        }
#endif
    }
}
