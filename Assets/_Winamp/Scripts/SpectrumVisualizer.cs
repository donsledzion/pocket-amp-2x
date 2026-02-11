using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace SoftAware.Winamp
{
    public class SpectrumVisualizer : MonoBehaviour, IPointerClickHandler
    {
        public enum VisMode { Spectrum, Waveform, None }

        [Header("References")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private RawImage outputImage;

        [Header("Settings")]
        [SerializeField] private VisMode currentMode = VisMode.Spectrum;
        [SerializeField] private int spectrumBars = 19; // Winamp has about 19-32 bars depending on scaling, 76/4 = 19

        [Header("Peaks")]
        [SerializeField] private bool showPeaks = true;
        [SerializeField] private float peakFallSpeed = 0.5f;
        [SerializeField] private float peakHoldTime = 0.1f;
        
        public System.Action<VisMode> OnModeChanged;

        private Texture2D visualizerTexture;
        private Color[] palette;
        private float[] spectrumData = new float[512];
        private float[] waveformData = new float[512];
        private float[] peakHeights;
        private float[] peakTimers;
        private Color colorTransparent;

        private const int Width = 76;
        private const int Height = 16;

        private void Awake()
        {
            InitializePalette();
            CreateTexture();
            peakHeights = new float[spectrumBars];
            peakTimers = new float[spectrumBars];
        }

        private void InitializePalette()
        {
            palette = new Color[24];
            palette[0] = new Color(0, 0, 0, 0); // Transparent background
            palette[1] = FromRGB(24, 33, 41);    // dots
            palette[2] = FromRGB(239, 49, 16);   // top spect
            palette[3] = FromRGB(206, 41, 16);
            palette[4] = FromRGB(214, 90, 0);
            palette[5] = FromRGB(214, 102, 0);
            palette[6] = FromRGB(214, 115, 0);
            palette[7] = FromRGB(198, 123, 8);
            palette[8] = FromRGB(222, 165, 24);
            palette[9] = FromRGB(214, 181, 33);
            palette[10] = FromRGB(189, 222, 41);
            palette[11] = FromRGB(148, 222, 33);
            palette[12] = FromRGB(41, 206, 16);
            palette[13] = FromRGB(50, 190, 16);
            palette[14] = FromRGB(57, 181, 16);
            palette[15] = FromRGB(49, 156, 8);
            palette[16] = FromRGB(41, 148, 0);
            palette[17] = FromRGB(24, 132, 8);   // bottom spect
            palette[18] = FromRGB(255, 255, 255); // osc 1
            palette[19] = FromRGB(214, 214, 222); // osc 2
            palette[20] = FromRGB(181, 189, 189); // osc 3
            palette[21] = FromRGB(160, 170, 175); // osc 4
            palette[22] = FromRGB(148, 156, 165); // osc 5
            palette[23] = FromRGB(150, 150, 150); // peak dots

            colorTransparent = palette[0];
        }

        private Color FromRGB(int r, int g, int b) => new Color(r / 255f, g / 255f, b / 255f);

        private void CreateTexture()
        {
            // Use RGBA32 to support transparency
            visualizerTexture = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
            visualizerTexture.filterMode = FilterMode.Point;
            visualizerTexture.wrapMode = TextureWrapMode.Clamp;
            outputImage.texture = visualizerTexture;
            ClearTexture();
        }

        private void ClearTexture()
        {
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    visualizerTexture.SetPixel(x, y, colorTransparent);
        }

        private void Update()
        {
            try
            {
                var player = audioSource != null ? audioSource.GetComponent<AudioPlayer>() : null;
                float vol = (player != null) ? player.CurrentVolume : 1f;

#if UNITY_ANDROID && !UNITY_EDITOR
                if (player != null)
                {
                    if (player.IsPaused) return; // Frozen
                    
                    if (!player.IsPlaying)
                    {
                        // Stopped
                        ClearTexture();
                        visualizerTexture.Apply();
                        return;
                    }
                }
#else
                if (audioSource == null) return;
                
                bool isPausedNow = player != null && player.IsPaused;

                if (!audioSource.isPlaying && !isPausedNow)
                {
                    // Stopped - clear texture
                    UpdatePeaks(null, 1.0f);
                    ClearTexture();
                    visualizerTexture.Apply();
                    return;
                }

                if (isPausedNow) return; // Frozen - skip draw but don't clear
#endif

                if (currentMode == VisMode.None)
                {
                    ClearTexture();
                    visualizerTexture.Apply();
                    return;
                }

                ClearTexture();

                if (currentMode == VisMode.Spectrum)
                    DrawSpectrum(vol);
                else if (currentMode == VisMode.Waveform)
                    DrawWaveform(vol);

                visualizerTexture.Apply();
            }
            catch (System.Exception e)
            {
                Playlist.Log($"[Vis] Update ERR: {e.Message}");
            }
        }

        // Calibrated Winamp frequency ranges for 19 bars
        // Shifted higher thresholds to ensure "Kick" hits early bars (0-2)
        private static readonly int[] WINAMP_RANGES_HZ = {
            100, 200, 300, 450, 600, 900, 1300, 1800, 2500, 3300, 4500, 6000, 8000, 10000, 12000, 14000, 16000, 18000, 21000
        };

        private void DrawSpectrum(float volume)
        {
            float[] bars = new float[spectrumBars];

#if UNITY_ANDROID && !UNITY_EDITOR
            // Android: Get pre-processed (now with high treble boost) 19 bars
            bars = AndroidVisualizerBridge.GetWinampFFT();
#else
            // Desktop: Get raw FFT and group into 19 log bars
            audioSource.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);
            
            float nyquist = AudioSettings.outputSampleRate / 2.0f;
            float binWidth = nyquist / spectrumData.Length;
            int startBin = 1;

            for (int i = 0; i < spectrumBars; i++)
            {
                int endBin = Mathf.FloorToInt(WINAMP_RANGES_HZ[i] / binWidth);
                if (endBin <= startBin) endBin = startBin + 1;
                if (endBin > spectrumData.Length) endBin = spectrumData.Length;

                float maxVal = 0;
                for (int b = startBin; b < endBin; b++)
                {
                    if (spectrumData[b] > maxVal) maxVal = spectrumData[b];
                }

                // Desktop normalization: 
                // Unity FFT results are linear power. We use sqrt to get amplitude, 
                // then apply a calibrated multiplier and a milder Treble Boost.
                float amplitude = Mathf.Sqrt(maxVal); 
                float trebleBoost = 1.0f + (i * 0.15f); // 1x to ~3.8x boost
                bars[i] = amplitude * 3.5f * trebleBoost;
                
                startBin = endBin;
            }
#endif

            // Update peaks
            UpdatePeaks(bars, 1.0f);

            int barWidth = Width / spectrumBars;
            for (int i = 0; i < spectrumBars; i++)
            {
                float val = bars[i];
                int barHeight = Mathf.Clamp(Mathf.RoundToInt(val * Height), 0, Height);

                for (int y = 0; y < barHeight; y++)
                {
                    int colorIdx = 17 - y; 
                    if (colorIdx < 2) colorIdx = 2;

                    for (int x = 0; x < barWidth - 1; x++)
                    {
                        visualizerTexture.SetPixel(i * barWidth + x, y, palette[colorIdx]);
                    }
                }

                if (showPeaks)
                {
                    int peakY = Mathf.Clamp(Mathf.RoundToInt(peakHeights[i] * Height), 0, Height - 1);
                    if (peakY >= 0)
                    {
                        for (int x = 0; x < barWidth - 1; x++)
                        {
                            visualizerTexture.SetPixel(i * barWidth + x, peakY, palette[23]);
                        }
                    }
                }
            }
        }

        private void UpdatePeaks(float[] data, float multiplier = 1.0f)
        {
            for (int i = 0; i < spectrumBars; i++)
            {
                float val;
                if (data == null) val = 0;
                else if (data.Length == spectrumBars) val = data[i] * multiplier; // Exact match (Android)
                else if (i + 1 < data.Length) val = data[i + 1] * multiplier; // Raw FFT (skip DC)
                else val = 0;
                
                if (val >= peakHeights[i])
                {
                    peakHeights[i] = val;
                    peakTimers[i] = peakHoldTime;
                }
                else
                {
                    if (peakTimers[i] > 0)
                    {
                        peakTimers[i] -= Time.deltaTime;
                    }
                    else
                    {
                        peakHeights[i] -= peakFallSpeed * Time.deltaTime;
                    }
                }

                peakHeights[i] = Mathf.Clamp(peakHeights[i], 0, 1f);
            }
        }

        private void DrawWaveform(float volume)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            waveformData = AndroidVisualizerBridge.GetWaveformData(512);
            if (waveformData == null || waveformData.Length == 0) return;
            float baseMult = 0.8f; 
#else
            audioSource.GetOutputData(waveformData, 0);
            float baseMult = 1.0f;
#endif
#if UNITY_ANDROID && !UNITY_EDITOR
            float multiplier = baseMult; 
#else
            float sensitivity = 1.0f / Mathf.Max(volume, 0.1f);
            float multiplier = baseMult * sensitivity;
#endif

            int centerY = Height / 2;
            int lastY = centerY;
            float samplesPerPixel = 512f / Width;

            for (int x = 0; x < Width; x++)
            {
                int startIdx = Mathf.FloorToInt(x * samplesPerPixel);
                int endIdx = Mathf.FloorToInt((x + 1) * samplesPerPixel);
                float sum = 0;
                int count = 0;

                for (int i = startIdx; i < endIdx && i < waveformData.Length; i++)
                {
                    sum += waveformData[i];
                    count++;
                }

                float avgVal = (count > 0) ? (sum / count) : 0;
                float val = avgVal * multiplier;
                
                int y = Mathf.Clamp(Mathf.RoundToInt((val + 1f) * 0.5f * Height), 0, Height - 1);
                
                int startSeg = Mathf.Min(y, lastY);
                int endSeg = Mathf.Max(y, lastY);

                for (int ty = startSeg; ty <= endSeg; ty++)
                {
                    visualizerTexture.SetPixel(x, ty, palette[18]);
                }
                
                lastY = y;
            }
        }

        private void OnDisable()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            AndroidVisualizerBridge.Release();
#endif
        }

        public void SetMode(VisMode mode)
        {
            currentMode = mode;
            OnModeChanged?.Invoke(currentMode);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // Cycle: Spectrum -> Waveform -> None -> Spectrum
            if (currentMode == VisMode.Spectrum) currentMode = VisMode.Waveform;
            else if (currentMode == VisMode.Waveform) currentMode = VisMode.None;
            else currentMode = VisMode.Spectrum;

            Playlist.Log($"[Vis] Clicked! New Mode: {currentMode}");
            OnModeChanged?.Invoke(currentMode);
        }
    }
}
