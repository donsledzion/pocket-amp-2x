using UnityEngine.UI;

namespace SoftAware.Winamp
{
    public class RemoveContextMenu : ContextMenuButton
    {
        internal Button RemoveMiscButton => MenuButtons[0];
        internal Button RemoveAllButton => MenuButtons[1];
        internal Button CropButton => MenuButtons[2];
        internal Button RemoveSelectedButton => MenuButtons[3];
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

        private void RemoveMisc() => throw new System.NotImplementedException();

        private void RemoveAll() => throw new System.NotImplementedException();

        private void Crop() => throw new System.NotImplementedException();

        private void RemoveSelected() => throw new System.NotImplementedException();
    }
}