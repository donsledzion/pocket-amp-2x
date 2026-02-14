using UnityEngine;
using System.Collections.Generic;

namespace SoftAware.Winamp.Visualizers
{
    public class VisPluginManager : MonoBehaviour
    {
        public static VisPluginManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private VisWindow visWindow;
        
        [Header("Settings")]
        [SerializeField] private MonoBehaviour initialPlugin; // Drag a plugin MonoBehaviour here

        private IVisualizerPlugin activePlugin;
        private float[] fftBuffer = new float[1024];
        private float[] waveBuffer = new float[1024];

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            // Auto-start if a plugin is assigned in the Inspector
            if (initialPlugin != null && initialPlugin is IVisualizerPlugin plugin)
            {
                SetPlugin(plugin);
            }
        }

        public void SetPlugin(IVisualizerPlugin plugin)
        {
            if (activePlugin != null) activePlugin.OnPluginDisable();
            
            activePlugin = plugin;
            
            if (activePlugin != null && visWindow != null)
            {
                activePlugin.OnPluginEnable(visWindow.Container.gameObject);
                visWindow.SetTitle(activePlugin.PluginName);
            }
        }

        private void Update()
        {
            if (activePlugin == null) return;
            if (visWindow == null || !visWindow.gameObject.activeInHierarchy) return;

            CaptureData();
            activePlugin.OnUpdate(fftBuffer, waveBuffer);
        }

        private void CaptureData()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            float[] sharedFft = AndroidVisualizerBridge.GetSharedFFT(1024);
            float[] sharedWave = AndroidVisualizerBridge.GetSharedWaveform(1024);
            
            if (sharedFft != null) System.Array.Copy(sharedFft, fftBuffer, 1024);
            if (sharedWave != null) System.Array.Copy(sharedWave, waveBuffer, 1024);
#else
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.GetSpectrumData(fftBuffer, 0, FFTWindow.BlackmanHarris);
                audioSource.GetOutputData(waveBuffer, 0);
            }
            else if (audioSource == null)
            {
                Debug.LogWarning("[VisPluginManager] AudioSource is not assigned!");
            }
#endif
        }
    }
}
