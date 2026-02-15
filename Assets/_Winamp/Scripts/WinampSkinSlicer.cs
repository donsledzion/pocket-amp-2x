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
        public static readonly Rect TitleBar = new Rect(0, 0, 275, 14); // Standard Title Bar height
        
        // Title bar buttons (MAIN.BMP)
        public static readonly Rect MinimizeButton = new Rect(244, 3, 9, 9);
        public static readonly Rect MinimizeButtonPressed = new Rect(244, 13, 9, 9); // usually below? Need to verification or trial. Standard is usually below.
        
        public static readonly Rect ShadeButton = new Rect(254, 3, 9, 9);
        public static readonly Rect ShadeButtonPressed = new Rect(254, 13, 9, 9);
        
        public static readonly Rect CloseButton = new Rect(264, 3, 9, 9);
        public static readonly Rect CloseButtonPressed = new Rect(264, 13, 9, 9);
        
        // Toggle buttons (MAIN.BMP - top right)
        public static readonly Rect EqualizerButton = new Rect(219, 58, 23, 12);
        public static readonly Rect PlaylistButton = new Rect(242, 58, 23, 12);
        
        // Shuffle and Repeat buttons (MAIN.BMP)
        // Shuffle: 47x15
        public static readonly Rect ShuffleButtonOff = new Rect(164, 89, 46, 15);
        public static readonly Rect ShuffleButtonOffPressed = new Rect(164, 104, 46, 15); // Down state
        public static readonly Rect ShuffleButtonOn = new Rect(28, 89, 46, 15); // Green LED
        public static readonly Rect ShuffleButtonOnPressed = new Rect(28, 104, 46, 15); // Down state with Green LED

        // Repeat: 28x15
        public static readonly Rect RepeatButtonOff = new Rect(210, 89, 28, 15);
        public static readonly Rect RepeatButtonOffPressed = new Rect(210, 104, 28, 15);
        public static readonly Rect RepeatButtonOn = new Rect(76, 89, 28, 15);
        public static readonly Rect RepeatButtonOnPressed = new Rect(76, 104, 28, 15);
        
        // Volume slider background (MAIN.BMP)
        public static readonly Rect VolumeSliderBg = new Rect(107, 57, 68, 13);
        
        // Balance slider background (MAIN.BMP)
        public static readonly Rect BalanceSliderBg = new Rect(177, 57, 38, 13);
        
        // Position bar background (MAIN.BMP)
        public static readonly Rect PositionBarBg = new Rect(16, 72, 248, 10);
        
        // Position bar knob (MAIN.BMP)
        // Normal: 246, 29 (29x10) - Adjusted to fit 275px width (246+29=275)
        public static readonly Rect PositionBarKnobNormal = new Rect(246, 29, 29, 10);
        // Pressed: 246, 39? 
        // Let's assume standard Y spacing.
        // Based on visually inspecting skins, pressed is often immediately below.
        public static readonly Rect PositionBarKnobPressed = new Rect(246, 39, 29, 10);

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
            public static readonly Rect Next = new Rect(91, 0, 23, 18);
            public static readonly Rect Eject = new Rect(114, 0, 22, 16);
            
            public static readonly Rect PreviousPressed = new Rect(0, 18, 23, 18);
            public static readonly Rect PlayPressed = new Rect(23, 18, 23, 18);
            public static readonly Rect PausePressed = new Rect(46, 18, 23, 18);
            public static readonly Rect StopPressed = new Rect(69, 18, 23, 18);
            public static readonly Rect NextPressed = new Rect(91, 18, 23, 18);
            public static readonly Rect EjectPressed = new Rect(114, 16, 22, 16);
        }

        /// <summary>
        /// Rectangles for SHUFREP.BMP (Shuffle/Repeat/Eq/Playlist Buttons)
        /// Based on analyzed meta file structure (Vertical layout)
        /// </summary>
        public static class ShufRep
        {
            // Shuffle (47x15)
            // Y coordinates from Top (assuming standard height or relative to top if sliced manually)
            // Meta Y (from bottom): 70, 55, 40, 25. Diff is 15.
            // Let's assume height is enough. Since we slice from Texture, we need Top-Down Y for our SliceSprite method (which flips internally).
            // Wait, SliceSprite expects "rect.y" as Top-Down Y? 
            // NO. SliceSprite implementation: "texture.height - rect.y - rect.height".
            // So SliceSprite expects `rect.y` to be Top-Down Y coordinate.
            // If Meta says Y=70 (bottom-up), and height=15.
            // If texture height is e.g. 85.
            // 70 + 15 = 85. So Y=70 is at the very top (0 in Top-Down).
            // So:
            // Off Normal (Meta Y=70) -> Top-Down Y = 0
            // Off Pressed (Meta Y=55) -> Top-Down Y = 15
            // On Normal (Meta Y=40) -> Top-Down Y = 30
            // On Pressed (Meta Y=25) -> Top-Down Y = 45
            
            public static readonly Rect ShuffleOffNormal = new Rect(28, 0, 47, 15);
            public static readonly Rect ShuffleOffPressed = new Rect(28, 15, 47, 15);
            public static readonly Rect ShuffleOnNormal = new Rect(28, 30, 47, 15);
            public static readonly Rect ShuffleOnPressed = new Rect(28, 45, 47, 15);

            // Repeat (28x15)
            public static readonly Rect RepeatOffNormal = new Rect(0, 0, 28, 15);
            public static readonly Rect RepeatOffPressed = new Rect(0, 15, 28, 15);
            public static readonly Rect RepeatOnNormal = new Rect(0, 30, 28, 15);
            public static readonly Rect RepeatOnPressed = new Rect(0, 45, 28, 15);
            
            // Equalizer (23x12)
            // Assumed Texture Height = 85 (based on Shuffle at Y=0)
            public static readonly Rect EqOffNormal = new Rect(0, 61, 23, 12);
            public static readonly Rect EqOnNormal = new Rect(0, 73, 23, 12);
            public static readonly Rect EqOffPressed = new Rect(46, 61, 23, 12);
            public static readonly Rect EqOnPressed = new Rect(46, 73, 23, 12);
            
            // Playlist (23x12)
            public static readonly Rect PlOffNormal = new Rect(23, 61, 23, 12);
            public static readonly Rect PlOnNormal = new Rect(23, 73, 23, 12);
            public static readonly Rect PlOffPressed = new Rect(69, 61, 23, 12);
            public static readonly Rect PlOnPressed = new Rect(69, 73, 23, 12);
        }

        /// <summary>
        /// Rectangles for PLAYPAUS.BMP (Play/Pause/Stop/Indicators)
        /// </summary>
        public static class PlayPaus
        {
            public static readonly Rect PlayIcon = new Rect(1, 0, 8, 9);
            public static readonly Rect PauseIcon = new Rect(9, 0, 9, 9);
            public static readonly Rect StopIcon = new Rect(18, 0, 9, 9);
            public static readonly Rect PlayingIndicator = new Rect(36, 0, 3, 9); // Green
            public static readonly Rect LoadingIndicator = new Rect(39, 0, 3, 9); // Red
        }

        /// <summary>
        /// Rectangles for EQMAIN.BMP (Equalizer)
        /// Coordinates converted to Top-Down based on standard 275x315 layout
        /// </summary>
        public static class Equalizer
        {
            public static readonly Rect Background = new Rect(0, 0, 275, 116);
            public static readonly Rect TitleBar = new Rect(0, 134, 275, 14);
            
            public static readonly Rect CloseNormal = new Rect(0, 116, 9, 9);
            public static readonly Rect ClosePressed = new Rect(0, 125, 9, 9);
            
            public static readonly Rect On_Off_Normal = new Rect(10, 119, 25, 12);
            public static readonly Rect On_On_Normal = new Rect(69, 119, 25, 12);
            public static readonly Rect On_Off_Pressed = new Rect(128, 119, 25, 12);
            public static readonly Rect On_On_Pressed = new Rect(187, 119, 25, 12);
            
            public static readonly Rect Auto_Off_Normal = new Rect(35, 119, 33, 12);
            public static readonly Rect Auto_On_Normal = new Rect(94, 119, 33, 12);
            public static readonly Rect Auto_Off_Pressed = new Rect(153, 119, 33, 12);
            public static readonly Rect Auto_On_Pressed = new Rect(212, 119, 33, 12);
            
            public static readonly Rect PresetsNormal = new Rect(224, 164, 44, 12);
            public static readonly Rect PresetsPressed = new Rect(224, 176, 44, 12);
            
            public static readonly Rect KnobNormal = new Rect(0, 164, 11, 11);
            public static readonly Rect KnobPressed = new Rect(0, 176, 11, 11);
            
            // Sliders start at X=13, width 14, height 63. There are 28 frames total in vertical rows.
            // But we will use the dynamic logic in Importer to load all 28 frames.
            public static readonly Rect SliderFirstFrame = new Rect(13, 164, 14, 63);
            
            public static readonly Rect GraphBG = new Rect(0, 294, 113, 19);
            public static readonly Rect GraphColors = new Rect(115, 294, 1, 19);
            public static readonly Rect PreampLine = new Rect(0, 314, 113, 1);
        }

        public static class Numbers
        {
            public static readonly int Width = 9;
            public static readonly int Height = 13;
            public static Rect GetDigitRect(int digit) => new Rect(digit * Width, 0, Width, Height);
        }

        public static class NumsEx
        {
            public static readonly Rect MinusSign = new Rect(101, 6, 5, 1);
        }

        /// <summary>
        /// Rectangles for MONOSTER.BMP (Mono/Stereo Indicators)
        /// Texture Size: 58x24
        /// </summary>
        public static class MonoSter
        {
            // Stereo (29x12) - Left half
            // Meta: ON (Y=12) -> Top Y=0
            // Meta: OFF (Y=0) -> Top Y=12
            public static readonly Rect StereoOn = new Rect(0, 0, 29, 12);
            public static readonly Rect StereoOff = new Rect(0, 12, 29, 12);
            
            // Mono (29x12) - Right half
            // Meta: ON (Y=12) -> Top Y=0
            // Meta: OFF (Y=0) -> Top Y=12
            public static readonly Rect MonoOn = new Rect(29, 0, 29, 12);
            public static readonly Rect MonoOff = new Rect(29, 12, 29, 12);
        }

        public static class Volume
        {
            // Knobs (14x11)
            // Meta: Normal X=15, Y=0. Pressed X=0, Y=0.
            // Assuming texture height ~420+ (BG starts at 15..420).
            // Actually, let's look at the meta again.
            // Y=0 is bottom.
            // If we use SliceSprite which expects Top-Down Y, we need to know Texture Height.
            // Or we can just pass Rect with Y from bottom if we change SliceSprite?
            // No, SliceSprite logic is: flippedRect.y = texture.height - rect.y - rect.height.
            // So rect.y MUST be top-down Y.
            
            // Standard Volume.bmp height is usually 422? (28*15 + 2??)
            // Let's assume dynamic slicing based on texture height in Manager?
            // Or define relative to bottom?
            // "Standard" Main.bmp is fixed size. Secondary bmps might vary?
            // But let's define the X/W/H here. Y will be calculated dynamically or assumed.
            
            public static readonly Rect KnobNormal = new Rect(15, 0, 14, 11); // Y will need adjustment if not 0-at-top
            public static readonly Rect KnobPressed = new Rect(0, 0, 14, 11);
            
            // Frame constants
            public const int FrameCount = 28;
            public const int FrameWidth = 65;
            public const int FrameHeight = 13;
            public const int FrameStride = 15; // Distance between frame starts
        }
        
        public static class Balance
        {
            public static readonly Rect KnobNormal = new Rect(15, 0, 14, 11);
            public static readonly Rect KnobPressed = new Rect(0, 0, 14, 11);
            
            public const int FrameCount = 28;
            public const int FrameWidth = 38;
            public const int FrameHeight = 13;
            public const int FrameStride = 15;
        }

        /// <summary>
        /// Rectangles for TEXT.BMP Font
        /// Character size is mostly 5x6
        /// </summary>
        public static class Font
        {
            public const int CharHeight = 6;
            
            // Layout based on standard Winamp TEXT.BMP and provided meta
            // Row 1 (Top, y=0 in Top-Down)
            public static readonly string Row1 = "ABCDEFGHIJKLMNOPQRSTUVWXYZ\"";
            // Row 2 (Middle, y=6)
            public static readonly string Row2 = "0123456789"; 

            public static Rect GetCharRect(char c)
            {
                c = char.ToUpper(c);
                
                // Top Row: A-Z (0-125), " (130)
                int row1Index = Row1.IndexOf(c);
                if (row1Index >= 0)
                {
                    return new Rect(row1Index * 5, 0, 5, CharHeight);
                }

                if (c == '@') return new Rect(135, 0, 8, CharHeight);

                // Middle Row: 0-9 (0-45)
                int row2Index = Row2.IndexOf(c);
                if (row2Index >= 0)
                {
                    return new Rect(row2Index * 5, 6, 5, CharHeight);
                }

                // Special characters in Row 2
                if (c == '.') return new Rect(49, 6, 3, CharHeight);
                if (c == ':') return new Rect(60, 6, 5, CharHeight);
                if (c == '(') return new Rect(65, 6, 5, CharHeight);
                if (c == ')') return new Rect(70, 6, 5, CharHeight);
                if (c == '-') return new Rect(75, 6, 5, CharHeight);
                if (c == '\'') return new Rect(80, 6, 5, CharHeight);
                if (c == '!') return new Rect(85, 6, 5, CharHeight);
                if (c == '_') return new Rect(90, 6, 5, CharHeight);
                if (c == '+') return new Rect(95, 6, 5, CharHeight);
                if (c == '\\') return new Rect(100, 6, 5, CharHeight);
                if (c == '/') return new Rect(105, 6, 5, CharHeight);
                if (c == '[') return new Rect(110, 6, 5, CharHeight);
                if (c == ']') return new Rect(115, 6, 5, CharHeight);
                if (c == '^') return new Rect(120, 6, 5, CharHeight);
                if (c == '&') return new Rect(125, 6, 5, CharHeight);
                if (c == '%') return new Rect(130, 6, 5, CharHeight);
                if (c == ',') return new Rect(135, 6, 5, CharHeight);
                if (c == '=') return new Rect(140, 6, 5, CharHeight);
                if (c == '$') return new Rect(145, 6, 5, CharHeight);
                if (c == '#') return new Rect(150, 6, 5, CharHeight);

                // Bottom Row (y=12)
                if (c == '?') return new Rect(15, 12, 5, CharHeight);
                if (c == '*') return new Rect(20, 12, 5, CharHeight);
                if (c == ' ') return new Rect(25, 12, 5, CharHeight);

                return Rect.zero;
            }

            public static string GetSpriteName(char c)
            {
                if (c == '"') return "Text_''";
                if (c == ':') return "Text_Colon";
                if (c == '(') return "Text_LBracket";
                if (c == ')') return "Text_RBracket";
                if (c == '*') return "Text_Shuriken";
                if (c == ' ') return "Text_Space";
                if (c == '?') return "Text_Question";
                return $"Text_{c}";
            }
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
