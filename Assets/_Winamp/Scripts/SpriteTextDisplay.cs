using UnityEngine;
using UnityEngine.UI;

namespace SoftAware
{
    /// <summary>
    /// Displays text using sprites from TextSpriteProvider.
    /// Each character position has its own Image component.
    /// </summary>
    public class SpriteTextDisplay : MonoBehaviour, IWinampSkinApplicator
    {
        [SerializeField] private Image[] characterImages;
        [SerializeField] private bool hideUnusedCharacters = true;
        
        private string currentText = "";

        /// <summary>
        /// Sets the displayed text. Characters without sprites will be skipped.
        /// When hideUnusedCharacters is true, text is right-aligned (fills from right).
        /// </summary>
        public void SetText(string text)
        {
            currentText = text;
            if (characterImages == null || characterImages.Length == 0) return;
            
            if (string.IsNullOrEmpty(text))
            {
                Clear();
                return;
            }

            text = text.ToUpper();
            // ... (rest of the method remains the same)

            if (hideUnusedCharacters)
            {
                // Right-aligned: fill from right to left
                int textIndex = text.Length - 1;
                
                for (int i = characterImages.Length - 1; i >= 0; i--)
                {
                    if (characterImages[i] == null) continue;

                    bool spriteSet = false;
                    if (textIndex >= 0)
                    {
                        char c = text[textIndex];
                        Sprite sprite = TextSpriteProvider.GetSprite(c);

                        if (sprite != null)
                        {
                            characterImages[i].sprite = sprite;
                            characterImages[i].enabled = true;
                            spriteSet = true;
                        }
                        textIndex--;
                    }
                    
                    if (!spriteSet)
                    {
                        characterImages[i].enabled = false;
                    }
                }
            }
            else
            {
                // Left-aligned: fill from left to right
                int textIndex = 0;

                for (int i = 0; i < characterImages.Length; i++)
                {
                    if (characterImages[i] == null) continue;

                    bool spriteSet = false;
                    if (textIndex < text.Length)
                    {
                        char c = text[textIndex];
                        Sprite sprite = TextSpriteProvider.GetSprite(c);

                        if (sprite != null)
                        {
                            characterImages[i].sprite = sprite;
                            characterImages[i].enabled = true;
                            spriteSet = true;
                        }
                        textIndex++;
                    }

                    if (!spriteSet)
                    {
                        characterImages[i].enabled = false;
                    }
                }
            }
        }

        /// <summary>
        /// Sets a numeric value with optional padding.
        /// </summary>
        public void SetNumber(int value, int minDigits = 0)
        {
            string text = value.ToString();
            if (minDigits > 0)
            {
                text = text.PadLeft(minDigits, ' ');
            }
            SetText(text);
        }

        /// <summary>
        /// Clears all character displays.
        /// </summary>
        public void Clear()
        {
            if (characterImages == null) return;

            foreach (var img in characterImages)
            {
                if (img != null && hideUnusedCharacters)
                    img.enabled = false;
            }
        }

        public void ApplySkin(WinampSkin skin)
        {
            // Refresh visuals with the current stored text using the new skin/font
            SetText(currentText);
        }
    }
}
