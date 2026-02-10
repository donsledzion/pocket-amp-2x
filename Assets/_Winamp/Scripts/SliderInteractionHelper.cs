using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace SoftAware
{
    public class SliderInteractionHelper : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public Action OnPointerDownAction;
        public Action OnPointerUpAction;

        public void OnPointerDown(PointerEventData eventData)
        {
            OnPointerDownAction?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            OnPointerUpAction?.Invoke();
        }
    }
}
