using UnityEngine.UI;

namespace SoftAware.Winamp
{
    public class AddContextMenu : ContextMenuButton
    {
        internal Button AddUrlButton => MenuButtons[0];
        internal Button AddDirButton => MenuButtons[1];
        internal Button AddFileButton => MenuButtons[2];
    }
}
