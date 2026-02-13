using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;


namespace SoftAware.Winamp
{
    public class WinampPlaylistUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Main main;
        [SerializeField] private Playlist playlist;
        [SerializeField] private AudioPlayer audioPlayer;
        [SerializeField] private GameObject trackPrefab;
        [SerializeField] private Transform contentContainer;

        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private Scrollbar scrollbar;
        [SerializeField] private SelectContextMenu selectContextMenu;
        [SerializeField] private RemoveContextMenu removeContextMenu;
        [SerializeField] private MiscContextMenu miscContextMenu;
        [SerializeField] private ListOptionsContextMenu listOptionsContextMenu;

        [Header("Window Controls")]
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI timeCounterText;
        [SerializeField] private TextMeshProUGUI currentTrackTimeText;


        private readonly List<WinampPlaylistTrack> trackUIItems = new ();
        private bool isUpdatingScroll = false;
        private bool isRemainingMode = false;
        private float lastUpdateTime = 0f;

        private void Start()
        {
            if (closeButton != null) closeButton.onClick.AddListener(CloseWindow);
        }

        public void Initialize()
        {
            if (playlist == null) playlist = FindAnyObjectByType<Playlist>();
            if (audioPlayer == null) audioPlayer = FindAnyObjectByType<AudioPlayer>();

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
                removeContextMenu.OnMiscRequested += main.SongTitleDisplay.ShowNotReadyYetMessage;
            }

            if (listOptionsContextMenu != null)
            {
                listOptionsContextMenu.OnNewListRequested += playlist.NewList;
                listOptionsContextMenu.OnSaveListRequested += playlist.PickSaveList;
                listOptionsContextMenu.OnLoadListRequested += playlist.PickLoadList;
            }

            if (miscContextMenu != null)
            {
                miscContextMenu.OnSortListButtonClicked += main.SongTitleDisplay.ShowNotReadyYetMessage;
                miscContextMenu.OnFileInfoButtonClicked += main.SongTitleDisplay.ShowNotReadyYetMessage;
                miscContextMenu.OnMiscOptionsButtonClicked += main.SongTitleDisplay.ShowNotReadyYetMessage;
            }

            if (main != null && main.TimeDisplay != null)
            {
                main.TimeDisplay.OnModeChanged += HandleTimeModeChanged;
                // Initial sync
                isRemainingMode = SettingsManager.Instance != null ? SettingsManager.Instance.IsRemainingMode : false;
            }
            
            RefreshList();
            UpdateTimeCounter();
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
                removeContextMenu.OnMiscRequested -= main.SongTitleDisplay.ShowNotReadyYetMessage;
            }

            if (miscContextMenu != null)
            {
                miscContextMenu.OnSortListButtonClicked -= main.SongTitleDisplay.ShowNotReadyYetMessage;
                miscContextMenu.OnFileInfoButtonClicked -= main.SongTitleDisplay.ShowNotReadyYetMessage;
                miscContextMenu.OnMiscOptionsButtonClicked -= main.SongTitleDisplay.ShowNotReadyYetMessage;
            }

            if (listOptionsContextMenu != null)
            {
                listOptionsContextMenu.OnNewListRequested -= playlist.NewList;
                listOptionsContextMenu.OnSaveListRequested -= playlist.PickSaveList;
                listOptionsContextMenu.OnLoadListRequested -= playlist.PickLoadList;
            }

            if (main != null && main.TimeDisplay != null)
            {
                main.TimeDisplay.OnModeChanged -= HandleTimeModeChanged;
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

        private void Update()
        {
            // Simple throttle for UI update
            if (Time.time - lastUpdateTime > 0.1f)
            {
                UpdateCurrentTrackTime();
                lastUpdateTime = Time.time;
            }
        }

        public void RefreshList()
        {
            if (!playlist) return;
            if (!trackPrefab)
            {
                Debug.LogError("[WinampPlaylistUI] Track Prefab is not assigned!", this);
                return;
            }

            // Clear existing
            foreach (var item in trackUIItems)
            {
                if (item) Destroy(item.gameObject);
            }
            trackUIItems.Clear();

            // Populate
            var songs = playlist.AllSongs;
            for (var i = 0; i < songs.Count; i++)
            {
                var go = Instantiate(trackPrefab, contentContainer);
                var trackUI = go.GetComponent<WinampPlaylistTrack>();
                if (!trackUI) continue;
                if (WinampSkinLoader.Instance)
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

            UpdateHighlights();

            // Reset scroll on refresh
            if (scrollRect) scrollRect.verticalNormalizedPosition = 1f;
            if (scrollbar) scrollbar.value = 1f;
            UpdateTimeCounter();
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
            UpdateTimeCounter();
        }


        private void UpdateHighlights()
        {
            var playingIndex = playlist.CurrentIndex;
            
            for (var i = 0; i < trackUIItems.Count; i++)
            {
                if (!trackUIItems[i]) continue;
                trackUIItems[i].SetSelected(playlist.IsSelected(i));
                trackUIItems[i].SetPlaying(i == playingIndex);
            }

            UpdateTimeCounter();
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
                if (track == null) continue;
                track.SetColors(
                    WinampSkinLoader.Instance.PlaylistNormalColor,
                    WinampSkinLoader.Instance.PlaylistCurrentColor,
                    WinampSkinLoader.Instance.PlaylistNormalBGColor,
                    WinampSkinLoader.Instance.PlaylistSelectedBGColor
                );
                track.SetPlaying(playlist.CurrentIndex == trackUIItems.IndexOf(track));
            }
        }

        private void UpdateTimeCounter()
        {
            if (timeCounterText == null || playlist == null) return;

            float totalDuration = 0;
            float selectedDuration = 0;
            var songs = playlist.AllSongs;

            for (int i = 0; i < songs.Count; i++)
            {
                totalDuration += songs[i].Duration;
                if (playlist.IsSelected(i))
                {
                    selectedDuration += songs[i].Duration;
                }
            }

            string totalStr = AudioMetadataUtils.FormatTime(totalDuration);
            string selectedStr = AudioMetadataUtils.FormatTime(selectedDuration);

            timeCounterText.text = $"{selectedStr}/{totalStr}";
        }

        private void UpdateCurrentTrackTime()
        {
            if (currentTrackTimeText == null || audioPlayer == null) return;

            if (!audioPlayer.IsPlaying && !audioPlayer.IsPaused)
            {
                currentTrackTimeText.text = "";
                return;
            }

            float currentTime = audioPlayer.CurrentTime;
            float totalTime = audioPlayer.Duration;
            float displayTime = isRemainingMode ? (totalTime - currentTime) : currentTime;

            int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(displayTime));
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;

            string timeStr = isRemainingMode ? $"-{minutes}  {seconds:D2}" : $"{minutes}  {seconds:D2}";
            currentTrackTimeText.text = timeStr;
        }

        private void HandleTimeModeChanged(bool remaining)
        {
            isRemainingMode = remaining;
            UpdateCurrentTrackTime();
        }

        private void CloseWindow() => main.ClosePlaylistWindow();


    }
}
