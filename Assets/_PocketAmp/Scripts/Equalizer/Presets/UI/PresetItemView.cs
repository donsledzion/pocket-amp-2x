using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace SoftAware.PocketAmp.Equalizer.Presets.UI
{
    public class PresetItemView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private TextMeshProUGUI presetNameText;

        [Header("Background")]
        [SerializeField] private Image background; 
        [SerializeField] private Color selectedBackgroundColor = new Color(0.2f, 0.4f, 0.8f, 1f);
        [SerializeField] private Color normalBackgroundColor = new Color(0, 0, 0, 0); // Transparent by default

        [Header("Text Colors")]
        [SerializeField] private Color selectedTextColor = Color.white;
        [SerializeField] private Color normalTextColor = Color.white;

        public EqPresetData Preset { get; private set; }
        private System.Action<EqPresetData> onClick;
        private System.Action<EqPresetData> onDoubleClick;

        public void Setup(EqPresetData preset, System.Action<EqPresetData> onClick, System.Action<EqPresetData> onDoubleClick)
        {
            this.Preset = preset;
            this.onClick = onClick;
            this.onDoubleClick = onDoubleClick;
            
            if (presetNameText) presetNameText.text = preset.name;
            SetSelected(false);
        }

        public void SetSelected(bool selected)
        {
            if (background)
            {
                background.enabled = true; 
                background.color = selected ? selectedBackgroundColor : normalBackgroundColor;
            }
            
            if (presetNameText)
            {
                presetNameText.color = selected ? selectedTextColor : normalTextColor;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.clickCount == 2)
            {
                onDoubleClick?.Invoke(Preset);
            }
            else
            {
                onClick?.Invoke(Preset);
            }
        }
    }
}
