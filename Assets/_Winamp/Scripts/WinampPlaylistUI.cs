using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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
            if (playlist != null)
            {
                playlist.OnPlaylistChanged += RefreshList;
                playlist.OnCurrentIndexChanged += HandleCurrentIndexChanged;
            }
            
            RefreshList();
        }

        private void OnDestroy()
        {
            if (playlist != null)
            {
                playlist.OnPlaylistChanged -= RefreshList;
                playlist.OnCurrentIndexChanged -= HandleCurrentIndexChanged;
            }
        }

        public void RefreshList()
        {
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
    }
}
