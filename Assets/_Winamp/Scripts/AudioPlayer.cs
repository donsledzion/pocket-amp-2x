using UnityEngine;

namespace SoftAware
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AudioSource))]
    public class AudioPlayer : MonoBehaviour
    {
        [SerializeField] private Playlist playlist;
        private AudioSource audioSource;

        private void Awake()
        {
            if (!TryGetComponent(out audioSource))
                throw new($"Missing AudioSource component on {gameObject.name}");
        }

        public void Play(AudioClip clip)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }

        public void Pause()
        {
            if(!audioSource.isPlaying) return;
            audioSource.Pause();
        }

        public void Unpause()
        {
            if (audioSource.isPlaying) return;
            audioSource.UnPause();
        }

        public void Stop()
        {
            if(!audioSource.isPlaying) return;
            audioSource.Stop();
        }
        
        
    }
}
