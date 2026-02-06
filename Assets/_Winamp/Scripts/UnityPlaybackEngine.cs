using UnityEngine;

namespace SoftAware
{
    public class UnityPlaybackEngine : IPlaybackEngine
    {
        private readonly AudioSource audioSource;
        private Playlist.SongInfo currentSong;

        public UnityPlaybackEngine(AudioSource source)
        {
            audioSource = source;
        }

        public bool IsPlaying => audioSource != null && audioSource.isPlaying;
        public float CurrentTime => audioSource != null ? audioSource.time : 0f;
        public float Duration => (audioSource != null && audioSource.clip != null) ? audioSource.clip.length : 0f;
        public int AudioSessionId => -1; // Not used in Unity standard playback

        public void Play(Playlist.SongInfo song)
        {
            if (song == null || song.Clip == null) return;
            currentSong = song;
            audioSource.clip = song.Clip;
            audioSource.Play();
        }

        public void Pause()
        {
            if (audioSource != null) audioSource.Pause();
        }

        public void Resume()
        {
            if (audioSource != null) audioSource.UnPause();
        }

        public void Stop()
        {
            if (audioSource != null) audioSource.Stop();
        }

        public void Seek(float normalizedTime)
        {
            if (audioSource != null && audioSource.clip != null)
            {
                audioSource.time = audioSource.clip.length * Mathf.Clamp01(normalizedTime);
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
