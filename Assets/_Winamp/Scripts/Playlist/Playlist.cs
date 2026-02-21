using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using SimpleFileBrowser;
using TMPro;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

namespace SoftAware.PocketAmp
{
    [DisallowMultipleComponent]
    public class Playlist : MonoBehaviour
    {
        [Serializable]
        public class SongInfo
        {
            public string Title;
            public string Artist;
            public AudioClip Clip;
            public string FilePath;
            public float Duration;
            public bool MetadataLoaded = false;
            
            public bool HasNativePath => !string.IsNullOrEmpty(FilePath);
        }

        [Serializable]
        private class PlaylistData
        {
            // For backward compatibility
            public List<string> paths = new List<string>();
            // For rich metadata persistence
            public List<SongInfo> songs = new List<SongInfo>();
        }

        public event Action OnPlaylistChanged;
        public event Action<int> OnCurrentIndexChanged;
        public event Action<int, SongInfo> OnSongMetadataUpdated;
        public event Action OnPlaylistReady;
        public event Action OnSelectionChanged;

        [SerializeField] private Main main;
        [SerializeField] private List<SongInfo> songs = new ();
        [SerializeField] private AddContextMenu addContextMenu;
        private static Playlist instance;
        
        private int currentIndex = -1;
        private HashSet<int> selectedIndices = new ();
        private bool shuffleEnabled = false;
        private Coroutine metadataScannerCoroutine;
        internal SongInfo CurrentSong => (songs.Count > 0 && currentIndex >= 0) ? songs[currentIndex] : null;
        internal AudioClip CurrentClip => CurrentSong?.Clip;
        internal int Count => songs.Count;
        public int CurrentIndex1Based => songs.Count > 0 ? currentIndex + 1 : 0;
        public int CurrentIndex => currentIndex;
        public List<SongInfo> AllSongs => songs;
        public int SelectedCount => selectedIndices.Count;

        public bool IsSelected(int index) => selectedIndices.Contains(index);

        public float TotalDuration => songs.Sum(s => s.Duration);
        public float SelectionDuration => songs.Where((s, i) => selectedIndices.Contains(i)).Sum(s => s.Duration);

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            InitializeInspectorSongs();

#if UNITY_ANDROID && !UNITY_EDITOR
            StartCoroutine(CheckPermissionsCoroutine());
#endif

            LoadPlaylist();

            if (addContextMenu != null)
            {
                addContextMenu.OnAddDirRequested += PickFolder;
                addContextMenu.OnAddFileRequested += PickFile;
                addContextMenu.OnAddUrlRequested += main.OverlayWindowsController.OpenAddUrlWindow;
            }

            StartCoroutine(InitializeDemoTrackCoroutine());
        }

        private void OnDestroy()
        {
            if (addContextMenu != null)
            {
                addContextMenu.OnAddDirRequested -= PickFolder;
                addContextMenu.OnAddFileRequested -= PickFile;
                addContextMenu.OnAddUrlRequested -= main.OverlayWindowsController.OpenAddUrlWindow;
            }
        }

        private IEnumerator InitializeDemoTrackCoroutine()
        {
            var demoFileName = "demo.mp3";
            var targetPath = Path.Combine(Application.persistentDataPath, demoFileName);
            var playlistPath = GetPlaylistSavePath();
            var isFirstRun = SettingsManager.Instance && SettingsManager.Instance.IsFirstRun;
            var playlistExists = File.Exists(playlistPath);

            // Copy from StreamingAssets if it doesn't exist in persistentDataPath
            if (!File.Exists(targetPath))
            {
                var sourcePath = Path.Combine(Application.streamingAssetsPath, demoFileName);

#if UNITY_ANDROID && !UNITY_EDITOR
                using (UnityWebRequest www = UnityWebRequest.Get(sourcePath))
                {
                    yield return www.SendWebRequest();
                    if (www.result == UnityWebRequest.Result.Success)
                    {
                        File.WriteAllBytes(targetPath, www.downloadHandler.data);
                    }
                    else
                    {
                        Debug.LogError($"ERROR copying demo track: {www.error}");
                    }
                }
#else
                if (File.Exists(sourcePath))
                {
                    File.Copy(sourcePath, targetPath, true);
                }
#endif
            }

            // Ensure demo track is in playlist ONLY if it's first run or NO playlist exists yet
            if (File.Exists(targetPath) && (isFirstRun || !playlistExists))
            {
                if (!songs.Exists(s => s.FilePath == targetPath))
                {
                    songs.Insert(0, new SongInfo
                    {
                        Title = "demo.mp3",
                        FilePath = targetPath,
                        MetadataLoaded = false
                    });
                    
                    if (isFirstRun && SettingsManager.Instance != null)
                        SettingsManager.Instance.IsFirstRun = false;

                    OnPlaylistChanged?.Invoke();
                    SavePlaylist();
                }
            }

            if (songs.Count > 0)
            {
                int lastIndex = SettingsManager.Instance ? SettingsManager.Instance.LastPlaylistIndex : 0;
                lastIndex = Mathf.Clamp(lastIndex, 0, songs.Count - 1);
                
                // If it was -1 (default for First Run), we set it to 0 (demo track)
                if (lastIndex < 0) lastIndex = 0;
                
                SetCurrentClip(lastIndex);
            }

            OnPlaylistReady?.Invoke();
            yield break;
        }

        private void InitializeInspectorSongs()
        {
            foreach (var song in songs)
            {
                if (song.Clip != null && string.IsNullOrEmpty(song.Title))
                {
                    song.Title = song.Clip.name;
                }
            }
        }

        private IEnumerator CheckPermissionsCoroutine()
        {
            #if UNITY_ANDROID

            var audioPerm = "android.permission.READ_MEDIA_AUDIO";
            var storagePerm = Permission.ExternalStorageRead;
            var micPerm = "android.permission.RECORD_AUDIO";

            if (Application.platform != RuntimePlatform.Android) yield break;
            // Request permissions if they are not granted
            if (!Permission.HasUserAuthorizedPermission(audioPerm))
                Permission.RequestUserPermission(audioPerm);

            if (!Permission.HasUserAuthorizedPermission(storagePerm))
                Permission.RequestUserPermission(storagePerm);
                
            if (!Permission.HasUserAuthorizedPermission(micPerm))
                Permission.RequestUserPermission(micPerm);

            // Poll for permission status (up to 10 seconds)
            float timer = 0;
            while (timer < 10f)
            {
                var hasAudio = Permission.HasUserAuthorizedPermission(audioPerm);
                var hasStorage = Permission.HasUserAuthorizedPermission(storagePerm);
                    
                if (hasAudio || hasStorage) break;

                yield return new WaitForSeconds(0.5f);
                timer += 0.5f;
            }
#endif
            yield break;
        }

        private void AddDirectory(string directoryPath)
        {
            if (!FileBrowserHelpers.DirectoryExists(directoryPath))
            {
                return;
            }

            StartCoroutine(LoadDirectoryCoroutine(directoryPath));
        }

        private IEnumerator LoadDirectoryCoroutine(string rootPath)
        {
            var validAudioFound = 0;
            
            var directoriesToScan = new Stack<string>();
            directoriesToScan.Push(rootPath);

            while (directoriesToScan.Count > 0)
            {
                var currentPath = directoriesToScan.Pop();

                FileSystemEntry[] entries = null;
                try 
                {
                    entries = FileBrowserHelpers.GetEntriesInDirectory(currentPath, false);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[Playlist] LoadDirectoryCoroutine Exception: {e.Message}");
                    continue;
                }
                
                if (entries == null) continue;

                foreach (var entry in entries)
                {
                    if (entry.IsDirectory)
                    {
                        directoriesToScan.Push(entry.Path);
                    }
                    else
                    {
                        var entryName = entry.Name.ToLower();
                        if (!entryName.EndsWith(".mp3") && !entryName.EndsWith(".wav") &&
                            !entryName.EndsWith(".ogg")) continue;
                        validAudioFound++;
                            
                        // Unified Fast Path for ALL platforms
                        songs.Add(new SongInfo 
                        { 
                            Title = GetCleanFileName(entry.Path), 
                            Clip = null, 
                            FilePath = entry.Path,
                            Duration = 0,
                            MetadataLoaded = false
                        });
                    }
                }
                
                // Yield occasionally to prevent long hitches with many folders
                yield return null;
            }
            
            if (currentIndex == -1 && songs.Count > 0)
            {
                SetCurrentClip(0);
            }
            
            if (metadataScannerCoroutine != null) StopCoroutine(metadataScannerCoroutine);
            metadataScannerCoroutine = StartCoroutine(MetadataScannerCoroutine());
            
            OnPlaylistChanged?.Invoke();
            SavePlaylist();
            yield break;
        }

        public void NewList()
        {
            songs.Clear();
            selectedIndices.Clear();
            currentIndex = -1;
            OnPlaylistChanged?.Invoke();
            OnCurrentIndexChanged?.Invoke(currentIndex);
            SavePlaylist(); // Savve default one
        }

        public void PickSaveList()
        {
            var folderPath = GetPlaylistsFolder();
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            FileBrowser.ShowSaveDialog((paths) => {
                if (paths is { Length: > 0 }) SavePlaylist(paths[0]);
            }, null, FileBrowser.PickMode.Files, false, folderPath, "playlist.json", "Save Playlist", "Save");
        }

        public void PickLoadList()
        {
            var folderPath = GetPlaylistsFolder();
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            FileBrowser.ShowLoadDialog((paths) => {
                if (paths is { Length: > 0 }) LoadPlaylist(paths[0]);
            }, null, FileBrowser.PickMode.Files, false, folderPath, null, "Load Playlist", "Load");
        }

        private void SavePlaylist(string path = null)
        {
            var isExplicitPath = !string.IsNullOrEmpty(path);
            if (string.IsNullOrEmpty(path)) path = GetPlaylistSavePath();
            
            // Ensure directory exists for custom paths too
            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            var data = new PlaylistData();
            foreach (var song in songs)
            {
                if (string.IsNullOrEmpty(song.FilePath)) continue;
                // Add to both for maximum compatibility/visibility
                data.paths.Add(song.FilePath);
                data.songs.Add(new SongInfo
                {
                    Title = song.Title,
                    Artist = song.Artist,
                    FilePath = song.FilePath,
                    Duration = song.Duration,
                    MetadataLoaded = song.MetadataLoaded
                });
            }

            try
            {
                var json = JsonUtility.ToJson(data);
                File.WriteAllText(path, json);

                // If we saved an explicit external path, also update our master playlist
                if (isExplicitPath && path != GetPlaylistSavePath())
                {
                    SavePlaylist(null);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"SAVE ERROR: {e.Message}");
            }
        }

        private void LoadPlaylist(string path = null)
        {
            var isMaster = string.IsNullOrEmpty(path);
            if (string.IsNullOrEmpty(path)) path = GetPlaylistSavePath();
            if (!File.Exists(path)) return;

            try
            {
                var json = File.ReadAllText(path);
                var data = JsonUtility.FromJson<PlaylistData>(json);

                if (data == null) return;
                songs.Clear(); 
                selectedIndices.Clear();
                currentIndex = -1;

                // Support for Rich metadata (new format)
                if (data.songs != null && data.songs.Count > 0)
                {
                    foreach (var song in data.songs)
                    {
                        songs.Add(new SongInfo
                        {
                            Title = song.Title,
                            Artist = song.Artist,
                            FilePath = song.FilePath,
                            Duration = song.Duration,
                            MetadataLoaded = song.MetadataLoaded
                        });
                    }
                }
                // Support for Backward compatibility (old format)
                else if (data.paths != null && data.paths.Count > 0)
                {
                    foreach (string filePath in data.paths)
                    {
                        songs.Add(new SongInfo
                        {
                            Title = GetCleanFileName(filePath),
                            FilePath = filePath,
                            MetadataLoaded = false
                        });
                    }
                }
                    
                if (metadataScannerCoroutine != null) StopCoroutine(metadataScannerCoroutine);
                metadataScannerCoroutine = StartCoroutine(MetadataScannerCoroutine());
                    
                OnPlaylistChanged?.Invoke();
                OnCurrentIndexChanged?.Invoke(currentIndex);

                if (isMaster)
                {
                    // If we loaded default, try to restore settings index
                    if (!SettingsManager.Instance) return;
                    var lastIndex = SettingsManager.Instance.LastPlaylistIndex;
                    if (lastIndex >= 0 && lastIndex < songs.Count) SetCurrentClip(lastIndex);
                    else if (songs.Count > 0) SetCurrentClip(0);
                }
                else
                {
                    // If we loaded EXTERNAL list, we should save it immediately as our master list
                    SavePlaylist(null);
                        
                    // Always set first track as current for explicit external load
                    if (songs.Count > 0) SetCurrentClip(0);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"LOAD ERROR: {e.Message}");
            }
        }

        private string GetPlaylistsFolder()
        {
            return Path.Combine(Application.persistentDataPath, "playlists");
        }

        private string GetPlaylistSavePath()
        {
            return Path.Combine(GetPlaylistsFolder(), "winamp_playlist.json");
        }

        private IEnumerator MetadataScannerCoroutine()
        {
            bool anyMetadataUpdated = false;
            // Scan through all songs that haven't loaded metadata yet
            for (int i = 0; i < songs.Count; i++)
            {
                var song = songs[i];
                if (song.MetadataLoaded) continue;

#if UNITY_ANDROID && !UNITY_EDITOR
                string path = song.FilePath;
                // Retrieve duration and metadata from native bridge
                float duration = AndroidAudioInfoBridge.GetDuration(path);
                string[] meta = AndroidAudioInfoBridge.GetMetadata(path);
                
                song.Duration = duration;
                if (!string.IsNullOrEmpty(meta[0])) // Title
                {
                    song.Title = meta[0];
                    if (!string.IsNullOrEmpty(meta[1])) // Artist
                    {
                        song.Artist = meta[1];
                        song.Title = $"{meta[1]} - {meta[0]}";
                    }
                    else
                    {
                        song.Artist = "";
                    }
                    anyMetadataUpdated = true;
                }
                else
                {
                    // If no native title, use filename (or url) but mark as loaded so we don't try again
                    song.Title = GetCleanFileName(path);
                    if (path.StartsWith("http://") || path.StartsWith("https://")) song.Title = path;
                    song.Artist = "";
                }
#else
                // In Editor/Non-Android, we don't have a fast native way to get duration
                // without loading the whole clip. We'll mark it as loaded for now, 
                // and it will be updated when the song is actually played/loaded.
                if (string.IsNullOrEmpty(song.Title)) song.Title = GetCleanFileName(song.FilePath);
#endif
                song.MetadataLoaded = true;
                OnSongMetadataUpdated?.Invoke(i, song);
                yield return null; // Wait for next frame to avoid hitching
            }

            if (anyMetadataUpdated)
            {
                SavePlaylist(); // Auto-save updated metadata
            }

            metadataScannerCoroutine = null;
        }

        public IEnumerator LoadSongClip(SongInfo song)
        {
            if (song == null || song.Clip) yield break;

            var type = AudioType.UNKNOWN;
            var ext = song.Title.ToLower();
            if (ext.EndsWith(".mp3")) type = AudioType.MPEG;
            else if (ext.EndsWith(".wav")) type = AudioType.WAV;
            else if (ext.EndsWith(".ogg")) type = AudioType.OGGVORBIS;

            string finalUrl = "";

            if (Application.platform == RuntimePlatform.Android)
            {
                if (song.FilePath.StartsWith("http://") || song.FilePath.StartsWith("https://"))
                {
                    // Streams shouldn't be loaded as audio clips anyway (Android Native handles it directly)
                    yield break;
                }

                if (FileBrowserHelpers.FileExists(song.FilePath))
                {
                    finalUrl = song.FilePath; 
                    if (!finalUrl.StartsWith("content://") && !finalUrl.StartsWith("file://"))
                        finalUrl = "file:///" + finalUrl.TrimStart('/');
                }
                else
                {
                    yield break;
                }
            }
            else
            {
                finalUrl = "file:///" + song.FilePath.TrimStart('/');
            }

            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(finalUrl, type))
            {
                www.timeout = 10;
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                    clip.name = song.Title;
                    song.Clip = clip;
                    
                    // Update duration if it was 0 or incorrect
                    if (song.Duration <= 0)
                    {
                        song.Duration = clip.length;
                        OnSongMetadataUpdated?.Invoke(songs.IndexOf(song), song);
                    }
                }
                else
                {
                    Debug.LogError($"<color=red>LOAD ERROR: {www.error}</color>");
                }
            }
        }

        public void SetCurrentClip(int index)
        {
            if (songs.Count == 0) return;
            if (index < 0 || index >= songs.Count) return;
            var changed = (currentIndex != index);
            currentIndex = index;
            if (!changed) return;
            OnCurrentIndexChanged?.Invoke(currentIndex);
            if (SettingsManager.Instance)
                SettingsManager.Instance.LastPlaylistIndex = currentIndex;
        }

        // Thread-safe random for background threads
        private System.Random rng = new System.Random();

        internal SongInfo GetNextSong()
        {
            if (songs.Count == 0) return null;
            
            int newIndex;
            if (shuffleEnabled)
            {
                // Use System.Random for thread safety potential
                newIndex = rng.Next(0, songs.Count);
            }
            else
            {
                // Sequential
                newIndex = (currentIndex == songs.Count - 1) ? 0 : currentIndex + 1;
            }
            
            SetCurrentClip(newIndex);
            
            return CurrentSong;
        }

        // Returns the index of the next song without modifying state
        public int GetNextSongIndex()
        {
            if (songs.Count == 0) return -1;
            
            if (shuffleEnabled)
            {
                lock(rng) { return rng.Next(0, songs.Count); }
            }
            else
            {
                return (currentIndex == songs.Count - 1) ? 0 : currentIndex + 1;
            }
        }
        
        // Returns the index of the previous song without modifying state
        public int GetPreviousSongIndex()
        {
            if (songs.Count == 0) return -1;
            return (currentIndex <= 0) ? songs.Count - 1 : currentIndex - 1;
        }

        // Silent update for background threads - updates index but defers events
        public void SetCurrentIndexSilent(int index)
        {
             if (index >= 0 && index < songs.Count)
             {
                 currentIndex = index;
             }
        }

        /// <summary>
        /// Forces the OnCurrentIndexChanged event to fire, synchronizing the UI 
        /// (e.g. playlist highlights) with the current state.
        /// Call this from the Main Thread.
        /// </summary>
        public void UI_SyncIndex()
        {
            if (currentIndex >= 0 && currentIndex < songs.Count)
            {
                OnCurrentIndexChanged?.Invoke(currentIndex);
            }
        }

        public void ToggleShuffle()
        {
            shuffleEnabled = !shuffleEnabled;
        }

        public void SetShuffle(bool enabled)
        {
            shuffleEnabled = enabled;
        }

        public bool IsShuffleEnabled => shuffleEnabled;

        internal SongInfo GetPreviousSong()
        {
            if (songs.Count == 0) return null;
            int newIndex = (currentIndex <= 0) ? songs.Count - 1 : currentIndex - 1;
            SetCurrentClip(newIndex);
            return CurrentSong;
        }

        public void PickFolder()
        {
            FileBrowser.ShowLoadDialog((paths) => {
                if (paths != null && paths.Length > 0) AddDirectory(paths[0]);
            }, null, FileBrowser.PickMode.Folders, false, null, null, "Select Audio Folder", "Select");
        }

        public void PickFile()
        {
            FileBrowser.ShowLoadDialog((paths) => {
                if (paths != null && paths.Length > 0) AddFile(paths[0]);
            }, null, FileBrowser.PickMode.Files, false, null, null, "Select Audio File", "Select");
        }

        public void AddFile(string filePath)
        {
            if (!FileBrowserHelpers.FileExists(filePath)) return;
            
            songs.Add(new SongInfo 
            { 
                Title = GetCleanFileName(filePath), 
                FilePath = filePath,
                MetadataLoaded = false
            });
            
            if (currentIndex == -1 && songs.Count > 0)
            {
                SetCurrentClip(0);
            }
            
            if (metadataScannerCoroutine != null) StopCoroutine(metadataScannerCoroutine);
            metadataScannerCoroutine = StartCoroutine(MetadataScannerCoroutine());
            
            OnPlaylistChanged?.Invoke();
            SavePlaylist();
        }

        public void AddUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return;
            StartCoroutine(ResolveUrlCoroutine(url.Trim()));
        }

        private IEnumerator ResolveUrlCoroutine(string url)
        {
            Refs.UIController?.ShowLoading();
            //main.UIController?.ShowLoading();

            bool isPlaylist = false;
            string finalUrl = url;

            using (var www = UnityWebRequest.Get(url))
            {
                // We only want a bit of text, so a small timeout is fine. 
                // We don't want to download a continuous stream into RAM!
                // But UnityWebRequest.Get on a stream might freeze? 
                // We should use SetRequestHeader to only get a few bytes, or use Head.
                // Actually, a safer way to avoid freezing on continuous streams is to use SendWebRequest, 
                // let it connect, read headers, and if it's an audio stream, abort.
            }

            // Instead of dealing with UnityWebRequest freezing on infinite streams,
            // we will do a fast extension-based check or simple GET for known text formats.
            if (url.EndsWith(".pls", StringComparison.OrdinalIgnoreCase) || 
                url.EndsWith(".m3u", StringComparison.OrdinalIgnoreCase) || 
                url.EndsWith(".m3u8", StringComparison.OrdinalIgnoreCase) ||
                url.Contains("listen.pls") || url.Contains("listen.m3u"))
            {
                 using (var www = UnityWebRequest.Get(url))
                 {
                     yield return www.SendWebRequest();
                     if (www.result == UnityWebRequest.Result.Success)
                     {
                         string text = www.downloadHandler.text;
                         string streamUrl = ParsePlaylistForStream(text);
                         if (!string.IsNullOrEmpty(streamUrl))
                         {
                             finalUrl = streamUrl;
                         }
                     }
                 }
            }

            songs.Add(new SongInfo 
            { 
                Title = finalUrl, 
                Artist = "Internet Stream",
                FilePath = finalUrl,
                MetadataLoaded = false
            });
            
            if (currentIndex == -1 && songs.Count > 0)
            {
                SetCurrentClip(0);
            }
            
            if (metadataScannerCoroutine != null) StopCoroutine(metadataScannerCoroutine);
            metadataScannerCoroutine = StartCoroutine(MetadataScannerCoroutine());
            
            OnPlaylistChanged?.Invoke();
            SavePlaylist();
            
            Refs.UIController?.HideLoading();
        }

        private string ParsePlaylistForStream(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var line in lines)
            {
                string t = line.Trim();
                // PLS format: File1=http://...
                if (t.StartsWith("File1=", StringComparison.OrdinalIgnoreCase))
                {
                    return t.Substring(6).Trim();
                }
                // M3U format: just the URL on a line not starting with #
                if (t.StartsWith("http", StringComparison.OrdinalIgnoreCase) && !t.StartsWith("#"))
                {
                    return t;
                }
            }
            return null;
        }

        public void SelectAll()
        {
            selectedIndices.Clear();
            for (int i = 0; i < songs.Count; i++) selectedIndices.Add(i);
            OnSelectionChanged?.Invoke();
        }

        public void ClearSelection()
        {
            selectedIndices.Clear();
            OnSelectionChanged?.Invoke();
        }

        public void InvertSelection()
        {
            HashSet<int> next = new HashSet<int>();
            for (int i = 0; i < songs.Count; i++)
            {
                if (!selectedIndices.Contains(i)) next.Add(i);
            }
            selectedIndices = next;
            OnSelectionChanged?.Invoke();
        }

        public void SetSelected(int index, bool isSelected, bool clearOthers)
        {
            if (clearOthers) selectedIndices.Clear();
            
            if (isSelected) selectedIndices.Add(index);
            else selectedIndices.Remove(index);
            
            OnSelectionChanged?.Invoke();
        }

        public void RemoveAll()
        {
            songs.Clear();
            selectedIndices.Clear();
            currentIndex = -1;
            OnPlaylistChanged?.Invoke();
            OnCurrentIndexChanged?.Invoke(currentIndex);
            SavePlaylist();
        }

        public void RemoveSelected()
        {
            if (selectedIndices.Count == 0) return;

            // Sort indices descending to remove from end to avoid index shifting problems
            List<int> toRemove = new List<int>(selectedIndices);
            toRemove.Sort((a, b) => b.CompareTo(a));

            bool playingRemoved = false;
            foreach (int idx in toRemove)
            {
                if (idx == currentIndex) playingRemoved = true;
                songs.RemoveAt(idx);
            }

            if (playingRemoved) currentIndex = -1;
            else if (currentIndex >= 0)
            {
                // Adjust currentIndex if necessary
                int removedBefore = 0;
                foreach (int idx in toRemove) if (idx < currentIndex) removedBefore++;
                currentIndex -= removedBefore;
            }

            selectedIndices.Clear();
            OnPlaylistChanged?.Invoke();
            if (playingRemoved || toRemove.Any(idx => idx <= currentIndex)) OnCurrentIndexChanged?.Invoke(currentIndex);
            SavePlaylist();
        }

        public void Crop()
        {
            if (selectedIndices.Count == 0)
            {
                RemoveAll();
                return;
            }

            List<SongInfo> nextSongs = new List<SongInfo>();
            int nextCurrentIndex = -1;
            
            for (int i = 0; i < songs.Count; i++)
            {
                if (selectedIndices.Contains(i))
                {
                    if (i == currentIndex) nextCurrentIndex = nextSongs.Count;
                    nextSongs.Add(songs[i]);
                }
            }

            songs = nextSongs;
            currentIndex = nextCurrentIndex;
            selectedIndices.Clear();
            
            OnPlaylistChanged?.Invoke();
            OnCurrentIndexChanged?.Invoke(currentIndex);
            SavePlaylist();
        }

        private void OnApplicationQuit()
        {
            // No cleanup needed anymore since we don't copy files!
        }
        private string GetCleanFileName(string path)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return AndroidAudioInfoBridge.GetFileName(path);
#else
            try { return Path.GetFileName(path); }
            catch { return path; }
#endif
        }
    }
}
