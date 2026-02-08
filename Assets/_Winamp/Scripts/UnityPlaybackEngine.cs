using UnityEngine;

namespace SoftAware
{
    public class UnityPlaybackEngine : IPlaybackEngine
    {
        public event System.Action OnPlaybackFinished;
        
        private readonly AudioSource audioSource;
        private Playlist.SongInfo currentSong;
        private bool wasPlayingLastFrame = false;
        private bool isInternalStop = false;

        // Manual time tracking
        private float startTime;
        private float pauseTime;
        private float currentDuration;

        public UnityPlaybackEngine(AudioSource source)
        {
            audioSource = source;
        }

        public bool IsPlaying {
            get {
                bool playing = audioSource != null && audioSource.isPlaying;
                
                // Detect completion: was playing, now stopped, and not stopped by user (internal stop)
                if (wasPlayingLastFrame && !playing && !isInternalStop)
                {
                    OnPlaybackFinished?.Invoke();
                }
                
                wasPlayingLastFrame = playing;
                return playing;
            }
        }

        public float CurrentTime 
        {
            get 
            {
                if (audioSource == null) return 0f;

                // CRITICAL: Only access audioSource.time if playing and has a clip to avoid console spam.
                // Unity 2022.2+ throws warnings if clip is null or resource is not a clip.
                if (audioSource.isPlaying && audioSource.clip != null)
                {
                    return audioSource.time;
                }

                // Fallback to manual time tracking if playing but audioSource.time is unavailable/unreliable
                if (IsPlaying)
                {
                    return Mathf.Clamp(Time.time - startTime, 0f, Duration);
                }

                // If paused, return the time at which we paused
                if (isInternalStop && wasPlayingLastFrame)
                {
                    return pauseTime;
                }

                return 0f;
            }
        }

        public float Duration => (audioSource != null && audioSource.clip != null) ? audioSource.clip.length : currentDuration;
        public int AudioSessionId => -1; // Not used in Unity standard playback

        public void Play(Playlist.SongInfo song)
        {
            if (song == null || (song.Clip == null && !song.HasNativePath)) return;
            
            currentSong = song;
            currentDuration = song.Duration;
            isInternalStop = false;
            
            if (song.Clip != null)
            {
                audioSource.clip = song.Clip;
            }
            
            audioSource.Play();
            startTime = Time.time;
            pauseTime = 0f;
            wasPlayingLastFrame = true;
        }

        public void Pause()
        {
            if (audioSource == null || !audioSource.isPlaying) return;
            
            isInternalStop = true;
            pauseTime = Time.time - startTime;
            audioSource.Pause();
        }

        public void Resume()
        {
            if (audioSource == null) return;
            
            isInternalStop = false;
            startTime = Time.time - pauseTime;
            audioSource.UnPause();
        }

        public void Stop()
        {
            isInternalStop = true;
            if (audioSource != null) audioSource.Stop();
            
            startTime = 0f;
            pauseTime = 0f;
            currentDuration = 0f;
        }

        public void Seek(float normalizedTime)
        {
            if (audioSource == null) return;

            float targetTime = Duration * Mathf.Clamp01(normalizedTime);
            
            if (audioSource.clip != null)
            {
                audioSource.time = targetTime;
            }
            
            // Update manual timer
            startTime = Time.time - targetTime;
            if (isInternalStop)
            {
                pauseTime = targetTime;
            }
        }

        public void SetVolume(float left, float right)
        {
            if (audioSource != null)
            {
                // Unity standard AudioSource doesn't support separate L/R volume easily
                // We use balance (pan) and average volume
                audioSource.volume = (left + right) / 2f;
                float pan = (right - left) / Mathf.Max(0.001f, left + right); // Rough mapping
                audioSource.panStereo = Mathf.Clamp(pan, -1f, 1f);
            }
        }

        public void SetEqualizerEnabled(bool enabled)
        {
            // Unity standard AudioSource doesn't have a built-in EQ.
            // Could be implemented via Audio Mixer, but for now it's a stub.
        }

        public void SetEqualizerGains(float preamp, float[] bands)
        {
            // Stub implementation for Unity editor/desktop.
        }
    }
}
