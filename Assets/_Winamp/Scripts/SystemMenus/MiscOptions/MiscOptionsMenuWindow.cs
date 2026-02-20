using SoftAware.PocketAmp;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MiscOptionsMenuWindow : MonoBehaviour
{
    [SerializeField] private Button closeButton;
    [SerializeField] private Button skinsLibraryButton;
    [SerializeField] private Button notificationsBarToggle;
    [SerializeField] private Button navigationBarToggle;
    [SerializeField] private Button eqPresetsBehaviorToggle;

    private void Awake()
    {
        closeButton.onClick.AddListener(CloseMenu);
        skinsLibraryButton.onClick.AddListener(OpenSkinsLibrary);
        notificationsBarToggle.onClick.AddListener(ToggleNotificationsBar);
        if (navigationBarToggle != null) navigationBarToggle.onClick.AddListener(ToggleNavigationBar);
        if (eqPresetsBehaviorToggle != null) eqPresetsBehaviorToggle.onClick.AddListener(ToggleEQPresetsBehavior);
    }

    private void Start()
    {
        UpdateNotificationsButtonText();
        UpdateNavigationButtonText();
        UpdateEQPresetsBehaviorText();
    }

    private void ToggleNotificationsBar()
    {
        bool newFullscreenState = !SettingsManager.Instance.IsFullscreen;
        SettingsManager.Instance.IsFullscreen = newFullscreenState;

        SetFullscreenState(newFullscreenState);
        UpdateNotificationsButtonText();
    }

    private void ToggleNavigationBar()
    {
        bool newState = !SettingsManager.Instance.IsNavigationBarVisible;
        SettingsManager.Instance.IsNavigationBarVisible = newState;
        
        if (Application.platform == RuntimePlatform.Android)
        {
            SettingsManager.Instance.ResolveSystemBars();
        }
        
        UpdateNavigationButtonText();
    }

    private void SetFullscreenState(bool isFullscreen)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            SettingsManager.Instance.ResolveSystemBars();
        }
        else
        {
            Screen.fullScreen = isFullscreen;
        }
    }

    private void UpdateNotificationsButtonText()
    {
        var textRequest = notificationsBarToggle.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (textRequest != null)
        {
            // If Fullscreen is TRUE -> Notifications Bar is OFF (Hidden)
            // If Fullscreen is FALSE -> Notifications Bar is ON (Visible)
            textRequest.text = $"Notifications bar: {(SettingsManager.Instance.IsFullscreen ? "OFF" : "ON")}";
        }
    }

    private void UpdateNavigationButtonText()
    {
        if (navigationBarToggle == null) return;
        
        var textRequest = navigationBarToggle.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (textRequest != null)
        {
            textRequest.text = $"Navigation bar: {(SettingsManager.Instance.IsNavigationBarVisible ? "ON" : "OFF")}";
        }
    }

    private void ToggleEQPresetsBehavior()
    {
        int current = SettingsManager.Instance.EQPresetsLoadBehavior;
        int next = current == 0 ? 1 : 0;
        SettingsManager.Instance.EQPresetsLoadBehavior = next;

        var presetsWindow = FindFirstObjectByType<SoftAware.PocketAmp.Equalizer.Presets.UI.PresetsLibraryWindow>(FindObjectsInactive.Include);
        if (presetsWindow != null)
        {
            presetsWindow.SetLoadBehavior((SoftAware.PocketAmp.Equalizer.Presets.UI.PresetsLibraryWindow.LoadBehavior)next);
        }

        UpdateEQPresetsBehaviorText();
    }

    private void UpdateEQPresetsBehaviorText()
    {
        if (eqPresetsBehaviorToggle == null) return;
        var textRequest = eqPresetsBehaviorToggle.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (textRequest != null)
        {
            string modeStr = SettingsManager.Instance.EQPresetsLoadBehavior == 0 ? "Require Load" : "Load on Select";
            textRequest.text = $"EQ Presets: {modeStr}";
        }
    }

    private void OpenSkinsLibrary()
    {
        CloseMenu();
        Refs.Main.OpenSkinsLibrary();
    }

    internal void CloseMenu() => Refs.Main.CloseMiscOptionsMenu();
}
