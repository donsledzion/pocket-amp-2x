using UnityEngine.UI;

namespace SoftAware.Winamp
{
    public class RemoveContextMenu : ContextMenuButton, IWinampSkinApplicator
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

        public void ApplySkin(WinampSkin skin)
        {
            if (skin == null) return;
            if (RemoveMiscButton)
            {
                RemoveMiscButton.image.sprite = skin.PlRemoveOptNormal;
                var ss = RemoveMiscButton.spriteState;
                ss.pressedSprite = skin.PlRemoveOptPressed;
                RemoveMiscButton.spriteState = ss;
            }
            if (RemoveAllButton)
            {
                RemoveAllButton.image.sprite = skin.PlRemoveAllNormal;
                var ss = RemoveAllButton.spriteState;
                ss.pressedSprite = skin.PlRemoveAllPressed;
                RemoveAllButton.spriteState = ss;
            }
            if (CropButton)
            {
                CropButton.image.sprite = skin.PlRemoveCropNormal;
                var ss = CropButton.spriteState;
                ss.pressedSprite = skin.PlRemoveCropPressed;
                CropButton.spriteState = ss;
            }
            if (RemoveSelectedButton)
            {
                RemoveSelectedButton.image.sprite = skin.PlRemoveSelNormal;
                var ss = RemoveSelectedButton.spriteState;
                ss.pressedSprite = skin.PlRemoveSelPressed;
                RemoveSelectedButton.spriteState = ss;
            }
        }
    }
}