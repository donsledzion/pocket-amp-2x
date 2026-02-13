using UnityEngine;

namespace SoftAware
{
    /// <summary>
    /// Bridge to retrieve audio metadata (bitrate, sample rate) from Android native layer.
    /// </summary>
    public static class AndroidAudioInfoBridge
    {
        private static AndroidJavaObject audioService;

        private static void Initialize()
        {
            if (audioService != null) return;

#if UNITY_ANDROID && !UNITY_EDITOR
            using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                var context = player.GetStatic<AndroidJavaObject>("currentActivity");
                audioService = new AndroidJavaObject("com.softaware.winamp.WinampAudioService");
            }
#endif
        }

        /// <summary>
        /// Gets the sample rate (in Hz) for the given audio file.
        /// </summary>
        public static int GetSampleRate(string filePath)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (string.IsNullOrEmpty(filePath)) return 0;

            try
            {
                using (var extractor = new AndroidJavaObject("android.media.MediaExtractor"))
                {
                    SetDataSource(extractor, filePath);
                    
                    int trackCount = extractor.Call<int>("getTrackCount");
                    
                    for (int i = 0; i < trackCount; i++)
                    {
                        var format = extractor.Call<AndroidJavaObject>("getTrackFormat", i);
                        string mime = format.Call<string>("getString", "mime");
                        
                        if (mime.StartsWith("audio/"))
                        {
                            int sampleRate = 0;
                            if (format.Call<bool>("containsKey", "sample-rate"))
                                sampleRate = format.Call<int>("getInteger", "sample-rate");
                            
                            format.Dispose();
                            extractor.Call("release");
                            return sampleRate;
                        }
                        
                        format.Dispose();
                    }
                    
                    extractor.Call("release");
                }
            }
            catch
            {
                // Silent failure in production
            }
#endif
            return 0;
        }

        /// <summary>
        /// Gets the channel count (1 for mono, 2 for stereo) for the given audio file.
        /// </summary>
        public static int GetChannelCount(string filePath)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (string.IsNullOrEmpty(filePath)) return 0;

            try
            {
                using (var extractor = new AndroidJavaObject("android.media.MediaExtractor"))
                {
                    SetDataSource(extractor, filePath);
                    
                    int trackCount = extractor.Call<int>("getTrackCount");
                    for (int i = 0; i < trackCount; i++)
                    {
                        var format = extractor.Call<AndroidJavaObject>("getTrackFormat", i);
                        string mime = format.Call<string>("getString", "mime");
                        
                        if (mime.StartsWith("audio/"))
                        {
                            int channels = 0;
                            if (format.Call<bool>("containsKey", "channel-count"))
                                channels = format.Call<int>("getInteger", "channel-count");
                            
                            format.Dispose();
                            extractor.Call("release");
                            return channels;
                        }
                        
                        format.Dispose();
                    }
                    
                    extractor.Call("release");
                }
            }
            catch
            {
                // Silent
            }
#endif
            return 0;
        }

        /// <summary>
        /// Gets the bitrate (in bps) for the given audio file.
        /// </summary>
        public static int GetBitrate(string filePath)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (string.IsNullOrEmpty(filePath)) return 0;

            try
            {
                using (var extractor = new AndroidJavaObject("android.media.MediaExtractor"))
                {
                    SetDataSource(extractor, filePath);
                    
                    int trackCount = extractor.Call<int>("getTrackCount");
                    for (int i = 0; i < trackCount; i++)
                    {
                        var format = extractor.Call<AndroidJavaObject>("getTrackFormat", i);
                        string mime = format.Call<string>("getString", "mime");
                        
                        if (mime.StartsWith("audio/"))
                        {
                            // Try to get bitrate - may not be available for all formats
                            int bitrate = 0;
                            if (format.Call<bool>("containsKey", "bitrate"))
                            {
                                bitrate = format.Call<int>("getInteger", "bitrate");
                            }

                            if (bitrate > 0)
                            {
                                format.Dispose();
                                extractor.Call("release");
                                return bitrate;
                            }
                            else
                            {
                                // Bitrate not available or 0, estimate from file size and duration
                                long duration = 0;
                                if (format.Call<bool>("containsKey", "durationUs"))
                                    duration = format.Call<long>("getLong", "durationUs");
                                
                                format.Dispose();
                                extractor.Call("release");
                                
                                if (duration > 0)
                                {
                                    return EstimateBitrateFromFile(filePath, duration);
                                }
                            }
                        }
                        
                        format.Dispose();
                    }
                    
                    extractor.Call("release");
                }
            }
            catch
            {
                // Silent
            }
#endif
            return 0;
        }

        /// <summary>
        /// Gets the duration (in seconds) for the given audio file.
        /// </summary>
        public static float GetDuration(string filePath)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (string.IsNullOrEmpty(filePath)) return 0;

            try
            {
                using (var extractor = new AndroidJavaObject("android.media.MediaExtractor"))
                {
                    SetDataSource(extractor, filePath);
                    
                    int trackCount = extractor.Call<int>("getTrackCount");
                    for (int i = 0; i < trackCount; i++)
                    {
                        var format = extractor.Call<AndroidJavaObject>("getTrackFormat", i);
                        string mime = format.Call<string>("getString", "mime");
                        
                        if (mime.StartsWith("audio/"))
                        {
                            long durationUs = 0;
                            if (format.Call<bool>("containsKey", "durationUs"))
                                durationUs = format.Call<long>("getLong", "durationUs");
                            
                            format.Dispose();
                            extractor.Call("release");
                            return durationUs / 1000000f;
                        }
                        
                        format.Dispose();
                    }
                    
                    extractor.Call("release");
                }
            }
            catch { }
#endif
            return 0;
        }

        public static void SetDataSource(AndroidJavaObject extractor, string path)
        {
            if (path.StartsWith("content://"))
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var contentResolver = currentActivity.Call<AndroidJavaObject>("getContentResolver"))
                using (var uriClass = new AndroidJavaClass("android.net.Uri"))
                using (var uri = uriClass.CallStatic<AndroidJavaObject>("parse", path))
                using (var pfd = contentResolver.Call<AndroidJavaObject>("openFileDescriptor", uri, "r"))
                using (var fd = pfd.Call<AndroidJavaObject>("getFileDescriptor"))
                {
                    extractor.Call("setDataSource", fd);
                }
#endif
            }
            else
            {
                extractor.Call("setDataSource", path);
            }
        }

        /// <summary>
        /// Retrieves metadata (Title, Artist) for the given audio file using MediaMetadataRetriever.
        /// Returns a string array [Title, Artist]
        /// </summary>
        public static string[] GetMetadata(string filePath)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (string.IsNullOrEmpty(filePath)) return new string[] { "", "" };

            try
            {
                using (var retriever = new AndroidJavaObject("android.media.MediaMetadataRetriever"))
                {
                    SetRetrieverDataSource(retriever, filePath);
                    
                    // Metadata keys: TITLE = 7, ARTIST = 2
                    string title = retriever.Call<string>("extractMetadata", 7);
                    string artist = retriever.Call<string>("extractMetadata", 2);
                    
                    retriever.Call("release");
                    return new string[] { title ?? "", artist ?? "" };
                }
            }
            catch { }
#endif
            return new string[] { "", "" };
        }

        /// <summary>
        /// Attempts to get a clean file name from a path or content URI.
        /// </summary>
        public static string GetFileName(string path)
        {
            if (string.IsNullOrEmpty(path)) return "";

#if UNITY_ANDROID && !UNITY_EDITOR
            if (path.StartsWith("content://"))
            {
                try
                {
                    using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                    using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                    using (var contentResolver = currentActivity.Call<AndroidJavaObject>("getContentResolver"))
                    using (var uriClass = new AndroidJavaClass("android.net.Uri"))
                    using (var uri = uriClass.CallStatic<AndroidJavaObject>("parse", path))
                    {
                        // Open cursor for the URI
                        // Columns: _display_name (standard for SAF)
                        using (var cursor = contentResolver.Call<AndroidJavaObject>("query", uri, null, null, null, null))
                        {
                            if (cursor != null && cursor.Call<bool>("moveToFirst"))
                            {
                                int nameIndex = cursor.Call<int>("getColumnIndex", "_display_name");
                                if (nameIndex != -1)
                                {
                                    string name = cursor.Call<string>("getString", nameIndex);
                                    if (!string.IsNullOrEmpty(name)) return name;
                                }
                            }
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[AndroidAudioInfoBridge] Failed to query display name for {path}: {e.Message}");
                }

                // Fallback for content URIs: Try to unescape the last segment
                try 
                {
                    string lastSegment = path.Substring(path.LastIndexOf('/') + 1);
                    return System.Uri.UnescapeDataString(lastSegment);
                }
                catch { }
            }
#endif
            // Standard file path or fallback
            try
            {
                string fileName = System.IO.Path.GetFileName(path);
                // Even for regular paths, it might be URL encoded if it came from certain Android sources
                if (fileName.Contains("%"))
                {
                    return System.Uri.UnescapeDataString(fileName);
                }
                return fileName;
            }
            catch
            {
                return path;
            }
        }

        private static void SetRetrieverDataSource(AndroidJavaObject retriever, string path)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (path.StartsWith("content://"))
            {
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var uriClass = new AndroidJavaClass("android.net.Uri"))
                using (var uri = uriClass.CallStatic<AndroidJavaObject>("parse", path))
                {
                    retriever.Call("setDataSource", currentActivity, uri);
                }
            }
            else
            {
                retriever.Call("setDataSource", path);
            }
#endif
        }

        private static int EstimateBitrateFromFile(string filePath, long durationUs)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                if (filePath.StartsWith("content://"))
                {
                    using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                    using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                    using (var contentResolver = currentActivity.Call<AndroidJavaObject>("getContentResolver"))
                    using (var uriClass = new AndroidJavaClass("android.net.Uri"))
                    using (var uri = uriClass.CallStatic<AndroidJavaObject>("parse", filePath))
                    using (var pfd = contentResolver.Call<AndroidJavaObject>("openFileDescriptor", uri, "r"))
                    {
                        long fileSize = pfd.Call<long>("getStatSize");
                        double durationSeconds = durationUs / 1000000.0;
                        if (durationSeconds > 0)
                        {
                            return (int)((fileSize * 8) / durationSeconds);
                        }
                    }
                    return 0;
                }

                using (var file = new AndroidJavaObject("java.io.File", filePath))
                {
                    long fileSize = file.Call<long>("length");
                    
                    // bitrate (bps) = (fileSize * 8) / (duration in seconds)
                    double durationSeconds = durationUs / 1000000.0;
                    if (durationSeconds > 0)
                    {
                        return (int)((fileSize * 8) / durationSeconds);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[AndroidAudioInfoBridge] Failed to estimate bitrate: {e.Message}");
            }
#endif
            return 0;
        }
    }
}
