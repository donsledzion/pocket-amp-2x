using System;
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
            
            // Still using throttling for volume updates when EQ is ON to be ultra-safe,
            // but we removed the preamp multiplier which caused frequent re-calculations.
            if (eqEnabled)
            {
                isVolumeDirty = true;
                lastVolInteractionTimeMs = _throttleClock.ElapsedMilliseconds;
            }
            else
            {
                ApplyFinalVolume();
                isVolumeDirty = false;
            }
        }

        private void ApplyFinalVolume()
        {
            if (currentMusicID == -1) return;
            // Pure volume and balance only. No preamp scaling here anymore.
            ANAMusic.setVolume(currentMusicID, baseVolumeL, baseVolumeR);
        }

        private AndroidJavaObject nativeEq;
        
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

        private bool isVolumeDirty = false;
        private long lastVolInteractionTimeMs = 0;
        private const int VolSettleTimeMs = 250; 

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
                // 1. Initialize Equalizer (if needed)
                if (nativeEq == null)
                {
                    Debug.Log($"[EQ-Debug] Creating Equalizer for session {currentMusicID}");
                    nativeEq = new AndroidJavaObject("android.media.audiofx.Equalizer", 1000, currentMusicID);
                    
                    IntPtr clazz = nativeEq.GetRawClass();
                    Debug.Log($"[EQ-Debug] Equalizer created. RawClassPtr: {clazz}");
                    
                    // Use more robust JNI calls for hardware info
                    IntPtr getNumBandsMethod = AndroidJNI.GetMethodID(clazz, "getNumberOfBands", "()S");
                    Debug.Log($"[EQ-Debug] getNumberOfBands MethodID: {getNumBandsMethod}");
                    
                    cachedNumBands = AndroidJNI.CallShortMethod(nativeEq.GetRawObject(), getNumBandsMethod, new jvalue[0]);
                    Debug.Log($"[EQ-Debug] Hardware NumBands: {cachedNumBands}");

                    short[] range = nativeEq.Call<short[]>("getBandLevelRange");
                    cachedMinLevel = range[0];
                    cachedMaxLevel = range[1];
                    Debug.Log($"[EQ-Debug] Range: {cachedMinLevel} to {cachedMaxLevel} mB");
                    
                    cachedCenterFreqs = new int[cachedNumBands];
                    for (short i = 0; i < cachedNumBands; i++)
                    {
                        cachedCenterFreqs[i] = nativeEq.Call<int>("getCenterFreq", (short)i) / 1000;
                    }
                }

                IntPtr currentClazz = nativeEq.GetRawClass();

                // 2. Only toggle state if changed (major cause of pops)
                if (eqEnabled != lastAppliedEnabledState || forced)
                {
                    Debug.Log($"[EQ-Debug] Setting enabled to {eqEnabled}. Forced: {forced}");
                    IntPtr setEnabledMethod = AndroidJNI.GetMethodID(currentClazz, "setEnabled", "(Z)I");
                    if (setEnabledMethod != IntPtr.Zero)
                    {
                        jvalue[] args = new jvalue[1];
                        args[0].z = eqEnabled;
                        int result = AndroidJNI.CallIntMethod(nativeEq.GetRawObject(), setEnabledMethod, args);
                        Debug.Log($"[EQ-Debug] setEnabled({eqEnabled}) result: {result}");
                        if (AndroidJNI.ExceptionOccurred() != IntPtr.Zero) 
                        {
                            Debug.LogError("[EQ-Debug] Exception occurred during setEnabled!");
                            AndroidJNI.ExceptionClear();
                        }
                    }
                    else
                    {
                        Debug.LogError("[EQ-Debug] FAILED to find setEnabled(Z)I method!");
                    }
                    lastAppliedEnabledState = eqEnabled;
                }

                // 3. Update Frequency Bands (Including Preamp integration)
                if (eqEnabled && lastBands != null)
                {
                    // Correct signature for setBandLevel is (SS)V (takes two shorts, returns void)
                    IntPtr setBandLevelMethod = AndroidJNI.GetMethodID(currentClazz, "setBandLevel", "(SS)V");
                    if (setBandLevelMethod != IntPtr.Zero)
                    {
                        Debug.Log("[EQ-Debug] Found setBandLevel(SS)V method.");
                        jvalue[] jniArgs = new jvalue[2];
                        for (short i = 0; i < cachedNumBands; i++)
                        {
                            float interpolatedGain = GetInterpolatedWinampGain(cachedCenterFreqs[i]);
                            float finalGain = interpolatedGain + lastPreamp;
                            float clampedGain = Mathf.Clamp(finalGain, -20f, 20f);
                            short level = (short)Mathf.Lerp(cachedMinLevel, cachedMaxLevel, (clampedGain + 20f) / 40f);

                            jniArgs[0].s = i;
                            jniArgs[1].s = level;
                            
                            AndroidJNI.CallVoidMethod(nativeEq.GetRawObject(), setBandLevelMethod, jniArgs);
                            
                            if (AndroidJNI.ExceptionOccurred() != IntPtr.Zero) 
                            {
                                Debug.LogError($"[EQ-Debug] Exception setting band {i} to {level}");
                                AndroidJNI.ExceptionClear();
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("[EQ-Debug] FAILED to find setBandLevel(SS)V method!");
                    }
                }

                lastAppliedPreamp = lastPreamp;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Native EQ Error: {e.Message}");
                ReleaseEffects();
            }
        }

        public void Update()
        {
            long currentTimeMs = _throttleClock.ElapsedMilliseconds;

            if (isPendingUpdate)
            {
                if (currentTimeMs - lastInteractionTimeMs >= SettleTimeMs)
                {
                    isPendingUpdate = false;
                    isVolumeDirty = false; // UpdateNativeEQ will handle volume
                    UpdateNativeEQ(true); // Apply settled changes
                }
            }

            if (isVolumeDirty)
            {
                if (currentTimeMs - lastVolInteractionTimeMs >= VolSettleTimeMs)
                {
                    isVolumeDirty = false;
                    ApplyFinalVolume();
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
