using UnityEngine;

namespace SoftAware
{
    public class WinampSkinLoader : MonoBehaviour
    {
        [Header("Playlist Skin Colors (from PLEDIT.TXT)")]
        [SerializeField] private Color normalColor = new Color(0.51f, 0.91f, 0.21f); // #83e736
        [SerializeField] private Color currentColor = Color.white;                 // #FFFFFF
        [SerializeField] private Color normalBGColor = Color.black;                // #000000
        [SerializeField] private Color selectedBGColor = new Color(0, 0, 0.78f);   // #0000c6
        
        public static WinampSkinLoader Instance { get; private set; }

        public Color PlaylistNormalColor => normalColor;
        public Color PlaylistCurrentColor => currentColor;
        public Color PlaylistNormalBGColor => normalBGColor;
        public Color PlaylistSelectedBGColor => selectedBGColor;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // Keep as a public stub to avoid breaking WinampUIController
        public void LoadPlaylistSkin() { }
    }
}
