using UnityEngine.UI;

namespace SoftAware.Winamp
{
    public class MiscContextMenu : ContextMenuButton
    {
        internal Button SortListButton => MenuButtons[0];
        internal Button FileInfoButton => MenuButtons[1];
        internal Button MiscOptionsButton => MenuButtons[2];
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

        private void SortList() => throw new System.NotImplementedException();

        private void FileInfo() => throw new System.NotImplementedException();

        private void MiscOptions() => throw new System.NotImplementedException();
    }
}