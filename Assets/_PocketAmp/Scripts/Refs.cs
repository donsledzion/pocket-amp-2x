using UnityEngine;

namespace SoftAware.PocketAmp
{
    [DefaultExecutionOrder(-100)]
    public class Refs : MonoBehaviour
    {
        [SerializeField] private Main main;
        [SerializeField] private EqualizerController equalizer;
        [SerializeField] private Playlist playlist;
        [SerializeField] private PlaylistUI playlistUi;
        
        [SerializeField] private UIController uIController;
        [SerializeField] private SkinManager skinManager;
        [SerializeField] private AudioPlayer audioPlayer;

        internal static Main Main => instance.main;
        internal static EqualizerController EqualizerController => instance.equalizer;
        internal static Playlist Playlist => instance.playlist;
        internal static PlaylistUI PlaylistUI => instance.playlistUi;

        internal static SkinManager SkinManager => instance.skinManager;
        internal static AudioPlayer AudioPlayer => instance.audioPlayer;
        internal static UIController UIController => instance.uIController;
        
        private static Refs instance { get; set; }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
