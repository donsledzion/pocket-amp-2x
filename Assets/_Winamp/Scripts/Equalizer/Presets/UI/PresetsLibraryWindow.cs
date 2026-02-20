using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SoftAware.PocketAmp.Equalizer.Presets.UI
{
    public class PresetsLibraryWindow : MonoBehaviour
    {
        public enum LoadBehavior 
        {
            RequireLoadButton,
            LoadOnSelection
        }

        [Header("Settings")]
        public LoadBehavior loadBehavior = LoadBehavior.RequireLoadButton;

        public void SetLoadBehavior(LoadBehavior behavior)
        {
            loadBehavior = behavior;
            UpdateButtonsState();
        }

        [Header("References")]
        [SerializeField] private PresetItemView presetItemPrefab;
        [SerializeField] private Transform listContent;
        
        [Header("Buttons")]
        [SerializeField] private Button loadButton;
        [SerializeField] private Button closeButton;

        private EqualizerController equalizerController => Refs.EqualizerController;
        private EqPresetLibrary library;
        private EqPresetData selectedPreset;
        private readonly List<PresetItemView> currentItems = new();

        private void Awake()
        {
            if (loadButton != null) loadButton.onClick.AddListener(OnLoadClicked);
            if (closeButton != null) closeButton.onClick.AddListener(OnCloseClicked);
        }

        private void OnEnable()
        {
            if (SettingsManager.Instance != null)
            {
                SetLoadBehavior((LoadBehavior)SettingsManager.Instance.EQPresetsLoadBehavior);
            }

            if (library == null)
            {
                LoadPresetsFile();
            }
            
            UpdateButtonsState();
        }

        private void LoadPresetsFile()
        {
            var textAsset = Resources.Load<TextAsset>("default-presets");
            if (textAsset != null)
            {
                library = JsonUtility.FromJson<EqPresetLibrary>(textAsset.text);
                RefreshList();
            }
            else
            {
                Debug.LogError("[PresetsLibraryWindow] Cannot find default-presets.json in Resources!");
            }
        }

        private void RefreshList()
        {
            foreach (var item in currentItems)
                if (item) Destroy(item.gameObject);
            currentItems.Clear();

            if (library?.presets == null) return;

            foreach (var preset in library.presets)
            {
                var view = Instantiate(presetItemPrefab, listContent);
                if (!view) continue;
                view.Setup(preset, OnPresetSelected, OnPresetDoubleClicked);
                currentItems.Add(view);
            }
            
            selectedPreset = null;
            UpdateSelectionVisuals();
            UpdateButtonsState();
        }

        private void OnPresetSelected(EqPresetData preset)
        {
            selectedPreset = preset;
            UpdateSelectionVisuals();
            UpdateButtonsState();

            if (loadBehavior == LoadBehavior.LoadOnSelection)
            {
                ApplySelectedPreset();
            }
        }

        private void OnPresetDoubleClicked(EqPresetData preset)
        {
            selectedPreset = preset;
            UpdateSelectionVisuals();
            ApplySelectedPreset();
        }

        private void UpdateSelectionVisuals()
        {
            foreach (var item in currentItems)
            {
                item.SetSelected(item.Preset == selectedPreset);
            }
        }

        private void UpdateButtonsState()
        {
            bool hasSelection = selectedPreset != null;
            bool requireLoad = loadBehavior == LoadBehavior.RequireLoadButton;

            if (loadButton) 
            {
                loadButton.gameObject.SetActive(requireLoad);
                loadButton.interactable = hasSelection;
            }
        }

        private void OnLoadClicked()
        {
            if (selectedPreset == null) return;
            ApplySelectedPreset();
        }

        private void ApplySelectedPreset()
        {
            if (selectedPreset == null || equalizerController == null) return;

            float[] gains = selectedPreset.GetBandsAsGains();
            float preampGain = selectedPreset.GetPreampAsGain();
            equalizerController.SetPreset(preampGain, gains);
        }

        private void OnCloseClicked()
        {
            var main = FindFirstObjectByType<Main>();
            if (main != null)
            {
                main.ClosePresetsLibrary();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
