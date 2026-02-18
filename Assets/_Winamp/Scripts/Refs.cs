using UnityEngine;

namespace SoftAware.PocketAmp
{
    [DefaultExecutionOrder(-100)]
    public class Refs : MonoBehaviour
    {
        [SerializeField] private Main main;
        [SerializeField] private EqualizerController equalizer;
        [SerializeField] private Playlist playlist;
        
        [SerializeField] private SkinManager skinManager;

        internal static Main Main => instance.main;
        internal static EqualizerController EqualizerController => instance.equalizer;
        internal static Playlist Playlist => instance.playlist;

        internal static SkinManager SkinManager => instance.skinManager;
        
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
