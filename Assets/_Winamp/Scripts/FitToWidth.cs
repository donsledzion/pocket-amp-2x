using UnityEngine;

public class FitToWidth : MonoBehaviour
{
    [SerializeField] private RectTransform panel;

    private void Start()
    {
        var screenWidth = ((RectTransform)transform).rect.width;
        var scale = screenWidth / panel.rect.width;
        panel.localScale = Vector3.one * scale;
    }
}