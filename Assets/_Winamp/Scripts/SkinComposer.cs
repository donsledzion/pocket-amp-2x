using UnityEngine;
using System.Collections.Generic;

namespace SoftAware
{
    /// <summary>
    /// Responsible for slicing textures into sprites and populating the WinampSkin object.
    /// Moves slicing logic out of Importer/Manager.
    /// </summary>
    public class SkinComposer
    {
        public void ComposeMain(WinampSkin skin, Texture2D mainTex)
        {
            if (skin == null || mainTex == null) return;
            
            skin.MainBackground = WinampSkinSlicer.SliceSprite(mainTex, WinampSkinSlicer.MainPanel);
            
            // Standard TitleBar components from MAIN.BMP
            skin.TitleBar = WinampSkinSlicer.SliceSprite(mainTex, WinampSkinSlicer.TitleBar);
            skin.MinimizeBtn_Normal = WinampSkinSlicer.SliceSprite(mainTex, WinampSkinSlicer.MinimizeButton);
            skin.MinimizeBtn_Pressed = WinampSkinSlicer.SliceSprite(mainTex, WinampSkinSlicer.MinimizeButtonPressed);
            skin.CloseBtn_Normal = WinampSkinSlicer.SliceSprite(mainTex, WinampSkinSlicer.CloseButton);
            skin.CloseBtn_Pressed = WinampSkinSlicer.SliceSprite(mainTex, WinampSkinSlicer.CloseButtonPressed);
        }

        public void ComposeTitleBar(WinampSkin skin, Texture2D titleBarTex)
        {
            if (skin == null || titleBarTex == null) return;

            // Overrides Main TitleBar with TITLEBAR.BMP if present
            skin.TitleBar = WinampSkinSlicer.SliceSpriteBottomUp(titleBarTex, WinampSkinSlicer.TitleBarSeparate.Focused);
            skin.MinimizeBtn_Normal = WinampSkinSlicer.SliceSpriteBottomUp(titleBarTex, WinampSkinSlicer.TitleBarSeparate.MinimizeNormal);
            skin.MinimizeBtn_Pressed = WinampSkinSlicer.SliceSpriteBottomUp(titleBarTex, WinampSkinSlicer.TitleBarSeparate.MinimizePressed);
            skin.CloseBtn_Normal = WinampSkinSlicer.SliceSpriteBottomUp(titleBarTex, WinampSkinSlicer.TitleBarSeparate.CloseNormal);
            skin.CloseBtn_Pressed = WinampSkinSlicer.SliceSpriteBottomUp(titleBarTex, WinampSkinSlicer.TitleBarSeparate.ClosePressed);
        }

        public void ComposeCButtons(WinampSkin skin, Texture2D cbuttonsTex)
        {
            if (skin == null || cbuttonsTex == null) return;

            skin.PlayBtn_Normal = WinampSkinSlicer.SliceSprite(cbuttonsTex, WinampSkinSlicer.CButtons.Play);
            skin.PlayBtn_Pressed = WinampSkinSlicer.SliceSprite(cbuttonsTex, WinampSkinSlicer.CButtons.PlayPressed);
            skin.PauseBtn_Normal = WinampSkinSlicer.SliceSprite(cbuttonsTex, WinampSkinSlicer.CButtons.Pause);
            skin.PauseBtn_Pressed = WinampSkinSlicer.SliceSprite(cbuttonsTex, WinampSkinSlicer.CButtons.PausePressed);
            skin.StopBtn_Normal = WinampSkinSlicer.SliceSprite(cbuttonsTex, WinampSkinSlicer.CButtons.Stop);
            skin.StopBtn_Pressed = WinampSkinSlicer.SliceSprite(cbuttonsTex, WinampSkinSlicer.CButtons.StopPressed);
            skin.PrevBtn_Normal = WinampSkinSlicer.SliceSprite(cbuttonsTex, WinampSkinSlicer.CButtons.Previous);
            skin.PrevBtn_Pressed = WinampSkinSlicer.SliceSprite(cbuttonsTex, WinampSkinSlicer.CButtons.PreviousPressed);
            skin.NextBtn_Normal = WinampSkinSlicer.SliceSprite(cbuttonsTex, WinampSkinSlicer.CButtons.Next);
            skin.NextBtn_Pressed = WinampSkinSlicer.SliceSprite(cbuttonsTex, WinampSkinSlicer.CButtons.NextPressed);
            skin.EjectBtn_Normal = WinampSkinSlicer.SliceSprite(cbuttonsTex, WinampSkinSlicer.CButtons.Eject);
            skin.EjectBtn_Pressed = WinampSkinSlicer.SliceSprite(cbuttonsTex, WinampSkinSlicer.CButtons.EjectPressed);
        }

        public void ComposeShufRep(WinampSkin skin, Texture2D shufrepTex, Texture2D fallbackMainTex = null)
        {
            if (skin == null) return;

            if (shufrepTex != null)
            {
                skin.Shuffle_Off_Normal = WinampSkinSlicer.SliceSprite(shufrepTex, WinampSkinSlicer.ShufRep.ShuffleOffNormal);
                skin.Shuffle_Off_Pressed = WinampSkinSlicer.SliceSprite(shufrepTex, WinampSkinSlicer.ShufRep.ShuffleOffPressed);
                skin.Shuffle_On_Normal = WinampSkinSlicer.SliceSprite(shufrepTex, WinampSkinSlicer.ShufRep.ShuffleOnNormal);
                skin.Shuffle_On_Pressed = WinampSkinSlicer.SliceSprite(shufrepTex, WinampSkinSlicer.ShufRep.ShuffleOnPressed);

                skin.Repeat_Off_Normal = WinampSkinSlicer.SliceSprite(shufrepTex, WinampSkinSlicer.ShufRep.RepeatOffNormal);
                skin.Repeat_Off_Pressed = WinampSkinSlicer.SliceSprite(shufrepTex, WinampSkinSlicer.ShufRep.RepeatOffPressed);
                skin.Repeat_On_Normal = WinampSkinSlicer.SliceSprite(shufrepTex, WinampSkinSlicer.ShufRep.RepeatOnNormal);
                skin.Repeat_On_Pressed = WinampSkinSlicer.SliceSprite(shufrepTex, WinampSkinSlicer.ShufRep.RepeatOnPressed);

                skin.EQ_Off_Normal = WinampSkinSlicer.SliceSprite(shufrepTex, WinampSkinSlicer.ShufRep.EqOffNormal);
                skin.EQ_Off_Pressed = WinampSkinSlicer.SliceSprite(shufrepTex, WinampSkinSlicer.ShufRep.EqOffPressed);
                skin.EQ_On_Normal = WinampSkinSlicer.SliceSprite(shufrepTex, WinampSkinSlicer.ShufRep.EqOnNormal);
                skin.EQ_On_Pressed = WinampSkinSlicer.SliceSprite(shufrepTex, WinampSkinSlicer.ShufRep.EqOnPressed);

                skin.PL_Off_Normal = WinampSkinSlicer.SliceSprite(shufrepTex, WinampSkinSlicer.ShufRep.PlOffNormal);
                skin.PL_Off_Pressed = WinampSkinSlicer.SliceSprite(shufrepTex, WinampSkinSlicer.ShufRep.PlOffPressed);
                skin.PL_On_Normal = WinampSkinSlicer.SliceSprite(shufrepTex, WinampSkinSlicer.ShufRep.PlOnNormal);
                skin.PL_On_Pressed = WinampSkinSlicer.SliceSprite(shufrepTex, WinampSkinSlicer.ShufRep.PlOnPressed);
            }
            else if (fallbackMainTex != null)
            {
                // Fallback to MAIN.BMP
                skin.Shuffle_Off_Normal = WinampSkinSlicer.SliceSprite(fallbackMainTex, WinampSkinSlicer.ShuffleButtonOff);
                skin.Shuffle_Off_Pressed = WinampSkinSlicer.SliceSprite(fallbackMainTex, WinampSkinSlicer.ShuffleButtonOffPressed);
                skin.Shuffle_On_Normal = WinampSkinSlicer.SliceSprite(fallbackMainTex, WinampSkinSlicer.ShuffleButtonOn);
                skin.Shuffle_On_Pressed = WinampSkinSlicer.SliceSprite(fallbackMainTex, WinampSkinSlicer.ShuffleButtonOnPressed);

                skin.Repeat_Off_Normal = WinampSkinSlicer.SliceSprite(fallbackMainTex, WinampSkinSlicer.RepeatButtonOff);
                skin.Repeat_Off_Pressed = WinampSkinSlicer.SliceSprite(fallbackMainTex, WinampSkinSlicer.RepeatButtonOffPressed);
                skin.Repeat_On_Normal = WinampSkinSlicer.SliceSprite(fallbackMainTex, WinampSkinSlicer.RepeatButtonOn);
                skin.Repeat_On_Pressed = WinampSkinSlicer.SliceSprite(fallbackMainTex, WinampSkinSlicer.RepeatButtonOnPressed);

                skin.EQ_Off_Normal = WinampSkinSlicer.SliceSprite(fallbackMainTex, WinampSkinSlicer.EqualizerButton); 
                skin.EQ_On_Normal = skin.EQ_Off_Normal; 
                skin.PL_Off_Normal = WinampSkinSlicer.SliceSprite(fallbackMainTex, WinampSkinSlicer.PlaylistButton);
                skin.PL_On_Normal = skin.PL_Off_Normal;
            }
        }

        public void ComposeEqualizer(WinampSkin skin, Texture2D eqMainTex)
        {
            if (skin == null || eqMainTex == null) return;

            // Background & Title
            skin.EqBackground = WinampSkinSlicer.SliceSprite(eqMainTex, WinampSkinSlicer.Equalizer.Background);
            skin.EqTitleBar = WinampSkinSlicer.SliceSprite(eqMainTex, WinampSkinSlicer.Equalizer.TitleBar);
            
            // Close button
            skin.EqCloseNormal = WinampSkinSlicer.SliceSprite(eqMainTex, WinampSkinSlicer.Equalizer.CloseNormal);
            skin.EqClosePressed = WinampSkinSlicer.SliceSprite(eqMainTex, WinampSkinSlicer.Equalizer.ClosePressed);
            
            // Toggles (On/Auto)
            skin.EqOn_Off_Normal = WinampSkinSlicer.SliceSprite(eqMainTex, WinampSkinSlicer.Equalizer.On_Off_Normal);
            skin.EqOn_On_Normal = WinampSkinSlicer.SliceSprite(eqMainTex, WinampSkinSlicer.Equalizer.On_On_Normal);
            skin.EqOn_Off_Pressed = WinampSkinSlicer.SliceSprite(eqMainTex, WinampSkinSlicer.Equalizer.On_Off_Pressed);
            skin.EqOn_On_Pressed = WinampSkinSlicer.SliceSprite(eqMainTex, WinampSkinSlicer.Equalizer.On_On_Pressed);
            
            skin.EqAuto_Off_Normal = WinampSkinSlicer.SliceSprite(eqMainTex, WinampSkinSlicer.Equalizer.Auto_Off_Normal);
            skin.EqAuto_On_Normal = WinampSkinSlicer.SliceSprite(eqMainTex, WinampSkinSlicer.Equalizer.Auto_On_Normal);
            skin.EqAuto_Off_Pressed = WinampSkinSlicer.SliceSprite(eqMainTex, WinampSkinSlicer.Equalizer.Auto_Off_Pressed);
            skin.EqAuto_On_Pressed = WinampSkinSlicer.SliceSprite(eqMainTex, WinampSkinSlicer.Equalizer.Auto_On_Pressed);
            
            // Presets
            skin.EqPresetsNormal = WinampSkinSlicer.SliceSprite(eqMainTex, WinampSkinSlicer.Equalizer.PresetsNormal);
            skin.EqPresetsPressed = WinampSkinSlicer.SliceSprite(eqMainTex, WinampSkinSlicer.Equalizer.PresetsPressed);
            
            // Knob
            skin.EqKnobNormal = WinampSkinSlicer.SliceSprite(eqMainTex, WinampSkinSlicer.Equalizer.KnobNormal);
            skin.EqKnobPressed = WinampSkinSlicer.SliceSprite(eqMainTex, WinampSkinSlicer.Equalizer.KnobPressed);
            
            // Slider Backgrounds
            var sliderFames = new List<Sprite>();
            Rect first = WinampSkinSlicer.Equalizer.SliderFirstFrame;
            for (int row = 0; row < 2; row++)
            {
                for (int col = 0; col < 14; col++)
                {
                    Rect frameRect = new Rect(
                        first.x + (col * 15), 
                        first.y + (row * 65), 
                        first.width, 
                        first.height);
                    sliderFames.Add(WinampSkinSlicer.SliceSprite(eqMainTex, frameRect));
                }
            }
            skin.EqSliderBackgrounds = sliderFames.ToArray();
            
            // Graph Elements
            skin.EqGraphBackground = WinampSkinSlicer.SliceSprite(eqMainTex, WinampSkinSlicer.Equalizer.GraphBG);
            skin.EqGraphColors = WinampSkinSlicer.SliceSprite(eqMainTex, WinampSkinSlicer.Equalizer.GraphColors);
            skin.EqGraphPreampLine = WinampSkinSlicer.SliceSprite(eqMainTex, WinampSkinSlicer.Equalizer.PreampLine);
        }

        public void ComposePlaylist(WinampSkin skin, Texture2D plEditTex)
        {
            if (skin == null || plEditTex == null) return;

             // Borders & Title
            skin.PlTopLeft = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.TopLeft);
            skin.PlTopTitle = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.TopTitle);
            skin.PlTopStretch = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.TopStretch);
            skin.PlTopLeftStretch = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.TopLeftStretch);
            skin.PlTopRightStretch = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.TopRightStretch);
            skin.PlTopRight = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.TopRight);
            
            skin.PlBottomLeft = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.BottomLeft);
            skin.PlBottomRight = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.BottomRight);
            skin.PlBottomStretch = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.BottomStretch);
            
            skin.PlLeftEdge = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.LeftEdge);
            skin.PlRightEdge = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.RightEdge);

            // Buttons Add
            skin.PlAddUrlNormal = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.AddUrlNormal);
            skin.PlAddUrlPressed = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.AddUrlPressed);
            skin.PlAddDirNormal = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.AddDirNormal);
            skin.PlAddDirPressed = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.AddDirPressed);
            skin.PlAddFileNormal = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.AddFileNormal);
            skin.PlAddFilePressed = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.AddFilePressed);

            // Buttons Remove
            skin.PlRemoveAllNormal = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.RemAllNormal);
            skin.PlRemoveAllPressed = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.RemAllPressed);
            skin.PlRemoveSelNormal = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.RemSelNormal);
            skin.PlRemoveSelPressed = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.RemSelPressed);
            skin.PlRemoveCropNormal = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.RemCropNormal);
            skin.PlRemoveCropPressed = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.RemCropPressed);
            skin.PlRemoveOptNormal = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.RemMiscNormal);
            skin.PlRemoveOptPressed = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.RemMiscPressed);

            // Buttons Select
            skin.PlSelectAllNormal = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.SelAllNormal);
            skin.PlSelectAllPressed = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.SelAllPressed);
            skin.PlSelectNoneNormal = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.SelNoneNormal);
            skin.PlSelectNonePressed = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.SelNonePressed);
            skin.PlSelectInvNormal = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.SelInvNormal);
            skin.PlSelectInvPressed = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.SelInvPressed);

            // Buttons Sort/Misc
            skin.PlSortNormal = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.SortNormal);
            skin.PlSortPressed = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.SortPressed);
            skin.PlFileInfoNormal = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.FileInfoNormal);
            skin.PlFileInfoPressed = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.FileInfoPressed);
            skin.PlMiscNormal = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.MiscNormal);
            skin.PlMiscPressed = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.MiscPressed);

            // Buttons List
            skin.PlNewListNormal = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.NewListNormal);
            skin.PlNewListPressed = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.NewListPressed);
            skin.PlSaveListNormal = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.SaveListNormal);
            skin.PlSaveListPressed = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.SaveListPressed);
            skin.PlLoadListNormal = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.LoadListNormal);
            skin.PlLoadListPressed = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.LoadListPressed);

            // Scrollbar
            skin.PlScrollHandleNormal = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.SliderHandleNormal);
            skin.PlScrollHandlePressed = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.SliderHandlePressed);

            // Close Button
            skin.PlCloseNormal = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.CloseNormal);
            skin.PlClosePressed = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.ClosePressed);

            // Clippers
            skin.PlAddClipper = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.AddClipper);
            skin.PlRemoveClipper = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.RemoveClipper);
            skin.PlSelectClipper = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.SelectClipper);
            skin.PlMiscClipper = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.MiscClipper);
            skin.PlListOptionsClipper = WinampSkinSlicer.SliceSprite(plEditTex, WinampSkinSlicer.Playlist.ListOptionsClipper);
        }

        public void ComposePosBar(WinampSkin skin, Texture2D posbarTex, Texture2D mainTex)
        {
            Texture2D source = posbarTex != null ? posbarTex : mainTex;
            if (skin == null || source == null) return;

            skin.PosKnobNormal = WinampSkinSlicer.SliceSprite(source, new Rect(248, 0, 29, 10));
            skin.PosKnobPressed = WinampSkinSlicer.SliceSprite(source, new Rect(278, 0, 29, 10));
        }

        public void ComposeMonoSter(WinampSkin skin, Texture2D monosterTex)
        {
            if (skin == null || monosterTex == null) return;

            int w = monosterTex.width;
            
            // Standard Winamp MonoSter is 29px width per item (total 58px)
            // But some skins (and sometimes default exports) are 56px (28px each) or 57px.
            // We calculate width dynamically to prevent out-of-bounds errors.
            int halfW = w / 2;
            int rightX = halfW;
            
            // Should be 29 if width is 58+.
            // If width is 56, halfW is 28.
            
            // Use Min to ensure we don't exceed 29 if texture is huge for some reason, 
            // though usually we just want to split it.
            // Actually, best to just split whatever we have.
            
            skin.Stereo_Active = WinampSkinSlicer.SliceSprite(monosterTex, new Rect(0, 0, halfW, 12));
            skin.Stereo_Inactive = WinampSkinSlicer.SliceSprite(monosterTex, new Rect(0, 12, halfW, 12));
            skin.Mono_Inactive = WinampSkinSlicer.SliceSprite(monosterTex, new Rect(rightX, 12, w - rightX, 12));
            
            // Note: Mono_Active (Rect(rightX, 0, ...)) seems unused in current skin def?
            // If we needed it: skin.Mono_Active = ... new Rect(rightX, 0, w - rightX, 12));
        }

        public void ComposeVolume(WinampSkin skin, Texture2D volumeTex)
        {
            if (skin == null || volumeTex == null) return;

            float h = volumeTex.height;
            // Standard Volume height is ~420px + knobs (11px) = ~431px+
            // If height is small (e.g. just slider frames ~420), we likely have no knobs.
            // Knobs are usually at Y=0 (bottom) or Y=Height (top) depending on format, 
            // but standard Winamp Volume.bmp has slider frames from top, and knobs at the very bottom?
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
               skin.VolumeKnobNormal = WinampSkinSlicer.SliceSprite(volumeTex, new Rect(WinampSkinSlicer.Volume.KnobNormal.x, h - WinampSkinSlicer.Volume.KnobNormal.y - WinampSkinSlicer.Volume.KnobNormal.height, WinampSkinSlicer.Volume.KnobNormal.width, WinampSkinSlicer.Volume.KnobNormal.height));
               skin.VolumeKnobPressed = WinampSkinSlicer.SliceSprite(volumeTex, new Rect(WinampSkinSlicer.Volume.KnobPressed.x, h - WinampSkinSlicer.Volume.KnobPressed.y - WinampSkinSlicer.Volume.KnobPressed.height, WinampSkinSlicer.Volume.KnobPressed.width, WinampSkinSlicer.Volume.KnobPressed.height));
            }
            else
            {
                // Create transparent sprites for missing knobs
                skin.VolumeKnobNormal = WinampSkinSlicer.CreateTransparentSprite();
                skin.VolumeKnobPressed = WinampSkinSlicer.CreateTransparentSprite();
            }

            
            skin.VolumeAnimation = new Sprite[WinampSkinSlicer.Volume.FrameCount];
            for (int i = 0; i < WinampSkinSlicer.Volume.FrameCount; i++)
            {
                // Standard logic: Frames are 15px high (stride), 13px actual visual? 
                // WinampSkinSlicer.Volume.FrameStride = 15.
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
                
                float topY = i * WinampSkinSlicer.Volume.FrameStride;
                
                // Robust check: valid only if within texture
                if (topY + WinampSkinSlicer.Volume.FrameHeight <= h)
                {
                     skin.VolumeAnimation[i] = WinampSkinSlicer.SliceSprite(volumeTex, new Rect(0, topY, WinampSkinSlicer.Volume.FrameWidth, WinampSkinSlicer.Volume.FrameHeight));
                }
                else
                {
                     // Fallback if texture is too short for all frames, repeat last valid or transparent?
                     // Usually shouldn't happen if h >= 420.
                     // If h < 420, we might be in "Single Frame" territory?
                     if (i > 0 && skin.VolumeAnimation[i-1] != null)
                        skin.VolumeAnimation[i] = skin.VolumeAnimation[i-1];
                     else
                        skin.VolumeAnimation[i] = WinampSkinSlicer.CreateTransparentSprite();
                }
            }
        }

        public void ComposeBalance(WinampSkin skin, Texture2D balanceTex)
        {
            if (skin == null || balanceTex == null) return;

            float h = balanceTex.height;
            
            // Check for Single Frame Case
            // Standard Balance has 28 frames * 15px = 420px.
            // If height is small (e.g. < 50px), it's likely a single frame to be repeated.
            bool isSingleFrame = h < 100; // Heuristic
            
            skin.BalanceAnimation = new Sprite[WinampSkinSlicer.Balance.FrameCount];

            if (isSingleFrame)
            {
                // Slice the only frame we have (assuming it's at 0,0 or similar)
                // Use a safe rect based on texture size
                Rect singleRect = new Rect(9, 0, WinampSkinSlicer.Balance.FrameWidth, Mathf.Min(WinampSkinSlicer.Balance.FrameHeight, h));
                
                // Often in single frame skins, the graphic might not be exactly at X=9 if it's a "compact" style?
                // But let's stick to standard X=9 if possible, or X=0 if texture is narrow?
                if (balanceTex.width < 15) singleRect.x = 0; // If texture is just the slider knob width
                
                Sprite singleSprite = WinampSkinSlicer.SliceSprite(balanceTex, singleRect);
                
                for (int i = 0; i < WinampSkinSlicer.Balance.FrameCount; i++)
                {
                    skin.BalanceAnimation[i] = singleSprite;
                }
                
                // Knobs for single frame usually don't exist
                skin.BalanceKnobNormal = WinampSkinSlicer.CreateTransparentSprite();
                skin.BalanceKnobPressed = WinampSkinSlicer.CreateTransparentSprite();
            }
            else
            {
                // Standard Multi-Frame Logic
                
                // Check for knobs (Standard height > 422?)
                bool hasKnobs = h >= 422;
                
                if (hasKnobs)
                {
                    skin.BalanceKnobNormal = WinampSkinSlicer.SliceSprite(balanceTex, new Rect(WinampSkinSlicer.Balance.KnobNormal.x, h - WinampSkinSlicer.Balance.KnobNormal.y - WinampSkinSlicer.Balance.KnobNormal.height, WinampSkinSlicer.Balance.KnobNormal.width, WinampSkinSlicer.Balance.KnobNormal.height));
                    skin.BalanceKnobPressed = WinampSkinSlicer.SliceSprite(balanceTex, new Rect(WinampSkinSlicer.Balance.KnobPressed.x, h - WinampSkinSlicer.Balance.KnobPressed.y - WinampSkinSlicer.Balance.KnobPressed.height, WinampSkinSlicer.Balance.KnobPressed.width, WinampSkinSlicer.Balance.KnobPressed.height));
                }
                else
                {
                    skin.BalanceKnobNormal = WinampSkinSlicer.CreateTransparentSprite();
                    skin.BalanceKnobPressed = WinampSkinSlicer.CreateTransparentSprite();
                }

                for (int i = 0; i < WinampSkinSlicer.Balance.FrameCount; i++)
                {
                    float topY = i * WinampSkinSlicer.Balance.FrameStride;
                    
                    if (topY + WinampSkinSlicer.Balance.FrameHeight <= h)
                    {
                        skin.BalanceAnimation[i] = WinampSkinSlicer.SliceSprite(balanceTex, new Rect(9, topY, WinampSkinSlicer.Balance.FrameWidth, WinampSkinSlicer.Balance.FrameHeight));
                    }
                    else
                    {
                        if (i > 0 && skin.BalanceAnimation[i-1] != null)
                            skin.BalanceAnimation[i] = skin.BalanceAnimation[i-1];
                        else
                             skin.BalanceAnimation[i] = WinampSkinSlicer.CreateTransparentSprite();
                    }
                }
            }
        }

        public void ComposePlayPaus(WinampSkin skin, Texture2D playpausTex)
        {
            if (skin == null || playpausTex == null) return;

            skin.Status_Play = WinampSkinSlicer.SliceSprite(playpausTex, WinampSkinSlicer.PlayPaus.PlayIcon);
            skin.Status_Pause = WinampSkinSlicer.SliceSprite(playpausTex, WinampSkinSlicer.PlayPaus.PauseIcon);
            skin.Status_Stop = WinampSkinSlicer.SliceSprite(playpausTex, WinampSkinSlicer.PlayPaus.StopIcon);
            skin.Status_Indicator_Play = WinampSkinSlicer.SliceSprite(playpausTex, WinampSkinSlicer.PlayPaus.PlayingIndicator);
            skin.Status_Indicator_Load = WinampSkinSlicer.SliceSprite(playpausTex, WinampSkinSlicer.PlayPaus.LoadingIndicator);
        }

        public void ComposeNumbers(WinampSkin skin, Texture2D numbersTex, Texture2D numsExTex)
        {
            if (skin == null) return;

            bool numbersFound = false;
            if (numbersTex != null)
            {
                skin.TimeDigits = new Sprite[10];
                for (int i = 0; i < 10; i++) skin.TimeDigits[i] = WinampSkinSlicer.SliceSprite(numbersTex, WinampSkinSlicer.Numbers.GetDigitRect(i));
                numbersFound = true;
            }

            if (numsExTex != null)
            {
                if (!numbersFound && numsExTex.width >= 90)
                {
                    skin.TimeDigits = new Sprite[10];
                    for (int i = 0; i < 10; i++) skin.TimeDigits[i] = WinampSkinSlicer.SliceSprite(numsExTex, WinampSkinSlicer.Numbers.GetDigitRect(i));
                }
                skin.TimeMinus = WinampSkinSlicer.SliceSprite(numsExTex, WinampSkinSlicer.NumsEx.MinusSign);
            }
        }

        public void ComposeText(WinampSkin skin, Texture2D textTex)
        {
            if (skin == null || textTex == null) return;

            string allChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ\"@0123456789.:()-'!_+\\/[]^&%,=$#?* ";
            var fontSprites = new List<Sprite>();
            foreach (char c in allChars)
            {
                Rect r = WinampSkinSlicer.Font.GetCharRect(c);
                if (r != Rect.zero)
                {
                    Sprite s = WinampSkinSlicer.SliceSprite(textTex, r);
                    if (s != null)
                    {
                        s.name = WinampSkinSlicer.Font.GetSpriteName(c);
                        fontSprites.Add(s);
                    }
                }
            }
            skin.TextSprites = fontSprites.ToArray();
        }
    }
}
