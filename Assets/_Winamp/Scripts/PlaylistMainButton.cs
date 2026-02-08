using UnityEngine;
using UnityEngine.UI;

namespace SoftAware.Winamp
{
    public class PlaylistMainButton : MonoBehaviour
    {
        [SerializeField] private Transform optionsContainer;

        private Button button;

        private void Awake()
        {
            if (!TryGetComponent(out button)) throw new("Missing Button component!");
            optionsContainer.gameObject.SetActive(false);
        }

        private void Start()
        {
            button.onClick.AddListener(HandleButtonClicked);
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(HandleButtonClicked);
        }

        private void HandleButtonClicked()
        {
            optionsContainer.gameObject.SetActive(true);
        }
    }
}
