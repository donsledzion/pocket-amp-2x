using UnityEngine.UI;

namespace SoftAware.Winamp
{
    public class ListOptionsContextMenu : ContextMenuButton
    {
        internal Button NewListButton => MenuButtons[0];
        internal Button SaveListButton => MenuButtons[1];
        internal Button LoadListButton => MenuButtons[2];
    }
}