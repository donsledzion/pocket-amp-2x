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
        
        // Status Icons (Play/Pause/Stop/Indicators from PLAYPAUS.BMP)
        public Sprite Status_Play;
        public Sprite Status_Pause;
        public Sprite Status_Stop;
        public Sprite Status_Indicator_Play; // Green/Active
        public Sprite Status_Indicator_Load; // Red/Loading
        
        public Sprite[] TimeDigits; // Numbers 0-9
        public Sprite TimeMinus;   // Minus sign

        [Header("Font (TEXT.BMP)")]
        public Sprite[] TextSprites;

        // Song Title Display (optional font/bg)]
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

        [Header("Equalizer (EQMAIN.BMP)")]
        public Sprite EqBackground;
        public Sprite EqTitleBar;
        public Sprite EqCloseNormal;
        public Sprite EqClosePressed;
        
        public Sprite EqOn_Off_Normal;
        public Sprite EqOn_Off_Pressed;
        public Sprite EqOn_On_Normal;
        public Sprite EqOn_On_Pressed;
        
        public Sprite EqAuto_Off_Normal;
        public Sprite EqAuto_Off_Pressed;
        public Sprite EqAuto_On_Normal;
        public Sprite EqAuto_On_Pressed;
        
        public Sprite EqPresetsNormal;
        public Sprite EqPresetsPressed;
        
        public Sprite EqKnobNormal;
        public Sprite EqKnobPressed;
        
        public Sprite[] EqSliderBackgrounds; // 28 states
        
        public Sprite EqGraphBackground;
        public Sprite EqGraphColors;
        public Sprite EqGraphPreampLine;

        [Header("Playlist (PLEDIT.BMP / PLEDIT.TXT)")]
        // Background & Borders
        public Sprite PlTopLeft;
        public Sprite PlTopTitle;
        public Sprite PlTopStretch; // This will be used as the "center" or "main" stretch
        public Sprite PlTopLeftStretch; // New: Stretch on the left of title
        public Sprite PlTopRightStretch; // New: Stretch on the right of title
        public Sprite PlTopRight;
        public Sprite PlBottomLeft;
        public Sprite PlBottomRight;
        public Sprite PlBottomStretch;
        public Sprite PlLeftEdge;
        public Sprite PlRightEdge;
        public Sprite PlBackground;

        // Buttons
        public Sprite PlAddUrlNormal, PlAddUrlPressed;
        public Sprite PlAddDirNormal, PlAddDirPressed;
        public Sprite PlAddFileNormal, PlAddFilePressed;
        public Sprite PlRemoveAllNormal, PlRemoveAllPressed;
        public Sprite PlRemoveSelNormal, PlRemoveSelPressed;
        public Sprite PlRemoveCropNormal, PlRemoveCropPressed;
        public Sprite PlRemoveOptNormal, PlRemoveOptPressed;
        public Sprite PlSelectAllNormal, PlSelectAllPressed;
        public Sprite PlSelectNoneNormal, PlSelectNonePressed;
        public Sprite PlSelectInvNormal, PlSelectInvPressed;
        public Sprite PlSortNormal, PlSortPressed;
        public Sprite PlFileInfoNormal, PlFileInfoPressed;
        public Sprite PlMiscNormal, PlMiscPressed;
        public Sprite PlNewListNormal, PlNewListPressed;
        public Sprite PlSaveListNormal, PlSaveListPressed;
        public Sprite PlLoadListNormal, PlLoadListPressed;

        // Scrollbar
        public Sprite PlScrollHandleNormal, PlScrollHandlePressed;

        // Close Button
        public Sprite PlCloseNormal, PlClosePressed;

        // Colors (from PLEDIT.TXT)
        public Color PlNormalColor = Color.green;
        public Color PlCurrentColor = Color.white;
        public Color PlNormalBGColor = Color.black;
        public Color PlSelectedBGColor = Color.blue;
        public Color PlMbFGColor = Color.green;
        public Color PlMbBGColor = Color.black;

        [Header("Visualizer (VISCOLOR.TXT)")]
        public Color[] VisColors;
        
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
