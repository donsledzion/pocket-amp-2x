using UnityEngine.UI;

namespace SoftAware.Winamp
{
    public class SelectContextMenu : ContextMenuButton
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
    }
}