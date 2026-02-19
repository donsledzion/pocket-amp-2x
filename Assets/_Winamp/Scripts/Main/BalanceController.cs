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
        [SerializeField] private string spriteNamePrefix = "BALANCE_BG"; // Default based on Winamp skins

        public Slider Slider => slider;

        private static Main main => Refs.Main;

        private bool isInteracting;

        public void ApplySkin(WinampSkin skin)
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

        private void UpdateTitleDisplay(float value)
        {
            if (main == null || main.SongTitleDisplay == null) return;
            // Logic:
            // 0% - 48% (approx < 0.48): LEFT
            // 48% - 52% (approx > 0.48 && < 0.52): CENTER
            // 52% - 100% (approx > 0.52): RIGHT
                
            // Winamp logic is slightly different:
            // It shows "BALANCE: CENTER" for a range.
            // It shows "BALANCE: XX% LEFT" or "RIGHT".
                
            var text = "";
            var dist = value - 0.5f; // -0.5 to 0.5

            // Winamp usually treats center as exactly center internally, but displays it with a buffer.
            // Let's use a small epsilon for display "center"
            if (Mathf.Abs(dist) < 0.04f) // +/- 4%
            {
                text = "BALANCE: CENTER";
            }
            else if (dist < 0)
            {
                // Left
                // Map -0.5...-0.04 to 100%...0%
                // actually Winamp displays simplified percentage. 
                // If slider is 0 (Left), it is 100% Left.
                // If slider is 0.25, it is 50% Left.
                float percent = (Mathf.Abs(dist) / 0.5f) * 100f;
                text = $"BALANCE: {Mathf.RoundToInt(percent)}% LEFT";
            }
            else
            {
                // Right
                float percent = (dist / 0.5f) * 100f;
                text = $"BALANCE: {Mathf.RoundToInt(percent)}% RIGHT";
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
