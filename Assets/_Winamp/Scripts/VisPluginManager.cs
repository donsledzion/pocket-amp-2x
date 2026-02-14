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
        [SerializeField] private MonoBehaviour initialPlugin; 
        [SerializeField] private float editorGain = 1.0f;
        [SerializeField] private float androidGain = 0.5f;

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
            float currentGain = 1.0f;
#if UNITY_ANDROID && !UNITY_EDITOR
            currentGain = androidGain;
            float[] sharedFft = AndroidVisualizerBridge.GetSharedFFT(1024);
            float[] sharedWave = AndroidVisualizerBridge.GetSharedWaveform(1024);
            
            if (sharedFft != null) 
            {
                for(int i=0; i<1024; i++) 
                {
                    fftBuffer[i] = sharedFft[i] * currentGain;
                    waveBuffer[i] = sharedWave[i] * currentGain;
                }
            }
#else
            currentGain = editorGain;
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.GetSpectrumData(fftBuffer, 0, FFTWindow.BlackmanHarris);
                audioSource.GetOutputData(waveBuffer, 0);
                
                // Apply gain
                for(int i=0; i<1024; i++) 
                {
                    fftBuffer[i] *= currentGain;
                    waveBuffer[i] *= currentGain;
                }
            }
            else if (audioSource == null)
            {
                Debug.LogWarning("[VisPluginManager] AudioSource is not assigned!");
            }
#endif
        }
    }
}
