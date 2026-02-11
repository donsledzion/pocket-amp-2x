using UnityEngine.UI;

namespace SoftAware.Winamp
{
    public class AddContextMenu : ContextMenuButton
    {
        internal Button AddUrlButton => MenuButtons[0];
        internal Button AddDirButton => MenuButtons[1];
        internal Button AddFileButton => MenuButtons[2];

        public System.Action OnAddUrlRequested;
        public System.Action OnAddDirRequested;
        public System.Action OnAddFileRequested;

        internal override void SetupBindings()
        {
            AddUrlButton.onClick.AddListener(AddUrl);
            AddDirButton.onClick.AddListener(AddDir);
            AddFileButton.onClick.AddListener(AddFile);
        }

        internal override void ClearBindings()
        {
            AddUrlButton.onClick.RemoveListener(AddUrl);
            AddDirButton.onClick.RemoveListener(AddDir);
            AddFileButton.onClick.RemoveListener(AddFile);
        }

        private void AddUrl() => OnAddUrlRequested?.Invoke();
        private void AddDir() => OnAddDirRequested?.Invoke();
        private void AddFile() => OnAddFileRequested?.Invoke();
    }
}
