using UnityEngine.UI;

namespace SoftAware.Winamp
{
    public class MiscContextMenu : ContextMenuButton
    {
        internal Button SortListButton => MenuButtons[0];
        internal Button FileInfoButton => MenuButtons[1];
        internal Button MiscOptionsButton => MenuButtons[2];
    }
}