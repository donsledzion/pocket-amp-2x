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

    private void Awake()
    {
        closeButton.onClick.AddListener(CloseMenu);
        skinsLibraryButton.onClick.AddListener(OpenSkinsLibrary);
        notificationsBarToggle.onClick.AddListener(ToggleNotificationsBar);
        if (navigationBarToggle != null) navigationBarToggle.onClick.AddListener(ToggleNavigationBar);
    }

    private void Start()
    {
        UpdateNotificationsButtonText();
        UpdateNavigationButtonText();
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
            // Update Screen.fullScreen based on Navigation Bar visibility
            // Visible = Not Fullscreen (Windowed)
            // Hidden = Fullscreen (Immersive)
            Screen.fullScreen = !newState;

            // Re-apply Status Bar State because changing Screen.fullScreen might reset it
            // IsFullscreen (true) -> Status Bar Hidden
            // IsFullscreen (false) -> Status Bar Visible
            AndroidStatusBar.SetVisible(!SettingsManager.Instance.IsFullscreen);
        }
        
        UpdateNavigationButtonText();
    }

    private void SetFullscreenState(bool isFullscreen)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            // On Android, "Fullscreen" toggle specifically controls "Status Bar" visibility
            // independent of the Navigation Bar (which controls Screen.fullScreen)
            // Fullscreen = Status Bar HIDDEN
            // Windowed = Status Bar VISIBLE
            AndroidStatusBar.SetVisible(!isFullscreen); 
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

    private void OpenSkinsLibrary()
    {
        CloseMenu();
        Refs.Main.OpenSkinsLibrary();
    }

    internal void CloseMenu() => Refs.Main.CloseMiscOptionsMenu();
}
