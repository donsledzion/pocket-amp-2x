using UnityEngine.UI;

namespace SoftAware.PocketAmp
{
    public class SelectContextMenu : ContextMenuButton, IWinampSkinApplicator
    {
        internal Button InvertSelectionButton => MenuButtons[0];
        internal Button SelectZeroButton => MenuButtons[1];
        internal Button SelectAllButton => MenuButtons[2];
        public System.Action OnInvertSelectionRequested;
        public System.Action OnSelectNoneRequested;
        public System.Action OnSelectAllRequested;

        internal override void SetupBindings()
        {
            InvertSelectionButton.onClick.AddListener(InvertSelection);
            SelectZeroButton.onClick.AddListener(SelectZero);
            SelectAllButton.onClick.AddListener(SelectAll);
        }

        internal override void ClearBindings()
        {
            InvertSelectionButton.onClick.RemoveListener(InvertSelection);
            SelectZeroButton.onClick.RemoveListener(SelectZero);
            SelectAllButton.onClick.RemoveListener(SelectAll);
        }

        private void InvertSelection() => OnInvertSelectionRequested?.Invoke();

        private void SelectZero() => OnSelectNoneRequested?.Invoke();

        private void SelectAll() => OnSelectAllRequested?.Invoke();

        public void ApplySkin(WinampSkin skin)
        {
            if (skin == null) return;
            if (InvertSelectionButton)
            {
                InvertSelectionButton.image.sprite = skin.PlSelectInvNormal;
                var ss = InvertSelectionButton.spriteState;
                ss.pressedSprite = skin.PlSelectInvPressed;
                InvertSelectionButton.spriteState = ss;
            }
            if (SelectZeroButton)
            {
                SelectZeroButton.image.sprite = skin.PlSelectNoneNormal;
                var ss = SelectZeroButton.spriteState;
                ss.pressedSprite = skin.PlSelectNonePressed;
                SelectZeroButton.spriteState = ss;
            }
            if (SelectAllButton)
            {
                SelectAllButton.image.sprite = skin.PlSelectAllNormal;
                var ss = SelectAllButton.spriteState;
                ss.pressedSprite = skin.PlSelectAllPressed;
                SelectAllButton.spriteState = ss;
            }

            if (MenuClipper) MenuClipper.sprite = skin.PlSelectClipper;
        }
    }
}