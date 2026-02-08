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

        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private Scrollbar scrollbar;
        [SerializeField] private Winamp.SelectContextMenu selectContextMenu;
        [SerializeField] private Winamp.RemoveContextMenu removeContextMenu;
        [SerializeField] private Winamp.ListOptionsContextMenu listOptionsContextMenu;

        private List<WinampPlaylistTrack> trackUIItems = new List<WinampPlaylistTrack>();
        private bool isUpdatingScroll = false;

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
                playlist.OnSelectionChanged += UpdateHighlights;
            }
            else
            {
                Debug.LogError("[WinampPlaylistUI] Playlist reference is missing!", this);
            }

            // Custom Scroll Synchronization
            if (scrollRect != null && scrollbar != null)
            {
                scrollRect.onValueChanged.AddListener(HandleScrollRectChanged);
                scrollbar.onValueChanged.AddListener(HandleScrollbarChanged);
            }

            if (selectContextMenu != null)
            {
                selectContextMenu.OnSelectAllRequested += playlist.SelectAll;
                selectContextMenu.OnSelectNoneRequested += playlist.ClearSelection;
                selectContextMenu.OnInvertSelectionRequested += playlist.InvertSelection;
            }

            if (removeContextMenu != null)
            {
                removeContextMenu.OnRemoveAllRequested += playlist.RemoveAll;
                removeContextMenu.OnRemoveSelectedRequested += playlist.RemoveSelected;
                removeContextMenu.OnCropRequested += playlist.Crop;
            }

            if (listOptionsContextMenu != null)
            {
                listOptionsContextMenu.OnNewListRequested += playlist.NewList;
                listOptionsContextMenu.OnSaveListRequested += playlist.PickSaveList;
                listOptionsContextMenu.OnLoadListRequested += playlist.PickLoadList;
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
                playlist.OnSelectionChanged -= UpdateHighlights;
            }

            if (selectContextMenu != null)
            {
                selectContextMenu.OnSelectAllRequested -= playlist.SelectAll;
                selectContextMenu.OnSelectNoneRequested -= playlist.ClearSelection;
                selectContextMenu.OnInvertSelectionRequested -= playlist.InvertSelection;
            }

            if (removeContextMenu != null)
            {
                removeContextMenu.OnRemoveAllRequested -= playlist.RemoveAll;
                removeContextMenu.OnRemoveSelectedRequested -= playlist.RemoveSelected;
                removeContextMenu.OnCropRequested -= playlist.Crop;
            }

            if (listOptionsContextMenu != null)
            {
                listOptionsContextMenu.OnNewListRequested -= playlist.NewList;
                listOptionsContextMenu.OnSaveListRequested -= playlist.PickSaveList;
                listOptionsContextMenu.OnLoadListRequested -= playlist.PickLoadList;
            }

            if (scrollRect != null) scrollRect.onValueChanged.RemoveListener(HandleScrollRectChanged);
            if (scrollbar != null) scrollbar.onValueChanged.RemoveListener(HandleScrollbarChanged);
        }

        private void HandleScrollRectChanged(Vector2 value)
        {
            if (isUpdatingScroll || scrollbar == null) return;
            isUpdatingScroll = true;
            scrollbar.value = scrollRect.verticalNormalizedPosition;
            isUpdatingScroll = false;
        }

        private void HandleScrollbarChanged(float value)
        {
            if (isUpdatingScroll || scrollRect == null) return;
            isUpdatingScroll = true;
            scrollRect.verticalNormalizedPosition = value;
            isUpdatingScroll = false;
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

            // Reset scroll on refresh
            if (scrollRect != null) scrollRect.verticalNormalizedPosition = 1f;
            if (scrollbar != null) scrollbar.value = 1f;
        }

        private void HandleTrackClick(int index)
        {
            playlist.SetSelected(index, true, true);
        }

        private void HandleTrackDoubleClick(int index)
        {
            playlist.SetCurrentClip(index);
            audioPlayer.Play();
            playlist.SetSelected(index, true, true);
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
                trackUIItems[i].SetSelected(playlist.IsSelected(i));
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
