using UnityEngine.UI;

namespace SoftAware.Winamp
{
    public class RemoveContextMenu : ContextMenuButton
    {
        internal Button RemoveMiscButton => MenuButtons[0];
        internal Button RemoveAllButton => MenuButtons[1];
        internal Button CropButton => MenuButtons[2];
        internal Button RemoveSelectedButton => MenuButtons[3];
    }
}