using UnityEngine;

namespace SoftAware.PocketAmp.Visualizers
{
    /// <summary>
    /// Base interface for all audio visualizer plugins (AVS/MilkDrop style).
    /// </summary>
    public interface IVisualizerPlugin
    {
        string PluginName { get; }
        string Author { get; }
        
        /// <summary>
        /// Called when the plugin is loaded into the VisWindow.
        /// </summary>
        void OnPluginEnable(GameObject host);
        
        /// <summary>
        /// Called when the plugin is removed or the window is closed.
        /// </summary>
        void OnPluginDisable();
        
        /// <summary>
        /// Main update loop for the plugin.
        /// </summary>
        /// <param name="fftData">Normalized FFT data (0.0 to 1.0)</param>
        /// <param name="waveformData">Normalized PCM waveform (-1.0 to 1.0)</param>
        void OnUpdate(float[] fftData, float[] waveformData);
    }
}
