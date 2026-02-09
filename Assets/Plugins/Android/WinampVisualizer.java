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
}
