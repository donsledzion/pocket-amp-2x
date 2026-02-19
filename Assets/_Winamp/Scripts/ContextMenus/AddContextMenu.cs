using UnityEngine.UI;

namespace SoftAware.PocketAmp
{
    public class AddContextMenu : ContextMenuButton, ISkinApplicator
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

        private void AddFile() => OnAddFileRequested?.Invoke();
        private void AddDir() => OnAddDirRequested?.Invoke();
        private void AddUrl() => OnAddUrlRequested?.Invoke();

        public void ApplySkin(WinampSkin skin)
        {
            if (skin == null) return;

            if (AddUrlButton)
            {
                AddUrlButton.image.sprite = skin.PlAddUrlNormal;
                var spriteState = AddUrlButton.spriteState;
                spriteState.pressedSprite = skin.PlAddUrlPressed;
                AddUrlButton.spriteState = spriteState;
            }
            if (AddDirButton)
            {
                AddDirButton.image.sprite = skin.PlAddDirNormal;
                var spriteState = AddDirButton.spriteState;
                spriteState.pressedSprite = skin.PlAddDirPressed;
                AddDirButton.spriteState = spriteState;
            }
            if (AddFileButton)
            {
                AddFileButton.image.sprite = skin.PlAddFileNormal;
                var spriteState = AddFileButton.spriteState;
                spriteState.pressedSprite = skin.PlAddFilePressed;
                AddFileButton.spriteState = spriteState;
            }

            if (MenuClipper) MenuClipper.sprite = skin.PlAddClipper;
        }
    }
}
