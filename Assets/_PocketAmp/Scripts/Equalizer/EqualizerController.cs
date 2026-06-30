using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SoftAware.PocketAmp
{
    public class EqualizerController : MonoBehaviour, ISkinApplicator
    {
        [Header("Toggles")]
        [SerializeField] private ToggleButton onButton;
        [SerializeField] private ToggleButton autoButton;
        [SerializeField] private Button presetsButton;

        [Header("Sliders")]
        [SerializeField] private Slider preampSlider;
        [SerializeField] private List<Slider> frequencyBands = new List<Slider>();

        [Header("Visuals")]
        [SerializeField] private EqualizerGraph graph;
        [SerializeField] private Image background;
        [SerializeField] private Image titleBar;
        [SerializeField] private Image graphBackground;

        [Header("Window Controls")]
        [SerializeField] private Button closeButton;

        [Header("Preset Controls")]
        [SerializeField] private Button allMaxButton;
        [SerializeField] private Button allMinButton;
        [SerializeField] private Button allFlatButton;

        [Header("Settings")]
        [Range(-12f, 12f)] 
        [SerializeField] private float defaultGain = 0f;

        // Frequencies: 60, 170, 310, 600, 1K, 3K, 6K, 12K, 14K, 16K
        public static readonly float[] Frequencies = { 60f, 170f, 310f, 600f, 1000f, 3000f, 6000f, 12000f, 14000f, 16000f };

        public bool IsOn => onButton != null && onButton.IsOn;
        public bool IsAuto => autoButton != null && autoButton.IsOn;

        public System.Action OnValuesChanged;

        private static Main main => Refs.Main;

        private void Start()
        {
            if (closeButton != null) closeButton.onClick.AddListener(CloseWindow);

            if (allMaxButton != null) allMaxButton.onClick.AddListener(() => SetAllBands(20f));
            if (allMinButton != null) allMinButton.onClick.AddListener(() => SetAllBands(-20f));
            if (allFlatButton != null) allFlatButton.onClick.AddListener(() => SetAllBands(0f));
            if (presetsButton != null) presetsButton.onClick.AddListener(() => main.OverlayWindowsController.OpenPresetsLibrary());

            InitializeSliders();
            LoadSettings();
        }

        private void OnDestroy()
        {
            if (closeButton != null) closeButton.onClick.RemoveListener(CloseWindow);

            if (allMaxButton != null) allMaxButton.onClick.RemoveAllListeners();
            if (allMinButton != null) allMinButton.onClick.RemoveAllListeners();
            if (allFlatButton != null) allFlatButton.onClick.RemoveAllListeners();
            if (presetsButton != null) presetsButton.onClick.RemoveAllListeners();
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

            for (var i = 0; i < frequencyBands.Count; i++)
            {
                var index = i; // Bootstrap for closure
                var slider = frequencyBands[i];
                if (slider == null) continue;
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
            if (main.SongTitleDisplay != null)
            {
                main.SongTitleDisplay.ClearOverrideText();
            }
        }

        private void OnPreampChanged(float value)
        {
            UpdateGraph();
            OnValuesChanged?.Invoke();

            if (interactingSlider == preampSlider)
            {
                UpdateTitleDisplay(value, "PREAMP");
            }
        }

        private void OnBandChanged(int bandIndex, float value)
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

        [Header("Localization")]
        [SerializeField] private UnityEngine.Localization.LocalizedString eqBandText;

        private void UpdateTitleDisplay(float value, string label)
        {
            if (main != null && main.SongTitleDisplay != null)
            {
                string sign = (value > 0) ? "+" : "";
                if (eqBandText != null && !eqBandText.IsEmpty)
                {
                    eqBandText.Arguments = new object[] { label, sign, value.ToString("F1") };
                    main.SongTitleDisplay.SetOverrideText(eqBandText.GetLocalizedString());
                }
                else
                {
                    main.SongTitleDisplay.SetOverrideText($"EQ: {label}: {sign}{value:F1} DB");
                }
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

        public void ApplySkin(Skin skin)
        {
            if (skin == null) return;

            // 1. Backgrounds
            if (background != null && skin.EqBackground != null) background.sprite = skin.EqBackground;
            if (titleBar != null && skin.EqTitleBar != null) titleBar.sprite = skin.EqTitleBar;
            if (graphBackground != null && skin.EqGraphBackground != null) graphBackground.sprite = skin.EqGraphBackground;

            // 2. Buttons
            if (onButton != null)
                onButton.SetSprites(skin.EqOn_Off_Normal, skin.EqOn_Off_Pressed, skin.EqOn_On_Normal, skin.EqOn_On_Pressed);
            
            if (autoButton != null)
                autoButton.SetSprites(skin.EqAuto_Off_Normal, skin.EqAuto_Off_Pressed, skin.EqAuto_On_Normal, skin.EqAuto_On_Pressed);

            if (presetsButton != null && skin.EqPresetsNormal != null)
            {
                presetsButton.image.sprite = skin.EqPresetsNormal;
                var ss = presetsButton.spriteState;
                if (skin.EqPresetsPressed != null) ss.pressedSprite = skin.EqPresetsPressed;
                presetsButton.spriteState = ss;
            }

            if (closeButton != null && skin.EqCloseNormal != null)
            {
                closeButton.image.sprite = skin.EqCloseNormal;
                var ss = closeButton.spriteState;
                if (skin.EqClosePressed != null) ss.pressedSprite = skin.EqClosePressed;
                closeButton.spriteState = ss;
            }

            // 3. Sliders (Preamp + Bands)
            if (preampSlider != null)
            {
                if(preampSlider.TryGetComponent(out SliderVisuals visuals))
                    if (visuals != null) visuals.ApplySkin(skin);
            }

            foreach (var slider in frequencyBands)
            {
                if (slider == null) continue;
                if (!slider.TryGetComponent(out SliderVisuals visuals)) continue;
                if (visuals != null) visuals.ApplySkin(skin);
            }

            // 4. Graph
            if (graph != null)
            {
                graph.ApplySkin(skin);
            }
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

        public void SetPreset(float preampValue, float[] gains)
        {
            if (preampSlider != null) preampSlider.value = preampValue;

            if (gains == null || gains.Length != frequencyBands.Count) return;

            for (int i = 0; i < gains.Length; i++)
            {
                if (frequencyBands[i] != null)
                {
                    frequencyBands[i].value = gains[i];
                }
            }
            UpdateGraph();
        }

        public void SetPreset(float[] gains)
        {
            SetPreset(0f, gains);
        }

        private static void CloseWindow()
        {
            main.CloseEqualizerWindow();
        }

        private void SetAllBands(float value)
        {
            foreach (var slider in frequencyBands)
                if (slider != null) slider.value = value;
        }
    }
}
