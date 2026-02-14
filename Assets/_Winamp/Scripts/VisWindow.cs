using UnityEngine;
using UnityEngine.UI;

namespace SoftAware.Winamp.Visualizers
{
    /// <summary>
    /// Dedicated window for advanced visualizer plugins.
    /// Acts as a host for IVisualizerPlugin implementations.
    /// </summary>
    public class VisWindow : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform container;
        [SerializeField] private Text titleText;
        [SerializeField] private Button closeButton;

        private void Start()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(() => gameObject.SetActive(false));
            }

            // Ensure we have a black background for additive shaders to pop
            Image bg = container.GetComponent<Image>();
            if (bg == null) bg = container.gameObject.AddComponent<Image>();
            bg.color = Color.black;
            
            // Add a RectMask2D to prevent visualizer from bleeding out of window
            if (container.GetComponent<RectMask2D>() == null)
                container.gameObject.AddComponent<RectMask2D>();
        }

        public RectTransform Container => container;

        public void SetTitle(string title)
        {
            if (titleText != null) titleText.text = title;
        }
    }
}
