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
