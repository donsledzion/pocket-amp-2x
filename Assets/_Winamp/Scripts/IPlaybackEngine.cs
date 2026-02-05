using UnityEngine;

namespace SoftAware
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
    }
}
