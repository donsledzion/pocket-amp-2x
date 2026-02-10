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

        [Header("Visuals")]
        [SerializeField] private WinampEqualizerGraph graph;

        [Header("Window Controls")]
        [SerializeField] private Button closeButton;

        [Header("Preset Controls")]
        [SerializeField] private Button allMaxButton;
        [SerializeField] private Button allMinButton;
        [SerializeField] private Button allFlatButton;

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
            if (closeButton != null) closeButton.onClick.AddListener(CloseWindow);

            if (allMaxButton != null) allMaxButton.onClick.AddListener(() => SetAllBands(20f));
            if (allMinButton != null) allMinButton.onClick.AddListener(() => SetAllBands(-20f));
            if (allFlatButton != null) allFlatButton.onClick.AddListener(() => SetAllBands(0f));

            InitializeSliders();
            LoadSettings();
        }

        private void LoadSettings()
        {
            if (SettingsManager.Instance == null) return;

            if (onButton != null) onButton.SetState(SettingsManager.Instance.EQOn);
            if (autoButton != null) autoButton.SetState(SettingsManager.Instance.EQAuto);
            if (preampSlider != null) preampSlider.value = SettingsManager.Instance.EQPreamp;

            for (int i = 0; i < frequencyBands.Count; i++)
            {
                if (frequencyBands[i] != null)
                {
                    frequencyBands[i].value = SettingsManager.Instance.GetEQBand(i);
                }
            }
        }

        private Slider interactingSlider;
        private string[] frequencyLabels = { "60HZ", "170HZ", "310HZ", "600HZ", "1K", "3K", "6K", "12K", "14K", "16K" };

        private void InitializeSliders()
        {
            if (onButton != null)
            {
                onButton.OnValueChanged.AddListener((isOn) => {
                    if (SettingsManager.Instance != null) SettingsManager.Instance.EQOn = isOn;
                    OnValuesChanged?.Invoke();
                });
            }

            if (autoButton != null)
            {
                autoButton.OnValueChanged.AddListener((isOn) => {
                    if (SettingsManager.Instance != null) SettingsManager.Instance.EQAuto = isOn;
                    OnValuesChanged?.Invoke();
                });
            }

            if (preampSlider != null)
            {
                preampSlider.minValue = -20f;
                preampSlider.maxValue = 20f;
                preampSlider.onValueChanged.AddListener((val) => {
                    if (SettingsManager.Instance != null) SettingsManager.Instance.EQPreamp = val;
                    OnPreampChanged(val);
                });

                // Attach interaction helper for Preamp
                var interaction = preampSlider.gameObject.AddComponent<SliderInteractionHelper>();
                interaction.OnPointerDownAction += () => OnSliderPointerDown(preampSlider, "PREAMP");
                interaction.OnPointerUpAction += OnSliderPointerUp;
            }

            for (int i = 0; i < frequencyBands.Count; i++)
            {
                int index = i; // Bootstrap for closure
                Slider slider = frequencyBands[i];
                if (slider != null)
                {
                    slider.minValue = -20f;
                    slider.maxValue = 20f;
                    slider.onValueChanged.AddListener((val) => {
                        if (SettingsManager.Instance != null) SettingsManager.Instance.SetEQBand(index, val);
                        OnBandChanged(index, val);
                    });
                    
                    // Attach interaction helper for Band
                    var interaction = slider.gameObject.AddComponent<SliderInteractionHelper>();
                    string label = (index < frequencyLabels.Length) ? frequencyLabels[index] : $"BAND {index}";
                    interaction.OnPointerDownAction += () => OnSliderPointerDown(slider, label);
                    interaction.OnPointerUpAction += OnSliderPointerUp;
                }
            }

            UpdateGraph();
        }

        private void OnSliderPointerDown(Slider slider, string label)
        {
            interactingSlider = slider;
            UpdateTitleDisplay(slider.value, label);
        }

        private void OnSliderPointerUp()
        {
            interactingSlider = null;
            Main main = FindObjectOfType<Main>();
            if (main != null && main.SongTitleDisplay != null)
            {
                main.SongTitleDisplay.ClearOverrideText();
            }
        }

        public void OnPreampChanged(float value)
        {
            UpdateGraph();
            OnValuesChanged?.Invoke();
            // Debug.Log($"Preamp changed: {value}"); // Reduced log spam

            if (interactingSlider == preampSlider)
            {
                UpdateTitleDisplay(value, "PREAMP");
            }
        }

        public void OnBandChanged(int bandIndex, float value)
        {
            if (bandIndex < 0 || bandIndex >= Frequencies.Length) return;
            
            UpdateGraph();
            OnValuesChanged?.Invoke();

            string label = (bandIndex < frequencyLabels.Length) ? frequencyLabels[bandIndex] : $"BAND {bandIndex}";
            if (interactingSlider == frequencyBands[bandIndex])
            {
                UpdateTitleDisplay(value, label);
            }
        }

        private void UpdateTitleDisplay(float value, string label)
        {
            Main main = FindObjectOfType<Main>();
            if (main != null && main.SongTitleDisplay != null)
            {
                // Format: "EQ: PREAMP: +8.6 DB" | "EQ: 60HZ: -2.9 DB"
                // Value is -20 to +20
                string sign = (value > 0) ? "+" : ""; // value will handle its own '-' if negative
                // Using "F1" for one decimal place
                main.SongTitleDisplay.SetOverrideText($"EQ: {label}: {sign}{value:F1} DB");
            }
        }

        private void UpdateGraph()
        {
            if (graph != null)
            {
                graph.SetGains(PreampValue, GetBandGains());
            }
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
            UpdateGraph();
        }

        public void SetPreset(float[] gains)
        {
            if (gains == null || gains.Length != frequencyBands.Count) return;

            for (int i = 0; i < gains.Length; i++)
            {
                if (frequencyBands[i] != null)
                {
                    frequencyBands[i].value = gains[i];
                    // Visuals are updated via onValueChanged listener
                }
            }
            UpdateGraph();
        }
        public void CloseWindow()
        {
            Main main = FindObjectOfType<Main>();
            if (main != null)
            {
                main.CloseEqualizerWindow();
            }
        }

        private void SetAllBands(float value)
        {
            foreach (var slider in frequencyBands)
            {
                if (slider != null) slider.value = value;
            }
        }
    }
}
