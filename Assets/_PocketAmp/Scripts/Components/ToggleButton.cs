using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace SoftAware
{
    public class ToggleButton : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Button button;
        [SerializeField] private Image targetImage;

        [Header("Sprites")]
        [SerializeField] private Sprite offNormal;
        [SerializeField] private Sprite offPressed;
        [SerializeField] private Sprite onNormal;
        [SerializeField] private Sprite onPressed;

        public UnityEvent<bool> OnValueChanged;

        private bool isOn = false;

        public bool IsOn => isOn;

        private void Start()
        {
            if (button != null)
            {
                button.onClick.AddListener(Toggle);
            }
            UpdateVisuals();
        }

        public void Toggle()
        {
            isOn = !isOn;
            UpdateVisuals();
            OnValueChanged?.Invoke(isOn);
        }

        public void SetState(bool state)
        {
            isOn = state;
            UpdateVisuals();
            OnValueChanged?.Invoke(isOn);
        }

        private void UpdateVisuals()
        {
            if (targetImage == null || button == null) return;

            // Update normal sprite
            targetImage.sprite = isOn ? onNormal : offNormal;

            // Update pressed sprite
            SpriteState spriteState = button.spriteState;
            spriteState.pressedSprite = isOn ? onPressed : offPressed;
            button.spriteState = spriteState;
        }
        public void SetSprites(Sprite offN, Sprite offP, Sprite onN, Sprite onP)
        {
            this.offNormal = offN;
            this.offPressed = offP;
            this.onNormal = onN;
            this.onPressed = onP;
            UpdateVisuals();
        }
    }
}
