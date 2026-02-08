using UnityEngine.UI;

namespace SoftAware.Winamp
{
    public class SelectContextMenu : ContextMenuButton
    {
        internal Button InvertSelectionButton => MenuButtons[0];
        internal Button SelectZeroButton => MenuButtons[1];
        internal Button SelectAllButton => MenuButtons[2];
    }
}