using UnityEngine.UI;

namespace SoftAware.Winamp
{
    public class MiscContextMenu : ContextMenuButton
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
    }
}