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
                UpdateNativeEQ(); // Re-apply current settings to new session
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

        public void SetEqualizerEnabled(bool enabled)
        {
            eqEnabled = enabled;
            UpdateNativeEQ();
        }

        public void SetEqualizerGains(float preamp, float[] bands)
        {
            lastPreamp = preamp;
            lastBands = bands;
            UpdateNativeEQ();
        }

        private void UpdateNativeEQ()
        {
            if (currentMusicID == -1) return;

            try
            {
                // 1. Initialize Equalizer
                if (nativeEq == null)
                {
                    nativeEq = new AndroidJavaObject("android.media.audiofx.Equalizer", 0, currentMusicID);
                    Debug.Log($"Created native Equalizer for session {currentMusicID}");
                }

                // 2. Initialize LoudnessEnhancer (for Preamp boost)
                if (nativeLoudness == null)
                {
                    nativeLoudness = new AndroidJavaObject("android.media.audiofx.LoudnessEnhancer", currentMusicID);
                    Debug.Log($"Created native LoudnessEnhancer for session {currentMusicID}");
                }

                // 3. Enable/Disable effects
                nativeEq.Call<int>("setEnabled", eqEnabled);
                nativeLoudness.Call<int>("setEnabled", eqEnabled);

                // 4. Update Volume (Preamp cut)
                ApplyFinalVolume();

                // 5. Update Preamp Boost
                if (eqEnabled)
                {
                    // LoudnessEnhancer takes gain in mB (millibels, 1/100 of a dB)
                    int boostmB = lastPreamp > 0 ? (int)(lastPreamp * 100) : 0;
                    nativeLoudness.Call("setTargetGain", boostmB);
                }

                // 6. Update Frequency Bands
                if (eqEnabled && lastBands != null)
                {
                    short numBands = nativeEq.Call<short>("getNumberOfBands");
                    short[] range = nativeEq.Call<short[]>("getBandLevelRange");
                    short minLevel = range[0];
                    short maxLevel = range[1];
                    
                    for (short i = 0; i < numBands; i++)
                    {
                        int centerFreqHz = nativeEq.Call<int>("getCenterFreq", i) / 1000;
                        float interpolatedGain = GetInterpolatedWinampGain(centerFreqHz);
                        
                        // Map gain (usually -20..+20) to native range
                        // Winamp sliders are -20..+20. Hardware range varies (often -1500..1500 mB)
                        // Most hardware supports +/- 15dB or 12dB.
                        // We clamp the gain to -20..20 before mapping to native levels.
                        float clampedGain = Mathf.Clamp(interpolatedGain, -20f, 20f);
                        
                        // Mapping: -20 dB -> minLevel, +20 dB -> maxLevel
                        short level = (short)Mathf.Lerp(minLevel, maxLevel, (clampedGain + 20f) / 40f);
                        nativeEq.Call("setBandLevel", i, level);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Native EQ/Loudness Error: {e.Message}\n{e.StackTrace}");
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
