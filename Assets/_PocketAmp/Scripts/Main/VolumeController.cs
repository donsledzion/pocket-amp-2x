using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq; // For sorting
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SoftAware.PocketAmp
{
    public class VolumeController : MonoBehaviour, ISkinApplicator
    {
        [Header("References")]
        [SerializeField] private Slider slider;
        [SerializeField] private Image targetImage;

        [Header("Data")]
        [SerializeField] private List<Sprite> sprites = new List<Sprite>();

        [Header("Editor Tools")]
        [SerializeField] private Texture2D spriteSheet;
        [SerializeField] private string spriteNamePrefix = "VOLUME_BG"; // Default based on Skins

        public Slider Slider => slider;
        private static Main main => Refs.Main;

        private bool isInteracting;

        public void ApplySkin(Skin skin)
        {
            if (skin == null) return;

            // Apply Animation Sprites
            if (skin.VolumeAnimation != null && skin.VolumeAnimation.Length > 0)
            {
                sprites = new List<Sprite>(skin.VolumeAnimation);
                // Refresh display
                if (slider != null) OnVolumeChanged(slider.value);
            }

            // Apply Knob
            if (slider != null && slider.targetGraphic is Image handleImage)
            {
                 if (skin.VolumeKnobNormal != null)
                 {
                     handleImage.sprite = skin.VolumeKnobNormal;
                     // SetNativeSize REMOVED as per strict user instruction
                 }
            }
            
            // Apply Pressed State to Slider
            if (slider == null) return;
            var ss = slider.spriteState;
            if (skin.VolumeKnobPressed != null)
            {
                ss.pressedSprite = skin.VolumeKnobPressed;
            }
            slider.spriteState = ss;
        }

        private void Start()
        {
            if (slider == null) return;
            slider.onValueChanged.AddListener(OnVolumeChanged);
            // Initialize visual state
            OnVolumeChanged(slider.value);

            // Attach interaction helper
            var interaction = slider.gameObject.AddComponent<SliderInteractionHelper>();
            interaction.OnPointerDownAction += OnPointerDown;
            interaction.OnPointerUpAction += OnPointerUp;
        }

        private void OnPointerDown()
        {
            isInteracting = true;
            UpdateTitleDisplay(slider.value);
        }

        private void OnPointerUp()
        {
            isInteracting = false;
            if (main != null && main.SongTitleDisplay != null)
            {
                main.SongTitleDisplay.ClearOverrideText();
            }
        }

        private void OnVolumeChanged(float value)
        {
            if (targetImage == null || sprites == null || sprites.Count == 0) return;

            value = Mathf.Clamp01(value);
            // Calculate index: 0 to Count-1
            int maxIndex = sprites.Count - 1;
            int index = Mathf.RoundToInt(value * maxIndex);

            if (index >= 0 && index < sprites.Count)
            {
                targetImage.sprite = sprites[index];
            }

            if (isInteracting)
                UpdateTitleDisplay(value);
        }

        private void UpdateTitleDisplay(float value)
        {
            var percent = Mathf.RoundToInt(value * 100f);
            main.SongTitleDisplay.SetOverrideText($"VOLUME: {percent}%");
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

            var path = AssetDatabase.GetAssetPath(spriteSheet);
            var assets = AssetDatabase.LoadAllAssetsAtPath(path);

            sprites.Clear();
            foreach (var asset in assets)
            {
                if (asset is not Sprite s) continue;
                if (string.IsNullOrEmpty(spriteNamePrefix) || s.name.StartsWith(spriteNamePrefix))
                {
                    sprites.Add(s);
                }
            }

            // Sort by name to ensure correct order (e.g. frame_00, frame_01...)
            sprites = sprites.OrderBy(s => s.name).ToList();

            Debug.Log($"Loaded {sprites.Count} sprites from {spriteSheet.name} (Prefix: '{spriteNamePrefix}')");
            EditorUtility.SetDirty(this);
        }
#endif
    }
}
