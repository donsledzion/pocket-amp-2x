using UnityEngine;

namespace SoftAware
{
    public class AndroidPlaybackEngine : IPlaybackEngine
    {
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
                if (nativeEq != null)
                {
                    try { nativeEq.Call("release"); } catch { }
                    nativeEq = null;
                }
                currentMusicID = musicID;
                UpdateNativeEQ(); // Re-apply current settings to new session
            }
        }

        public void Pause()
        {
            if (currentMusicID != -1) ANAMusic.pause(currentMusicID);
        }

        public void Resume()
        {
            if (currentMusicID != -1) ANAMusic.play(currentMusicID);
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

        public void SetVolume(float left, float right)
        {
            if (currentMusicID != -1)
            {
                ANAMusic.setVolume(currentMusicID, left, right);
            }
        }

        private AndroidJavaObject nativeEq;
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
                if (nativeEq == null)
                {
                    // priority: 0, audioSession: currentMusicID
                    nativeEq = new AndroidJavaObject("android.media.audiofx.Equalizer", 0, currentMusicID);
                    Debug.Log($"Created native Equalizer for session {currentMusicID}");
                }

                // Use bool for setEnabled (matches Java boolean signature 'Z')
                int result = nativeEq.Call<int>("setEnabled", eqEnabled);
                if (result != 0) Debug.LogWarning($"nativeEq.setEnabled returned {result}");

                if (eqEnabled && lastBands != null)
                {
                    short numBands = nativeEq.Call<short>("getNumberOfBands");
                    
                    for (short i = 0; i < numBands; i++)
                    {
                        int centerFreq = nativeEq.Call<int>("getCenterFreq", (int)i) / 1000; // Hz
                        
                        int winampIndex = FindClosestBand(centerFreq);
                        float gain = lastBands[winampIndex];
                        
                        short[] range = nativeEq.Call<short[]>("getBandLevelRange");
                        short minLevel = range[0];
                        short maxLevel = range[1];
                        
                        short level = (short)Mathf.Lerp(minLevel, maxLevel, (gain + 12f) / 24f);
                        nativeEq.Call("setBandLevel", i, level);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Native EQ Error: {e.Message}\n{e.StackTrace}");
                // If it failed, don't keep a broken reference
                nativeEq = null;
            }
        }

        private int FindClosestBand(int freqHz)
        {
            float[] winampFreqs = { 60f, 170f, 310f, 600f, 1000f, 3000f, 6000f, 12000f, 14000f, 16000f };
            int bestIndex = 0;
            float minDiff = float.MaxValue;
            for (int i = 0; i < winampFreqs.Length; i++)
            {
                float diff = Mathf.Abs(winampFreqs[i] - freqHz);
                if (diff < minDiff)
                {
                    minDiff = diff;
                    bestIndex = i;
                }
            }
            return bestIndex;
        }
    }
}
