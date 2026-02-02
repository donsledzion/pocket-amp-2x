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
            public AudioClip Clip;
            public string FilePath;
            
            public bool HasNativePath => !string.IsNullOrEmpty(FilePath);
        }

        [SerializeField] private List<SongInfo> songs = new List<SongInfo>();
        [SerializeField] private TextMeshProUGUI debugText;
        
        private int currentIndex;
        internal SongInfo CurrentSong => songs.Count > 0 ? songs[currentIndex] : null;
        internal AudioClip CurrentClip => CurrentSong?.Clip;
        internal int Count => songs.Count;

        private void Start()
        {
            InitializeInspectorSongs();

#if UNITY_ANDROID && !UNITY_EDITOR
            StartCoroutine(CheckPermissionsCoroutine());
#endif

            if (songs.Count > 0)
                SetCurrentClip(0);
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

            if (Application.platform == RuntimePlatform.Android)
            {
                // Request both permissions if they are not granted
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

        private void LogDebug(string message, bool append = true)
        {
            if (debugText == null) return;
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

        private IEnumerator LoadDirectoryCoroutine(string path)
        {
            LogDebug("Scanning folder...", false);
            FileSystemEntry[] entries = null;
            try 
            {
                entries = FileBrowserHelpers.GetEntriesInDirectory(path, false);
            }
            catch (System.Exception e)
            {
                LogDebug($"EXCEPTION: {e.Message}");
                yield break;
            }
            
            if (entries == null)
            {
                LogDebug("ERROR: result is NULL");
                yield break;
            }

            LogDebug($"FOUND: {entries.Length} entries.");

            int validAudioFound = 0;
            foreach (FileSystemEntry entry in entries)
            {
                if (entry.IsDirectory) continue;

                string entryName = entry.Name.ToLower();
                if (entryName.EndsWith(".mp3") || entryName.EndsWith(".wav") || entryName.EndsWith(".ogg"))
                {
                    validAudioFound++;
                    LogDebug($"[#{validAudioFound}] Loading: {entry.Name}");
                    
                    // Sequential loading is more stable for Android file system (I/O)
                    yield return LoadAudioClip(entry.Path, entry.Name);
                    
                    LogDebug($"Total Songs: {songs.Count}");
                }
            }

            LogDebug($"FINISHED! Songs in playlist: {songs.Count}");
            yield break;
        }

        private IEnumerator LoadAudioClip(string filePath, string fileName)
        {
            LogDebug($"> Processing: {fileName}");

            AudioType type = AudioType.UNKNOWN;
            string ext = fileName.ToLower();
            if (ext.EndsWith(".mp3")) type = AudioType.MPEG;
            else if (ext.EndsWith(".wav")) type = AudioType.WAV;
            else if (ext.EndsWith(".ogg")) type = AudioType.OGGVORBIS;

            string finalUrl = "";

            if (Application.platform == RuntimePlatform.Android)
            {
                // Use persistentDataPath for ANA compatibility
                string cachePath = Path.Combine(Application.persistentDataPath, fileName);
                
                LogDebug($"> Copying to cache...");
                
                // FileBrowserHelpers.CopyFile returns void in some versions
                FileBrowserHelpers.CopyFile(filePath, cachePath);
                
                if (File.Exists(cachePath))
                {
                    LogDebug($"> Copy SUCCESS");
                    // Store only the filename for Android relative to persistentDataPath
                    finalUrl = "file:///" + cachePath.TrimStart('/');
                }
                else
                {
                    LogDebug($"> <color=red>Copy FAILED!</color>");
                    yield break;
                }
            }
            else
            {
                finalUrl = "file:///" + filePath.TrimStart('/');
            }

            LogDebug($"> Requesting: {Path.GetFileName(finalUrl)}");

            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(finalUrl, type))
            {
                www.timeout = 10;
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                    clip.name = fileName;
                    
                    songs.Add(new SongInfo 
                    { 
                        Title = fileName, 
                        Clip = clip, 
                        // For Android, store relative to persistentDataPath (as required by ANAMusic)
                        // For Editor, store absolute path.
                        FilePath = Application.platform == RuntimePlatform.Android ? 
                                   fileName : filePath 
                    });

                    LogDebug($"<color=green>SUCCESS: {clip.name}</color>");
                    
                    // Note: We NO LONGER delete the cached file here, 
                    // because ANAMusic needs it to play natively on Android.
                    // We should cleanup on application quit or when removing from playlist.
                }
                else
                {
                    LogDebug($"<color=red>LOAD ERROR: {www.error}</color>");
                }
            }
        }

        private void SetCurrentClip(int index)
        {
            if (songs.Count == 0) return;
            if (index < 0 || index >= songs.Count) return;
            currentIndex = index;
        }

        internal SongInfo GetNextSong()
        {
            if (songs.Count == 0) return null;
            SetCurrentClip(currentIndex == songs.Count - 1 ? 0 : ++currentIndex);
            return CurrentSong;
        }

        internal SongInfo GetPreviousSong()
        {
            if (songs.Count == 0) return null;
            SetCurrentClip(currentIndex == 0 ? songs.Count - 1 : --currentIndex);
            return CurrentSong;
        }

        private void OnApplicationQuit()
        {
            // Cleanup cached files on exit
            if (Application.platform == RuntimePlatform.Android)
            {
                foreach (var song in songs)
                {
                    // Check if file is in persistentDataPath before deleting
                    if (!string.IsNullOrEmpty(song.FilePath))
                    {
                        string fullPath = Path.Combine(Application.persistentDataPath, song.FilePath);
                        if (File.Exists(fullPath))
                        {
                            try { File.Delete(fullPath); } catch { }
                        }
                    }
                }
            }
        }
    }
}
