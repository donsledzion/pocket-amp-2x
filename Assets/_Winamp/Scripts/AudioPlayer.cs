using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SimpleFileBrowser;
using System.Collections.Concurrent;

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
        private bool isPaused = false;
        private ConcurrentQueue<Action> mainThreadActions = new ConcurrentQueue<Action>();

        private float currentVolume = 1f;
        private float currentBalance = 0.5f; // 0.0 = Left, 1.0 = Right, 0.5 = Center
        private bool repeatEnabled = false;

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
            
            // Execute Main Thread Actions
            while (mainThreadActions.TryDequeue(out var action))
            {
                action?.Invoke();
            }
        }

        private void UpdateSlider()
        {
            if (panelMain.ProgressSlider == null) return;

            bool isPlaying = false;
            float progress = 0f;
            float currentTime = 0f;
            float totalTime = 0f;

#if UNITY_ANDROID && !UNITY_EDITOR
            if (currentMusicID != -1)
            {
                isPlaying = ANAMusic.isPlaying(currentMusicID);
                int duration = ANAMusic.getDuration(currentMusicID);
                int current = ANAMusic.getCurrentPosition(currentMusicID);
                if (duration > 0) progress = (float)current / duration;

                currentTime = current / 1000f;
                totalTime = duration / 1000f;
            }
#else
            if (audioSource.clip != null)
            {
                isPlaying = audioSource.isPlaying;
                if (audioSource.clip.length > 0) 
                {
                    progress = audioSource.time / audioSource.clip.length;
                    currentTime = audioSource.time;
                    totalTime = audioSource.clip.length;
                }
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

            // Update Time Display
            if (panelMain.TimeDisplay != null)
            {
                if (isPlaying || isPaused)
                {
                    panelMain.TimeDisplay.SetTime(currentTime, totalTime);
                    panelMain.TimeDisplay.SetPaused(isPaused);
                }
                else
                {
                    panelMain.TimeDisplay.Clear();
                }
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

            BindSlider();
            BindVolume();
            BindBalance();
            BindToggles();
        }

        private void BindToggles()
        {
            if (panelMain.ShuffleButton != null)
            {
                panelMain.ShuffleButton.GetComponent<Button>().onClick.AddListener(OnShuffleToggle);
            }
            
            if (panelMain.RepeatButton != null)
            {
                panelMain.RepeatButton.GetComponent<Button>().onClick.AddListener(OnRepeatToggle);
            }
        }

        private void OnShuffleToggle()
        {
            if (panelMain.ShuffleButton != null)
            {
                playlist.SetShuffle(panelMain.ShuffleButton.IsOn);
            }
        }

        private void OnRepeatToggle()
        {
            if (panelMain.RepeatButton != null)
            {
                repeatEnabled = panelMain.RepeatButton.IsOn;
            }
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
            
            isPaused = false;
            
            if (panelMain.StatusDisplay != null)
                panelMain.StatusDisplay.SetStatus(WinampStatusDisplay.WinampStatus.Loading);

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
                    // Enqueue UI/Unity API updates to Main Thread
                    mainThreadActions.Enqueue(() => 
                    {
                        // Set flag for main thread update
                        pendingVisualizerSessionId = id;
                        
                        // Apply current volume to new track
                        ApplyVolumeBalance();
                        
                        // TODO: Native channel detection requires Java implementation
                        // Now using bridge to get actual channels
                        int channels = AndroidAudioInfoBridge.GetChannelCount(currentSong.FilePath);
                        UpdateChannelsDisplay(true, channels);
                        
                        // Update audio info displays
                        UpdateAudioInfo();

                        if (panelMain.StatusDisplay != null)
                            panelMain.StatusDisplay.SetStatus(WinampStatusDisplay.WinampStatus.Playing);
                    });

                    ANAMusic.play(id, (finishedID) => 
                    {
                        // Automatic song progression
                        // This callback might also be on a bg thread, so safe queueing:
                        mainThreadActions.Enqueue(() => PlayNext());
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
                    UpdateChannelsDisplay(true, currentClip.channels);
                    UpdateAudioInfo();
                    
                    if (panelMain.StatusDisplay != null)
                        panelMain.StatusDisplay.SetStatus(WinampStatusDisplay.WinampStatus.Playing);

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
                UpdateChannelsDisplay(true, currentClip.channels);
                UpdateAudioInfo();
                
                if (panelMain.StatusDisplay != null)
                    panelMain.StatusDisplay.SetStatus(WinampStatusDisplay.WinampStatus.Playing);

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
            
            // Check playing status for UI update
            bool currentlyPlaying = false;
#if UNITY_ANDROID && !UNITY_EDITOR
            if (currentMusicID != -1) currentlyPlaying = ANAMusic.isPlaying(currentMusicID);
#else 
            currentlyPlaying = audioSource.isPlaying;
#endif  

            isPaused = !currentlyPlaying;

            // Maintain channel info if just pausing
            int channels = 2; // Default
#if !UNITY_ANDROID || UNITY_EDITOR
             if (currentClip != null) channels = currentClip.channels;
#endif
            UpdateChannelsDisplay(currentlyPlaying, channels);
        }

        private void UpdateChannelsDisplay(bool isPlaying, int channels)
        {
            if (panelMain.ChannelsDisplay != null)
            {
                panelMain.ChannelsDisplay.UpdateDisplay(isPlaying, channels);
            }
        }

        private void UpdateAudioInfo()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            // Android Native Playback - use file path
            if (currentSong != null && currentSong.HasNativePath)
            {
                // Sample Rate (kHz)
                if (panelMain.SampleRateDisplay != null)
                {
                    int sampleRateHz = AndroidAudioInfoBridge.GetSampleRate(currentSong.FilePath);
                    int sampleRateKHz = sampleRateHz / 1000;
                    panelMain.SampleRateDisplay.SetNumber(sampleRateKHz);
                }

                // Bitrate (kbps)
                if (panelMain.BitrateDisplay != null)
                {
                    int bitrateBps = AndroidAudioInfoBridge.GetBitrate(currentSong.FilePath);
                    int bitrateKbps = bitrateBps / 1000;
                    panelMain.BitrateDisplay.SetNumber(bitrateKbps);
                }
                
                return;
            }
#endif

            // Unity AudioClip Playback
            if (currentClip == null) return;

            // Sample Rate (kHz)
            if (panelMain.SampleRateDisplay != null)
            {
                int sampleRateKHz = currentClip.frequency / 1000;
                panelMain.SampleRateDisplay.SetNumber(sampleRateKHz);
            }

            // Bitrate (kbps) - Estimated
            if (panelMain.BitrateDisplay != null)
            {
                // Unity doesn't provide bitrate directly
                // Estimate: (samples * channels * bits_per_sample) / duration / 1000
                // Assuming 16-bit audio
                int estimatedBitrate = EstimateBitrate(currentClip);
                panelMain.BitrateDisplay.SetNumber(estimatedBitrate);
            }
        }

        private int EstimateBitrate(AudioClip clip)
        {
            if (clip == null || clip.length == 0) return 0;

            // Estimate based on uncompressed PCM data
            // (sample_rate * channels * bits_per_sample) / 1000
            int uncompressedBitrate = (clip.frequency * clip.channels * 16) / 1000;

            // For compressed formats (MP3, OGG), assume ~10-20% of uncompressed
            // This is a rough estimate - actual bitrate varies
            int estimatedBitrate = uncompressedBitrate / 8; // ~12.5% compression ratio

            // Clamp to reasonable values (32-320 kbps for MP3)
            return Mathf.Clamp(estimatedBitrate, 32, 320);
        }

        public void StopPlayback()
        {
            if(playCoroutine != null) StopCoroutine(playCoroutine);
            if(autoPlayNextClipCoroutine != null)
                StopCoroutine(autoPlayNextClipCoroutine);
            
            StopNativeIfRunning();
            audioSource.Stop();
            AndroidMediaBridge.StopService();
            UpdateChannelsDisplay(false, 0);
            
            // Clear audio info displays
            if (panelMain.BitrateDisplay != null)
                panelMain.BitrateDisplay.Clear();
            if (panelMain.SampleRateDisplay != null)
                panelMain.SampleRateDisplay.Clear();
            
            if (panelMain.TimeDisplay != null)
                panelMain.TimeDisplay.SetPaused(false);
            
            if (panelMain.StatusDisplay != null)
                panelMain.StatusDisplay.SetStatus(WinampStatusDisplay.WinampStatus.Stop);

            isPaused = false;
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
