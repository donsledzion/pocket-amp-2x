using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

namespace SoftAware
{
    public class WinampPlaylistUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Playlist playlist;
        [SerializeField] private AudioPlayer audioPlayer;
        [SerializeField] private GameObject trackPrefab;
        [SerializeField] private Transform contentContainer;

        private List<WinampPlaylistTrack> trackUIItems = new List<WinampPlaylistTrack>();
        private int selectedIndex = -1;

        public void Initialize()
        {
            if (playlist == null) playlist = GetComponent<Playlist>();
            if (playlist == null) playlist = FindObjectOfType<Playlist>();
            if (audioPlayer == null) audioPlayer = FindObjectOfType<AudioPlayer>();

            if (playlist != null)
            {
                playlist.OnPlaylistChanged += RefreshList;
                playlist.OnCurrentIndexChanged += HandleCurrentIndexChanged;
                playlist.OnSongMetadataUpdated += HandleSongMetadataUpdated;
            }
            else
            {
                Debug.LogError("[WinampPlaylistUI] Playlist reference is missing!", this);
            }
            
            RefreshList();
        }

        private void OnDestroy()
        {
            if (playlist != null)
            {
                playlist.OnPlaylistChanged -= RefreshList;
                playlist.OnCurrentIndexChanged -= HandleCurrentIndexChanged;
                playlist.OnSongMetadataUpdated -= HandleSongMetadataUpdated;
            }
        }

        public void RefreshList()
        {
            if (playlist == null) return;
            if (trackPrefab == null)
            {
                Debug.LogError("[WinampPlaylistUI] Track Prefab is not assigned!", this);
                return;
            }

            // Clear existing
            foreach (var item in trackUIItems)
            {
                if (item != null) Destroy(item.gameObject);
            }
            trackUIItems.Clear();

            // Populate
            var songs = playlist.AllSongs;
            for (int i = 0; i < songs.Count; i++)
            {
                GameObject go = Instantiate(trackPrefab, contentContainer);
                WinampPlaylistTrack trackUI = go.GetComponent<WinampPlaylistTrack>();
                if (trackUI != null)
                {
                    if (WinampSkinLoader.Instance != null)
                    {
                        trackUI.SetColors(
                            WinampSkinLoader.Instance.PlaylistNormalColor,
                            WinampSkinLoader.Instance.PlaylistCurrentColor,
                            WinampSkinLoader.Instance.PlaylistNormalBGColor,
                            WinampSkinLoader.Instance.PlaylistSelectedBGColor
                        );
                    }
                    trackUI.Setup(i, songs[i].Title, songs[i].Duration, HandleTrackClick, HandleTrackDoubleClick);
                    trackUIItems.Add(trackUI);
                }
            }

            UpdateHighlights();
        }

        private void HandleTrackClick(int index)
        {
            selectedIndex = index;
            UpdateHighlights();
        }

        private void HandleTrackDoubleClick(int index)
        {
            selectedIndex = index;
            playlist.SetCurrentClip(index);
            audioPlayer.Play();
            UpdateHighlights();
        }

        private void HandleCurrentIndexChanged(int index)
        {
            UpdateHighlights();
        }

        private void HandleSongMetadataUpdated(int index, Playlist.SongInfo song)
        {
            UpdateTrackDuration(index, song.Title, song.Duration);
        }

        private void UpdateHighlights()
        {
            int playingIndex = playlist.CurrentIndex;
            
            for (int i = 0; i < trackUIItems.Count; i++)
            {
                if (trackUIItems[i] == null) continue;
                trackUIItems[i].SetSelected(i == selectedIndex);
                trackUIItems[i].SetPlaying(i == playingIndex);
            }
        }

        public void UpdateTrackDuration(int index, string title, float duration)
        {
            if (index >= 0 && index < trackUIItems.Count)
            {
                trackUIItems[index].RefreshDuration(title, duration);
            }
        }

        public void RefreshColors()
        {
            if (WinampSkinLoader.Instance == null) return;

            foreach (var track in trackUIItems)
            {
                if (track != null)
                {
                    track.SetColors(
                        WinampSkinLoader.Instance.PlaylistNormalColor,
                        WinampSkinLoader.Instance.PlaylistCurrentColor,
                        WinampSkinLoader.Instance.PlaylistNormalBGColor,
                        WinampSkinLoader.Instance.PlaylistSelectedBGColor
                    );
                    track.SetPlaying(playlist.CurrentIndex == trackUIItems.IndexOf(track));
                }
            }
        }
    }
}
