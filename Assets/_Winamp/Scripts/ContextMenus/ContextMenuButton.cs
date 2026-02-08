using UnityEngine;
using UnityEngine.UI;

namespace SoftAware.Winamp
{
    public abstract class ContextMenuButton : MonoBehaviour
    {
        [SerializeField] private Transform optionsContainer;
        internal Button[] MenuButtons { get; private set; }

        private Button button;

        private void Awake()
        {
            if (!TryGetComponent(out button)) throw new("Missing Button component!");
            MenuButtons = GetComponentsInChildren<Button>();
            if (MenuButtons.Length < 1) throw new("Missing Buttons in children!");
            CloseMenu();
        }

        private void Start()
        {
            button.onClick.AddListener(HandleButtonClicked);
            SetupBindings();
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(HandleButtonClicked);
            ClearBindings();
        }

        internal abstract void SetupBindings();
        internal abstract void ClearBindings();

        private void OpenMenu() => optionsContainer.gameObject.SetActive(true);
        
        internal void CloseMenu() => optionsContainer.gameObject.SetActive(false);

        private void HandleButtonClicked() => OpenMenu();
    }
}
