using UnityEngine;
using UnityEngine.UI;

namespace SoftAware.PocketAmp
{
    public class OverlayWindowsController : MonoBehaviour
    {
        [SerializeField] private GraphicRaycaster[] windowsBlockingBackground;
        [SerializeField] private GameObject skinsLibraryWindow; 
        [SerializeField] private GameObject presetsLibraryWindow; 
        [SerializeField] private GameObject miscOptionsMenu; 
        [SerializeField] private GameObject addUrlWindow;
        
        
        internal void OpenMiscOptionsMenu() => SetWindowVisibility(miscOptionsMenu, true);

        internal void CloseMiscOptionsMenu() => SetWindowVisibility(miscOptionsMenu, false);

        internal void OpenAddUrlWindow() => SetWindowVisibility(addUrlWindow, true);
        internal void CloseAddUrlWindow() => SetWindowVisibility(addUrlWindow, false);

        internal void OpenSkinsLibrary() => SetWindowVisibility(skinsLibraryWindow, true);
        internal void CloseSkinsLibrary() => SetWindowVisibility(skinsLibraryWindow, false);

        internal void OpenPresetsLibrary() => SetWindowVisibility(presetsLibraryWindow, true);
        internal void ClosePresetsLibrary() => SetWindowVisibility(presetsLibraryWindow, false);
        

        private void SetOverlayWindowBackground(bool state)
        {
            foreach (var raycaster in windowsBlockingBackground)
                raycaster.enabled = state;
        }

        private void SetWindowVisibility(GameObject window, bool state)
        {
            SetOverlayWindowBackground(state);
            if (state)
                window.transform.localPosition = Vector3.zero;
            window.SetActive(state);
        }
    }
}
