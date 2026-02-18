using UnityEngine;
using UnityEngine.UI;

namespace SoftAware.PocketAmp
{
    [ExecuteInEditMode]
    public class WinampLayoutManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform layoutRoot;
        [SerializeField] private RectTransform leftColumn;
        [SerializeField] private RectTransform rightColumn;

        [Header("Panels")]
        [SerializeField] private RectTransform mainPanel;
        [SerializeField] private RectTransform eqPanel;
        [SerializeField] private RectTransform playlistPanel;
        [SerializeField] private RectTransform visualizationPanel;

        [Header("Configuration")]
        [SerializeField] private float nativeWidth = 275f;
        [SerializeField] private float mainHeight = 116f;
        [SerializeField] private float eqHeight = 116f;

        [Header("Debug Status (ReadOnly)")]
        [SerializeField] private bool isLandscape;
        [SerializeField] private float currentScale = 1f;

        [Header("Debug Tools")]
        [SerializeField] private bool showDebugLogs = true;
        [SerializeField] private Color debugColor = Color.yellow;

        private CanvasGroup eqCanvasGroup;
        private CanvasGroup playlistCanvasGroup;
        private bool isDirty;

        private void Awake()
        {
            CacheComponents();
        }

        private void OnValidate()
        {
            // Resetuj cache przy zmianach w inspektorze
            CacheComponents();
        }

        private void Start()
        {
            // Subskrypcja zdarzeń z Main
            Main main = FindFirstObjectByType<Main>();
            if (main != null)
            {
                if (main.EqButton != null) 
                    main.EqButton.OnValueChanged.AddListener(_ => SetDirty());
                if (main.PlaylistButton != null) 
                    main.PlaylistButton.OnValueChanged.AddListener(_ => SetDirty());
            }
            
            SetDirty();
        }

        private void CacheComponents()
        {
            if (eqPanel != null) eqPanel.TryGetComponent(out eqCanvasGroup);
            if (playlistPanel != null) playlistPanel.TryGetComponent(out playlistCanvasGroup);
        }

        private void OnRectTransformDimensionsChange()
        {
            SetDirty();
        }

#if UNITY_EDITOR
        private Vector2 lastParentSize;
#endif
        private ScreenOrientation lastOrientation;
        private Vector2 lastRuntimeParentSize;

        private void LateUpdate()
        {
            if (isDirty)
            {
                UpdateLayout();
                isDirty = false;
            }

#if UNITY_EDITOR
            // W edytorze (Simulator) śledź zmiany rozmiaru rodzica
            if (!Application.isPlaying)
            {
                RectTransform parentRect = transform.parent as RectTransform;
                if (parentRect != null)
                {
                    Vector2 currentSize = parentRect.rect.size;
                    if (currentSize != lastParentSize)
                    {
                        lastParentSize = currentSize;
                        SetDirty();
                    }
                }
                
                if (transform.hasChanged)
                {
                    UpdateLayout();
                }
            }
#endif

            // W runtime śledź zmiany orientacji i rozmiaru
            if (Application.isPlaying)
            {
                ScreenOrientation currentOrientation = Screen.orientation;
                if (currentOrientation != lastOrientation)
                {
                    lastOrientation = currentOrientation;
                    SetDirty();
                }

                // Dodatkowo śledź rozmiar rodzica (SafeArea może się zmienić)
                RectTransform parentRect = transform.parent as RectTransform;
                if (parentRect != null)
                {
                    Vector2 currentSize = parentRect.rect.size;
                    if (currentSize != lastRuntimeParentSize)
                    {
                        lastRuntimeParentSize = currentSize;
                        SetDirty();
                    }
                }
            }
        }

        public void SetDirty()
        {
            isDirty = true;
        }

        private bool IsPanelVisible(RectTransform panel)
        {
            if (panel == null) return false;
            if (!panel.gameObject.activeInHierarchy) return false;

            // Spróbuj pobrać CanvasGroup jeśli cache jest pusty (szczególnie w Editorze)
            CanvasGroup cg = null;
            if (panel == eqPanel) cg = eqCanvasGroup;
            else if (panel == playlistPanel) cg = playlistCanvasGroup;

            if (cg == null) panel.TryGetComponent(out cg);

            if (cg != null && cg.alpha <= 0.001f) return false;

            // Dodatkowo sprawdźmy LayoutElement.ignoreLayout (na wszelki wypadek)
            if (panel.TryGetComponent(out LayoutElement le) && le.ignoreLayout) return false;

            return true;
        }

        [ContextMenu("Update Layout")]
        public void UpdateLayout()
        {
            if (layoutRoot == null || leftColumn == null || rightColumn == null) return;

            RectTransform parentRect = transform.parent as RectTransform;
            if (parentRect == null) return;

            float screenWidth = parentRect.rect.width;
            float screenHeight = parentRect.rect.height;
            isLandscape = screenWidth > screenHeight;

            if (showDebugLogs)
                Debug.Log($"[WinampLayout] Updating. Parent: {screenWidth}x{screenHeight}, Mode: {(isLandscape ? "Landscape" : "Portrait")}");

            if (isLandscape)
            {
                ApplyLandscapeLayout(screenWidth, screenHeight);
            }
            else
            {
                ApplyPortraitLayout(screenWidth, screenHeight);
            }
            
            transform.hasChanged = false;
        }

        private void ApplyPortraitLayout(float screenWidth, float screenHeight)
        {
            currentScale = screenWidth / nativeWidth;
            
            layoutRoot.anchorMin = new Vector2(0.5f, 1f);
            layoutRoot.anchorMax = new Vector2(0.5f, 1f);
            layoutRoot.pivot = new Vector2(0.5f, 1f);
            layoutRoot.anchoredPosition = Vector2.zero;
            layoutRoot.sizeDelta = new Vector2(nativeWidth, 0); 
            layoutRoot.localScale = Vector3.one * currentScale;

            ConfigureColumn(leftColumn, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), Vector2.zero);
            leftColumn.sizeDelta = new Vector2(nativeWidth, 0); 
            
            PositionPanel(mainPanel, 0);
            float nextY = -mainHeight;

            if (IsPanelVisible(eqPanel))
            {
                PositionPanel(eqPanel, nextY);
                nextY -= eqHeight;
            }

            // VisualizationPanel w LeftColumn - kwadrat pod Main/EQ (może przykryć EQ jeśli nie ma miejsca)
            if (visualizationPanel != null && IsPanelVisible(visualizationPanel))
            {
                // Przenieś do LeftColumn jeśli nie jest tam
                if (visualizationPanel.parent != leftColumn)
                    visualizationPanel.SetParent(leftColumn, false);
                
                // Pozycjonuj bezpośrednio pod Mainem (przykryje EQ jeśli jest)
                PositionPanel(visualizationPanel, -mainHeight);
                visualizationPanel.sizeDelta = new Vector2(nativeWidth, nativeWidth);
            }

            // RightColumn - zawsze rozciągnięta od nextY do dołu (dla Playlist)
            ConfigureColumn(rightColumn, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, nextY));
            
            float unscaledScreenHeight = screenHeight / currentScale;
            float playlistHeight = unscaledScreenHeight + nextY;
            
            rightColumn.sizeDelta = new Vector2(nativeWidth, Mathf.Max(0, playlistHeight));
            
            if (playlistPanel != null)
                SetPanelStretch(playlistPanel);

            if (showDebugLogs)
                Debug.Log($"[WinampLayout] Portrait: NextY={nextY}, PlaylistHeight={playlistHeight}");
        }

        private void ApplyLandscapeLayout(float screenWidth, float screenHeight)
        {
            float targetHalfWidth = screenWidth / 2f;
            currentScale = targetHalfWidth / nativeWidth;
            
            float neededHeight = mainHeight;
            if (IsPanelVisible(eqPanel))
                neededHeight += eqHeight;

            float scaledNeededHeight = neededHeight * currentScale;

            if (scaledNeededHeight > screenHeight)
            {
                currentScale = screenHeight / neededHeight;
            }

            layoutRoot.anchorMin = new Vector2(0f, 1f);
            layoutRoot.anchorMax = new Vector2(0f, 1f);
            layoutRoot.pivot = new Vector2(0f, 1f);
            layoutRoot.anchoredPosition = Vector2.zero;
            layoutRoot.sizeDelta = new Vector2(nativeWidth, 0);
            layoutRoot.localScale = Vector3.one * currentScale;

            float scaledLeftWidth = nativeWidth * currentScale;

            ConfigureColumn(leftColumn, new Vector2(0f, 1f), new Vector2(0f, 1f), Vector2.zero);
            leftColumn.sizeDelta = new Vector2(nativeWidth, neededHeight);

            PositionPanel(mainPanel, 0);
            if (IsPanelVisible(eqPanel))
                PositionPanel(eqPanel, -mainHeight);

            // Prawa kolumna - zawsze pełna wysokość
            float remainingWidthScaled = screenWidth - scaledLeftWidth;
            float remainingWidthUnscaled = remainingWidthScaled / currentScale;
            float unscaledScreenHeight = screenHeight / currentScale;

            ConfigureColumn(rightColumn, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(nativeWidth, 0));
            rightColumn.sizeDelta = new Vector2(remainingWidthUnscaled, unscaledScreenHeight);

            // Playlist - zawsze stretch (jeśli widoczna)
            if (playlistPanel != null && IsPanelVisible(playlistPanel))
                SetPanelStretch(playlistPanel);

            // VisualizationPanel - kwadrat wycentrowany w RightColumn (nakłada się na Playlist jeśli widoczny)
            if (visualizationPanel != null && IsPanelVisible(visualizationPanel))
            {
                // Przenieś do RightColumn jeśli nie jest tam
                if (visualizationPanel.parent != rightColumn)
                    visualizationPanel.SetParent(rightColumn, false);
                
                visualizationPanel.anchorMin = new Vector2(0.5f, 0.5f);
                visualizationPanel.anchorMax = new Vector2(0.5f, 0.5f);
                visualizationPanel.pivot = new Vector2(0.5f, 0.5f);
                visualizationPanel.anchoredPosition = Vector2.zero;
                visualizationPanel.sizeDelta = new Vector2(nativeWidth, nativeWidth);
            }

            if (showDebugLogs)
                Debug.Log($"[WinampLayout] Landscape: RightColumnWidth={remainingWidthUnscaled}");
        }

        private void ConfigureColumn(RectTransform col, Vector2 anchor, Vector2 pivot, Vector2 pos)
        {
            col.anchorMin = anchor;
            col.anchorMax = anchor;
            col.pivot = pivot;
            col.anchoredPosition = pos;
        }

        private void PositionPanel(RectTransform panel, float topY)
        {
            if (panel == null) return;
            panel.anchorMin = new Vector2(0.5f, 1f);
            panel.anchorMax = new Vector2(0.5f, 1f);
            panel.pivot = new Vector2(0.5f, 1f);
            panel.anchoredPosition = new Vector2(0, topY);
            panel.sizeDelta = new Vector2(nativeWidth, panel.rect.height);
        }

        private void SetPanelStretch(RectTransform panel)
        {
            panel.anchorMin = Vector2.zero;
            panel.anchorMax = Vector2.one;
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;
        }

        private void OnDrawGizmosSelected()
        {
            if (leftColumn == null || rightColumn == null) return;
            
            Gizmos.color = debugColor;
            DrawRectGizmo(leftColumn);
            Gizmos.color = Color.cyan;
            DrawRectGizmo(rightColumn);
        }

        private void DrawRectGizmo(RectTransform rect)
        {
            Vector3[] corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            for (int i = 0; i < 4; i++)
                Gizmos.DrawLine(corners[i], corners[(i + 1) % 4]);
        }
    }
}
