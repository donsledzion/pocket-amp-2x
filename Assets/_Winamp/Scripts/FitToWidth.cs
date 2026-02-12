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
        // 1. Wymuszenie poprawnych ustawień layoutu przed obliczeniami
        EnforceCorrectLayout();

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

    /// <summary>
    /// Naprawia konflikt layoutu: wymusza, aby Equalizer miał stałą wysokość, 
    /// a Playlist wypełniał resztę - przy włączonym ControlChildSize.
    /// </summary>
    [ContextMenu("Enforce Layout Settings")]
    private void EnforceCorrectLayout()
    {
        // 1. Napraw ustawienia kontenera (BottomContainer)
        if (bottomLayout == null) return;
        var vlg = bottomLayout.GetComponent<VerticalLayoutGroup>();
        if (vlg != null)
        {
            // To musi być włączone, żeby Playlist nie uciekał
            vlg.childControlHeight = true; 
            // To musi być wyłączone, żeby nie rozciągać Equalizera na siłę
            vlg.childForceExpandHeight = false; 
        }

        // 2. Napraw Equalizer (stała wysokość)
        Transform bottomTr = bottomLayout.transform;
        Transform eqTr = bottomTr.Find("Equalizer");
        if (eqTr != null)
        {
            var le = eqTr.GetComponent<LayoutElement>();
            if (le == null) le = eqTr.gameObject.AddComponent<LayoutElement>();

            le.flexibleHeight = 0;      // Nie rozciągaj się
            le.preferredHeight = 116f;  // Oryginalna wysokość Winampa
            le.minHeight = 116f;        // Wymuś minimalną
        }

        // 3. Napraw Playlist (wypełnij resztę)
        Transform plTr = bottomTr.Find("Playlist");
        if (plTr != null)
        {
            var le = plTr.GetComponent<LayoutElement>();
            if (le == null) le = plTr.gameObject.AddComponent<LayoutElement>();

            le.flexibleHeight = 1;      // Rozciągnij się na resztę miejsca
            le.preferredHeight = 0;     // Nie narzucaj preferowanej
        }
    }
}