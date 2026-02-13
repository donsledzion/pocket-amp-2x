using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Concurrent;

namespace SoftAware.Winamp
{
    /// <summary>
    /// The main playback manager. Handles playlist logic, high-level states,
    /// and delegates actual playback to an IPlaybackEngine and UI to a WinampUIController.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AudioSource))]
    public class AudioPlayer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Playlist playlist;
        [SerializeField] private Main panelMain;
        [SerializeField] private WinampUIController uiController;
        [SerializeField] private EqualizerController eqController;

        private AudioSource audioSource;
        private IPlaybackEngine engine;
        
        public float CurrentTime => engine != null ? engine.CurrentTime : 0;
        public float Duration => engine != null ? engine.Duration : 0;
        
        private Playlist.SongInfo currentSong => playlist.CurrentSong;
        private bool isPaused = false;
        public bool IsPaused => isPaused;
        public bool IsPlaying => engine != null && engine.IsPlaying;

        // Coroutines for track management
        private Coroutine playCoroutine;
        
        // State tracking
        private bool isDraggingSlider = false;
        private ConcurrentQueue<Action> mainThreadActions = new ConcurrentQueue<Action>();

        // Audio parameters
        private float currentVolume = 1f;
        public float CurrentVolume => currentVolume;
        private float currentBalance = 0.5f; 
        public float CurrentBalance => currentBalance;
        private bool repeatEnabled = false;
        private bool isAppPaused = false;

        private void Awake()
        {
            if (!TryGetComponent(out audioSource))
                throw new($"Missing AudioSource component on {gameObject.name}");
        }

        private void Start()
        {
            Application.runInBackground = true;
            AudioListener.pause = false; 

            // Initialize the correct engine based on platform
#if UNITY_ANDROID && !UNITY_EDITOR
            engine = new AndroidPlaybackEngine();
#else
            engine = new UnityPlaybackEngine(audioSource);
#endif
            engine.OnPlaybackFinished += HandlePlaybackFinished;
            
            // Link UI Controller
            if (uiController != null) uiController.Initialize(this);
            
            RegisterBackgroundCallbacks();
            BindButtons();

            if (playlist != null)
            {
                playlist.OnPlaylistReady += HandlePlaylistReady;
            }
        }

        private void HandlePlaylistReady()
        {
            if (SettingsManager.Instance != null && SettingsManager.Instance.IsFirstRun)
            {
                Debug.Log("AudioPlayer: First run detected. Auto-playing demo track.");
                SettingsManager.Instance.IsFirstRun = false; // Clear the flag
                Play();
            }
        }

        private void RegisterBackgroundCallbacks()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            AndroidMediaBridge.RegisterRemoteControlListener(
                OnNativePlay, OnNativePause, OnNativeNext, OnNativePrev, OnNativeSeek
            );
#endif
        }

        private void HandlePlaybackFinished()
        {
            if (isAppPaused)
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                PlayNextBackground();
#else
                mainThreadActions.Enqueue(() => PlayNext(true));
#endif
            }
            else
            {
                mainThreadActions.Enqueue(() => PlayNext(true));
            }
        }

        private void OnApplicationPause(bool pause)
        {
            isAppPaused = pause;
            if (!pause) UpdateNotification(); // Refresh on resume
        }

        private void Update()
        {
            // Execute any actions queued from background threads (like Android callbacks)
            while (mainThreadActions.TryDequeue(out var action))
            {
                action?.Invoke();
            }

            // Update playback engine (for debounce timers, etc.)
            engine?.Update();

            // Centralized UI Update
            if (uiController && engine != null)
            {
                uiController.UpdateUI(engine.CurrentTime, engine.Duration, engine.IsPlaying, isPaused);
            }
        }

        private void BindButtons()
        {
            panelMain.PrevButton.onClick.AddListener(PlayPrevious);
            panelMain.PlayButton.onClick.AddListener(Play);
            panelMain.PauseButton.onClick.AddListener(Pause);
            panelMain.StopButton.onClick.AddListener(StopPlayback);
            panelMain.NextButton.onClick.AddListener(() => PlayNext());
            panelMain.EjectButton.onClick.AddListener(playlist.PickFolder);

            BindSlider();
            BindVolume();
            BindBalance();
            BindToggles();
            BindEqualizer();
        }

        private void BindEqualizer()
        {
            if (eqController != null)
            {
                eqController.OnValuesChanged += ApplyEqualizer;
                // Initial apply
                ApplyEqualizer();
            }
        }

        private void ApplyEqualizer()
        {
            if (engine == null || eqController == null) return;
            
            engine.SetEqualizerEnabled(eqController.IsOn);
            engine.SetEqualizerGains(eqController.PreampValue, eqController.GetBandGains());
        }

        private void BindToggles()
        {
            if (panelMain.ShuffleButton != null)
            {
                panelMain.ShuffleButton.OnValueChanged.AddListener((isOn) => {
                    playlist.SetShuffle(isOn);
                });
                // Initial sync
                playlist.SetShuffle(panelMain.ShuffleButton.IsOn);
            }
            
            if (panelMain.RepeatButton != null)
            {
                panelMain.RepeatButton.OnValueChanged.AddListener((isOn) => {
                    repeatEnabled = isOn;
                });
                // Initial sync
                repeatEnabled = panelMain.RepeatButton.IsOn;
            }
        }

        private void BindVolume() => panelMain.VolumeController?.Slider.onValueChanged.AddListener(SetVolume);
        private void BindBalance() => panelMain.BalanceController?.Slider.onValueChanged.AddListener(SetBalance);

        private void SetVolume(float volume) { currentVolume = volume; ApplyVolumeBalance(); }
        private void SetBalance(float balance) { currentBalance = balance; ApplyVolumeBalance(); }

        private void ApplyVolumeBalance()
        {
            float left = currentVolume;
            float right = currentVolume;

            if (currentBalance < 0.5f) right *= currentBalance * 2f;
            else if (currentBalance > 0.5f) left *= (1f - currentBalance) * 2f;

            engine?.SetVolume(left, right);
        }

        private void BindSlider()
        {
            if (panelMain.ProgressSlider == null) return;
            EventTrigger trigger = panelMain.ProgressSlider.gameObject.GetComponent<EventTrigger>() ?? panelMain.ProgressSlider.gameObject.AddComponent<EventTrigger>();

            var entryDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            entryDown.callback.AddListener((data) => { isDraggingSlider = true; uiController?.SetDragging(true); });
            trigger.triggers.Add(entryDown);

            var entryUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            entryUp.callback.AddListener((data) => { 
                isDraggingSlider = false; 
                uiController?.SetDragging(false);
                engine.Seek(panelMain.ProgressSlider.value, true);
                UpdateNotification(); // Sync notification after manual seek
            });
            trigger.triggers.Add(entryUp);

            panelMain.ProgressSlider.onValueChanged.AddListener((val) => {
                if (isDraggingSlider)
                {
                    uiController?.HandleSliderDrag(val, engine.Duration);
                }
            });
        }


        private Playlist.SongInfo lastPlayedSong;

        public void Play()
        {
            // If we are paused AND it's the same song, just resume.
            // If it's a DIFFERENT song, we must start a fresh playback process.
            if (isPaused && currentSong == lastPlayedSong && lastPlayedSong != null) 
            { 
                Resume(); 
                return; 
            }

            if (playCoroutine != null) StopCoroutine(playCoroutine);
            playCoroutine = StartCoroutine(PlayProcess());
        }

        private void Resume() 
        { 
            isPaused = false; 
            engine.Resume(); 
            UpdateNotification(); // Request Focus
        }

        private IEnumerator PlayProcess()
        {
            isPaused = false;
            lastPlayedSong = currentSong;
            uiController?.ShowLoading();

            if (currentSong == null)
            {
                Debug.LogWarning("AudioPlayer: No song selected!");
                yield break;
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            // Android Native Load and Play
            if (currentSong.HasNativePath)
            {
                PerformNativePlay(currentSong.FilePath, isAppPaused);
            }
#else
            // Standard Unity Playback
            if (currentSong.Clip == null && currentSong.HasNativePath)
                yield return playlist.LoadSongClip(currentSong);

            if (currentSong.Clip != null)
            {
                if (currentSong.Duration <= 0) currentSong.Duration = currentSong.Clip.length;
                engine.Play(currentSong);
                OnPlaybackStarted();
            }
#endif
            UpdateNotification();
        }

        private int currentLoadingTicket = 0;

        /// <summary>
        /// Orchestrates native playback. 
        /// Executes native commands (Stop/Load) immediately for responsiveness in background,
        /// but uses 'loadingTicket' to ensure only the last requested track actually plays.
        /// </summary>
        private void PerformNativePlay(string path, bool isHeadless)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            int myTicket = ++currentLoadingTicket;

            // 1. Immediate Native Stop: Prevents multiple songs from playing at once.
            // Safe to call from background thread now that ANAMusic has locks.
            engine.Stop();

            // 2. Immediate Native Load: Starts loading right away even in background.
            ANAMusic.load(path, false, false, (id) => {
                
                // --- Background Thread (Native Callback) ---
                
                // 3. Stacking Prevention: If a newer ticket exists, this request is obsolete.
                if (myTicket != currentLoadingTicket)
                {
                    ANAMusic.release(id); // Discard the orphaned player immediately
                    return;
                }

                if (engine is AndroidPlaybackEngine androidEngine)
                {
                    // 4. Immediate Native Resume: Start audio NOW.
                    androidEngine.SetNativeMusicID(id);
                    androidEngine.SetVolume(currentVolume, currentVolume); 
                    androidEngine.Resume(); 

                    // 5. Defer Unity/UI updates to the main thread.
                    mainThreadActions.Enqueue(() => {
                        // Double check ticket on main thread too before UI sync
                        if (myTicket != currentLoadingTicket) return;

                        OnPlaybackStarted();
                        AndroidVisualizerBridge.Initialize(id);
                    });
                }
            }, true, true);
#endif
        }

        private void OnPlaybackStarted()
        {
            UpdateAudioInfo();
            UpdateNotification(); // Ensure we notify service that we are playing to trigger Audio Focus
        }


        public void PlayNext(bool forcePlay = false) 
        { 
            // If this is an automatic transition (track finished)
            // and repeat is off, and we are at the last track, stop.
            if (forcePlay && !repeatEnabled && !playlist.IsShuffleEnabled && playlist.CurrentIndex == playlist.Count - 1)
            {
                StopPlayback();
                return;
            }

            SwitchSong(playlist.GetNextSong(), forcePlay); 
        }
        public void PlayPrevious() { SwitchSong(playlist.GetPreviousSong()); }

        private void SwitchSong(Playlist.SongInfo next, bool forcePlay = false)
        {
            bool wasActive = engine.IsPlaying || isPaused || forcePlay;
            StopPlaybackInternal();
            if (wasActive) Play();
            else UpdateAudioInfo();
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private void PlayNextBackground()
        {
            // Headless transition: Calculate next and start audio immediately without touching UI.
            // This runs on the Android Background Thread (Native Callback).
            
            int nextIndex = playlist.GetNextSongIndex();
            if (nextIndex == -1) return;

            bool isLast = (playlist.CurrentIndex == playlist.Count - 1);
            if (!repeatEnabled && !playlist.IsShuffleEnabled && isLast)
            {
                // In background, we stop immediately to avoid "Stacking" if focused later
                engine.Stop();
                mainThreadActions.Enqueue(() => StopPlayback());
                return;
            }

            // Update Playlist index silently (state change only, no events)
            playlist.SetCurrentIndexSilent(nextIndex);
            Playlist.SongInfo nextSong = playlist.AllSongs[nextIndex];

            if (nextSong.HasNativePath)
            {
                PerformNativePlay(nextSong.FilePath, true); 
                
                // Update notification IMMEDIATELY so the user sees the new title in the tray.
                // This is safe because AndroidMediaBridge handles the JNI call.
                int durationMs = (int)(AndroidAudioInfoBridge.GetDuration(nextSong.FilePath) * 1000);
                AndroidMediaBridge.UpdateMetadata(nextSong.Title, "Winamp Android", durationMs, 0, true);
            }
        }

        private void PlayPreviousBackground()
        {
            int prevIndex = playlist.GetPreviousSongIndex();
            if (prevIndex == -1) return;

            playlist.SetCurrentIndexSilent(prevIndex);
            Playlist.SongInfo prevSong = playlist.AllSongs[prevIndex];

            if (prevSong.HasNativePath)
            {
                PerformNativePlay(prevSong.FilePath, true);
                
                int durationMs = (int)(AndroidAudioInfoBridge.GetDuration(prevSong.FilePath) * 1000);
                AndroidMediaBridge.UpdateMetadata(prevSong.Title, "Winamp Android", durationMs, 0, true);
            }
        }
#endif

        private void Pause()
        {
            if (!engine.IsPlaying && !isPaused) return;
            if (isPaused) { Resume(); return; }

            isPaused = true;
            engine.Pause();
            UpdateNotification(); // Abandon Focus / Update UI
        }

        public void StopPlayback() { StopPlaybackInternal(); }

        private void StopPlaybackInternal()
        {
            if (playCoroutine != null) StopCoroutine(playCoroutine);
            
            engine.Stop();
            isPaused = false;
            
            uiController?.ClearSongInfo();
            uiController?.UpdateMetadata(0, 0, 0, false);
            UpdateNotification();
        }

        private void UpdateAudioInfo()
        {
            if (currentSong == null) { uiController?.ClearSongInfo(); return; }

            // Ensure playlist UI (highlights) stays in sync, 
            // especially after background transitions.
            playlist.UI_SyncIndex();

            int bitrateK = 0;
            int sampleRateK = 0;

#if UNITY_ANDROID && !UNITY_EDITOR
            sampleRateK = AndroidAudioInfoBridge.GetSampleRate(currentSong.FilePath) / 1000;
            bitrateK = AndroidAudioInfoBridge.GetBitrate(currentSong.FilePath) / 1000;
#else
            if (currentSong.Clip != null)
            {
                sampleRateK = currentSong.Clip.frequency / 1000;
                bitrateK = AudioMetadataUtils.EstimateBitrate(currentSong.Clip);
            }
#endif
            uiController?.UpdateSongInfo(playlist.CurrentIndex1Based, currentSong.Title, engine.Duration);
            uiController?.UpdateMetadata(bitrateK, sampleRateK, 2, true);
        }

        private void UpdateNotification()
        {
            if (currentSong != null)
            {
                int durationMs = (int)(engine.Duration * 1000);
                int positionMs = (int)(engine.CurrentTime * 1000);
                AndroidMediaBridge.UpdateMetadata(currentSong.Title, "Winamp Android", durationMs, positionMs, engine.IsPlaying);
            }
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        // Native Callbacks - called from AndroidMediaBridge (via JNI proxy)
        // --------------------------------------------------------------------------
        // IMPORTANT: These methods are called from a Java background thread.
        // DO NOT call Unity APIs or modify UI components directly here.
        // We use isAppPaused to determine if we should switch to "Background" logic
        // which only interacts with the native Android engine components.
        // --------------------------------------------------------------------------

        public void OnNativePlay() { if (isAppPaused) Resume(); else mainThreadActions.Enqueue(Play); }
        public void OnNativePause() { if (isAppPaused) Pause(); else mainThreadActions.Enqueue(Pause); }
        public void OnNativeNext() { if (isAppPaused) PlayNextBackground(); else mainThreadActions.Enqueue(() => PlayNext()); }
        public void OnNativePrev() { if (isAppPaused) PlayPreviousBackground(); else mainThreadActions.Enqueue(PlayPrevious); }
        public void OnNativeSeek(string positionMsStr)
        {
            if (long.TryParse(positionMsStr, out long positionMs))
            {
                OnNativeSeek(positionMs);
            }
        }

        public void OnNativeSeek(long positionMs)
        {
            float positionSec = positionMs / 1000f;
            
            // 1. IMMEDIATE Native Action: Change playback position NOW.
            // This is safe to call from background thread and ensures responsiveness.
            engine.Seek(positionSec, false);
            
            // 2. ENQUEUED UI Action: Update sliders and text.
            // Unity UI (MarkDirty) is NEVER thread-safe, even when app is paused.
            mainThreadActions.Enqueue(() => {
                // Update UI slider if possible
                if (panelMain.ProgressSlider != null)
                {
                    float duration = engine.Duration;
                    if (duration > 0)
                        panelMain.ProgressSlider.value = Mathf.Clamp01(positionSec / duration);
                }
                
                uiController?.UpdateUI(engine.CurrentTime, engine.Duration, engine.IsPlaying, isPaused);
                UpdateNotification(); // Confirm the seek to notification
            });
        }
#endif

        private void OnDestroy()
        {
            if (playlist != null)
            {
                playlist.OnPlaylistReady -= HandlePlaylistReady;
            }
        }

        private void OnApplicationQuit() => AndroidMediaBridge.StopService();
    }
}
