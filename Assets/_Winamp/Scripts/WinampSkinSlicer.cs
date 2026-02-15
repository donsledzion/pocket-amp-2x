using UnityEngine;

namespace SoftAware
{
    /// <summary>
    /// Defines sprite rectangles for Winamp 2.x classic skin layout
    /// All coordinates are based on the standard main.bmp (275x116 pixels)
    /// </summary>
    public static class WinampSkinSlicer
    {
        // Main window dimensions (MAIN.BMP)
        public static readonly Rect MainPanel = new Rect(0, 0, 275, 116);
        
        // Title bar buttons (MAIN.BMP)
        public static readonly Rect MinimizeButton = new Rect(244, 3, 9, 9);
        public static readonly Rect ShadeButton = new Rect(254, 3, 9, 9);
        public static readonly Rect CloseButton = new Rect(264, 3, 9, 9);
        
        // Toggle buttons (MAIN.BMP - top right)
        public static readonly Rect EqualizerButton = new Rect(219, 58, 23, 12);
        public static readonly Rect PlaylistButton = new Rect(242, 58, 23, 12);
        
        // Shuffle and Repeat buttons (MAIN.BMP)
        public static readonly Rect ShuffleButton = new Rect(164, 89, 46, 15);
        public static readonly Rect RepeatButton = new Rect(210, 89, 28, 15);
        
        // Volume slider background (MAIN.BMP)
        public static readonly Rect VolumeSliderBg = new Rect(107, 57, 68, 13);
        
        // Balance slider background (MAIN.BMP)
        public static readonly Rect BalanceSliderBg = new Rect(177, 57, 38, 13);
        
        // Position bar background (MAIN.BMP)
        public static readonly Rect PositionBarBg = new Rect(16, 72, 248, 10);

        /// <summary>
        /// Rectangles for CBUTTONS.BMP (Transport Buttons)
        /// Image size: 136x34
        /// Row 0 (y=0): Normal state
        /// Row 1 (y=18): Pressed state
        /// </summary>
        public static class CButtons
        {
            public static readonly Rect Previous = new Rect(0, 0, 23, 18);
            public static readonly Rect Play = new Rect(23, 0, 23, 18);
            public static readonly Rect Pause = new Rect(46, 0, 23, 18);
            public static readonly Rect Stop = new Rect(69, 0, 23, 18);
            public static readonly Rect Next = new Rect(92, 0, 22, 18);
            public static readonly Rect Eject = new Rect(114, 0, 22, 16);
            
            public static readonly Rect PreviousPressed = new Rect(0, 18, 23, 18);
            public static readonly Rect PlayPressed = new Rect(23, 18, 23, 18);
            public static readonly Rect PausePressed = new Rect(46, 18, 23, 18);
            public static readonly Rect StopPressed = new Rect(69, 18, 23, 18);
            public static readonly Rect NextPressed = new Rect(92, 18, 22, 18);
            public static readonly Rect EjectPressed = new Rect(114, 18, 22, 16);
        }
        
        /// <summary>
        /// Creates a sprite from a texture using the specified rectangle
        /// </summary>
        /// <param name="texture">Source texture (e.g., main.bmp or cbuttons.bmp)</param>
        /// <param name="rect">Rectangle defining the sprite area</param>
        /// <param name="pixelsPerUnit">Pixels per unit (default 1 for pixel-perfect)</param>
        /// <returns>Sprite cut from the texture</returns>
        public static Sprite SliceSprite(Texture2D texture, Rect rect, float pixelsPerUnit = 1f)
        {
            if (texture == null)
            {
                Debug.LogError("Cannot slice sprite from null texture");
                return null;
            }
            
            // Validate rect is within texture bounds
            if (rect.x < 0 || rect.y < 0 || 
                rect.x + rect.width > texture.width || 
                rect.y + rect.height > texture.height)
            {
                Debug.LogWarning($"Rect {rect} is outside texture bounds ({texture.width}x{texture.height})");
            }
            
            // Unity's Sprite.Create expects Y coordinate from bottom
            // BMP coordinates are from top, so we need to flip Y
            Rect flippedRect = new Rect(
                rect.x,
                texture.height - rect.y - rect.height, // Flip Y coordinate
                rect.width,
                rect.height
            );
            
            // Pivot at top-left (0, 1) for UI elements
            Vector2 pivot = new Vector2(0, 1);
            
            Sprite sprite = Sprite.Create(
                texture,
                flippedRect,
                pivot,
                pixelsPerUnit
            );
            
            return sprite;
        }
        
        /// <summary>
        /// Convenience method to slice the Play button sprite (from CBUTTONS.BMP)
        /// </summary>
        public static Sprite SlicePlayButton(Texture2D cbuttonsTexture)
        {
            return SliceSprite(cbuttonsTexture, CButtons.Play);
        }
        
        /// <summary>
        /// Convenience method to slice the Pause button sprite (from CBUTTONS.BMP)
        /// </summary>
        public static Sprite SlicePauseButton(Texture2D cbuttonsTexture)
        {
            return SliceSprite(cbuttonsTexture, CButtons.Pause);
        }
        
        /// <summary>
        /// Convenience method to slice the Stop button sprite (from CBUTTONS.BMP)
        /// </summary>
        public static Sprite SliceStopButton(Texture2D cbuttonsTexture)
        {
            return SliceSprite(cbuttonsTexture, CButtons.Stop);
        }
        
        /// <summary>
        /// Convenience method to slice the Previous button sprite (from CBUTTONS.BMP)
        /// </summary>
        public static Sprite SlicePreviousButton(Texture2D cbuttonsTexture)
        {
            return SliceSprite(cbuttonsTexture, CButtons.Previous);
        }
        
        /// <summary>
        /// Convenience method to slice the Next button sprite (from CBUTTONS.BMP)
        /// </summary>
        public static Sprite SliceNextButton(Texture2D cbuttonsTexture)
        {
            return SliceSprite(cbuttonsTexture, CButtons.Next);
        }

        /// <summary>
        /// Convenience method to slice the Eject button sprite (from CBUTTONS.BMP)
        /// </summary>
        public static Sprite SliceEjectButton(Texture2D cbuttonsTexture)
        {
            return SliceSprite(cbuttonsTexture, CButtons.Eject);
        }
    }
}
