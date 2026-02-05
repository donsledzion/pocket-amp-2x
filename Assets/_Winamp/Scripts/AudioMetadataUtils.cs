using UnityEngine;

namespace SoftAware
{
    public static class AudioMetadataUtils
    {
        public static int EstimateBitrate(AudioClip clip)
        {
            if (clip == null || clip.length == 0) return 0;

            // Estimate based on uncompressed PCM data
            // (sample_rate * channels * bits_per_sample) / 1000
            int uncompressedBitrate = (clip.frequency * clip.channels * 16) / 1000;

            // For compressed formats (MP3, OGG), assume ~12.5% of uncompressed
            int estimatedBitrate = uncompressedBitrate / 8;

            return Mathf.Clamp(estimatedBitrate, 32, 320);
        }

        public static string FormatTime(float seconds, bool hideLeadingZero = true)
        {
            int m = (int)(seconds / 60);
            int s = (int)(seconds % 60);
            if (hideLeadingZero)
                return $"{m}:{s:D2}";
            return $"{m:D2}:{s:D2}";
        }
    }
}
