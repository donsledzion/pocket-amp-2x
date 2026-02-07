using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FitToWidth : MonoBehaviour
{
    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private RectTransform panelContainer;
    [SerializeField] private RectTransform mainPanel;
    [SerializeField] private LayoutElement bottomLayout;

    [Space]
    [SerializeField] private bool fitBottom;

    private void Start()
    {
        Apply();
    }

    [ContextMenu("Apply")]
    private void Apply()
    {
        StartCoroutine(ApplyCoroutine());
    }

    private IEnumerator ApplyCoroutine()
    {
        yield return null; // ← KLUCZOWE

        // skala
        float screenWidth = canvasRect.rect.width;
        float scale = screenWidth / panelContainer.rect.width;
        panelContainer.localScale = Vector3.one * scale;

        yield return null; // ← jeszcze raz, po skali

        if(fitBottom)
            FitBottom();
    }

    [ContextMenu("Fit Bottom")]
    private void FitBottom()
    {
        float screenHeight = canvasRect.rect.height;
        float unscaledHeight = screenHeight / panelContainer.localScale.y;
        float mainHeight = mainPanel.rect.height;

        bottomLayout.preferredHeight =
            Mathf.Max(0f, unscaledHeight - mainHeight);

        LayoutRebuilder.ForceRebuildLayoutImmediate(panelContainer);
    }
}