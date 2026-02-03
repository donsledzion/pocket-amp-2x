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
        private Playlist.SongInfo currentSong => playlist.CurrentSong;
        private AudioClip currentClip => currentSong?.Clip;
        private Coroutine autoPlayNextClipCoroutine;
        private Coroutine playCoroutine;
        
        private int currentMusicID = -1;
        private int pendingVisualizerSessionId = -1;

        private void Awake()
        {
            if (!TryGetComponent(out audioSource))
                throw new($"Missing AudioSource component on {gameObject.name}");
        }

        private void Start()
        {
            Application.runInBackground = true;
            AudioListener.pause = false; 

            BindButtons();
        }

        private void Update()
        {
            // Process visualizer initialization on the main thread
            if (pendingVisualizerSessionId != -1)
            {
                StartCoroutine(InitVisualizerDelayed(pendingVisualizerSessionId));
                pendingVisualizerSessionId = -1;
            }
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
            // This coroutine is now primarily for non-Android platforms
            // On Android, we use ANAMusic completion callback
#if UNITY_ANDROID && !UNITY_EDITOR
            yield break;
#else
            yield return new WaitUntil(() => audioSource.isPlaying);
            yield return new WaitUntil(() => !audioSource.isPlaying);
            PlayNext();
#endif
        }

        private void Play()
        {
            if (playCoroutine != null) StopCoroutine(playCoroutine);
            playCoroutine = StartCoroutine(PlayProcess());
        }

        private IEnumerator PlayProcess()
        {
            if(autoPlayNextClipCoroutine != null)
                StopCoroutine(autoPlayNextClipCoroutine);
            
            if(currentSong == null)
            {
                Debug.LogWarning("Missing currentSong!");
                yield break;
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            // Native Android Playback only if we have a file path
            if (currentSong.HasNativePath)
            {
                if (currentMusicID != -1)
                {
                    ANAMusic.release(currentMusicID);
                    currentMusicID = -1;
                }

                // Load file using absolute path (direct access)
                currentMusicID = ANAMusic.load(currentSong.FilePath, false, false, (id) => 
                {
                    // Set flag for main thread update
                    pendingVisualizerSessionId = id;

                    ANAMusic.play(id, (finishedID) => 
                    {
                        // Automatic song progression
                        PlayNext();
                    });
                }, true, true); // playInBackground = true, isAbsolutePath = true
            }
            else
            {
                Debug.LogWarning($"Song {currentSong.Title} has no native path! Falling back to AudioSource. (Background play may be limited)");
                
                if (currentClip == null && currentSong.HasNativePath)
                    yield return playlist.LoadSongClip(currentSong);

                if (currentClip != null)
                {
                    audioSource.clip = currentClip;
                    audioSource.Play();
                    autoPlayNextClipCoroutine = StartCoroutine(PlayNextClipCoroutine());
                }
            }
#else
            // Standard Unity Playback
            if (currentClip == null && currentSong.HasNativePath)
                yield return playlist.LoadSongClip(currentSong);

            if (currentClip != null)
            {
                audioSource.clip = currentClip;
                audioSource.Play();
                autoPlayNextClipCoroutine = StartCoroutine(PlayNextClipCoroutine());
            }
#endif
            UpdateNotification();
        }

        private void PlayNext()
        {
            StopNativeIfRunning();
            audioSource.Stop();
            playlist.GetNextSong();
            Play();
        }

        private void PlayPrevious()
        {
            StopNativeIfRunning();
            audioSource.Stop();
            playlist.GetPreviousSong();
            Play();
        }

        private void Pause()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (currentMusicID != -1)
            {
                if (ANAMusic.isPlaying(currentMusicID))
                    ANAMusic.pause(currentMusicID);
                else
                    ANAMusic.play(currentMusicID);
            }
            else if (audioSource.clip != null)
            {
                // Fallback for non-native clips
                if (audioSource.isPlaying) audioSource.Pause();
                else audioSource.UnPause();
            }
#else
            if(audioSource.isPlaying)
                audioSource.Pause();
            else if (audioSource.clip != null)
                audioSource.UnPause();
#endif
            
            UpdateNotification();
        }

        public void StopPlayback()
        {
            if(playCoroutine != null) StopCoroutine(playCoroutine);
            if(autoPlayNextClipCoroutine != null)
                StopCoroutine(autoPlayNextClipCoroutine);
            
            StopNativeIfRunning();
            audioSource.Stop();
            AndroidMediaBridge.StopService();
        }

        private void StopNativeIfRunning()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (currentMusicID != -1)
            {
                ANAMusic.release(currentMusicID);
                currentMusicID = -1;
            }
#endif
        }

        private void UpdateNotification()
        {
            if (currentSong != null)
            {
                bool isPlaying = false;
#if UNITY_ANDROID && !UNITY_EDITOR
                if (currentMusicID != -1) isPlaying = ANAMusic.isPlaying(currentMusicID);
#else
                isPlaying = audioSource.isPlaying;
#endif
                AndroidMediaBridge.UpdateMetadata(currentSong.Title, "Winamp Android", isPlaying);
            }
        }

        // --- Native Callbacks (called from Java via UnitySendMessage) ---
        public void OnNativePlay() { Play(); }
        public void OnNativePause() { Pause(); }
        public void OnNativeNext() { PlayNext(); }
        public void OnNativePrev() { PlayPrevious(); }

        private IEnumerator InitVisualizerDelayed(int sessionId)
        {
            // Give Android a moment to activate the audio session
            yield return new WaitForSeconds(0.5f);
            
            // For debugging: Force TestMode to true to see if UI can even render bars
            // AndroidVisualizerBridge.TestMode = true; 
            
            AndroidVisualizerBridge.Initialize(sessionId);
        }

        private void OnApplicationQuit()
        {
            AndroidMediaBridge.StopService();
        }
        
        
    }
}
