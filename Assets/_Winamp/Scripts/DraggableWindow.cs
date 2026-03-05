using UnityEngine;
using UnityEngine.EventSystems;

namespace SoftAware.PocketAmp
{
    public class DraggableWindow : MonoBehaviour, IDragHandler
    {
        private Canvas canvas;
        private RectTransform rectTransform;
        private WinampLayoutManager layoutManager;
        
        private void Start()
        {
            if (!transform.parent.TryGetComponent(out rectTransform))
                throw new("Missing RectTransform component");
            canvas = GetComponentInParent<Canvas>();
            if(!canvas)
                throw new("Missing RectTransform component");
            layoutManager = GetComponentInParent<WinampLayoutManager>();
            if(!layoutManager)
                throw new("Missing WinampLayoutManager component");
        }

        public void OnDrag(PointerEventData eventData)
        {
            rectTransform.anchoredPosition += eventData.delta / (canvas.scaleFactor *  layoutManager.transform.localScale);
        }
    }
}
