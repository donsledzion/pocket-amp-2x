using UnityEngine;
using System.Threading.Tasks;
using SoftAware.PocketAmp.Tutorial;
using SoftAware;

namespace SoftAware.PocketAmp
{
    public class AppStartupOrchestrator : MonoBehaviour
    {
        [Header("UI Groups")]
        [SerializeField] private CanvasGroup mainCanvasGroup;
        [SerializeField] private CanvasGroup firstRunCanvasGroup;

        [Header("Managers")]
        [SerializeField] private SkinManager skinManager;
        [SerializeField] private Playlist playlist;
        [SerializeField] private TutorialManager tutorialManager;
        [SerializeField] private PermissionsManager permissionsManager;

        private async void Start()
        {
            // 1. On startup, immediately hide the main UI.
            if (mainCanvasGroup != null)
            {
                mainCanvasGroup.alpha = 0f;
                mainCanvasGroup.interactable = false;
                mainCanvasGroup.blocksRaycasts = false;
            }

            bool isFirstRun = PlayerPrefs.GetInt("DemoSkinsCopied", 0) == 0;

            // 2. Show "First Run" loading screen if necessary
            if (isFirstRun && firstRunCanvasGroup != null)
            {
                firstRunCanvasGroup.gameObject.SetActive(true);
                firstRunCanvasGroup.alpha = 1f;
            }
            else if (firstRunCanvasGroup != null)
            {
                firstRunCanvasGroup.alpha = 0f;
                firstRunCanvasGroup.gameObject.SetActive(false);
            }

            // 3. Initialize Skins (Copies demo skins on first run, loads active skin, applies materials)
            if (skinManager != null)
            {
                await skinManager.InitializeAsync();
            }

            // 4. Fade to Main UI
            await FadeCanvasGroupsAsync(firstRunCanvasGroup, mainCanvasGroup, 0.5f);

            // 5. Ask for permissions sequentially
            if (permissionsManager != null)
            {
                await permissionsManager.RequestStartupPermissionsAsync();
            }

            // 6. Initialize Playlist (Wait for permissions before loading files)
            if (playlist != null)
            {
                await playlist.InitializeAsync();
            }

            // 7. Initialize Tutorial (Alpaccino triggers if conditions are met)
            if (tutorialManager != null)
            {
                tutorialManager.InitializeTutorialState();
            }
        }

        private async Task FadeCanvasGroupsAsync(CanvasGroup from, CanvasGroup to, float duration)
        {
            float elapsed = 0f;

            // Ensure 'to' is active but invisible
            if (to != null)
            {
                to.gameObject.SetActive(true);
                to.alpha = 0f;
                to.interactable = false;
                to.blocksRaycasts = false;
            }

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                if (from != null) from.alpha = 1f - t;
                if (to != null) to.alpha = t;

                await Task.Yield();
            }

            if (from != null)
            {
                from.alpha = 0f;
                from.gameObject.SetActive(false);
            }

            if (to != null)
            {
                to.alpha = 1f;
                to.interactable = true;
                to.blocksRaycasts = true;
            }
        }
    }
}
