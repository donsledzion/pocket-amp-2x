using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
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
        private bool isDraggingSlider = false;

        private float currentVolume = 1f;
        private float currentBalance = 0.5f; // 0.0 = Left, 1.0 = Right, 0.5 = Center

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

            UpdateSlider();
        }

        private void UpdateSlider()
        {
            if (panelMain.ProgressSlider == null) return;

            bool isPlaying = false;
            float progress = 0f;

#if UNITY_ANDROID && !UNITY_EDITOR
            if (currentMusicID != -1)
            {
                if (ANAMusic.isPlaying(currentMusicID))
                {
                    isPlaying = true;
                    int duration = ANAMusic.getDuration(currentMusicID);
                    int current = ANAMusic.getCurrentPosition(currentMusicID);
                    if (duration > 0) progress = (float)current / duration;
                }
            }
#else
            if (audioSource.clip != null && audioSource.isPlaying)
            {
                isPlaying = true;
                if (audioSource.clip.length > 0) 
                    progress = audioSource.time / audioSource.clip.length;
            }
#endif

            // Handle Knob Visibility
            if (panelMain.ProgressSlider.handleRect != null)
                panelMain.ProgressSlider.handleRect.gameObject.SetActive(isPlaying || isDraggingSlider);

            // Update Value (only if not dragging)
            if (isPlaying && !isDraggingSlider)
            {
                panelMain.ProgressSlider.value = progress;
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

            panelMain.EjectButton.onClick.AddListener(PickFolder);

            BindSlider();
            BindVolume();
            BindBalance();
        }

        private void BindVolume()
        {
            if (panelMain.VolumeController != null && panelMain.VolumeController.Slider != null)
            {
                panelMain.VolumeController.Slider.onValueChanged.AddListener(SetVolume);
                // Initialize volume
                SetVolume(panelMain.VolumeController.Slider.value);
            }
        }

        private void BindBalance()
        {
            if (panelMain.BalanceController != null && panelMain.BalanceController.Slider != null)
            {
                panelMain.BalanceController.Slider.onValueChanged.AddListener(SetBalance);
                // Initialize balance
                SetBalance(panelMain.BalanceController.Slider.value);
            }
        }

        private void SetVolume(float volume)
        {
            currentVolume = volume;
            ApplyVolumeBalance();
        }

        private void SetBalance(float balance)
        {
            currentBalance = balance;
            ApplyVolumeBalance();
        }

        private void ApplyVolumeBalance()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
             // Native Volume Control (Left/Right)
             if (currentMusicID != -1)
             {
                 float left = currentVolume;
                 float right = currentVolume;

                 // Balance logic:
                 // 0.5 is Center (Left=Vol, Right=Vol)
                 // < 0.5 (Left side): Right fades out. Left stays at Vol.
                 // > 0.5 (Right side): Left fades out. Right stays at Vol.
                 
                 if (currentBalance < 0.5f)
                 {
                     // Fading out Right
                     // 0.0 -> Right=0
                     // 0.5 -> Right=1 (multiplier)
                     float multiplier = currentBalance * 2f;
                     right *= multiplier;
                 }
                 else if (currentBalance > 0.5f)
                 {
                     // Fading out Left
                     // 0.5 -> Left=1 (multiplier)
                     // 1.0 -> Left=0
                     float multiplier = (1f - currentBalance) * 2f;
                     left *= multiplier;
                 }

                 ANAMusic.setVolume(currentMusicID, left, right);
             }
#else
            // Unity Volume Control + Pan
            if (audioSource != null)
            {
                audioSource.volume = currentVolume;
                // Map 0..1 to -1..1
                // 0 -> -1
                // 0.5 -> 0
                // 1 -> 1
                float pan = (currentBalance - 0.5f) * 2f;
                audioSource.panStereo = pan;
            }
#endif
        }

        private void BindSlider()
        {
            if (panelMain.ProgressSlider == null) return;

            // Add EventTrigger if not present
            EventTrigger trigger = panelMain.ProgressSlider.gameObject.GetComponent<EventTrigger>();
            if (trigger == null) trigger = panelMain.ProgressSlider.gameObject.AddComponent<EventTrigger>();

            // Pointer Down
            EventTrigger.Entry entryDown = new EventTrigger.Entry();
            entryDown.eventID = EventTriggerType.PointerDown;
            entryDown.callback.AddListener((data) => { OnSliderDragStart(); });
            trigger.triggers.Add(entryDown);

            // Pointer Up
            EventTrigger.Entry entryUp = new EventTrigger.Entry();
            entryUp.eventID = EventTriggerType.PointerUp;
            entryUp.callback.AddListener((data) => { OnSliderDragEnd(); });
            trigger.triggers.Add(entryUp);
        }

        private void OnSliderDragStart()
        {
            isDraggingSlider = true;
        }

        private void OnSliderDragEnd()
        {
            isDraggingSlider = false;
            if (panelMain.ProgressSlider != null)
                SeekTo(panelMain.ProgressSlider.value);
        }

        private void SeekTo(float normalizedTime)
        {
            if (currentSong == null) return;
            normalizedTime = Mathf.Clamp01(normalizedTime);

#if UNITY_ANDROID && !UNITY_EDITOR
            if (currentMusicID != -1)
            {
                int duration = ANAMusic.getDuration(currentMusicID);
                ANAMusic.seekTo(currentMusicID, (int)(duration * normalizedTime));
            }
#else
            if (audioSource.clip != null)
            {
                audioSource.time = audioSource.clip.length * normalizedTime;
            }
#endif
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
                    
                    // Apply current volume to new track
                    ApplyVolumeBalance();

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
