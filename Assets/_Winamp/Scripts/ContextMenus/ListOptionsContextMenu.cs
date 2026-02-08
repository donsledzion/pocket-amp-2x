using UnityEngine.UI;

namespace SoftAware.Winamp
{
    public class ListOptionsContextMenu : ContextMenuButton
    {
        internal Button NewListButton => MenuButtons[0];
        internal Button SaveListButton => MenuButtons[1];
        internal Button LoadListButton => MenuButtons[2];

        public System.Action OnNewListRequested;
        public System.Action OnSaveListRequested;
        public System.Action OnLoadListRequested;

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

        private void NewList() => OnNewListRequested?.Invoke();

        private void SaveList() => OnSaveListRequested?.Invoke();

        private void LoadList() => OnLoadListRequested?.Invoke();
    }
}