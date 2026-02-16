using UnityEngine.UI;

namespace SoftAware.Winamp
{
    public class MiscContextMenu : ContextMenuButton, IWinampSkinApplicator
    {
        internal Button SortListButton => MenuButtons[0];
        internal Button FileInfoButton => MenuButtons[1];
        internal Button MiscOptionsButton => MenuButtons[2];
        
        public System.Action OnSortListButtonClicked;
        public System.Action OnFileInfoButtonClicked;
        public System.Action OnMiscOptionsButtonClicked;
        
        internal override void SetupBindings()
        {
            SortListButton.onClick.AddListener(SortList);   
            FileInfoButton.onClick.AddListener(FileInfo);   
            MiscOptionsButton.onClick.AddListener(MiscOptions);   
        }

        internal override void ClearBindings()
        {
            SortListButton.onClick.RemoveListener(SortList);   
            FileInfoButton.onClick.RemoveListener(FileInfo);   
            MiscOptionsButton.onClick.RemoveListener(MiscOptions);
        }

        private void SortList() => OnSortListButtonClicked?.Invoke();

        private void FileInfo() => OnFileInfoButtonClicked?.Invoke();

        private void MiscOptions() => OnMiscOptionsButtonClicked.Invoke();

        public void ApplySkin(WinampSkin skin)
        {
            if (skin == null) return;
            if (SortListButton)
            {
                SortListButton.image.sprite = skin.PlSortNormal;
                var ss = SortListButton.spriteState;
                ss.pressedSprite = skin.PlSortPressed;
                SortListButton.spriteState = ss;
            }
            if (FileInfoButton)
            {
                FileInfoButton.image.sprite = skin.PlFileInfoNormal;
                var ss = FileInfoButton.spriteState;
                ss.pressedSprite = skin.PlFileInfoPressed;
                FileInfoButton.spriteState = ss;
            }
            if (MiscOptionsButton)
            {
                MiscOptionsButton.image.sprite = skin.PlMiscNormal;
                var ss = MiscOptionsButton.spriteState;
                ss.pressedSprite = skin.PlMiscPressed;
                MiscOptionsButton.spriteState = ss;
            }

            if (MenuClipper) MenuClipper.sprite = skin.PlMiscClipper;
        }
    }
}