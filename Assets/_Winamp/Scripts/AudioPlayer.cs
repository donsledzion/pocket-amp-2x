using System;
using UnityEngine;

namespace SoftAware
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AudioSource))]
    public class AudioPlayer : MonoBehaviour
    {
        [SerializeField] private Playlist playlist;
        [SerializeField] private Main panelMain;
        private AudioSource audioSource;
        private AudioClip currentClip => playlist.CurrentClip;

        private void Awake()
        {
            if (!TryGetComponent(out audioSource))
                throw new($"Missing AudioSource component on {gameObject.name}");
        }

        private void Start()
        {
            BindButtons();
        }

        private void BindButtons()
        {
            panelMain.PrevButton.onClick.AddListener(PlayPrevious);
            panelMain.PlayButton.onClick.AddListener(Play);
            panelMain.PauseButton.onClick.AddListener(Pause);
            panelMain.StopButton.onClick.AddListener(Stop);
            panelMain.NextButton.onClick.AddListener(PlayNext);
        }

        private void Play()
        {
            if(!currentClip)
                Debug.LogWarning("Missing currentClip!");
            audioSource.clip = currentClip;
            audioSource.Play();
        }

        private void PlayNext()
        {
            Stop();
            playlist.GetNextClip();
            Play();
        }

        private void PlayPrevious()
        {
            Stop();
            playlist.GetPreviousClip();
            Play();
        }

        private void Pause()
        {
            if(!audioSource.isPlaying)
                audioSource.UnPause();
            audioSource.Pause();
        }

        private void Unpause()
        {
            if (audioSource.isPlaying) return;
            audioSource.UnPause();
        }

        private void Stop()
        {
            if(!audioSource.isPlaying) return;
            audioSource.Stop();
        }
        
        
    }
}
