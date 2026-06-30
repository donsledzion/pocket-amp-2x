using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq; // For sorting
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SoftAware.PocketAmp
{
    public class BalanceController : MonoBehaviour, ISkinApplicator
    {
        [Header("References")]
        [SerializeField] private Slider slider;
        [SerializeField] private Image targetImage;

        [Header("Data")]
        [SerializeField] private List<Sprite> sprites = new List<Sprite>();

        [Header("Editor Tools")]
        [SerializeField] private Texture2D spriteSheet;
        [SerializeField] private string spriteNamePrefix = "BALANCE_BG"; // Default based on Skins

        public Slider Slider => slider;

        private static Main main => Refs.Main;

        private bool isInteracting;

        public void ApplySkin(Skin skin)
        {
            if (skin == null) return;

            // Apply Animation Sprites
            if (skin.BalanceAnimation != null && skin.BalanceAnimation.Length > 0)
            {
                sprites = new List<Sprite>(skin.BalanceAnimation);
                // Refresh display
                if (slider != null) OnValueChanged(slider.value);
            }

            // Apply Knob
            if (slider != null && slider.targetGraphic is Image handleImage)
            {
                 if (skin.BalanceKnobNormal != null)
                 {
                     handleImage.sprite = skin.BalanceKnobNormal;
                     // handleImage.SetNativeSize(); REMOVED
                 }
            }
            
            // Apply Pressed State
            if (slider != null)
            {
                SpriteState ss = slider.spriteState;
                if (skin.BalanceKnobPressed != null)
                {
                    ss.pressedSprite = skin.BalanceKnobPressed;
                }
                slider.spriteState = ss;
            }
        }
        
        private void Start()
        {
            if (slider != null)
            {
                slider.onValueChanged.AddListener(OnValueChanged);
                // Initialize visual state
                OnValueChanged(slider.value);

                // Attach interaction helper
                var interaction = slider.gameObject.AddComponent<SliderInteractionHelper>();
                interaction.OnPointerDownAction += OnPointerDown;
                interaction.OnPointerUpAction += OnPointerUp;
            }
        }

        private void OnPointerDown()
        {
            isInteracting = true;
            UpdateTitleDisplay(slider.value);
        }

        private void OnPointerUp()
        {
            isInteracting = false;
            
            // Snap to center logic
            // Center is 0.5. Dead zone is typically around 48% - 52% (approx +/- 0.02 to 0.04)
            // Let's use a slightly wider snap on release: +/- 0.05 (45% to 55%)
            if (slider != null)
            {
                var val = slider.value;
                if (Mathf.Abs(val - 0.5f) < 0.05f)
                {
                    slider.value = 0.5f;
                    // Value change listener will update text if we were still interacting, but we are not.
                    // However, we might want to briefly show "CENTER" before clearing? 
                    // Standard behavior is usually just snap and clear.
                }
            }

            if (main != null && main.SongTitleDisplay != null)
            {
                main.SongTitleDisplay.ClearOverrideText();
            }
        }

        private void OnValueChanged(float value)
        {
            if (targetImage == null || sprites == null || sprites.Count == 0) return;

            value = Mathf.Clamp01(value);
            
            // Balance logic implementation:
            // Slider 0.5 (Center) is the "starting point" -> Sprite Index 0 (lowest intensity/green)
            // Slider 0.0 (Left) or 1.0 (Right) are max "deviation" -> Sprite Index Max (red)
            
            float deviation = Mathf.Abs(value - 0.5f); // 0.0 at center, 0.5 at edges
            float intensity = deviation * 2f; // 0.0 at center, 1.0 at edges
            
            // Calculate index: 0 to Count-1
            int maxIndex = sprites.Count - 1;
            int index = Mathf.RoundToInt(intensity * maxIndex);

            if (index >= 0 && index < sprites.Count)
            {
                targetImage.sprite = sprites[index];
            }

            if (isInteracting)
            {
                UpdateTitleDisplay(value);
            }
        }

        [Header("Localization")]
        [SerializeField] private UnityEngine.Localization.LocalizedString balanceCenterText;
        [SerializeField] private UnityEngine.Localization.LocalizedString balanceLeftText;
        [SerializeField] private UnityEngine.Localization.LocalizedString balanceRightText;

        private void UpdateTitleDisplay(float value)
        {
            if (main == null || main.SongTitleDisplay == null) return;
            
            var text = "";
            var dist = value - 0.5f; // -0.5 to 0.5
            
            if (Mathf.Abs(dist) < 0.04f) // +/- 4%
            {
                if (balanceCenterText != null && !balanceCenterText.IsEmpty)
                {
                    text = balanceCenterText.GetLocalizedString();
                }
                else
                {
                    text = "BALANCE: CENTER";
                }
            }
            else if (dist < 0)
            {
                float percent = (Mathf.Abs(dist) / 0.5f) * 100f;
                if (balanceLeftText != null && !balanceLeftText.IsEmpty)
                {
                    balanceLeftText.Arguments = new object[] { Mathf.RoundToInt(percent) };
                    text = balanceLeftText.GetLocalizedString();
                }
                else
                {
                    text = $"BALANCE: {Mathf.RoundToInt(percent)}% LEFT";
                }
            }
            else
            {
                float percent = (dist / 0.5f) * 100f;
                if (balanceRightText != null && !balanceRightText.IsEmpty)
                {
                    balanceRightText.Arguments = new object[] { Mathf.RoundToInt(percent) };
                    text = balanceRightText.GetLocalizedString();
                }
                else
                {
                    text = $"BALANCE: {Mathf.RoundToInt(percent)}% RIGHT";
                }
            }

            main.SongTitleDisplay.SetOverrideText(text);
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
            sprites = sprites.OrderBy(s => s.name).ToList();

            Debug.Log($"Loaded {sprites.Count} sprites from {spriteSheet.name} (Prefix: '{spriteNamePrefix}')");
            EditorUtility.SetDirty(this);
        }
#endif
    }
}
