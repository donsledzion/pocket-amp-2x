using UnityEngine;
using UnityEngine.UI;

namespace SoftAware.Winamp
{
    public abstract class ContextMenuButton : MonoBehaviour
    {
        [SerializeField] private Transform optionsContainer;
        [SerializeField] private Image menuClipper;
        internal Button[] MenuButtons { get; private set; }
        internal Image MenuClipper => menuClipper;

        private Button button;

        private void Awake()
        {
            if (!TryGetComponent(out button)) throw new("Missing Button component!");
            MenuButtons = optionsContainer.GetComponentsInChildren<Button>();
            if (MenuButtons.Length < 1) throw new("Missing Buttons in children!");
            CloseMenu();
        }

        private void Start()
        {
            button.onClick.AddListener(HandleButtonClicked);
            foreach (var menuButton in MenuButtons)
                menuButton.onClick.AddListener(CloseMenu);
            SetupBindings();
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(HandleButtonClicked);
            foreach (var menuButton in MenuButtons)
                menuButton.onClick.RemoveListener(CloseMenu);
            ClearBindings();
        }

        internal abstract void SetupBindings();
        internal abstract void ClearBindings();

        private void OpenMenu() => optionsContainer.gameObject.SetActive(true);
        
        internal void CloseMenu() => optionsContainer.gameObject.SetActive(false);

        private void HandleButtonClicked() => OpenMenu();
    }
}
