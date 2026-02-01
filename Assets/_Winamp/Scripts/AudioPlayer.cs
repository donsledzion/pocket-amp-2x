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
            Application.runInBackground = true;
            // Prevent Unity from pausing audio when pulling down the notification bar
            AudioListener.pause = false; 

            BindButtons();
        }

        private void BindButtons()
        {
            panelMain.PrevButton.onClick.AddListener(PlayPrevious);
            panelMain.PlayButton.onClick.AddListener(Play);
            panelMain.PauseButton.onClick.AddListener(Pause);
            panelMain.StopButton.onClick.AddListener(StopPlayback);
            panelMain.NextButton.onClick.AddListener(PlayNext);
            panelMain.EjectButton.onClick.AddListener(PickFolder);
        }

        private void PickFolder()
        {
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
            {
                Debug.LogWarning("Missing currentClip!");
                return;
            }

            audioSource.clip = currentClip;
            audioSource.Play();
            autoPlayNextClipCoroutine = StartCoroutine(PlayNextClipCoroutine());
            UpdateNotification();
        }

        private void PlayNext()
        {
            audioSource.Stop();
            playlist.GetNextClip();
            Play();
        }

        private void PlayPrevious()
        {
            audioSource.Stop();
            playlist.GetPreviousClip();
            Play();
        }

        private void Pause()
        {
            if(audioSource.isPlaying)
                audioSource.Pause();
            else if (audioSource.clip != null)
                audioSource.UnPause();
            
            UpdateNotification();
        }

        public void StopPlayback()
        {
            if(autoPlayNextClipCoroutine != null)
                StopCoroutine(autoPlayNextClipCoroutine);
            audioSource.Stop();
            AndroidMediaBridge.StopService();
        }

        private void UpdateNotification()
        {
            if (currentClip != null)
            {
                AndroidMediaBridge.UpdateMetadata(currentClip.name, "Winamp Android", audioSource.isPlaying);
            }
        }

        // --- Native Callbacks (called from Java via UnitySendMessage) ---
        public void OnNativePlay() { Play(); }
        public void OnNativePause() { Pause(); }
        public void OnNativeNext() { PlayNext(); }
        public void OnNativePrev() { PlayPrevious(); }

        private void OnApplicationQuit()
        {
            AndroidMediaBridge.StopService();
        }
        
        
    }
}
