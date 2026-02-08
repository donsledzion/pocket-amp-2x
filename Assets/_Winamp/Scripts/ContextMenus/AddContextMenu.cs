using UnityEngine.UI;

namespace SoftAware.Winamp
{
    public class AddContextMenu : ContextMenuButton
    {
        internal Button AddUrlButton => MenuButtons[0];
        internal Button AddDirButton => MenuButtons[1];
        internal Button AddFileButton => MenuButtons[2];

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

        private void AddUrl() => UnityEngine.Debug.Log("[AddContextMenu] AddUrl clicked");
        private void AddDir() 
        { 
            UnityEngine.Debug.Log($"[AddContextMenu] AddDir clicked. Event listeners: {(OnAddDirRequested != null ? OnAddDirRequested.GetInvocationList().Length : 0)}");
            OnAddDirRequested?.Invoke(); 
        }
        private void AddFile() 
        { 
            UnityEngine.Debug.Log($"[AddContextMenu] AddFile clicked. Event listeners: {(OnAddFileRequested != null ? OnAddFileRequested.GetInvocationList().Length : 0)}");
            OnAddFileRequested?.Invoke(); 
        }
    }
}
