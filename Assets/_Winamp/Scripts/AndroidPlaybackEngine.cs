using UnityEngine;

namespace SoftAware.Winamp
{
    public class AndroidPlaybackEngine : IPlaybackEngine
    {
        public event System.Action OnPlaybackFinished;
        private int currentMusicID = -1;

        public bool IsPlaying => currentMusicID != -1 && ANAMusic.isPlaying(currentMusicID);
        public float CurrentTime => currentMusicID != -1 ? ANAMusic.getCurrentPosition(currentMusicID) / 1000f : 0f;
        public float Duration => currentMusicID != -1 ? ANAMusic.getDuration(currentMusicID) / 1000f : 0f;
        public int AudioSessionId => currentMusicID;

        public void Play(Playlist.SongInfo song)
        {
            // Note: On Android, AudioPlayer still handles the load logic for now
            // since it involves native path translation and ANAMusic.load()
        }

        public void SetNativeMusicID(int musicID)
        {
            if (currentMusicID != musicID)
            {
                ReleaseEffects();
                currentMusicID = musicID;
                ApplyFinalVolume(); // Ensure the new session gets the current volume/balance immediately
                UpdateNativeEQ(true); // Re-apply current settings to new session
            }
        }

        private void ReleaseEffects()
        {
            if (nativeEq != null)
            {
                try { nativeEq.Call("release"); } catch { }
                nativeEq = null;
            }
            if (nativeLoudness != null)
            {
                try { nativeLoudness.Call("release"); } catch { }
                nativeLoudness = null;
            }
        }

        public void Pause()
        {
            if (currentMusicID != -1) ANAMusic.pause(currentMusicID);
        }

        public void Resume()
        {
            if (currentMusicID != -1)
            {
                ANAMusic.play(currentMusicID, (id) => {
                    OnPlaybackFinished?.Invoke();
                });
            }
        }

        public void Stop()
        {
            if (currentMusicID != -1)
            {
                ANAMusic.release(currentMusicID);
                currentMusicID = -1;
            }
        }

        public void Seek(float time, bool isNormalized = true)
        {
            if (currentMusicID != -1)
            {
                if (isNormalized)
                {
                    int duration = ANAMusic.getDuration(currentMusicID);
                    ANAMusic.seekTo(currentMusicID, (int)(duration * Mathf.Clamp01(time)));
                }
                else
                {
                    // Absolute time in seconds -> milliseconds
                    ANAMusic.seekTo(currentMusicID, (int)(time * 1000f));
                }
            }
        }

        private float baseVolumeL = 1f;
        private float baseVolumeR = 1f;

        public void SetVolume(float left, float right)
        {
            baseVolumeL = left;
            baseVolumeR = right;
            ApplyFinalVolume();
        }

        private void ApplyFinalVolume()
        {
            if (currentMusicID == -1) return;

            float multiplier = 1f;
            // Preamp cut: if < 0, we reduce the volume multiplier.
            // If > 0, we use LoudnessEnhancer for boost.
            if (eqEnabled && lastPreamp < 0)
            {
                // -20dB is approx 0.1 gain, 0dB is 1.0 gain.
                // Logarithmic mapping: multiplier = 10^(db/20)
                multiplier = Mathf.Pow(10f, lastPreamp / 20f);
            }

            // Fix for Android hardware/FX mute bug at extreme balance:
            // Some devices mute the whole session if one channel is exactly 0.0 while effects are active.
            // Using a tiny epsilon (0.001f is ~ -60dB, effectively silent but not zero).
            float volL = Mathf.Max(baseVolumeL * multiplier, 0.001f);
            float volR = Mathf.Max(baseVolumeR * multiplier, 0.001f);

            ANAMusic.setVolume(currentMusicID, volL, volR);
        }

        private AndroidJavaObject nativeEq;
        private AndroidJavaObject nativeLoudness;
        
        private bool eqEnabled = false;
        private float[] lastBands;
        private float lastPreamp;

        // Throttling and Cache
        private static readonly System.Diagnostics.Stopwatch _throttleClock = System.Diagnostics.Stopwatch.StartNew();
        private long lastUpdateTimeMs = 0;
        private const int UpdateThrottleMs = 50; 
        
        private bool lastAppliedEnabledState = false;
        private float lastAppliedPreamp = -999f;
        
        private short cachedNumBands = -1;
        private int[] cachedCenterFreqs;
        private short cachedMinLevel, cachedMaxLevel;

        private long lastInteractionTimeMs = 0;
        private bool isPendingUpdate = false;
        private const int SettleTimeMs = 1000; // Winamp-style delay

        public void SetEqualizerEnabled(bool enabled)
        {
            if (eqEnabled != enabled)
            {
                eqEnabled = enabled;
                UpdateNativeEQ(true); // Forced update ONLY for real state changes (ON/OFF)
            }
        }

        public void SetEqualizerGains(float preamp, float[] bands)
        {
            lastPreamp = preamp;
            lastBands = bands;
            
            // Interaction detected: reset settle timer
            lastInteractionTimeMs = _throttleClock.ElapsedMilliseconds;
            isPendingUpdate = true;
        }

        private void UpdateNativeEQ(bool forced)
        {
            if (currentMusicID == -1) return;

            long currentTimeMs = _throttleClock.ElapsedMilliseconds;
            if (!forced && currentTimeMs - lastUpdateTimeMs < UpdateThrottleMs)
            {
                return;
            }

            lastUpdateTimeMs = currentTimeMs;
            
            try
            {
                // 1. Initialize Effects (if needed)
                if (nativeEq == null)
                {
                    nativeEq = new AndroidJavaObject("android.media.audiofx.Equalizer", 1000, currentMusicID);
                    Debug.Log($"Created native Equalizer for session {currentMusicID} with priority 1000");
                    
                    // Cache hardware info once
                    cachedNumBands = nativeEq.Call<short>("getNumberOfBands");
                    short[] range = nativeEq.Call<short[]>("getBandLevelRange");
                    cachedMinLevel = range[0];
                    cachedMaxLevel = range[1];
                    
                    cachedCenterFreqs = new int[cachedNumBands];
                    for (short i = 0; i < cachedNumBands; i++)
                    {
                        cachedCenterFreqs[i] = nativeEq.Call<int>("getCenterFreq", i) / 1000;
                    }
                }

                if (nativeLoudness == null)
                {
                    // priority 1000 not supported by LoudnessEnhancer constructor in the same way, but it's fine
                    nativeLoudness = new AndroidJavaObject("android.media.audiofx.LoudnessEnhancer", currentMusicID);
                    Debug.Log($"Created native LoudnessEnhancer for session {currentMusicID}");
                }

                // 2. Only toggle state if changed (major cause of pops)
                if (eqEnabled != lastAppliedEnabledState || forced)
                {
                    nativeEq.Call<int>("setEnabled", eqEnabled);
                    nativeLoudness.Call<int>("setEnabled", eqEnabled);
                    lastAppliedEnabledState = eqEnabled;
                }

                // 3. Update Preamp Cut (Volume scaling)
                if (eqEnabled && !Mathf.Approximately(lastPreamp, lastAppliedPreamp))
                {
                    ApplyFinalVolume();
                }

                // 4. Update Preamp Boost (LoudnessEnhancer)
                if (eqEnabled)
                {
                    int boostmB = lastPreamp > 0 ? (int)(lastPreamp * 100) : 0;
                    nativeLoudness.Call("setTargetGain", boostmB);
                }

                // 5. Update Frequency Bands
                if (eqEnabled && lastBands != null)
                {
                    for (short i = 0; i < cachedNumBands; i++)
                    {
                        float interpolatedGain = GetInterpolatedWinampGain(cachedCenterFreqs[i]);
                        float clampedGain = Mathf.Clamp(interpolatedGain, -20f, 20f);
                        
                        short level = (short)Mathf.Lerp(cachedMinLevel, cachedMaxLevel, (clampedGain + 20f) / 40f);
                        nativeEq.Call("setBandLevel", i, level);
                    }
                }

                lastAppliedPreamp = lastPreamp;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Native EQ/Loudness Error: {e.Message}");
                ReleaseEffects();
            }
        }

        public void Update()
        {
            if (isPendingUpdate)
            {
                long currentTimeMs = _throttleClock.ElapsedMilliseconds;
                if (currentTimeMs - lastInteractionTimeMs >= SettleTimeMs)
                {
                    isPendingUpdate = false;
                    UpdateNativeEQ(true); // Apply settled changes
                }
            }
        }

        private float GetInterpolatedWinampGain(float freqHz)
        {
            if (lastBands == null || lastBands.Length == 0) return 0f;
            
            float[] winampFreqs = { 60f, 170f, 310f, 600f, 1000f, 3000f, 6000f, 12000f, 14000f, 16000f };
            
            if (freqHz <= winampFreqs[0]) return lastBands[0];
            if (freqHz >= winampFreqs[winampFreqs.Length - 1]) return lastBands[winampFreqs.Length - 1];
            
            // Find the two winamp bands this frequency falls between
            for (int i = 0; i < winampFreqs.Length - 1; i++)
            {
                if (freqHz >= winampFreqs[i] && freqHz <= winampFreqs[i + 1])
                {
                    float t = (freqHz - winampFreqs[i]) / (winampFreqs[i + 1] - winampFreqs[i]);
                    // Use logarithmic interpolation for frequency? Actually linear is fine for the gain mapping here
                    return Mathf.Lerp(lastBands[i], lastBands[i + 1], t);
                }
            }
            
            return 0f;
        }

    }
}
