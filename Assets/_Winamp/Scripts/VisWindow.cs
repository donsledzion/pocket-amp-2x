using UnityEngine;
using UnityEngine.UI;

namespace SoftAware.PocketAmp.Visualizers
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

        public RectTransform Container => container;

        public void SetTitle(string title)
        {
            if (titleText != null) titleText.text = title;
        }
    }
}
