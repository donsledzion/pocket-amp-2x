using UnityEngine.UI;

namespace SoftAware.Winamp
{
    public class ListOptionsContextMenu : ContextMenuButton
    {
        internal Button NewListButton => MenuButtons[0];
        internal Button SaveListButton => MenuButtons[1];
        internal Button LoadListButton => MenuButtons[2];

        internal override void SetupBindings()
        {
            NewListButton.onClick.AddListener(NewList);
            SaveListButton.onClick.AddListener(SaveList);
            LoadListButton.onClick.AddListener(LoadList);
        }

        internal override void ClearBindings()
        {
            NewListButton.onClick.RemoveListener(NewList);
            SaveListButton.onClick.RemoveListener(SaveList);
            LoadListButton.onClick.RemoveListener(LoadList);
        }

        private void NewList() => throw new System.NotImplementedException();

        private void SaveList() => throw new System.NotImplementedException();

        private void LoadList() => throw new System.NotImplementedException();
    }
}