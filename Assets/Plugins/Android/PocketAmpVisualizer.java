package com.softaware.pocketamp;

import android.media.audiofx.Visualizer;
import android.util.Log;
import java.lang.reflect.Method;

public class PocketAmpVisualizer {
    private Visualizer visualizer;
    private byte[] rawBuffer;
    private int captureSize = 1024;
    private static final String TAG = "PocketAmp";
    private Method getWaveformMethod;

    private int logCounter = 0;
    private int sessionID = -1;
    private boolean configLogged = false;

    public boolean initialize(int sessionId) {
        this.sessionID = sessionId;
        Log.i(TAG, "!!! initialize visualizer for session: " + sessionId);
        try {
            release();
            
            // If sessionId is -1, fallback to 0 (Global Mix)
            int targetSession = (sessionId == -1) ? 0 : sessionId; 
            
            visualizer = new Visualizer(targetSession);
            visualizer.setEnabled(false); // Must be disabled to change settings

            // maximize capture size - boost to 2048 for better bass resolution (21Hz/bin)
            int[] range = Visualizer.getCaptureSizeRange();
            int max = (range != null && range.length > 1) ? range[1] : 2048;
            captureSize = Math.min(max, 2048);
            
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
    // Returns 19-bar float array with PocketAmp-style grouping (0.0 - 1.0)
    public float[] getPocketAmpFft() {
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

            // Step 2: Downsample to 19 bars (PocketAmp style)
            result = downsampleTo19Bars(magnitudes);

            // Debug Dump (every ~2 seconds at 60fps)
            if (logCounter % 120 == 0) {
                 StringBuilder sb = new StringBuilder();
                 sb.append("PocketAmpVis Dump [").append(captureSize).append(" @ ").append(visualizer.getSamplingRate() / 1000).append("kHz]: ");
                 for (int i=0; i<Math.min(5, result.length); i++) sb.append(String.format("%.2f ", result[i]));
                 sb.append("... ");
                 for (int i=Math.max(0, result.length-5); i<result.length; i++) sb.append(String.format("%.2f ", result[i]));
                 Log.d(TAG, sb.toString());
            }

            return result;

        } catch (Exception e) {
            if (logCounter % 100 == 0) Log.e(TAG, "getPocketAmpFft failed: " + e.getMessage());
            return result;
        }
    }

    private float[] calculateMagnitudes(byte[] fft) {
        int numBands = fft.length / 2;
        float[] magnitudes = new float[numBands];
        
        magnitudes[0] = 0; // Skip DC
        
        for (int i = 2; i < fft.length; i += 2) {
            int bandIndex = i / 2;
            float real = (float) fft[i];
            float imag = (float) fft[i + 1];
            float magnitude = (float) Math.sqrt(real * real + imag * imag);
            
            // Back to punchy linear scaling - log10 was too sensitive for high bins.
            // Using 120 as a divisor to prevent constant clipping.
            magnitudes[bandIndex] = magnitude / 120.0f;
        }
        
        return magnitudes;
    }
    
    // Calibrated PocketAmp frequency ranges for 19 bars
    // Shifted higher thresholds to ensure "Kick" hits early bars (0-2)
    private static final int[] POCKETAMP_RANGES_HZ = {
        100, 200, 300, 450, 600,    // 0-4
        900, 1300, 1800, 2500, 3300, // 5-9
        4500, 6000, 8000, 10000, 12000, // 10-14
        14000, 16000, 18000, 21000 // 15-18
    };

    private float[] downsampleTo19Bars(float[] magnitudes) {
        float[] bars = new float[19];
        int numBands = magnitudes.length;
        
        int samplingRate = (visualizer != null) ? visualizer.getSamplingRate() : 44100000;
        samplingRate /= 1000;
        float nyquist = samplingRate / 2.0f;
        float bandwidthPerBin = nyquist / numBands;

        int startBin = 1;

        for (int bar = 0; bar < 19; bar++) {
            float cutoffHz = POCKETAMP_RANGES_HZ[bar];
            int endBin = (int)(cutoffHz / bandwidthPerBin);
            
            if (endBin >= numBands) endBin = numBands;
            if (endBin <= startBin) endBin = startBin + 1;
            if (endBin > numBands) endBin = numBands;

            float maxVal = 0;
            for (int band = startBin; band < endBin; band++) {
                if (magnitudes[band] > maxVal) maxVal = magnitudes[band];
            }
            
            // Raw peak value per bar
            float targetVal = maxVal;
            
            // Milder Treble Boost to balance visual energy (max ~3.8x)
            float trebleBoost = 1.0f + (bar * 0.15f); 
            targetVal *= trebleBoost;
            
            bars[bar] = targetVal;
            
            // Clamp
            if (bars[bar] > 1.0f) bars[bar] = 1.0f;
            if (bars[bar] < 0.0f) bars[bar] = 0.0f;
            
            startBin = endBin;
        }
        
        return bars;
    }
}
