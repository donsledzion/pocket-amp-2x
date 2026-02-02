using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace SoftAware
{
    public class SpectrumVisualizer : MonoBehaviour, IPointerClickHandler
    {
        public enum VisMode { Spectrum, Waveform }

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
#if UNITY_ANDROID && !UNITY_EDITOR
                // On Android, we don't use audioSource.isPlaying because we play natively
#else
                if (audioSource == null || !audioSource.isPlaying)
                {
                    UpdatePeaks(null);
                    ClearTexture();
                    visualizerTexture.Apply();
                    return;
                }
#endif

                ClearTexture();

                if (currentMode == VisMode.Spectrum)
                    DrawSpectrum();
                else
                    DrawWaveform();

                visualizerTexture.Apply();
            }
            catch (System.Exception e)
            {
                Playlist.Log($"[Vis] Update ERR: {e.Message}\n{e.StackTrace.Substring(0, Mathf.Min(e.StackTrace.Length, 100))}");
            }
        }

        private void DrawSpectrum()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            spectrumData = AndroidVisualizerBridge.GetFFTData(512);
#else
            audioSource.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);
#endif
            UpdatePeaks(spectrumData);

            int barWidth = Width / spectrumBars;
            for (int i = 0; i < spectrumBars; i++)
            {
                float val = spectrumData[i + 1] * 20f;
                int barHeight = Mathf.Clamp(Mathf.RoundToInt(val * Height), 0, Height);

                // Draw bar
                for (int y = 0; y < barHeight; y++)
                {
                    int colorIdx = 17 - y; 
                    if (colorIdx < 2) colorIdx = 2;

                    for (int x = 0; x < barWidth - 1; x++)
                    {
                        visualizerTexture.SetPixel(i * barWidth + x, y, palette[colorIdx]);
                    }
                }

                // Draw peak
                if (showPeaks)
                {
                    int peakY = Mathf.Clamp(Mathf.RoundToInt(peakHeights[i] * Height), 0, Height - 1);
                    for (int x = 0; x < barWidth - 1; x++)
                    {
                        visualizerTexture.SetPixel(i * barWidth + x, peakY, palette[23]);
                    }
                }
            }
        }

        private void UpdatePeaks(float[] data)
        {
            for (int i = 0; i < spectrumBars; i++)
            {
                float val = (data != null && i + 1 < data.Length) ? data[i + 1] * 20f : 0;
                
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

        private void DrawWaveform()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            waveformData = AndroidVisualizerBridge.GetWaveformData(512);
#else
            audioSource.GetOutputData(waveformData, 0);
#endif

            for (int x = 0; x < Width; x++)
            {
                float val = waveformData[x % 512];
                int y = Mathf.Clamp(Mathf.RoundToInt((val + 1f) * 0.5f * Height), 0, Height - 1);
                
                // Draw only a single pixel at the calculated Y position
                visualizerTexture.SetPixel(x, y, palette[18]);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            currentMode = (currentMode == VisMode.Spectrum) ? VisMode.Waveform : VisMode.Spectrum;
        }
    }
}
