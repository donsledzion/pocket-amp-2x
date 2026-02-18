using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace SoftAware.PocketAmp
{
    public class ContextMenuHandler : MonoBehaviour
    {
        private ContextMenuButton[] contextMenus;

        private void Awake()
        {
            contextMenus = FindObjectsByType<ContextMenuButton>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        }

        private void Update()
        {
            Vector2? inputPos = null;

            // Touch
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            {
                inputPos = Touchscreen.current.primaryTouch.position.ReadValue();
            }
            // Mouse (Editor / PC)
            else if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame)
            {
                inputPos = Mouse.current.position.ReadValue();
            }

            if (inputPos == null)
                return;

            var eventData = new PointerEventData(EventSystem.current)
            {
                position = inputPos.Value
            };

            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            foreach (var menu in contextMenus)
            {
                var hitMenu = false;
                foreach (var r in results)
                {
                    if (r.gameObject.transform.IsChildOf(menu.transform))
                    {
                        hitMenu = true;
                        break;
                    }
                }

                if (!hitMenu)
                    menu.CloseMenu();
            }
        }
    }
}