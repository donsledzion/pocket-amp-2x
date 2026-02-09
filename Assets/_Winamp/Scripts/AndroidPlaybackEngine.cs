using UnityEngine;

namespace SoftAware
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

        public void Seek(float normalizedTime)
        {
            if (currentMusicID != -1)
            {
                int duration = ANAMusic.getDuration(currentMusicID);
                ANAMusic.seekTo(currentMusicID, (int)(duration * Mathf.Clamp01(normalizedTime)));
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

            ANAMusic.setVolume(currentMusicID, baseVolumeL * multiplier, baseVolumeR * multiplier);
        }

        private AndroidJavaObject nativeEq;
        private AndroidJavaObject nativeLoudness;
        
        private bool eqEnabled = false;
        private float[] lastBands;
        private float lastPreamp;

        // Throttling and Cache
        private float lastUpdateTime = 0f;
        private const float UpdateThrottle = 0.05f; // 50ms
        
        private bool lastAppliedEnabledState = false;
        private float lastAppliedPreamp = -999f;
        
        private short cachedNumBands = -1;
        private int[] cachedCenterFreqs;
        private short cachedMinLevel, cachedMaxLevel;

        public void SetEqualizerEnabled(bool enabled)
        {
            eqEnabled = enabled;
            UpdateNativeEQ(true); // Forced update for state changes
        }

        public void SetEqualizerGains(float preamp, float[] bands)
        {
            lastPreamp = preamp;
            lastBands = bands;
            UpdateNativeEQ(false); // Throttled update for slider moves
        }

        private void UpdateNativeEQ(bool forced)
        {
            if (currentMusicID == -1) return;

            float currentTime = Time.realtimeSinceStartup;
            if (!forced && currentTime - lastUpdateTime < UpdateThrottle)
            {
                return;
            }

            lastUpdateTime = currentTime;
            
            try
            {
                // 1. Initialize Effects (if needed)
                if (nativeEq == null)
                {
                    nativeEq = new AndroidJavaObject("android.media.audiofx.Equalizer", 0, currentMusicID);
                    Debug.Log($"Created native Equalizer for session {currentMusicID}");
                    
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
