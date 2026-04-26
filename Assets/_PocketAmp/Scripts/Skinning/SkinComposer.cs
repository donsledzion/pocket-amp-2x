using UnityEngine;
using System.Collections.Generic;

namespace SoftAware
{
    /// <summary>
    /// Responsible for slicing textures into sprites and populating the Skin object.
    /// Moves slicing logic out of Importer/Manager.
    /// </summary>
    public class SkinComposer
    {
        public void ComposeMain(Skin skin, Texture2D mainTex)
        {
            if (skin == null || mainTex == null) return;
            
            skin.MainBackground = SkinSlicer.SliceSprite(mainTex, SkinSlicer.MainPanel);
            
            // Standard TitleBar components from MAIN.BMP
            skin.TitleBar = SkinSlicer.SliceSprite(mainTex, SkinSlicer.TitleBar);
            skin.MinimizeBtn_Normal = SkinSlicer.SliceSprite(mainTex, SkinSlicer.MinimizeButton);
            skin.MinimizeBtn_Pressed = SkinSlicer.SliceSprite(mainTex, SkinSlicer.MinimizeButtonPressed);
            skin.CloseBtn_Normal = SkinSlicer.SliceSprite(mainTex, SkinSlicer.CloseButton);
            skin.CloseBtn_Pressed = SkinSlicer.SliceSprite(mainTex, SkinSlicer.CloseButtonPressed);
        }

        public void ComposeTitleBar(Skin skin, Texture2D titleBarTex)
        {
            if (skin == null || titleBarTex == null) return;

            // Overrides Main TitleBar with TITLEBAR.BMP if present
            skin.TitleBar = SkinSlicer.SliceSpriteBottomUp(titleBarTex, SkinSlicer.TitleBarSeparate.Focused);
            skin.MinimizeBtn_Normal = SkinSlicer.SliceSpriteBottomUp(titleBarTex, SkinSlicer.TitleBarSeparate.MinimizeNormal);
            skin.MinimizeBtn_Pressed = SkinSlicer.SliceSpriteBottomUp(titleBarTex, SkinSlicer.TitleBarSeparate.MinimizePressed);
            skin.CloseBtn_Normal = SkinSlicer.SliceSpriteBottomUp(titleBarTex, SkinSlicer.TitleBarSeparate.CloseNormal);
            skin.CloseBtn_Pressed = SkinSlicer.SliceSpriteBottomUp(titleBarTex, SkinSlicer.TitleBarSeparate.ClosePressed);
        }

        public void ComposeCButtons(Skin skin, Texture2D cbuttonsTex)
        {
            if (skin == null || cbuttonsTex == null) return;

            skin.PlayBtn_Normal = SkinSlicer.SliceSprite(cbuttonsTex, SkinSlicer.CButtons.Play);
            skin.PlayBtn_Pressed = SkinSlicer.SliceSprite(cbuttonsTex, SkinSlicer.CButtons.PlayPressed);
            skin.PauseBtn_Normal = SkinSlicer.SliceSprite(cbuttonsTex, SkinSlicer.CButtons.Pause);
            skin.PauseBtn_Pressed = SkinSlicer.SliceSprite(cbuttonsTex, SkinSlicer.CButtons.PausePressed);
            skin.StopBtn_Normal = SkinSlicer.SliceSprite(cbuttonsTex, SkinSlicer.CButtons.Stop);
            skin.StopBtn_Pressed = SkinSlicer.SliceSprite(cbuttonsTex, SkinSlicer.CButtons.StopPressed);
            skin.PrevBtn_Normal = SkinSlicer.SliceSprite(cbuttonsTex, SkinSlicer.CButtons.Previous);
            skin.PrevBtn_Pressed = SkinSlicer.SliceSprite(cbuttonsTex, SkinSlicer.CButtons.PreviousPressed);
            skin.NextBtn_Normal = SkinSlicer.SliceSprite(cbuttonsTex, SkinSlicer.CButtons.Next);
            skin.NextBtn_Pressed = SkinSlicer.SliceSprite(cbuttonsTex, SkinSlicer.CButtons.NextPressed);
            skin.EjectBtn_Normal = SkinSlicer.SliceSprite(cbuttonsTex, SkinSlicer.CButtons.Eject);
            skin.EjectBtn_Pressed = SkinSlicer.SliceSprite(cbuttonsTex, SkinSlicer.CButtons.EjectPressed);
        }

        public void ComposeShufRep(Skin skin, Texture2D shufrepTex, Texture2D fallbackMainTex = null)
        {
            if (skin == null) return;

            if (shufrepTex != null)
            {
                skin.Shuffle_Off_Normal = SkinSlicer.SliceSprite(shufrepTex, SkinSlicer.ShufRep.ShuffleOffNormal);
                skin.Shuffle_Off_Pressed = SkinSlicer.SliceSprite(shufrepTex, SkinSlicer.ShufRep.ShuffleOffPressed);
                skin.Shuffle_On_Normal = SkinSlicer.SliceSprite(shufrepTex, SkinSlicer.ShufRep.ShuffleOnNormal);
                skin.Shuffle_On_Pressed = SkinSlicer.SliceSprite(shufrepTex, SkinSlicer.ShufRep.ShuffleOnPressed);

                skin.Repeat_Off_Normal = SkinSlicer.SliceSprite(shufrepTex, SkinSlicer.ShufRep.RepeatOffNormal);
                skin.Repeat_Off_Pressed = SkinSlicer.SliceSprite(shufrepTex, SkinSlicer.ShufRep.RepeatOffPressed);
                skin.Repeat_On_Normal = SkinSlicer.SliceSprite(shufrepTex, SkinSlicer.ShufRep.RepeatOnNormal);
                skin.Repeat_On_Pressed = SkinSlicer.SliceSprite(shufrepTex, SkinSlicer.ShufRep.RepeatOnPressed);

                skin.EQ_Off_Normal = SkinSlicer.SliceSprite(shufrepTex, SkinSlicer.ShufRep.EqOffNormal);
                skin.EQ_Off_Pressed = SkinSlicer.SliceSprite(shufrepTex, SkinSlicer.ShufRep.EqOffPressed);
                skin.EQ_On_Normal = SkinSlicer.SliceSprite(shufrepTex, SkinSlicer.ShufRep.EqOnNormal);
                skin.EQ_On_Pressed = SkinSlicer.SliceSprite(shufrepTex, SkinSlicer.ShufRep.EqOnPressed);

                skin.PL_Off_Normal = SkinSlicer.SliceSprite(shufrepTex, SkinSlicer.ShufRep.PlOffNormal);
                skin.PL_Off_Pressed = SkinSlicer.SliceSprite(shufrepTex, SkinSlicer.ShufRep.PlOffPressed);
                skin.PL_On_Normal = SkinSlicer.SliceSprite(shufrepTex, SkinSlicer.ShufRep.PlOnNormal);
                skin.PL_On_Pressed = SkinSlicer.SliceSprite(shufrepTex, SkinSlicer.ShufRep.PlOnPressed);
            }
            else if (fallbackMainTex != null)
            {
                // Fallback to MAIN.BMP
                skin.Shuffle_Off_Normal = SkinSlicer.SliceSprite(fallbackMainTex, SkinSlicer.ShuffleButtonOff);
                skin.Shuffle_Off_Pressed = SkinSlicer.SliceSprite(fallbackMainTex, SkinSlicer.ShuffleButtonOffPressed);
                skin.Shuffle_On_Normal = SkinSlicer.SliceSprite(fallbackMainTex, SkinSlicer.ShuffleButtonOn);
                skin.Shuffle_On_Pressed = SkinSlicer.SliceSprite(fallbackMainTex, SkinSlicer.ShuffleButtonOnPressed);

                skin.Repeat_Off_Normal = SkinSlicer.SliceSprite(fallbackMainTex, SkinSlicer.RepeatButtonOff);
                skin.Repeat_Off_Pressed = SkinSlicer.SliceSprite(fallbackMainTex, SkinSlicer.RepeatButtonOffPressed);
                skin.Repeat_On_Normal = SkinSlicer.SliceSprite(fallbackMainTex, SkinSlicer.RepeatButtonOn);
                skin.Repeat_On_Pressed = SkinSlicer.SliceSprite(fallbackMainTex, SkinSlicer.RepeatButtonOnPressed);

                skin.EQ_Off_Normal = SkinSlicer.SliceSprite(fallbackMainTex, SkinSlicer.EqualizerButton); 
                skin.EQ_On_Normal = skin.EQ_Off_Normal; 
                skin.PL_Off_Normal = SkinSlicer.SliceSprite(fallbackMainTex, SkinSlicer.PlaylistButton);
                skin.PL_On_Normal = skin.PL_Off_Normal;
            }
        }

        public void ComposeEqualizer(Skin skin, Texture2D eqMainTex)
        {
            if (skin == null || eqMainTex == null) return;

            // Background & Title
            skin.EqBackground = SkinSlicer.SliceSprite(eqMainTex, SkinSlicer.Equalizer.Background);
            skin.EqTitleBar = SkinSlicer.SliceSprite(eqMainTex, SkinSlicer.Equalizer.TitleBar);
            
            // Close button
            skin.EqCloseNormal = SkinSlicer.SliceSprite(eqMainTex, SkinSlicer.Equalizer.CloseNormal);
            skin.EqClosePressed = SkinSlicer.SliceSprite(eqMainTex, SkinSlicer.Equalizer.ClosePressed);
            
            // Toggles (On/Auto)
            skin.EqOn_Off_Normal = SkinSlicer.SliceSprite(eqMainTex, SkinSlicer.Equalizer.On_Off_Normal);
            skin.EqOn_On_Normal = SkinSlicer.SliceSprite(eqMainTex, SkinSlicer.Equalizer.On_On_Normal);
            skin.EqOn_Off_Pressed = SkinSlicer.SliceSprite(eqMainTex, SkinSlicer.Equalizer.On_Off_Pressed);
            skin.EqOn_On_Pressed = SkinSlicer.SliceSprite(eqMainTex, SkinSlicer.Equalizer.On_On_Pressed);
            
            skin.EqAuto_Off_Normal = SkinSlicer.SliceSprite(eqMainTex, SkinSlicer.Equalizer.Auto_Off_Normal);
            skin.EqAuto_On_Normal = SkinSlicer.SliceSprite(eqMainTex, SkinSlicer.Equalizer.Auto_On_Normal);
            skin.EqAuto_Off_Pressed = SkinSlicer.SliceSprite(eqMainTex, SkinSlicer.Equalizer.Auto_Off_Pressed);
            skin.EqAuto_On_Pressed = SkinSlicer.SliceSprite(eqMainTex, SkinSlicer.Equalizer.Auto_On_Pressed);
            
            // Presets
            skin.EqPresetsNormal = SkinSlicer.SliceSprite(eqMainTex, SkinSlicer.Equalizer.PresetsNormal);
            skin.EqPresetsPressed = SkinSlicer.SliceSprite(eqMainTex, SkinSlicer.Equalizer.PresetsPressed);
            
            // Knob
            skin.EqKnobNormal = SkinSlicer.SliceSprite(eqMainTex, SkinSlicer.Equalizer.KnobNormal);
            skin.EqKnobPressed = SkinSlicer.SliceSprite(eqMainTex, SkinSlicer.Equalizer.KnobPressed);
            
            // Slider Backgrounds
            var sliderFames = new List<Sprite>();
            Rect first = SkinSlicer.Equalizer.SliderFirstFrame;
            for (int row = 0; row < 2; row++)
            {
                for (int col = 0; col < 14; col++)
                {
                    Rect frameRect = new Rect(
                        first.x + (col * 15), 
                        first.y + (row * 65), 
                        first.width, 
                        first.height);
                    sliderFames.Add(SkinSlicer.SliceSprite(eqMainTex, frameRect));
                }
            }
            skin.EqSliderBackgrounds = sliderFames.ToArray();
            
            // Graph Elements
            skin.EqGraphBackground = SkinSlicer.SliceSprite(eqMainTex, SkinSlicer.Equalizer.GraphBG);
            skin.EqGraphColors = SkinSlicer.SliceSprite(eqMainTex, SkinSlicer.Equalizer.GraphColors);
            skin.EqGraphPreampLine = SkinSlicer.SliceSprite(eqMainTex, SkinSlicer.Equalizer.PreampLine);
        }

        public void ComposePlaylist(Skin skin, Texture2D plEditTex)
        {
            if (skin == null || plEditTex == null) return;

             // Borders & Title
            skin.PlTopLeft = SkinSlicer.SliceSprite(plEditTex, SkinSlicer.Playlist.TopLeft);
            skin.PlTopTitle = SkinSlicer.SliceSprite(plEditTex, SkinSlicer.Playlist.TopTitle);
            skin.PlTopStretch = SkinSlicer.SliceSprite(plEditTex, SkinSlicer.Playlist.TopStretch);
            skin.PlTopLeftStretch = SkinSlicer.SliceSprite(plEditTex, SkinSlicer.Playlist.TopLeftStretch);
            skin.PlTopRightStretch = SkinSlicer.SliceSprite(plEditTex, SkinSlicer.Playlist.TopRightStretch);
            skin.PlTopRight = SkinSlicer.SliceSprite(plEditTex, SkinSlicer.Playlist.TopRight);
            
            skin.PlBottomLeft = SkinSlicer.SliceSprite(plEditTex, SkinSlicer.Playlist.BottomLeft);
            skin.PlBottomRight = SkinSlicer.SliceSprite(plEditTex, SkinSlicer.Playlist.BottomRight);
            skin.PlBottomStretch = SkinSlicer.SliceSprite(plEditTex, SkinSlicer.Playlist.BottomStretch);
            
            skin.PlLeftEdge = SkinSlicer.SliceSprite(plEditTex, SkinSlicer.Playlist.LeftEdge);
            skin.PlRightEdge = SkinSlicer.SliceSprite(plEditTex, SkinSlicer.Playlist.RightEdge);

            // Buttons Add
            skin.PlAddUrlNormal = SkinSlicer.SliceSprite(plEditTex, SkinSlicer.Playlist.AddUrlNormal);
            skin.PlAddUrlPressed = SkinSlicer.SliceSprite(plEditTex, SkinSlicer.Playlist.AddUrlPressed);
            skin.PlAddDirNormal = SkinSlicer.SliceSprite(plEditTex, SkinSlicer.Playlist.AddDirNormal);
            skin.PlAddDirPressed = SkinSlicer.SliceSprite(plEditTex, SkinSlicer.Playlist.AddDirPressed);
            skin.PlAddFileNormal = SkinSlicer.SliceSprite(plEditTex, SkinSlicer.Playlist.AddFileNormal);
            skin.PlAddFilePressed = SkinSlicer.SliceSprite(plEditTex, SkinSlicer.Playlist.AddFilePressed);

            // Buttons Remove
            skin.PlRemoveAllNormal = SkinSlicer.SliceSprite(plEditTex, SkinSlicer.Playlist.RemAllNormal);
            skin.PlRemoveAllPressed = SkinSlicer.SliceSprite(plEditTex, SkinSlicer.Playlist.RemAllPressed);
            skin.PlRemoveSelNormal = SkinSlicer.SliceSprite(plEditTex, SkinSlicer.Playlist.RemSelNormal);
            skin.PlRemoveSelPressed = SkinSlicer.SliceSprite(plEditTex, SkinSlicer.Playlist.RemSelPressed);
            skin.PlRemoveCropNormal = SkinSlicer.SliceSprite(plEditTex, SkinSlicer.Playlist.RemCropNormal);
            skin.PlRemoveCropPressed = SkinSlicer.SliceSprite(plEditTex, SkinSlicer.Playlist.RemCropPressed);
            skin.PlRemoveOptNormal = SkinSlicer.SliceSprite(plEditTex, SkinSlicer.Playlist.RemMiscNormal);
            skin.PlRemoveOptPressed = SkinSlicer.SliceSprite(plEditTex, SkinSlicer.Playlist.RemMiscPressed);

            // Buttons Select
            skin.PlSelectAllNormal = SkinSlicer.SliceSprite(plEditTex, SkinSlicer.Playlist.SelAllNormal);
            skin.PlSelectAllPressed = SkinSlicer.SliceSprite(plEditTex, SkinSlicer.Playlist.SelAllPressed);
            skin.PlSelectNoneNormal = SkinSlicer.SliceSprite(plEditTex, SkinSlicer.Playlist.SelNoneNormal);
            skin.PlSelectNonePressed = SkinSlicer.SliceSprite(plEditTex, SkinSlicer.Playlist.SelNonePressed);
            skin.PlSelectInvNormal = SkinSlicer.SliceSprite(plEditTex, SkinSlicer.Playlist.SelInvNormal);
            skin.PlSelectInvPressed = SkinSlicer.SliceSprite(plEditTex, SkinSlicer.Playlist.SelInvPressed);

            // Buttons Sort/Misc
            skin.PlSortNormal = SkinSlicer.SliceSprite(plEditTex, SkinSlicer.Playlist.SortNormal);
            skin.PlSortPressed = SkinSlicer.SliceSprite(plEditTex, SkinSlicer.Playlist.SortPressed);
            skin.PlFileInfoNormal = SkinSlicer.SliceSprite(plEditTex, SkinSlicer.Playlist.FileInfoNormal);
            skin.PlFileInfoPressed = SkinSlicer.SliceSprite(plEditTex, SkinSlicer.Playlist.FileInfoPressed);
            skin.PlMiscNormal = SkinSlicer.SliceSprite(plEditTex, SkinSlicer.Playlist.MiscNormal);
            skin.PlMiscPressed = SkinSlicer.SliceSprite(plEditTex, SkinSlicer.Playlist.MiscPressed);

            // Buttons List
            skin.PlNewListNormal = SkinSlicer.SliceSprite(plEditTex, SkinSlicer.Playlist.NewListNormal);
            skin.PlNewListPressed = SkinSlicer.SliceSprite(plEditTex, SkinSlicer.Playlist.NewListPressed);
            skin.PlSaveListNormal = SkinSlicer.SliceSprite(plEditTex, SkinSlicer.Playlist.SaveListNormal);
            skin.PlSaveListPressed = SkinSlicer.SliceSprite(plEditTex, SkinSlicer.Playlist.SaveListPressed);
            skin.PlLoadListNormal = SkinSlicer.SliceSprite(plEditTex, SkinSlicer.Playlist.LoadListNormal);
            skin.PlLoadListPressed = SkinSlicer.SliceSprite(plEditTex, SkinSlicer.Playlist.LoadListPressed);

            // Scrollbar
            skin.PlScrollHandleNormal = SkinSlicer.SliceSprite(plEditTex, SkinSlicer.Playlist.SliderHandleNormal);
            skin.PlScrollHandlePressed = SkinSlicer.SliceSprite(plEditTex, SkinSlicer.Playlist.SliderHandlePressed);

            // Close Button
            skin.PlCloseNormal = SkinSlicer.SliceSprite(plEditTex, SkinSlicer.Playlist.CloseNormal);
            skin.PlClosePressed = SkinSlicer.SliceSprite(plEditTex, SkinSlicer.Playlist.ClosePressed);

            // Clippers
            skin.PlAddClipper = SkinSlicer.SliceSprite(plEditTex, SkinSlicer.Playlist.AddClipper);
            skin.PlRemoveClipper = SkinSlicer.SliceSprite(plEditTex, SkinSlicer.Playlist.RemoveClipper);
            skin.PlSelectClipper = SkinSlicer.SliceSprite(plEditTex, SkinSlicer.Playlist.SelectClipper);
            skin.PlMiscClipper = SkinSlicer.SliceSprite(plEditTex, SkinSlicer.Playlist.MiscClipper);
            skin.PlListOptionsClipper = SkinSlicer.SliceSprite(plEditTex, SkinSlicer.Playlist.ListOptionsClipper);
        }

        public void ComposePosBar(Skin skin, Texture2D posbarTex, Texture2D mainTex)
        {
            Texture2D source = posbarTex != null ? posbarTex : mainTex;
            if (skin == null || source == null) return;

            skin.PosKnobNormal = SkinSlicer.SliceSprite(source, new Rect(248, 0, 29, 10));
            skin.PosKnobPressed = SkinSlicer.SliceSprite(source, new Rect(278, 0, 29, 10));
        }

        public void ComposeMonoSter(Skin skin, Texture2D monosterTex)
        {
            if (skin == null || monosterTex == null) return;

            int w = monosterTex.width;
            
            // Standard Dimensions:
            // Total Width: 56px.
            // Stereo (Left): 29px.
            // Mono (Right): 27px.
            //
            // Some modern skins use 58px (29px + 29px).
            // We should always prioritize the standard 29px for Stereo.
            
            int stereoW = 29;
            
            // Safety for weirdly small textures
            if (w < 29) stereoW = w; 
            
            // Mono takes the rest
            int monoX = stereoW;
            int monoW = w - monoX;
            
            skin.Stereo_Active = SkinSlicer.SliceSprite(monosterTex, new Rect(0, 0, stereoW, 12));
            skin.Stereo_Inactive = SkinSlicer.SliceSprite(monosterTex, new Rect(0, 12, stereoW, 12));
            
            // Ensure we have width for Mono
            if (monoW > 0)
            {
                skin.Mono_Inactive = SkinSlicer.SliceSprite(monosterTex, new Rect(monoX, 12, monoW, 12));
            }
            else
            {
                skin.Mono_Inactive = SkinSlicer.CreateTransparentSprite();
            }
            
            // Note: Mono_Active (Rect(monoX, 0, ...)) seems unused in current skin def?
            // If we needed it: skin.Mono_Active = ... new Rect(monoX, 0, monoW, 12));
        }

        public void ComposeVolume(Skin skin, Texture2D volumeTex)
        {
            if (skin == null || volumeTex == null) return;

            float h = volumeTex.height;
            // Standard Volume height is ~420px + knobs (11px) = ~431px+
            // If height is small (e.g. just slider frames ~420), we likely have no knobs.
            // Knobs are usually at Y=0 (bottom) or Y=Height (top) depending on format, 
            // but standard Volume.bmp has slider frames from top, and knobs at the very bottom?
            // Actually standard: 
            // 0-14: Slider Background (28 frames)
            // ...
            // At the bottom: Slider Knobs.
            
            // In Unity Slicing (Bottom-Left 0,0), if we use Top-Down logic:
            // Volume Frames start at Y=0 (top) down to Y=420.
            // Knobs are usually below that (Y > 420 in Top-Down, or Y < something in Bottom-Up).
            
            // Let's protect against missing knobs.
            // Standard knob rects are at Top-Down Y ~422?
            // If texture height < 422, we probably don't have knobs.
            
            bool hasKnobs = h >= 422; 
            
            if (hasKnobs)
            {
               skin.VolumeKnobNormal = SkinSlicer.SliceSprite(volumeTex, new Rect(SkinSlicer.Volume.KnobNormal.x, h - SkinSlicer.Volume.KnobNormal.y - SkinSlicer.Volume.KnobNormal.height, SkinSlicer.Volume.KnobNormal.width, SkinSlicer.Volume.KnobNormal.height));
               skin.VolumeKnobPressed = SkinSlicer.SliceSprite(volumeTex, new Rect(SkinSlicer.Volume.KnobPressed.x, h - SkinSlicer.Volume.KnobPressed.y - SkinSlicer.Volume.KnobPressed.height, SkinSlicer.Volume.KnobPressed.width, SkinSlicer.Volume.KnobPressed.height));
            }
            else
            {
                // Create transparent sprites for missing knobs
                skin.VolumeKnobNormal = SkinSlicer.CreateTransparentSprite();
                skin.VolumeKnobPressed = SkinSlicer.CreateTransparentSprite();
            }

            
            skin.VolumeAnimation = new Sprite[SkinSlicer.Volume.FrameCount];
            for (int i = 0; i < SkinSlicer.Volume.FrameCount; i++)
            {
                // Standard logic: Frames are 15px high (stride), 13px actual visual? 
                // SkinSlicer.Volume.FrameStride = 15.
                // If texture is standard, we slice 28 frames.
                
                // If texture is single frame (unlikely for Volume but possible), we could handle it.
                // But usually Volume has all frames. 
                // The issue user reported was MISSING KNOBS, not single frame volume.
                
                // Using standard logic but with clamped Y from our robust Slicer.
                // Calculation:
                // i=0 -> Y=0 (Top)
                // i=27 -> Y=27*15 = 405.
                // 405 + 15 = 420.
                
                // In Bottom-Up:
                // yTop = h - (i * stride) - height? 
                // Wait, previous logic was:
                // float yBottom = 420 - (i * stride);
                // This '420' was a hardcoded assumption of texture content height excluding knobs?
                
                // Let's trust the "Top-Down" nature:
                // Frame 0 is at Top 0.
                // Frame 1 is at Top 15.
                // ...
                // Rect(0, i * 15, 65, 13). 
                // SliceSprite takes Top-Down Rect Y.
                
                float topY = i * SkinSlicer.Volume.FrameStride;
                
                // Robust check: valid only if within texture
                if (topY + SkinSlicer.Volume.FrameHeight <= h)
                {
                     skin.VolumeAnimation[i] = SkinSlicer.SliceSprite(volumeTex, new Rect(0, topY, SkinSlicer.Volume.FrameWidth, SkinSlicer.Volume.FrameHeight));
                }
                else
                {
                     // Fallback if texture is too short for all frames, repeat last valid or transparent?
                     // Usually shouldn't happen if h >= 420.
                     // If h < 420, we might be in "Single Frame" territory?
                     if (i > 0 && skin.VolumeAnimation[i-1] != null)
                        skin.VolumeAnimation[i] = skin.VolumeAnimation[i-1];
                     else
                        skin.VolumeAnimation[i] = SkinSlicer.CreateTransparentSprite();
                }
            }
        }

        public void ComposeBalance(Skin skin, Texture2D balanceTex)
        {
            if (skin == null || balanceTex == null) return;

            float h = balanceTex.height;
            
            // Check for Single Frame Case
            // Standard Balance has 28 frames * 15px = 420px.
            // If height is small (e.g. < 50px), it's likely a single frame to be repeated.
            bool isSingleFrame = h < 100; // Heuristic
            
            skin.BalanceAnimation = new Sprite[SkinSlicer.Balance.FrameCount];

            if (isSingleFrame)
            {
                // Slice the only frame we have (assuming it's at 0,0 or similar)
                // Use a safe rect based on texture size
                Rect singleRect = new Rect(9, 0, SkinSlicer.Balance.FrameWidth, Mathf.Min(SkinSlicer.Balance.FrameHeight, h));
                
                // Often in single frame skins, the graphic might not be exactly at X=9 if it's a "compact" style?
                // But let's stick to standard X=9 if possible, or X=0 if texture is narrow?
                if (balanceTex.width < 15) singleRect.x = 0; // If texture is just the slider knob width
                
                Sprite singleSprite = SkinSlicer.SliceSprite(balanceTex, singleRect);
                
                for (int i = 0; i < SkinSlicer.Balance.FrameCount; i++)
                {
                    skin.BalanceAnimation[i] = singleSprite;
                }
                
                // Knobs for single frame usually don't exist
                skin.BalanceKnobNormal = SkinSlicer.CreateTransparentSprite();
                skin.BalanceKnobPressed = SkinSlicer.CreateTransparentSprite();
            }
            else
            {
                // Standard Multi-Frame Logic
                
                // Check for knobs (Standard height > 422?)
                bool hasKnobs = h >= 422;
                
                if (hasKnobs)
                {
                    skin.BalanceKnobNormal = SkinSlicer.SliceSprite(balanceTex, new Rect(SkinSlicer.Balance.KnobNormal.x, h - SkinSlicer.Balance.KnobNormal.y - SkinSlicer.Balance.KnobNormal.height, SkinSlicer.Balance.KnobNormal.width, SkinSlicer.Balance.KnobNormal.height));
                    skin.BalanceKnobPressed = SkinSlicer.SliceSprite(balanceTex, new Rect(SkinSlicer.Balance.KnobPressed.x, h - SkinSlicer.Balance.KnobPressed.y - SkinSlicer.Balance.KnobPressed.height, SkinSlicer.Balance.KnobPressed.width, SkinSlicer.Balance.KnobPressed.height));
                }
                else
                {
                    skin.BalanceKnobNormal = SkinSlicer.CreateTransparentSprite();
                    skin.BalanceKnobPressed = SkinSlicer.CreateTransparentSprite();
                }

                for (int i = 0; i < SkinSlicer.Balance.FrameCount; i++)
                {
                    float topY = i * SkinSlicer.Balance.FrameStride;
                    
                    if (topY + SkinSlicer.Balance.FrameHeight <= h)
                    {
                        skin.BalanceAnimation[i] = SkinSlicer.SliceSprite(balanceTex, new Rect(9, topY, SkinSlicer.Balance.FrameWidth, SkinSlicer.Balance.FrameHeight));
                    }
                    else
                    {
                        if (i > 0 && skin.BalanceAnimation[i-1] != null)
                            skin.BalanceAnimation[i] = skin.BalanceAnimation[i-1];
                        else
                             skin.BalanceAnimation[i] = SkinSlicer.CreateTransparentSprite();
                    }
                }
            }
        }

        public void ComposePlayPaus(Skin skin, Texture2D playpausTex)
        {
            if (skin == null || playpausTex == null) return;

            skin.Status_Play = SkinSlicer.SliceSprite(playpausTex, SkinSlicer.PlayPaus.PlayIcon);
            skin.Status_Pause = SkinSlicer.SliceSprite(playpausTex, SkinSlicer.PlayPaus.PauseIcon);
            skin.Status_Stop = SkinSlicer.SliceSprite(playpausTex, SkinSlicer.PlayPaus.StopIcon);
            skin.Status_Indicator_Play = SkinSlicer.SliceSprite(playpausTex, SkinSlicer.PlayPaus.PlayingIndicator);
            skin.Status_Indicator_Load = SkinSlicer.SliceSprite(playpausTex, SkinSlicer.PlayPaus.LoadingIndicator);
        }

        public void ComposeNumbers(Skin skin, Texture2D numbersTex, Texture2D numsExTex)
        {
            if (skin == null) return;

            bool numbersFound = false;
            if (numbersTex != null)
            {
                skin.TimeDigits = new Sprite[10];
                for (int i = 0; i < 10; i++) skin.TimeDigits[i] = SkinSlicer.SliceSprite(numbersTex, SkinSlicer.Numbers.GetDigitRect(i));
                numbersFound = true;
            }

            if (numsExTex != null)
            {
                if (!numbersFound && numsExTex.width >= 90)
                {
                    skin.TimeDigits = new Sprite[10];
                    for (int i = 0; i < 10; i++) skin.TimeDigits[i] = SkinSlicer.SliceSprite(numsExTex, SkinSlicer.Numbers.GetDigitRect(i));
                }
                skin.TimeMinus = SkinSlicer.SliceSprite(numsExTex, SkinSlicer.NumsEx.MinusSign);
            }
        }

        public void ComposeText(Skin skin, Texture2D textTex)
        {
            if (skin == null || textTex == null) return;

            string allChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ\"@0123456789.:()-'!_+\\/[]^&%,=$#?* ";
            var fontSprites = new List<Sprite>();
            foreach (char c in allChars)
            {
                Rect r = SkinSlicer.Font.GetCharRect(c);
                if (r != Rect.zero)
                {
                    Sprite s = SkinSlicer.SliceSprite(textTex, r);
                    if (s != null)
                    {
                        s.name = SkinSlicer.Font.GetSpriteName(c);
                        fontSprites.Add(s);
                    }
                }
            }
            skin.TextSprites = fontSprites.ToArray();
        }
    }
}
