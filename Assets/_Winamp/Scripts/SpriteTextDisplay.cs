using UnityEngine;
using UnityEngine.UI;

namespace SoftAware
{
    /// <summary>
    /// Displays text using sprites from TextSpriteProvider.
    /// Each character position has its own Image component.
    /// </summary>
    public class SpriteTextDisplay : MonoBehaviour
    {
        [SerializeField] private Image[] characterImages;
        [SerializeField] private bool hideUnusedCharacters = true;

        /// <summary>
        /// Sets the displayed text. Characters without sprites will be skipped.
        /// </summary>
        public void SetText(string text)
        {
            if (characterImages == null || characterImages.Length == 0) return;

            text = text.ToUpper();
            int textIndex = 0;

            for (int i = 0; i < characterImages.Length; i++)
            {
                if (characterImages[i] == null) continue;

                if (textIndex < text.Length)
                {
                    char c = text[textIndex];
                    Sprite sprite = TextSpriteProvider.GetSprite(c);

                    if (sprite != null)
                    {
                        characterImages[i].sprite = sprite;
                        characterImages[i].enabled = true;
                        textIndex++;
                    }
                    else
                    {
                        // Character not found, skip or hide
                        if (hideUnusedCharacters)
                            characterImages[i].enabled = false;
                    }
                }
                else
                {
                    // No more characters to display
                    if (hideUnusedCharacters)
                        characterImages[i].enabled = false;
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
    }
}
