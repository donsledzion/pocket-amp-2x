using UnityEngine;
using UnityEngine.UI;

namespace SoftAware.Winamp
{
    public class ContextMenuButton : MonoBehaviour
    {
        [SerializeField] private Transform optionsContainer;

        private Button button;

        private void Awake()
        {
            if (!TryGetComponent(out button)) throw new("Missing Button component!");
            CloseMenu();
        }

        private void Start()
        {
            button.onClick.AddListener(HandleButtonClicked);
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(HandleButtonClicked);
        }

        private void OpenMenu() => optionsContainer.gameObject.SetActive(true);
        
        internal void CloseMenu() => optionsContainer.gameObject.SetActive(false);

        private void HandleButtonClicked() => OpenMenu();
    }
}
