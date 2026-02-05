using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace SoftAware
{
    /// <summary>
    /// Handles the main Winamp time display (MM:SS).
    /// Supports elapsed and remaining time modes.
    /// </summary>
    public class WinampTimeDisplay : MonoBehaviour, IPointerClickHandler
    {
        [Header("Digit Images (MM:SS)")]
        [SerializeField] private Image minTen;
        [SerializeField] private Image minUnit;
        [SerializeField] private Image secTen;
        [SerializeField] private Image secUnit;

        [Header("Minus Sign")]
        [SerializeField] private Image minusSign;

        [Header("Sprites - NUMBERS.png")]
        [SerializeField] private Sprite[] digitSprites; // NUMBERS_0 to NUMBERS_9

        [Header("Sprites - Nums_ex.png")]
        [SerializeField] private Sprite minusSprite; // Nums_ex_10

        private bool isRemainingMode = false;
        private bool isPaused = false;
        private float lastCurrentTime = -1f;
        private float blinkTimer = 0f;
        private const float BLINK_INTERVAL = 2.0f;

        private void Start()
        {
            Clear();
        }

        public void SetTime(float currentTime, float totalTime)
        {
            float displayTime = isRemainingMode ? (totalTime - currentTime) : currentTime;
            
            // Avoid unnecessary updates
            if (Mathf.Abs(displayTime - lastCurrentTime) < 0.1f) return;
            lastCurrentTime = displayTime;

            int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(displayTime));
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;

            // Clamp minutes to 99 (standard Winamp behavior for skin)
            minutes = Mathf.Min(99, minutes);

            // Update digits
            UpdateDigit(minTen, minutes / 10, true); 
            UpdateDigit(minUnit, minutes % 10, true);
            UpdateDigit(secTen, seconds / 10, true);
            UpdateDigit(secUnit, seconds % 10, true);

            // Update minus sign
            if (minusSign != null)
            {
                minusSign.enabled = isRemainingMode;
                minusSign.sprite = minusSprite;
            }
        }

        private void UpdateDigit(Image img, int value, bool visible)
        {
            if (img == null) return;

            if (visible && digitSprites != null && value >= 0 && value < digitSprites.Length)
            {
                img.sprite = digitSprites[value];
                img.enabled = true;
            }
            else
            {
                img.enabled = false;
            }
        }

        public void SetPaused(bool paused)
        {
            if (isPaused == paused) return;
            isPaused = paused;
            
            if (!isPaused)
            {
                // Ensure everything is visible when unpausing
                SetDisplayVisibility(true);
            }
        }

        public void Clear()
        {
            SetPaused(false);
            lastCurrentTime = -1f;
            SetDisplayVisibility(false);
        }

        private void Update()
        {
            if (!isPaused) return;

            blinkTimer += Time.deltaTime;
            if (blinkTimer >= BLINK_INTERVAL)
            {
                blinkTimer = 0f;
                bool isVisible = !minUnit.enabled; // Check one digit to toggle
                SetDisplayVisibility(isVisible);
            }
        }

        private void SetDisplayVisibility(bool visible)
        {
            // We only blink digits and minus sign (as per Winamp)
            // Note: UpdateDigit will override this on next SetTime call if playing, 
            // but while paused SetTime is called with same values and returns early due to optimization.
            // So we need to be careful.
            
            float displayTime = isRemainingMode ? lastCurrentTime : lastCurrentTime; // just to have context
            
            if (!visible)
            {
                minTen.enabled = false;
                minUnit.enabled = false;
                secTen.enabled = false;
                secUnit.enabled = false;
                if (minusSign != null) minusSign.enabled = false;
            }
            else
            {
                // Restore based on current time values
                lastCurrentTime = -1f; // Force refresh on next frame
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            isRemainingMode = !isRemainingMode;
            // Force update will happen on next SetTime call
        }

#if UNITY_EDITOR
        [ContextMenu("Auto-Assign Sprites")]
        private void AutoAssignSprites()
        {
            // Helper to fill arrays in editor
            string numbersPath = "Assets/_Winamp/Skins/Classic/NUMBERS.png";
            string numsExPath = "Assets/_Winamp/Skins/Classic/Nums_ex.png";

            digitSprites = new Sprite[10];
            var allNumbers = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(numbersPath);
            foreach (var asset in allNumbers)
            {
                if (asset is Sprite s)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        if (s.name == $"NUMBERS_{i}") digitSprites[i] = s;
                    }
                }
            }

            var allEx = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(numsExPath);
            foreach (var asset in allEx)
            {
                if (asset is Sprite s && s.name == "Nums_ex_10")
                {
                    minusSprite = s;
                }
            }
            
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
