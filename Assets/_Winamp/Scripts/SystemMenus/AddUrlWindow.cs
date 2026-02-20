using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SoftAware.PocketAmp
{
    public class AddUrlWindow : MonoBehaviour
    {
        [SerializeField] private TMP_InputField urlInput;
        [SerializeField] private Button openButton;
        [SerializeField] private Button cancelButton;

        private void Awake()
        {
            if (openButton != null) openButton.onClick.AddListener(OnOpenClicked);
            if (cancelButton != null) cancelButton.onClick.AddListener(CloseWindow);
        }

        private void OnEnable()
        {
            if (urlInput != null)
            {
                urlInput.text = "";
                urlInput.Select();
                urlInput.ActivateInputField();
            }
        }

        private void OnOpenClicked()
        {
            if (urlInput != null && !string.IsNullOrWhiteSpace(urlInput.text))
            {
                var url = urlInput.text.Trim();
                // Basic validation
                if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                {
                    url = "http://" + url;
                }
                
                Refs.Playlist.AddUrl(url);
            }
            CloseWindow();
        }

        private void CloseWindow()
        {
            Refs.Main.CloseAddUrlWindow();
        }
    }
}
