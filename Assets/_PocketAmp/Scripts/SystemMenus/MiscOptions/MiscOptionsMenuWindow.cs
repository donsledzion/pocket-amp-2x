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

    [Header("Localization")]
    [SerializeField] private UnityEngine.Localization.LocalizedString notificationsOnText;
    [SerializeField] private UnityEngine.Localization.LocalizedString notificationsOffText;
    [SerializeField] private UnityEngine.Localization.LocalizedString navigationOnText;
    [SerializeField] private UnityEngine.Localization.LocalizedString navigationOffText;
    [SerializeField] private UnityEngine.Localization.LocalizedString eqRequireLoadText;
    [SerializeField] private UnityEngine.Localization.LocalizedString eqLoadOnSelectText;

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
            if (SettingsManager.Instance.IsFullscreen)
            {
                textRequest.text = (notificationsOffText != null && !notificationsOffText.IsEmpty) ? notificationsOffText.GetLocalizedString() : "Notifications bar: OFF";
            }
            else
            {
                textRequest.text = (notificationsOnText != null && !notificationsOnText.IsEmpty) ? notificationsOnText.GetLocalizedString() : "Notifications bar: ON";
            }
        }
    }

    private void UpdateNavigationButtonText()
    {
        if (navigationBarToggle == null) return;
        
        var textRequest = navigationBarToggle.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (textRequest != null)
        {
            if (SettingsManager.Instance.IsNavigationBarVisible)
            {
                textRequest.text = (navigationOnText != null && !navigationOnText.IsEmpty) ? navigationOnText.GetLocalizedString() : "Navigation bar: ON";
            }
            else
            {
                textRequest.text = (navigationOffText != null && !navigationOffText.IsEmpty) ? navigationOffText.GetLocalizedString() : "Navigation bar: OFF";
            }
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
            if (SettingsManager.Instance.EQPresetsLoadBehavior == 0)
            {
                textRequest.text = (eqRequireLoadText != null && !eqRequireLoadText.IsEmpty) ? eqRequireLoadText.GetLocalizedString() : "EQ Presets: Require Load";
            }
            else
            {
                textRequest.text = (eqLoadOnSelectText != null && !eqLoadOnSelectText.IsEmpty) ? eqLoadOnSelectText.GetLocalizedString() : "EQ Presets: Load on Select";
            }
        }
    }

    private void OpenSkinsLibrary()
    {
        CloseMenu();
        Refs.Main.OverlayWindowsController.OpenSkinsLibrary();
    }

    internal void CloseMenu() => Refs.Main.OverlayWindowsController.CloseMiscOptionsMenu();
}
