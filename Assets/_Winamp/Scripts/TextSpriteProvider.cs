using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SoftAware
{
    /// <summary>
    /// Provides access to character sprites from the Text.png spritesheet.
    /// Sprites are named with convention: Text_{character} (e.g., Text_A, Text_5)
    /// </summary>
    public class TextSpriteProvider : MonoBehaviour
    {
        [SerializeField] private Sprite[] textSprites;
        
        private static TextSpriteProvider instance;
        private Dictionary<char, Sprite> spriteCache = new Dictionary<char, Sprite>();
        private bool isInitialized = false;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                Initialize();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Initialize()
        {
            if (isInitialized || textSprites == null || textSprites.Length == 0) return;

            foreach (var sprite in textSprites)
            {
                if (sprite == null) continue;
                
                if (sprite.name.StartsWith("Text_"))
                {
                    string suffix = sprite.name.Substring(5); // Don't ToUpper yet, keep original char if needed
                    
                    if (suffix.Length == 1)
                    {
                        char c = char.ToUpper(suffix[0]);
                        spriteCache[c] = sprite;
                    }
                    else
                    {
                        // Handle special names mapping to chars
                        string upperSuffix = suffix.ToUpper();
                        if (upperSuffix == "STAR" || upperSuffix == "SHURIKEN") spriteCache['*'] = sprite;
                        else if (upperSuffix == "DASH" || upperSuffix == "MINUS") spriteCache['-'] = sprite;
                        else if (upperSuffix == "DOT" || upperSuffix == "PERIOD") spriteCache['.'] = sprite;
                        else if (upperSuffix == "SPACE") spriteCache[' '] = sprite;
                        else if (upperSuffix == "LBRACKET") spriteCache['('] = sprite;
                        else if (upperSuffix == "RBRACKET") spriteCache[')'] = sprite;
                        else if (upperSuffix == "QUESTION") spriteCache['?'] = sprite;
                        else if (upperSuffix == "''") spriteCache['"'] = sprite;
                        else if (upperSuffix == "=") spriteCache['='] = sprite;
                        else if (upperSuffix == "COLON") spriteCache[':'] = sprite;
                    }
                }
            }

            isInitialized = true;
            Debug.Log($"[TextSpriteProvider] Loaded {spriteCache.Count} character sprites");
        }

        /// <summary>
        /// Gets the sprite for a specific character.
        /// </summary>
        public static Sprite GetSprite(char c)
        {
            if (instance == null || !instance.isInitialized)
            {
                Debug.LogWarning("[TextSpriteProvider] Not initialized!");
                return null;
            }

            // Convert to uppercase for consistency
            c = char.ToUpper(c);

            if (instance.spriteCache.TryGetValue(c, out Sprite sprite))
            {
                return sprite;
            }

            Debug.LogWarning($"[TextSpriteProvider] Sprite not found for character: {c}");
            return null;
        }

        /// <summary>
        /// Checks if a sprite exists for the given character.
        /// </summary>
        public static bool HasSprite(char c)
        {
            if (instance == null || !instance.isInitialized) return false;
            return instance.spriteCache.ContainsKey(char.ToUpper(c));
        }

#if UNITY_EDITOR
        [ContextMenu("Auto-Load Sprites from Texture")]
        private void AutoLoadSpritesFromTexture()
        {
            // Find the Text.png texture in the project
            string[] guids = UnityEditor.AssetDatabase.FindAssets("Text t:Texture2D", new[] { "Assets/_Winamp/Skins/Classic" });
            
            if (guids.Length == 0)
            {
                Debug.LogError("Text.png not found in Assets/_Winamp/Skins/Classic");
                return;
            }

            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
            Sprite[] sprites = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path)
                .OfType<Sprite>()
                .Where(s => s.name.StartsWith("Text_"))
                .OrderBy(s => s.name)
                .ToArray();

            textSprites = sprites;
            UnityEditor.EditorUtility.SetDirty(this);
            
            Debug.Log($"Loaded {sprites.Length} sprites from {path}");
        }
#endif
    }
}
