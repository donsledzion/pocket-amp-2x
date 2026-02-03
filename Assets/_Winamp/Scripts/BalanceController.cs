using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq; // For sorting
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SoftAware
{
    public class BalanceController : MonoBehaviour
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

        private void Start()
        {
            if (slider != null)
            {
                slider.onValueChanged.AddListener(OnValueChanged);
                // Initialize visual state
                OnValueChanged(slider.value);
            }
        }

        public void OnValueChanged(float value)
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
