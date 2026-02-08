using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using SimpleFileBrowser;
using TMPro;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

namespace SoftAware
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
            public List<string> paths = new List<string>();
        }

        public event Action OnPlaylistChanged;
        public event Action<int> OnCurrentIndexChanged;
        public event Action<int, SongInfo> OnSongMetadataUpdated;
        public event Action OnPlaylistReady;

        [SerializeField] private List<SongInfo> songs = new List<SongInfo>();
        [SerializeField] private TextMeshProUGUI debugText;
        private static Playlist instance;
        
        private int currentIndex = -1;
        private bool shuffleEnabled = false;
        private Coroutine metadataScannerCoroutine;
        internal SongInfo CurrentSong => (songs.Count > 0 && currentIndex >= 0) ? songs[currentIndex] : null;
        internal AudioClip CurrentClip => CurrentSong?.Clip;
        internal int Count => songs.Count;
        public int CurrentIndex1Based => songs.Count > 0 ? currentIndex + 1 : 0;
        public int CurrentIndex => currentIndex;
        public List<SongInfo> AllSongs => songs;

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

            StartCoroutine(InitializeDemoTrackCoroutine());
        }

        private IEnumerator InitializeDemoTrackCoroutine()
        {
            string demoFileName = "demo.mp3";
            string targetPath = Path.Combine(Application.persistentDataPath, demoFileName);

            // Copy from StreamingAssets if it doesn't exist in persistentDataPath
            if (!File.Exists(targetPath))
            {
                LogDebug("Demo track not found in persistent path. Copying from StreamingAssets...");
                string sourcePath = Path.Combine(Application.streamingAssetsPath, demoFileName);

#if UNITY_ANDROID && !UNITY_EDITOR
                using (UnityWebRequest www = UnityWebRequest.Get(sourcePath))
                {
                    yield return www.SendWebRequest();
                    if (www.result == UnityWebRequest.Result.Success)
                    {
                        File.WriteAllBytes(targetPath, www.downloadHandler.data);
                        LogDebug("Successfully copied demo track to persistent path.");
                    }
                    else
                    {
                        LogDebug($"ERROR copying demo track: {www.error}");
                    }
                }
#else
                if (File.Exists(sourcePath))
                {
                    File.Copy(sourcePath, targetPath, true);
                    LogDebug("Successfully copied demo track to persistent path.");
                }
#endif
            }

            // Ensure demo track is in playlist if it's the first run or playlist is empty
            if (File.Exists(targetPath))
            {
                if (!songs.Exists(s => s.FilePath == targetPath))
                {
                    songs.Insert(0, new SongInfo
                    {
                        Title = "demo.mp3",
                        FilePath = targetPath,
                        MetadataLoaded = false
                    });
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
            LogDebug("Requesting permissions...");

            string audioPerm = "android.permission.READ_MEDIA_AUDIO";
            string storagePerm = Permission.ExternalStorageRead;
            string micPerm = "android.permission.RECORD_AUDIO";

            if (Application.platform == RuntimePlatform.Android)
            {
                // Request permissions if they are not granted
                if (!Permission.HasUserAuthorizedPermission(audioPerm))
                {
                    LogDebug("Requesting MediaAudio...");
                    Permission.RequestUserPermission(audioPerm);
                }

                if (!Permission.HasUserAuthorizedPermission(storagePerm))
                {
                    LogDebug("Requesting StorageRead...");
                    Permission.RequestUserPermission(storagePerm);
                }
                
                if (!Permission.HasUserAuthorizedPermission(micPerm))
                {
                    LogDebug("Requesting RecordAudio (for visualizer)...");
                    Permission.RequestUserPermission(micPerm);
                }

                // Poll for permission status (up to 10 seconds)
                float timer = 0;
                while (timer < 10f)
                {
                    bool hasAudio = Permission.HasUserAuthorizedPermission(audioPerm);
                    bool hasStorage = Permission.HasUserAuthorizedPermission(storagePerm);
                    
                    if (hasAudio || hasStorage) 
                    {
                        LogDebug($"Status: Audio={hasAudio}, Storage={hasStorage}");
                        break; 
                    }

                    yield return new WaitForSeconds(0.5f);
                    timer += 0.5f;
                }

                LogDebug($"Final Check: Audio={Permission.HasUserAuthorizedPermission(audioPerm)}, Storage={Permission.HasUserAuthorizedPermission(storagePerm)}");
            }
            #endif
            yield break;
        }

        public static void Log(string message)
        {
            if (instance) instance.LogDebug(message);
            else Debug.Log("[PlaylistStatic] " + message);
        }

        private void LogDebug(string message, bool append = true)
        {
            if (!debugText) return;
            // Clean older logs if it gets too long
            if (debugText.text.Length > 2000) debugText.text = "... (too many logs)\n";
            
            if (append) debugText.text += message + "\n";
            else debugText.text = message + "\n";
            Debug.Log("[PlaylistDebug] " + message);
        }

        public void AddDirectory(string directoryPath)
        {
            LogDebug($"SCANNING: {directoryPath}", false);

            if (!FileBrowserHelpers.DirectoryExists(directoryPath))
            {
                LogDebug($"ERROR: Directory not found according to FileBrowserHelpers");
                return;
            }

            StartCoroutine(LoadDirectoryCoroutine(directoryPath));
        }

        private IEnumerator LoadDirectoryCoroutine(string rootPath)
        {
            LogDebug("Scanning folders recursively...", false);
            int validAudioFound = 0;
            
            Stack<string> directoriesToScan = new Stack<string>();
            directoriesToScan.Push(rootPath);

            while (directoriesToScan.Count > 0)
            {
                string currentPath = directoriesToScan.Pop();
                LogDebug($"Scanning: {Path.GetFileName(currentPath)}");

                FileSystemEntry[] entries = null;
                try 
                {
                    entries = FileBrowserHelpers.GetEntriesInDirectory(currentPath, false);
                }
                catch (System.Exception e)
                {
                    LogDebug($"EXCEPTION in {currentPath}: {e.Message}");
                    continue;
                }
                
                if (entries == null) continue;

                foreach (FileSystemEntry entry in entries)
                {
                    if (entry.IsDirectory)
                    {
                        directoriesToScan.Push(entry.Path);
                    }
                    else
                    {
                        string entryName = entry.Name.ToLower();
                        if (entryName.EndsWith(".mp3") || entryName.EndsWith(".wav") || entryName.EndsWith(".ogg"))
                        {
                            validAudioFound++;
                            
                            // Unified Fast Path for ALL platforms
                            songs.Add(new SongInfo 
                            { 
                                Title = entry.Name, 
                                Clip = null, 
                                FilePath = entry.Path,
                                Duration = 0,
                                MetadataLoaded = false
                            });
                        }
                    }
                }
                
                // Yield occasionally to prevent long hitches with many folders
                yield return null;
            }

            LogDebug($"FINISHED! Songs in playlist: {songs.Count}");
            
            if (metadataScannerCoroutine != null) StopCoroutine(metadataScannerCoroutine);
            metadataScannerCoroutine = StartCoroutine(MetadataScannerCoroutine());
            
            OnPlaylistChanged?.Invoke();
            SavePlaylist();
            yield break;
        }

        private void SavePlaylist()
        {
            PlaylistData data = new PlaylistData();
            foreach (var song in songs)
            {
                if (!string.IsNullOrEmpty(song.FilePath))
                    data.paths.Add(song.FilePath);
            }

            try
            {
                string json = JsonUtility.ToJson(data);
                File.WriteAllText(GetPlaylistSavePath(), json);
            }
            catch (Exception e)
            {
                LogDebug($"SAVE ERROR: {e.Message}");
            }
        }

        private void LoadPlaylist()
        {
            string path = GetPlaylistSavePath();
            if (!File.Exists(path)) return;

            try
            {
                string json = File.ReadAllText(path);
                PlaylistData data = JsonUtility.FromJson<PlaylistData>(json);
                
                if (data != null && data.paths != null)
                {
                    foreach (string filePath in data.paths)
                    {
                        // Avoid duplicates if already in list from inspector
                        if (songs.Exists(s => s.FilePath == filePath)) continue;

                        songs.Add(new SongInfo
                        {
                            Title = Path.GetFileName(filePath),
                            FilePath = filePath,
                            MetadataLoaded = false
                        });
                    }
                    
                    if (metadataScannerCoroutine != null) StopCoroutine(metadataScannerCoroutine);
                    metadataScannerCoroutine = StartCoroutine(MetadataScannerCoroutine());
                    OnPlaylistChanged?.Invoke();
                }
            }
            catch (Exception e)
            {
                LogDebug($"LOAD ERROR: {e.Message}");
            }
        }

        private string GetPlaylistSavePath()
        {
            return Path.Combine(Application.persistentDataPath, "winamp_playlist.json");
        }

        private IEnumerator MetadataScannerCoroutine()
        {
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
                        song.Title = $"{meta[1]} - {meta[0]}";
                    }
                }
#endif
                song.MetadataLoaded = true;
                OnSongMetadataUpdated?.Invoke(i, song);
                yield return null; // Wait for next frame to avoid hitching
            }
            metadataScannerCoroutine = null;
        }

        public IEnumerator LoadSongClip(SongInfo song)
        {
            if (song == null || song.Clip != null) yield break;

            LogDebug($"> Loading: {song.Title}");

            AudioType type = AudioType.UNKNOWN;
            string ext = song.Title.ToLower();
            if (ext.EndsWith(".mp3")) type = AudioType.MPEG;
            else if (ext.EndsWith(".wav")) type = AudioType.WAV;
            else if (ext.EndsWith(".ogg")) type = AudioType.OGGVORBIS;

            string finalUrl = "";

            if (Application.platform == RuntimePlatform.Android)
            {
                if (FileBrowserHelpers.FileExists(song.FilePath))
                {
                    finalUrl = song.FilePath; 
                    if (!finalUrl.StartsWith("content://") && !finalUrl.StartsWith("file://"))
                        finalUrl = "file:///" + finalUrl.TrimStart('/');
                }
                else
                {
                    LogDebug($"> <color=red>File not found: {song.FilePath}</color>");
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

                    LogDebug($"<color=green>LOADED: {clip.name}</color>");
                }
                else
                {
                    LogDebug($"<color=red>LOAD ERROR: {www.error}</color>");
                }
            }
        }

        public void SetCurrentClip(int index)
        {
            if (songs.Count == 0) return;
            if (index < 0 || index >= songs.Count) return;
            bool changed = (currentIndex != index);
            currentIndex = index;
            if (changed)
            {
                OnCurrentIndexChanged?.Invoke(currentIndex);
                if (SettingsManager.Instance != null)
                    SettingsManager.Instance.LastPlaylistIndex = currentIndex;
            }
        }

        internal SongInfo GetNextSong()
        {
            if (songs.Count == 0) return null;
            
            if (shuffleEnabled)
            {
                // Random next song
                int newIndex = UnityEngine.Random.Range(0, songs.Count);
                SetCurrentClip(newIndex);
            }
            else
            {
                // Sequential
                int newIndex = (currentIndex == songs.Count - 1) ? 0 : currentIndex + 1;
                SetCurrentClip(newIndex);
            }
            
            return CurrentSong;
        }

        public void ToggleShuffle()
        {
            shuffleEnabled = !shuffleEnabled;
            LogDebug($"Shuffle: {(shuffleEnabled ? "ON" : "OFF")}");
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

        private void OnApplicationQuit()
        {
            // No cleanup needed anymore since we don't copy files!
        }
    }
}
