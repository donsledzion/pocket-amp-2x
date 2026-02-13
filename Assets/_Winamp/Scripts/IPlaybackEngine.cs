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
        void Seek(float time, bool isNormalized = true);
        void SetVolume(float left, float right);
        
        bool IsPlaying { get; }
        float CurrentTime { get; }
        float Duration { get; }
        int AudioSessionId { get; }

        // Life cycle
        void Update();

        // Equalizer support
        void SetEqualizerEnabled(bool enabled);
        void SetEqualizerGains(float preamp, float[] bands);
    }
}
