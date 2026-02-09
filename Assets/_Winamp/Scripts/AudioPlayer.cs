using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Concurrent;

namespace SoftAware
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
            engine.OnPlaybackFinished += () => mainThreadActions.Enqueue(() => PlayNext(true));
            
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
                OnNativePlay, OnNativePause, OnNativeNext, OnNativePrev
            );
#endif
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
                engine.Seek(panelMain.ProgressSlider.value);
            });
            trigger.triggers.Add(entryUp);
        }


        public void Play()
        {
            if (isPaused) { Resume(); return; }
            if (playCoroutine != null) StopCoroutine(playCoroutine);
            playCoroutine = StartCoroutine(PlayProcess());
        }

        private void Resume() { isPaused = false; engine.Resume(); }

        private IEnumerator PlayProcess()
        {
            isPaused = false;
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

        private void PerformNativePlay(string path, bool immediate)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            engine.Stop();
            ANAMusic.load(path, false, false, (id) => {
                Action setupAction = () => {
                    if (engine is AndroidPlaybackEngine androidEngine)
                    {
                        androidEngine.SetNativeMusicID(id);
                        androidEngine.SetVolume(currentVolume, currentVolume); 
                        Application.runInBackground = true; 
                        androidEngine.Resume(); 
                        OnPlaybackStarted();

                        // Delayed Visualizer Init (only on main thread)
                        if (!immediate) 
                            StartCoroutine(InitVisualizerDelayed(id));
                        else
                            mainThreadActions.Enqueue(() => StartCoroutine(InitVisualizerDelayed(id)));
                    }
                };

                if (immediate) setupAction();
                else mainThreadActions.Enqueue(setupAction);

            }, true, true);
#endif
        }

        private IEnumerator InitVisualizerDelayed(int sessionId)
        {
            yield return new WaitForSeconds(0.5f);
            AndroidVisualizerBridge.Initialize(sessionId);
        }

        private void OnPlaybackStarted()
        {
            UpdateAudioInfo();
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

        private void Pause()
        {
            if (!engine.IsPlaying && !isPaused) return;
            if (isPaused) { Resume(); return; }

            isPaused = true;
            engine.Pause();
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
                AndroidMediaBridge.UpdateMetadata(currentSong.Title, "Winamp Android", engine.IsPlaying);
        }

        // Native Callbacks - called from AndroidMediaBridge (proxy) or direct UnitySendMessage
        public void OnNativePlay() { if (isAppPaused) Resume(); else mainThreadActions.Enqueue(Play); }
        public void OnNativePause() { if (isAppPaused) Pause(); else mainThreadActions.Enqueue(Pause); }
        public void OnNativeNext() { if (isAppPaused) PlayNext(true); else mainThreadActions.Enqueue(() => PlayNext()); }
        public void OnNativePrev() { if (isAppPaused) PlayPrevious(); else mainThreadActions.Enqueue(PlayPrevious); }

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
