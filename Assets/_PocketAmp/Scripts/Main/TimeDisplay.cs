using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace SoftAware
{
    /// <summary>
    /// Handles the main time display (MM:SS).
    /// Supports elapsed and remaining time modes.
    /// </summary>
    public class TimeDisplay : MonoBehaviour, IPointerClickHandler, ISkinApplicator
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

        public System.Action<bool> OnModeChanged;

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

            // Clamp minutes to 99 (standard behavior for skin)
            minutes = Mathf.Min(99, minutes);

            // Update digits
            UpdateDigit(minTen, minutes / 10, true); 
            UpdateDigit(minUnit, minutes % 10, true);
            UpdateDigit(secTen, seconds / 10, true);
            UpdateDigit(secUnit, seconds % 10, true);

            // Update minus sign
            if (!minusSign) return;
            minusSign.enabled = isRemainingMode;
            minusSign.sprite = minusSprite;
        }

        private void UpdateDigit(Image img, int value, bool visible)
        {
            if (!img) return;

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
            if (!(blinkTimer >= BLINK_INTERVAL)) return;
            blinkTimer = 0f;
            var isVisible = !minUnit.enabled; // Check one digit to toggle
            SetDisplayVisibility(isVisible);
        }

        private void SetDisplayVisibility(bool visible)
        {
            // We only blink digits and minus sign
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
                if (minusSign) minusSign.enabled = false;
            }
            else
            {
                // Restore based on current time values
                lastCurrentTime = -1f; // Force refresh on next frame
            }
        }

        public void SetMode(bool remaining)
        {
            isRemainingMode = remaining;
            lastCurrentTime = -1f; // Force refresh
            OnModeChanged?.Invoke(isRemainingMode);
        }

        public void ApplySkin(Skin skin)
        {
            if (skin == null) return;

            if (skin.TimeDigits != null && skin.TimeDigits.Length == 10)
            {
                digitSprites = skin.TimeDigits;
            }

            if (skin.TimeMinus != null)
            {
                minusSprite = skin.TimeMinus;
            }

            lastCurrentTime = -1f; // Force refresh
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            isRemainingMode = !isRemainingMode;
            lastCurrentTime = -1f; // Force refresh
            OnModeChanged?.Invoke(isRemainingMode);
        }

#if UNITY_EDITOR
        [ContextMenu("Auto-Assign Sprites")]
        private void AutoAssignSprites()
        {
            // Helper to fill arrays in editor
            string numbersPath = "Assets/_PocketAmp/Skins/Classic/NUMBERS.png";
            string numsExPath = "Assets/_PocketAmp/Skins/Classic/Nums_ex.png";

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
                if (asset is Sprite { name: "Nums_ex_10" } s)
                {
                    minusSprite = s;
                }
            }
            
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
