using UnityEngine;
using UnityEngine.UI;

namespace SoftAware.Winamp
{
    public class MainControls : MonoBehaviour, IWinampSkinApplicator
    {
        [Header("Transport Buttons")]
        [SerializeField] private Button prevButton;
        [SerializeField] private Button playButton;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button stopButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button ejectButton;

        [Header("Toggle Buttons")]
        [SerializeField] private ToggleButton shuffleButton;
        [SerializeField] private ToggleButton repeatButton;
        
        // Internal accessors for Main.cs
        internal Button PrevButton => prevButton;
        internal Button PlayButton => playButton;
        internal Button PauseButton => pauseButton;
        internal Button StopButton => stopButton;
        internal Button NextButton => nextButton;
        internal Button EjectButton => ejectButton;
        
        internal ToggleButton ShuffleButton => shuffleButton;
        internal ToggleButton RepeatButton => repeatButton;

        public void ApplySkin(WinampSkin skin)
        {
            if (skin == null) return;

            // Apply Transport Buttons
            ApplyButtonStyle(prevButton, skin.PrevBtn_Normal, skin.PrevBtn_Pressed);
            ApplyButtonStyle(playButton, skin.PlayBtn_Normal, skin.PlayBtn_Pressed);
            ApplyButtonStyle(pauseButton, skin.PauseBtn_Normal, skin.PauseBtn_Pressed);
            ApplyButtonStyle(stopButton, skin.StopBtn_Normal, skin.StopBtn_Pressed);
            ApplyButtonStyle(nextButton, skin.NextBtn_Normal, skin.NextBtn_Pressed);
            ApplyButtonStyle(ejectButton, skin.EjectBtn_Normal, skin.EjectBtn_Pressed);

            // Apply Toggle Buttons (Shuffle/Repeat)
            ApplyToggleStyle(shuffleButton, 
                skin.Shuffle_Off_Normal, skin.Shuffle_Off_Pressed,
                skin.Shuffle_On_Normal, skin.Shuffle_On_Pressed);

            ApplyToggleStyle(repeatButton,
                skin.Repeat_Off_Normal, skin.Repeat_Off_Pressed,
                skin.Repeat_On_Normal, skin.Repeat_On_Pressed);
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
        
        // Custom method for our custom ToggleButton component
        // Since ToggleButton component holds references to sprites internally, we need a way to inject them.
        // We might need to extend ToggleButton to accept "SkinData" or SetSprites.
        // For now, let's assume we can modify ToggleButton or use reflection/public fields if they are exposed.
        // Looking at ToggleButton.cs... fields are [SerializeField] private... 
        // We should add a public method to ToggleButton to set sprites at runtime!
        
        // Assuming we add SetSprites to ToggleButton.cs:
        private void ApplyToggleStyle(ToggleButton toggle, Sprite offNorm, Sprite offPress, Sprite onNorm, Sprite onPress)
        {
            if (toggle == null) return;
            // Calls a new method we will add to ToggleButton.cs
            toggle.SetSprites(offNorm, offPress, onNorm, onPress);
        }
    }
}
