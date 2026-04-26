using UnityEngine;

namespace SoftAware.PocketAmp.Visualizers
{
    public class VisPluginManager : MonoBehaviour
    {
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

        private void Start()
        {
            // Auto-start if a plugin is assigned in the Inspector
            if (initialPlugin != null && initialPlugin is IVisualizerPlugin plugin)
                SetPlugin(plugin);
        }

        private void SetPlugin(IVisualizerPlugin plugin)
        {
            activePlugin?.OnPluginDisable();

            activePlugin = plugin;

            if (activePlugin == null || visWindow == null) return;
            activePlugin.OnPluginEnable(visWindow.Container.gameObject);
            visWindow.SetTitle(activePlugin.PluginName);
        }

        private void Update()
        {
            if (activePlugin == null) return;
            if (!visWindow || !visWindow.gameObject.activeInHierarchy) return;

            CaptureData();
            activePlugin.OnUpdate(fftBuffer, waveBuffer);
        }

        private void CaptureData()
        {
            var currentGain = 1.0f;
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
            if (audioSource && audioSource.isPlaying)
            {
                audioSource.GetSpectrumData(fftBuffer, 0, FFTWindow.BlackmanHarris);
                audioSource.GetOutputData(waveBuffer, 0);
                
                // Apply gain
                for(var i=0; i<1024; i++) 
                {
                    fftBuffer[i] *= currentGain;
                    waveBuffer[i] *= currentGain;
                }
            }
            else if (!audioSource)
            {
                Debug.LogWarning("[VisPluginManager] AudioSource is not assigned!");
            }
#endif
        }
    }
}
