# Android Native Audio - Spectrum Visualizer Implementation Guide

## Goal
Implement a 19-bar spectrum visualizer (Winamp 2.x style) for Unity app using Android Native Audio plugin.

## Problem Context
- **Android Native Audio** plays audio outside Unity's Audio Engine (native Android MediaPlayer/AudioTrack)
- Unity's `AudioSource.GetSpectrumData()` doesn't work because audio stream is not accessible to Unity
- Solution: Extract FFT data on native Android side using **Visualizer API** and pass to Unity

---

## Implementation Steps

### 1. Android Manifest Permissions

Add required permissions to `AndroidManifest.xml`:

```xml
<uses-permission android:name="android.permission.MODIFY_AUDIO_SETTINGS" />
<uses-permission android:name="android.permission.RECORD_AUDIO" />
```

---

### 2. Java/Kotlin - Native Side Implementation

#### Initialize Visualizer

```java
import android.media.audiofx.Visualizer;
import android.media.MediaPlayer;

public class SpectrumAnalyzer {
    private Visualizer visualizer;
    private static final int CAPTURE_SIZE = 128; // Will give us 64 frequency bands
    private static final int BAR_COUNT = 19; // Winamp 2.x style
    
    public void initialize(int audioSessionId) {
        // Get audio session ID from your MediaPlayer/AudioTrack
        // Example: int audioSessionId = mediaPlayer.getAudioSessionId();
        
        visualizer = new Visualizer(audioSessionId);
        
        // Set capture size (must be power of 2)
        visualizer.setCaptureSize(CAPTURE_SIZE);
        
        // Setup data capture listener
        visualizer.setDataCaptureListener(
            new Visualizer.OnDataCaptureListener() {
                @Override
                public void onFftDataCapture(Visualizer visualizer, byte[] fft, int samplingRate) {
                    // Process FFT data and send to Unity
                    processFFTData(fft);
                }
                
                @Override
                public void onWaveFormDataCapture(Visualizer visualizer, byte[] waveform, int samplingRate) {
                    // Not needed for spectrum analyzer
                }
            },
            Visualizer.getMaxCaptureRate() / 2, // Capture rate (Hz)
            false, // waveform - not needed
            true   // fft - this is what we need
        );
        
        // Enable visualizer
        visualizer.setEnabled(true);
    }
    
    private void processFFTData(byte[] fft) {
        // Step 1: Convert FFT byte data to magnitudes
        float[] magnitudes = calculateMagnitudes(fft);
        
        // Step 2: Downsample to 19 bars (Winamp style)
        float[] bars = downsampleTo19Bars(magnitudes);
        
        // Step 3: Send to Unity
        sendToUnity(bars);
    }
    
    private float[] calculateMagnitudes(byte[] fft) {
        // FFT data format from Android Visualizer:
        // [DC, real1, imag1, real2, imag2, ..., nyquist]
        // We get captureSize/2 frequency bands
        
        int numBands = fft.length / 2;
        float[] magnitudes = new float[numBands];
        
        // DC component (index 0)
        magnitudes[0] = (float) Math.abs(fft[0]);
        
        // Calculate magnitude for each frequency band
        // magnitude = sqrt(real^2 + imag^2)
        for (int i = 2; i < fft.length; i += 2) {
            int bandIndex = i / 2;
            
            float real = (float) fft[i];
            float imag = (float) fft[i + 1];
            
            // Calculate magnitude
            float magnitude = (float) Math.sqrt(real * real + imag * imag);
            
            // Normalize to 0-1 range
            magnitudes[bandIndex] = magnitude / 128.0f;
        }
        
        return magnitudes;
    }
    
    private float[] downsampleTo19Bars(float[] magnitudes) {
        // We have 64 frequency bands, need to create 19 bars
        // Strategy: Group frequencies and average them
        // Lower frequencies = more detail (bass/mid)
        // Higher frequencies = less detail (treble)
        
        float[] bars = new float[BAR_COUNT];
        
        // Frequency grouping - logarithmic-like distribution
        // Winamp focuses more on lower frequencies (bass/mids)
        int[] bandRanges = {
            0, 2,    // Bar 0:  bass
            2, 4,    // Bar 1:  bass
            4, 6,    // Bar 2:  bass-mid
            6, 8,    // Bar 3:  bass-mid
            8, 10,   // Bar 4:  mid
            10, 12,  // Bar 5:  mid
            12, 14,  // Bar 6:  mid
            14, 17,  // Bar 7:  mid-high
            17, 20,  // Bar 8:  mid-high
            20, 23,  // Bar 9:  mid-high
            23, 27,  // Bar 10: high
            27, 31,  // Bar 11: high
            31, 35,  // Bar 12: high
            35, 40,  // Bar 13: high
            40, 45,  // Bar 14: very high
            45, 50,  // Bar 15: very high
            50, 55,  // Bar 16: very high
            55, 60,  // Bar 17: ultra high
            60, 64   // Bar 18: ultra high
        };
        
        // Calculate average for each bar
        for (int bar = 0; bar < BAR_COUNT; bar++) {
            int startBand = bandRanges[bar * 2];
            int endBand = bandRanges[bar * 2 + 1];
            
            float sum = 0;
            int count = 0;
            
            for (int band = startBand; band < endBand && band < magnitudes.length; band++) {
                sum += magnitudes[band];
                count++;
            }
            
            bars[bar] = count > 0 ? sum / count : 0;
            
            // Optional: Apply smoothing/boost for visual appeal
            bars[bar] = (float) Math.pow(bars[bar], 0.7); // Slight compression
        }
        
        return bars;
    }
    
    private void sendToUnity(float[] bars) {
        // Convert to comma-separated string for easy parsing in Unity
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < bars.length; i++) {
            sb.append(bars[i]);
            if (i < bars.length - 1) {
                sb.append(",");
            }
        }
        
        // Send to Unity GameObject
        // Replace "SpectrumVisualizer" with your actual GameObject name
        // Replace "OnSpectrumData" with your actual method name
        UnityPlayer.UnitySendMessage("SpectrumVisualizer", "OnSpectrumData", sb.toString());
    }
    
    public void release() {
        if (visualizer != null) {
            visualizer.setEnabled(false);
            visualizer.release();
            visualizer = null;
        }
    }
}
```

---

### 3. Unity - C# Side Implementation

Create a script to receive and visualize spectrum data:

```csharp
using UnityEngine;
using System.Globalization;

public class SpectrumVisualizer : MonoBehaviour
{
    [Header("Visualization Settings")]
    [SerializeField] private GameObject barPrefab;
    [SerializeField] private int barCount = 19;
    [SerializeField] private float barSpacing = 0.5f;
    [SerializeField] private float barWidth = 0.4f;
    [SerializeField] private float maxBarHeight = 5f;
    [SerializeField] private float smoothSpeed = 10f;
    
    private Transform[] bars;
    private float[] targetHeights;
    private float[] currentHeights;
    
    void Start()
    {
        InitializeBars();
    }
    
    void InitializeBars()
    {
        bars = new Transform[barCount];
        targetHeights = new float[barCount];
        currentHeights = new float[barCount];
        
        // Create visual bars
        for (int i = 0; i < barCount; i++)
        {
            GameObject bar = Instantiate(barPrefab, transform);
            bar.transform.localPosition = new Vector3(i * barSpacing, 0, 0);
            bar.transform.localScale = new Vector3(barWidth, 0.1f, barWidth);
            bars[i] = bar.transform;
        }
        
        // Center the visualizer
        transform.position -= new Vector3((barCount - 1) * barSpacing * 0.5f, 0, 0);
    }
    
    void Update()
    {
        // Smooth interpolation to target heights
        for (int i = 0; i < barCount; i++)
        {
            currentHeights[i] = Mathf.Lerp(
                currentHeights[i], 
                targetHeights[i], 
                Time.deltaTime * smoothSpeed
            );
            
            // Apply height with minimum scale
            float height = Mathf.Max(0.1f, currentHeights[i]);
            bars[i].localScale = new Vector3(
                barWidth, 
                height, 
                barWidth
            );
            
            // Adjust position so bars grow upward from base
            bars[i].localPosition = new Vector3(
                i * barSpacing,
                height * 0.5f,
                0
            );
        }
    }
    
    // Called from native Android code via UnitySendMessage
    public void OnSpectrumData(string data)
    {
        // Parse comma-separated values
        string[] values = data.Split(',');
        
        for (int i = 0; i < barCount && i < values.Length; i++)
        {
            if (float.TryParse(values[i], NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
            {
                // Scale to desired height
                targetHeights[i] = value * maxBarHeight;
            }
        }
    }
}
```

---

### 4. Integration with Android Native Audio Plugin

In your existing Android Native Audio integration code:

```java
// After initializing MediaPlayer/AudioTrack
MediaPlayer mediaPlayer = ...; // Your existing player
int audioSessionId = mediaPlayer.getAudioSessionId();

// Initialize spectrum analyzer
SpectrumAnalyzer spectrumAnalyzer = new SpectrumAnalyzer();
spectrumAnalyzer.initialize(audioSessionId);

// Don't forget to release when done
// spectrumAnalyzer.release();
```

---

### 5. Setup Checklist

- [ ] Add permissions to AndroidManifest.xml
- [ ] Implement SpectrumAnalyzer class in Android project
- [ ] Create SpectrumVisualizer GameObject in Unity scene
- [ ] Attach SpectrumVisualizer.cs script to GameObject
- [ ] Create bar prefab (simple cube with material)
- [ ] Assign bar prefab to SpectrumVisualizer
- [ ] Get audio session ID from Android Native Audio player
- [ ] Initialize SpectrumAnalyzer with correct audio session ID
- [ ] Verify GameObject name matches UnitySendMessage call
- [ ] Test on Android device (won't work in Unity Editor)

---

### 6. Troubleshooting

**No bars moving:**
- Check if permissions are granted (especially RECORD_AUDIO)
- Verify audio session ID is correct
- Check Unity GameObject name matches in UnitySendMessage
- Add debug logs in both Java and C# to verify data flow

**Bars moving but look wrong:**
- Adjust `maxBarHeight` in Unity
- Modify `smoothSpeed` for different animation feel
- Tweak frequency band ranges in `downsampleTo19Bars()` for different frequency distribution

**Performance issues:**
- Reduce capture rate in `setDataCaptureListener`
- Increase `smoothSpeed` for less interpolation overhead
- Use object pooling if instantiating/destroying bars dynamically

---

### 7. Optional Enhancements

**Add color gradient based on frequency:**
```csharp
// In SpectrumVisualizer.cs
Color GetBarColor(int index)
{
    float t = (float)index / (barCount - 1);
    return Color.Lerp(Color.red, Color.cyan, t); // Bass = red, Treble = cyan
}
```

**Add peak hold indicators:**
```csharp
private float[] peakHeights;
private float[] peakHoldTimes;

// Track and decay peaks over time
```

**Logarithmic scaling for more dynamic range:**
```csharp
targetHeights[i] = Mathf.Log10(1 + value * 9) * maxBarHeight;
```

---

## Technical Notes

- **Capture Size**: Must be power of 2 (64, 128, 256, 512, 1024)
- **FFT Output**: Gives half the capture size in usable frequency bands
- **Nyquist Frequency**: Maximum frequency = sampling rate / 2
- **Update Rate**: Android Visualizer captures at specified rate (Hz)
- **Thread Safety**: UnitySendMessage is thread-safe in Unity

---

## References

- [Android Visualizer API](https://developer.android.com/reference/android/media/audiofx/Visualizer)
- [FFT Wikipedia](https://en.wikipedia.org/wiki/Fast_Fourier_transform)
- [Unity SendMessage Documentation](https://docs.unity3d.com/ScriptReference/GameObject.SendMessage.html)

---

**Good luck with implementation! 🎵**
