using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;


namespace SoftAware.PocketAmp.SystemMenus.Skins.UI
{
    public class SkinItemView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private TextMeshProUGUI skinNameText;

        [Header("Background")]
        [SerializeField] private Image background; 
        [SerializeField] private Color selectedBackgroundColor = new Color(0.2f, 0.4f, 0.8f, 1f);
        [SerializeField] private Color normalBackgroundColor = new Color(0, 0, 0, 0); // Transparent by default

        [Header("Text Colors")]
        [SerializeField] private Color selectedTextColor = Color.white;
        [SerializeField] private Color normalTextColor = Color.white;

        public string SkinName { get; private set; } // This acts as ID (full filename)
        public string DisplayName => skinNameText != null ? skinNameText.text : "";
        private System.Action<string> onClick;
        private System.Action<string> onDoubleClick;

        public void Setup(string skinId, string displayName, System.Action<string> onClick, System.Action<string> onDoubleClick)
        {
            this.SkinName = skinId;
            this.onClick = onClick;
            this.onDoubleClick = onDoubleClick;
            
            if (skinNameText) skinNameText.text = displayName;
            SetSelected(false);
        }

        public void SetSelected(bool selected)
        {
            // Debug.Log($"[SkinItemView] SetSelected({selected}) on {SkinName}.");

            if (background)
            {
                // Ensure enabled if we want color visible
                background.enabled = true; 
                background.color = selected ? selectedBackgroundColor : normalBackgroundColor;
            }
            
            if (skinNameText)
            {
                skinNameText.color = selected ? selectedTextColor : normalTextColor;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.clickCount == 2)
            {
                onDoubleClick?.Invoke(SkinName);
            }
            else
            {
                onClick?.Invoke(SkinName);
            }
        }
    }
}
