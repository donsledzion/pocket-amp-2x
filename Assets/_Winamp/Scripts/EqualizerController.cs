using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SoftAware
{
    public class EqualizerController : MonoBehaviour
    {
        [Header("Toggles")]
        [SerializeField] private ToggleButton onButton;
        [SerializeField] private ToggleButton autoButton;

        [Header("Sliders")]
        [SerializeField] private Slider preampSlider;
        [SerializeField] private List<Slider> frequencyBands = new List<Slider>();

        [Header("Settings")]
        [Range(-12f, 12f)] 
        [SerializeField] private float defaultGain = 0f;

        // Frequencies for Winamp 2.7: 60, 170, 310, 600, 1K, 3K, 6K, 12K, 14K, 16K
        public static readonly float[] Frequencies = { 60f, 170f, 310f, 600f, 1000f, 3000f, 6000f, 12000f, 14000f, 16000f };

        public bool IsOn => onButton != null && onButton.IsOn;
        public bool IsAuto => autoButton != null && autoButton.IsOn;

        public System.Action OnValuesChanged;

        private void Start()
        {
            InitializeSliders();
        }

        private void InitializeSliders()
        {
            if (onButton != null)
            {
                onButton.OnValueChanged.AddListener((_) => OnValuesChanged?.Invoke());
            }

            if (preampSlider != null)
            {
                preampSlider.onValueChanged.AddListener(OnPreampChanged);
            }

            for (int i = 0; i < frequencyBands.Count; i++)
            {
                int index = i; // Bootstrap for closure
                if (frequencyBands[i] != null)
                {
                    frequencyBands[i].onValueChanged.AddListener((val) => OnBandChanged(index, val));
                }
            }
        }

        public void OnPreampChanged(float value)
        {
            OnValuesChanged?.Invoke();
            Debug.Log($"Preamp changed: {value}");
        }

        public void OnBandChanged(int bandIndex, float value)
        {
            if (bandIndex < 0 || bandIndex >= Frequencies.Length) return;
            
            OnValuesChanged?.Invoke();
            Debug.Log($"Band {Frequencies[bandIndex]}Hz changed: {value}");
        }

        public float PreampValue => preampSlider != null ? preampSlider.value : 0f;
        
        public float[] GetBandGains()
        {
            float[] gains = new float[frequencyBands.Count];
            for (int i = 0; i < frequencyBands.Count; i++)
            {
                gains[i] = frequencyBands[i] != null ? frequencyBands[i].value : 0f;
            }
            return gains;
        }

        public void ResetToDefault()
        {
            if (preampSlider != null) preampSlider.value = defaultGain;
            foreach (var slider in frequencyBands)
            {
                if (slider != null) slider.value = defaultGain;
            }
        }

        public void SetPreset(float[] gains)
        {
            if (gains == null || gains.Length != frequencyBands.Count) return;

            for (int i = 0; i < gains.Length; i++)
            {
                if (frequencyBands[i] != null)
                {
                    frequencyBands[i].value = gains[i];
                }
            }
        }
    }
}
