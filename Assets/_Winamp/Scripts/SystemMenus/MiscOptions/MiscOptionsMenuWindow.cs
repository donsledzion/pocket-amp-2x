using SoftAware.PocketAmp;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MiscOptionsMenuWindow : MonoBehaviour
{
    [SerializeField] private Button closeButton;
    [SerializeField] private Button skinsLibraryButton;
    [SerializeField] private Button notificationsBarToggle;

    private void Awake()
    {
        closeButton.onClick.AddListener(CloseMenu);
        skinsLibraryButton.onClick.AddListener(OpenSkinsLibrary);
        notificationsBarToggle.onClick.AddListener(ToggleNotificationsBar);
    }

    private void Start()
    {
        UpdateNotificationsButtonText();
    }

    private void ToggleNotificationsBar()
    {
        bool newFullscreenState = !SettingsManager.Instance.IsFullscreen;
        SettingsManager.Instance.IsFullscreen = newFullscreenState;

        SetFullscreenState(newFullscreenState);
        UpdateNotificationsButtonText();
    }

    private void SetFullscreenState(bool isFullscreen)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
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

    private void OpenSkinsLibrary()
    {
        CloseMenu();
        Refs.Main.OpenSkinsLibrary();
    }

    internal void CloseMenu() => Refs.Main.CloseMiscOptionsMenu();
}
