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

            skin.Stereo_Active = WinampSkinSlicer.SliceSprite(monosterTex, WinampSkinSlicer.MonoSter.StereoOn);
            skin.Stereo_Inactive = WinampSkinSlicer.SliceSprite(monosterTex, WinampSkinSlicer.MonoSter.StereoOff);
            skin.Mono_Inactive = WinampSkinSlicer.SliceSprite(monosterTex, WinampSkinSlicer.MonoSter.MonoOff);
        }

        public void ComposeVolume(WinampSkin skin, Texture2D volumeTex)
        {
            if (skin == null || volumeTex == null) return;

            float h = volumeTex.height;
            skin.VolumeKnobNormal = WinampSkinSlicer.SliceSprite(volumeTex, new Rect(WinampSkinSlicer.Volume.KnobNormal.x, h - WinampSkinSlicer.Volume.KnobNormal.y - WinampSkinSlicer.Volume.KnobNormal.height, WinampSkinSlicer.Volume.KnobNormal.width, WinampSkinSlicer.Volume.KnobNormal.height));
            skin.VolumeKnobPressed = WinampSkinSlicer.SliceSprite(volumeTex, new Rect(WinampSkinSlicer.Volume.KnobPressed.x, h - WinampSkinSlicer.Volume.KnobPressed.y - WinampSkinSlicer.Volume.KnobPressed.height, WinampSkinSlicer.Volume.KnobPressed.width, WinampSkinSlicer.Volume.KnobPressed.height));
            
            skin.VolumeAnimation = new Sprite[WinampSkinSlicer.Volume.FrameCount];
            for (int i = 0; i < WinampSkinSlicer.Volume.FrameCount; i++)
            {
                float yBottom = 420 - (i * WinampSkinSlicer.Volume.FrameStride);
                float yTop = h - yBottom - WinampSkinSlicer.Volume.FrameHeight;
                skin.VolumeAnimation[i] = WinampSkinSlicer.SliceSprite(volumeTex, new Rect(0, yTop, WinampSkinSlicer.Volume.FrameWidth, WinampSkinSlicer.Volume.FrameHeight));
            }
        }

        public void ComposeBalance(WinampSkin skin, Texture2D balanceTex)
        {
            if (skin == null || balanceTex == null) return;

            float h = balanceTex.height;
            skin.BalanceKnobNormal = WinampSkinSlicer.SliceSprite(balanceTex, new Rect(WinampSkinSlicer.Balance.KnobNormal.x, h - WinampSkinSlicer.Balance.KnobNormal.y - WinampSkinSlicer.Balance.KnobNormal.height, WinampSkinSlicer.Balance.KnobNormal.width, WinampSkinSlicer.Balance.KnobNormal.height));
            skin.BalanceKnobPressed = WinampSkinSlicer.SliceSprite(balanceTex, new Rect(WinampSkinSlicer.Balance.KnobPressed.x, h - WinampSkinSlicer.Balance.KnobPressed.y - WinampSkinSlicer.Balance.KnobPressed.height, WinampSkinSlicer.Balance.KnobPressed.width, WinampSkinSlicer.Balance.KnobPressed.height));
            
            skin.BalanceAnimation = new Sprite[WinampSkinSlicer.Balance.FrameCount];
            for (int i = 0; i < WinampSkinSlicer.Balance.FrameCount; i++)
            {
                float yBottom = 420 - (i * WinampSkinSlicer.Balance.FrameStride);
                float yTop = h - yBottom - WinampSkinSlicer.Balance.FrameHeight;
                skin.BalanceAnimation[i] = WinampSkinSlicer.SliceSprite(balanceTex, new Rect(9, yTop, WinampSkinSlicer.Balance.FrameWidth, WinampSkinSlicer.Balance.FrameHeight));
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
