package com.softaware.winamp;

import android.media.audiofx.Visualizer;
import android.util.Log;
import java.lang.reflect.Method;

public class WinampVisualizer {
    private Visualizer visualizer;
    private byte[] rawBuffer;
    private int captureSize = 1024;
    private static final String TAG = "Winamp";
    private Method getWaveformMethod;

    private int logCounter = 0;
    private int sessionID = -1;

    public boolean initialize(int sessionId) {
        this.sessionID = sessionId;
        Log.i(TAG, "!!! initialize visualizer for session: " + sessionId);
        try {
            release();
            
            // If sessionId is -1, fallback to 0 (Global Mix)
            int targetSession = (sessionId == -1) ? 0 : sessionId; 
            
            visualizer = new Visualizer(targetSession);
            visualizer.setEnabled(false); // Must be disabled to change settings

            // maximize capture size
            int[] range = Visualizer.getCaptureSizeRange();
            int max = (range != null && range.length > 1) ? range[1] : 1024;
            captureSize = Math.min(max, 1024);
            
            visualizer.setCaptureSize(captureSize);
            
            // SCALING_MODE_NORMALIZED (added in API 16) makes the visualizer 
            // independent of player volume. It auto-scales the signal to 8-bit peak-peak.
            try {
                visualizer.setScalingMode(Visualizer.SCALING_MODE_NORMALIZED);
                Log.i(TAG, "Scaling mode set to NORMALIZED");
            } catch (Exception e) {
                Log.e(TAG, "Could not set scaling mode: " + e.getMessage());
            }

            rawBuffer = new byte[captureSize];
            
            int status = visualizer.setEnabled(true);
            Log.i(TAG, "Initialized session " + targetSession + " with size " + captureSize + ". Enabled: " + (status == Visualizer.SUCCESS));
            
            // Brute-force reflection: Find getWaveform by iterating through all methods
            // to bypass signature mismatches in different Android environment versions.
            try {
                Method[] methods = visualizer.getClass().getMethods();
                for (Method m : methods) {
                    if (m.getName().equalsIgnoreCase("getWaveform")) {
                        getWaveformMethod = m;
                        getWaveformMethod.setAccessible(true);
                        Log.i(TAG, "Success! Brute-force found: " + m.toString());
                        break;
                    }
                }
                
                if (getWaveformMethod == null) {
                    Log.e(TAG, "Critical: getWaveform NOT FOUND in method list.");
                }
            } catch (Throwable t) {
                Log.e(TAG, "Brute-force reflection FAILED: " + t.getMessage());
            }

            return status == Visualizer.SUCCESS;
        } catch (Exception e) {
            Log.e(TAG, "Initialization failed: " + e.getMessage());
            e.printStackTrace();
            return false;
        }
    }

    public void release() {
        if (visualizer != null) {
            try {
                visualizer.setEnabled(false);
                visualizer.release();
                Log.i(TAG, "Released visualizer");
            } catch (Exception e) {
                e.printStackTrace();
            }
            visualizer = null;
        }
        getWaveformMethod = null;
    }

    // Returns float array with normalized magnitudes (0.0 - 1.0)
    public float[] getFft(int outSize) {
        float[] result = new float[outSize];
        if (visualizer == null || rawBuffer == null) {
            if (logCounter % 100 == 0) Log.e(TAG, "FFT: visualizer is null!");
            return result;
        }

        try {
            logCounter++;
            if (visualizer.getFft(rawBuffer) != Visualizer.SUCCESS) {
                return result;
            }

            // rawBuffer contains [real0, realN, real1, imag1, real2, imag2, ...]
            int validBins = captureSize / 2;
            float step = (float)validBins / outSize;

            boolean hasData = false;
            for (int i = 0; i < outSize; i++) {
                int binIndex = (int)(i * step);
                if (binIndex >= validBins) binIndex = validBins - 1;
                
                int rawIndex;
                if (binIndex == 0) rawIndex = 0;
                else if (binIndex == validBins - 1) rawIndex = 1;
                else rawIndex = binIndex * 2;

                if (rawIndex < rawBuffer.length) {
                    float real = (float)rawBuffer[rawIndex];
                    float imag = (rawIndex + 1 < rawBuffer.length) ? (float)rawBuffer[rawIndex + 1] : 0;
                    if (binIndex == 0 || binIndex == validBins - 1) imag = 0; 

                    float mag = (float)Math.sqrt(real * real + imag * imag);
                    if (mag > 0.001f) hasData = true;
                    // Scaling for visibility. 32.0f is more sensitive than 64.0f
                    result[i] = mag / 32.0f; 
                }
            }

            if (logCounter % 100 == 0) {
                Log.i(TAG, "FFT Data - active? " + hasData + " sample[0]:" + result[0]);
            }

            return result;
        } catch (Exception e) {
            return result;
        }
    }

    // Returns float array with normalized waveform (-1.0 to 1.0)
    public float[] getWaveformPCM(int outSize) {
        float[] result = new float[outSize];
        if (visualizer == null || rawBuffer == null) {
            if (logCounter % 50 == 0) Log.e(TAG, "WAVE: visualizer is NULL!");
            return result;
        }

        try {
            logCounter++;
            int status = -2;
            
            // Try reflection if available
            if (getWaveformMethod != null) {
                try {
                    Object statusObj = getWaveformMethod.invoke(visualizer, rawBuffer);
                    status = (statusObj instanceof Integer) ? (Integer)statusObj : -1;
                } catch (Throwable t) {
                    if (logCounter % 100 == 0) Log.e(TAG, "Reflection invoke failed: " + t.getMessage());
                }
            } else {
                if (logCounter % 100 == 0) Log.e(TAG, "getWaveformMethod is NULL - cannot capture PCM");
            }

            if (logCounter % 100 == 0) {
                Log.i(TAG, "PCM (" + sessionID + ") stat:" + status + " first:" + (rawBuffer[0] & 0xFF) + " cnt:" + logCounter);
            }

            if (status != 0) return result; // 0 == Visualizer.SUCCESS

            float step = (float)rawBuffer.length / outSize;
            for (int i = 0; i < outSize; i++) {
                int idx = (int)(i * step);
                if (idx < rawBuffer.length) {
                    int unsignedVal = rawBuffer[idx] & 0xFF; 
                    result[i] = (unsignedVal - 128) / 128f;
                }
            }

            return result;
        } catch (Throwable t) {
            if (logCounter % 20 == 0) Log.e(TAG, "getWaveform total fatal: " + t.getMessage());
            return result;
        }
    }
    // Returns 19-bar float array with Winamp-style grouping (0.0 - 1.0)
    public float[] getWinampFft() {
        float[] result = new float[19];
        if (visualizer == null || rawBuffer == null) {
            return result;
        }

        try {
            if (visualizer.getFft(rawBuffer) != Visualizer.SUCCESS) {
                return result;
            }

            // Step 1: Convert FFT byte data to magnitudes
            float[] magnitudes = calculateMagnitudes(rawBuffer);

            // Step 2: Downsample to 19 bars (Winamp style)
            return downsampleTo19Bars(magnitudes);

        } catch (Exception e) {
            if (logCounter % 100 == 0) Log.e(TAG, "getWinampFft failed: " + e.getMessage());
            return result;
        }
    }

    private float[] calculateMagnitudes(byte[] fft) {
        // FFT data format from Android Visualizer:
        // [DC, real1, imag1, real2, imag2, ..., nyquist]
        // We get captureSize/2 frequency bands
        
        int numBands = fft.length / 2;
        float[] magnitudes = new float[numBands];
        
        // Skip DC component (index 0) to avoid huge spike
        magnitudes[0] = 0; 
        
        // Calculate magnitude for each frequency band
        for (int i = 2; i < fft.length; i += 2) {
            int bandIndex = i / 2;
            
            float real = (float) fft[i];
            float imag = (float) fft[i + 1];
            
            // Calculate magnitude
            float magnitude = (float) Math.sqrt(real * real + imag * imag);
            
            // Normalize to 0-1 range (approximate) using sqrt for better dynamics (like VU meter)
            // 8-bit samples, max magnitude is approx 180.
            // Sqrt(180) ~= 13.4. We divide by 16.0f to be safe.
            // This acts as a compressor/limiter lifting quiet sounds.
            magnitudes[bandIndex] = (float)Math.sqrt(magnitude) / 14.0f;
        }
        
        return magnitudes;
    }
    
    private float[] downsampleTo19Bars(float[] magnitudes) {
        float[] bars = new float[19];
        int numBands = magnitudes.length;
        
        // Winamp-style logarithmic grouping
        // We want more resolution in lower frequencies.
        // Formula: index = start_index * powertrain ^ bar_index
        
        // We skip band 0 (DC). Start from band 1.
        // Effective range: 1 to numBands.
        
        // Logarithmic interpolation indices for 512 bands (Capture size 1024)
        // These are manually tuned to resemble Winamp's distribution
        int[] limits = new int[20];
        limits[0] = 1; // Start at 1 to skip DC
        
        // Generate logarithmic stops
        // We want the last band to end around numBands * 0.75 (cut off extreme highs > 16kHz)
        // If numBands = 512 (22kHz), we want to end around 370 (~16kHz).
        float maxBand = numBands * 0.75f; 
        if (maxBand < 20) maxBand = numBands; // Fallback for very low capture size

        for (int i = 1; i <= 19; i++) {
             // Logarithmic scale: 1 ... maxBand
             // val = 1 * (maxBand)^(i/19)
             double val = Math.pow(maxBand, (double)i / 19.0);
             limits[i] = (int)val;
             // Ensure monotonic growth
             if (limits[i] <= limits[i-1]) limits[i] = limits[i-1] + 1;
        }

        for (int bar = 0; bar < 19; bar++) {
            int startBand = limits[bar];
            int endBand = limits[bar+1];
            
            // Safety clamp
            if (startBand >= numBands) startBand = numBands - 1;
            if (endBand > numBands) endBand = numBands;
            
            float sum = 0;
            int count = 0;
            
            for (int band = startBand; band < endBand; band++) {
                sum += magnitudes[band];
                count++;
            }
            
            float avg = (count > 0) ? sum / count : 0;
            
            // Additional Linear Boost for High Frequencies (Pre-emphasis)
            // Highs are naturally weaker in music
            float boost = 1.0f + (bar * 0.05f); // 0% boost at bass, almost 100% boost at treble
            
            bars[bar] = avg * boost;
        }
        
        return bars;
    }
}
