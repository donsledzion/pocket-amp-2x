using UnityEngine;
using UnityEngine.UI;

namespace SoftAware
{
    /// <summary>
    /// Displays text using sprites from TextSpriteProvider.
    /// Each character position has its own Image component.
    /// </summary>
    public class SpriteTextDisplay : MonoBehaviour, ISkinApplicator
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
                var textIndex = text.Length - 1;
                
                for (var i = characterImages.Length - 1; i >= 0; i--)
                {
                    if (!characterImages[i]) continue;

                    var spriteSet = false;
                    if (textIndex >= 0)
                    {
                        var c = text[textIndex];
                        var sprite = TextSpriteProvider.GetSprite(c);

                        if (sprite)
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
                    if (!characterImages[i]) continue;

                    var spriteSet = false;
                    if (textIndex < text.Length)
                    {
                        var c = text[textIndex];
                        var sprite = TextSpriteProvider.GetSprite(c);

                        if (sprite)
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
            var text = value.ToString();
            if (minDigits > 0)
            {
                text = text.PadLeft(minDigits, ' ');
            }
            SetText(text);
        }

        public void Refresh()
        {
            SetText(currentText);
        }

        /// <summary>
        /// Clears all character displays.
        /// </summary>
        public void Clear()
        {
            if (characterImages == null) return;

            foreach (var img in characterImages)
            {
                if (img && hideUnusedCharacters)
                    img.enabled = false;
            }
        }

        public void ApplySkin(Skin skin)
        {
            // Refresh visuals with the current stored text using the new skin/font
            SetText(currentText);
        }
    }
}
