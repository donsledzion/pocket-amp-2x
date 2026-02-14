using UnityEngine;
using UnityEngine.UI;

namespace SoftAware.Winamp.Visualizers
{
    /// <summary>
    /// A sample high-performance shader-based visualizer plugin.
    /// Demonstrates how to pass FFT data to GPU for MilkDrop-style effects.
    /// </summary>
    public class AdvancedSpectrumPlugin : MonoBehaviour, IVisualizerPlugin
    {
        public string PluginName => "Advanced Spectrum (Shader)";
        public string Author => "SoftAware";

        [Header("Shader Settings")]
        [SerializeField] private Shader visualizerShader;
        
        private RawImage display;
        private Material visMaterial;
        private Texture2D dataTexture;

        public void OnPluginEnable(GameObject host)
        {
            Debug.Log($"[Vis] Enabling {PluginName} on {host.name}");
            
            // Setup the renderer as a child of the container
            transform.SetParent(host.transform, false);
            
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            if (rect == null) rect = gameObject.AddComponent<RectTransform>();
            
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            display = gameObject.GetComponent<RawImage>();
            if (display == null) display = gameObject.AddComponent<RawImage>();
            
            // Create a 1D texture to pass FFT data to the shader
            dataTexture = new Texture2D(1024, 1, TextureFormat.RFloat, false);
            dataTexture.filterMode = FilterMode.Bilinear;
            dataTexture.wrapMode = TextureWrapMode.Clamp;
            
            if (visualizerShader != null)
            {
                Debug.Log("[Vis] Shader found, creating material...");
                visMaterial = new Material(visualizerShader);
                display.material = visMaterial;
                display.color = Color.white; // Ensure base color doesn't interfere
            }
            else
            {
                Debug.LogError("[Vis] Visualizer Shader is NOT assigned in the Inspector!");
            }
        }

        public void OnPluginDisable()
        {
            // Reset parenting if needed, or wait for destruction
            if (dataTexture != null) Destroy(dataTexture);
            if (visMaterial != null) Destroy(visMaterial);
        }

        public void OnUpdate(float[] fftData, float[] waveformData)
        {
            if (dataTexture == null) return;

            // Upload FFT data to texture efficiently
            dataTexture.GetRawTextureData<float>().CopyFrom(fftData);
            dataTexture.Apply();

            if (visMaterial != null)
            {
                visMaterial.SetTexture("_AudioData", dataTexture);
                
                // Calculate simple peak for beat-like effects in shader
                float peak = 0;
                for(int j=0; j<10; j++) peak = Mathf.Max(peak, fftData[j]); // Sample low bands
                visMaterial.SetFloat("_BeatPulse", peak);
            }
        }
    }
}
