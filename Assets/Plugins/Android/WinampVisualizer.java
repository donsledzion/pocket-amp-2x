package com.softaware.winamp;

import android.media.audiofx.Visualizer;
import android.util.Log;
import java.lang.reflect.Method;

public class WinampVisualizer {
    private Visualizer visualizer;
    private byte[] rawBuffer;
    private int captureSize = 1024;
    private static final String TAG = "WinampVisualizer";
    private Method getWaveformMethod;

    public boolean initialize(int sessionId) {
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
            rawBuffer = new byte[captureSize];
            
            int status = visualizer.setEnabled(true);
            Log.d(TAG, "Initialized session " + targetSession + " with size " + captureSize + ". Enabled: " + (status == Visualizer.SUCCESS));
            
            // Try to find getWaveform via reflection to bypass compiler weirdness
            try {
                getWaveformMethod = visualizer.getClass().getMethod("getWaveform", byte[].class);
            } catch (Exception e) {
                Log.e(TAG, "Could not find getWaveform method: " + e.getMessage());
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
            } catch (Exception e) {
                e.printStackTrace();
            }
            visualizer = null;
        }
        getWaveformMethod = null;
    }

    // Returns float array with normalized magnitudes (0.0 - 1.0)
    public float[] getFft(int outSize) {
        if (visualizer == null || rawBuffer == null) return new float[outSize];

        try {
            if (visualizer.getFft(rawBuffer) != Visualizer.SUCCESS) {
                return new float[outSize];
            }

            // rawBuffer contains [real0, realN, real1, imag1, real2, imag2, ...]
            // We need to calculate magnitude and fill outSize
            float[] result = new float[outSize];
            
            // Calculate step to map captureSize/2 to outSize
            // We effective have captureSize/2 frequency bins useful for visualization
            int validBins = captureSize / 2;
            float step = (float)validBins / outSize;

            for (int i = 0; i < outSize; i++) {
                // Determine index in raw buffer
                // i * step gives bin index. 
                // rawBuffer index: bin 0 is at 0 (real) and 1 (imag - usually 0 for bin 0)
                // bin k is at 2k (real) and 2k+1 (imag)
                // We skip bin 0 (DC offset) usually, starting from bin 1 roughly? 
                // Let's stick to standard mapping: index = 2 + 2*binIndex
                
                int binIndex = (int)(i * step);
                if (binIndex >= validBins) binIndex = validBins - 1;
                
                int rawIndex = 2 + binIndex * 2;
                if (rawIndex + 1 < rawBuffer.length) {
                    float real = (float)rawBuffer[rawIndex];
                    float imag = (float)rawBuffer[rawIndex + 1];
                    // Magnitude
                    float mag = (float)Math.sqrt(real * real + imag * imag);
                    // Normalize (arbitrary scaling, same as C# 1024f)
                    result[i] = mag / 1024f;
                }
            }
            return result;
        } catch (Exception e) {
            return new float[outSize];
        }
    }

    // Returns float array with normalized waveform (-1.0 to 1.0)
    public float[] getWaveform(int outSize) {
        if (visualizer == null || rawBuffer == null || getWaveformMethod == null) return new float[outSize];

        try {
            // Use reflection to call getWaveform
            Object statusObj = getWaveformMethod.invoke(visualizer, rawBuffer);
            if (statusObj instanceof Integer && ((Integer)statusObj) != Visualizer.SUCCESS) {
                return new float[outSize];
            }

            float[] result = new float[outSize];
            float step = (float)rawBuffer.length / outSize;

            for (int i = 0; i < outSize; i++) {
                int idx = (int)(i * step);
                if (idx < rawBuffer.length) {
                    // Java byte is signed -128..127. 
                    // Visualizer returns unsigned byte 0..255 reinterpreted as signed.
                    // 128 (unsigned) is silence.
                    // To get unsigned value: rawBuffer[idx] & 0xFF
                    int unsignedVal = rawBuffer[idx] & 0xFF;
                    
                    // Center around 0 (-1.0 to 1.0)
                    result[i] = (unsignedVal - 128) / 128f;
                }
            }
            return result;
        } catch (Exception e) {
            return new float[outSize];
        }
    }
}
