using UnityEngine;

namespace SoftAware
{
    /// <summary>
    /// Data container for a loaded Winamp skin
    /// Holds references to all sliced sprites ready for application
    /// </summary>
    [System.Serializable]
    public class WinampSkin
    {
        public string SkinName;
        
        [Header("Main Window")]
        public Sprite MainBackground;
        public Sprite TitleBar;
        public Sprite ClutterBar; // The bar with O I A V scroll buttons etc.
        
        [Header("Title Bar Buttons")]
        public Sprite MinimizeBtn_Normal;
        public Sprite MinimizeBtn_Pressed;
        public Sprite CloseBtn_Normal;
        public Sprite CloseBtn_Pressed;
        public Sprite ShadeBtn_Normal; // Optional but good to have prepared
        public Sprite ShadeBtn_Pressed;

        [Header("Transport Buttons")]
        public Sprite PlayBtn_Normal;
        public Sprite PlayBtn_Pressed;
        public Sprite PauseBtn_Normal;
        public Sprite PauseBtn_Pressed;
        public Sprite StopBtn_Normal;
        public Sprite StopBtn_Pressed;
        public Sprite PrevBtn_Normal;
        public Sprite PrevBtn_Pressed;
        public Sprite NextBtn_Normal;
        public Sprite NextBtn_Pressed;
        public Sprite EjectBtn_Normal;
        public Sprite EjectBtn_Pressed;

        [Header("Toggles (Shuffle/Repeat)")]
        public Sprite Shuffle_Off_Normal;
        public Sprite Shuffle_Off_Pressed;
        public Sprite Shuffle_On_Normal;
        public Sprite Shuffle_On_Pressed;
        
        public Sprite Repeat_Off_Normal;
        public Sprite Repeat_Off_Pressed;
        public Sprite Repeat_On_Normal;
        public Sprite Repeat_On_Pressed;

        [Header("Toggles (EQ/Playlist)")]
        public Sprite EQ_Off_Normal;
        public Sprite EQ_Off_Pressed;
        public Sprite EQ_On_Normal;
        public Sprite EQ_On_Pressed;
        
        public Sprite PL_Off_Normal;
        public Sprite PL_Off_Pressed;
        public Sprite PL_On_Normal;
        public Sprite PL_On_Pressed;
        
        [Header("Indicators (Mono/Stereo)")]
        public Sprite Stereo_Active;
        public Sprite Stereo_Inactive;
        public Sprite Mono_Active;
        public Sprite Mono_Inactive;
        
        [Header("Sliders (Volume/Balance)")]
        public Sprite VolumeKnobNormal;
        public Sprite VolumeKnobPressed;
        public Sprite[] VolumeAnimation; // 28 frames
        
        public Sprite BalanceKnobNormal;
        public Sprite BalanceKnobPressed;
        public Sprite[] BalanceAnimation; // 28 frames
        
        [Header("Position Bar")]
        public Sprite PosKnobNormal;
        public Sprite PosKnobPressed;
        
        // Add more fields as we implement more components
    }

    /// <summary>
    /// Interface for any UI component that needs to receive skin data
    /// </summary>
    public interface IWinampSkinApplicator
    {
        /// <summary>
        /// Applies the provided skin to this component
        /// </summary>
        void ApplySkin(WinampSkin skin);
    }
}
