using UnityEngine.UI;

namespace SoftAware.Winamp
{
    public class RemoveContextMenu : ContextMenuButton
    {
        internal Button RemoveMiscButton => MenuButtons[0];
        internal Button RemoveAllButton => MenuButtons[1];
        internal Button CropButton => MenuButtons[2];
        internal Button RemoveSelectedButton => MenuButtons[3];
        public System.Action OnMiscRequested;
        public System.Action OnRemoveAllRequested;
        public System.Action OnCropRequested;
        public System.Action OnRemoveSelectedRequested;

        internal override void SetupBindings()
        {
            RemoveMiscButton.onClick.AddListener(RemoveMisc);
            RemoveAllButton.onClick.AddListener(RemoveAll);
            CropButton.onClick.AddListener(Crop);
            RemoveSelectedButton.onClick.AddListener(RemoveSelected);
        }

        internal override void ClearBindings()
        {
            RemoveMiscButton.onClick.RemoveListener(RemoveMisc);
            RemoveAllButton.onClick.RemoveListener(RemoveAll);
            CropButton.onClick.RemoveListener(Crop);
            RemoveSelectedButton.onClick.RemoveListener(RemoveSelected);
        }

        private void RemoveMisc() => OnMiscRequested?.Invoke();

        private void RemoveAll() => OnRemoveAllRequested?.Invoke();

        private void Crop() => OnCropRequested?.Invoke();

        private void RemoveSelected() => OnRemoveSelectedRequested?.Invoke();
    }
}