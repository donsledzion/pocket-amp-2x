using UnityEngine;
using UnityEngine.UI;

namespace SoftAware.Winamp
{
    public class MainTitleBar : MonoBehaviour, IWinampSkinApplicator
    {
        [Header("UI References")]
        [SerializeField] private Image background;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button minimizeButton;
        
        // Internal properties for Main to access
        internal Button CloseButton => closeButton;
        internal Button MinimizeButton => minimizeButton;

        public void ApplySkin(WinampSkin skin)
        {
            if (skin == null) return;

            // Apply Background
            if (background != null && skin.TitleBar != null)
            {
                background.sprite = skin.TitleBar;
                // NO SetNativeSize() as requested!
            }

            // Apply Close Button
            ApplyButtonStyle(closeButton, skin.CloseBtn_Normal, skin.CloseBtn_Pressed);

            // Apply Minimize Button
            ApplyButtonStyle(minimizeButton, skin.MinimizeBtn_Normal, skin.MinimizeBtn_Pressed);
        }

        private void ApplyButtonStyle(Button btn, Sprite normal, Sprite pressed)
        {
            if (btn == null) return;
            
            // Set Target Graphic (Image)
            if (btn.targetGraphic is Image img && normal != null)
            {
                img.sprite = normal;
                // NO SetNativeSize()
            }

            // Set SpriteState
            if (pressed != null)
            {
                SpriteState state = btn.spriteState;
                state.pressedSprite = pressed;
                btn.spriteState = state;
            }
        }
    }
}
