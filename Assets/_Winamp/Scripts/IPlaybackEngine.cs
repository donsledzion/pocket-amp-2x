namespace SoftAware.Winamp
{
    public enum PlaybackState
    {
        Stopped,
        Loading,
        Playing,
        Paused
    }

    public interface IPlaybackEngine
    {
        event System.Action OnPlaybackFinished;
        void Play(Playlist.SongInfo song);
        void Pause();
        void Resume();
        void Stop();
        void Seek(float normalizedTime);
        void SetVolume(float left, float right);
        
        bool IsPlaying { get; }
        float CurrentTime { get; }
        float Duration { get; }
        int AudioSessionId { get; }

        // Equalizer support
        void SetEqualizerEnabled(bool enabled);
        void SetEqualizerGains(float preamp, float[] bands);
    }
}
