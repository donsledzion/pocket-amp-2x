using System;
using System.Collections;
using UnityEngine;
using SimpleFileBrowser;

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
        private Coroutine autoPlayNextClipCoroutine;

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
            panelMain.EjectButton.onClick.AddListener(PickFolder);
        }

        private void PickFolder()
        {
            // SimpleFileBrowser supports folder selection and works on Windows/Android
            FileBrowser.ShowLoadDialog((paths) =>
            {
                if (paths != null && paths.Length > 0)
                {
                    Debug.Log("Picked folder: " + paths[0]);
                    playlist.AddDirectory(paths[0]);
                }
            }, 
            null, 
            FileBrowser.PickMode.Folders, 
            false, 
            null, 
            null, 
            "Select Audio Folder", 
            "Select");
        }

        private IEnumerator PlayNextClipCoroutine()
        {
            yield return new WaitUntil(() => audioSource.isPlaying);
            yield return new WaitUntil(() => !audioSource.isPlaying);
            PlayNext();
        }

        private void Play()
        {
            if(autoPlayNextClipCoroutine != null)
                StopCoroutine(autoPlayNextClipCoroutine);
            if(!currentClip)
                Debug.LogWarning("Missing currentClip!");
            audioSource.clip = currentClip;
            audioSource.Play();
            autoPlayNextClipCoroutine = StartCoroutine(PlayNextClipCoroutine());
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
            if(autoPlayNextClipCoroutine != null)
                StopCoroutine(autoPlayNextClipCoroutine);
            audioSource.Stop();
        }
        
        
    }
}
