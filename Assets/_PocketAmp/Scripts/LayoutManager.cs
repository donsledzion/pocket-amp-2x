using UnityEngine;
using UnityEngine.UI;

namespace SoftAware.PocketAmp
{
    [ExecuteInEditMode]
    public class LayoutManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform layoutRoot;
        [SerializeField] private RectTransform leftColumn;
        [SerializeField] private RectTransform rightColumn;
        [SerializeField] private RectTransform overlayWindows;

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
            // Reset cache on inspector changes
            CacheComponents();
        }

        private void Start()
        {
            // Subscribe to Main events
            var main = FindFirstObjectByType<Main>();
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
            // In Editor (Simulator) track parent size changes
            if (!Application.isPlaying)
            {
                var parentRect = transform.parent as RectTransform;
                if (parentRect != null)
                {
                    var currentSize = parentRect.rect.size;
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

            // At runtime track orientation and size changes
            if (Application.isPlaying)
            {
                var currentOrientation = Screen.orientation;
                if (currentOrientation != lastOrientation)
                {
                    lastOrientation = currentOrientation;
                    SetDirty();
                }

                // Additionally track parent size (SafeArea might change)
                var parentRect = transform.parent as RectTransform;
                if (parentRect != null)
                {
                    var currentSize = parentRect.rect.size;
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

            // Try fetching CanvasGroup if cache is empty (especially in Editor)
            CanvasGroup cg = null;
            if (panel == eqPanel) cg = eqCanvasGroup;
            else if (panel == playlistPanel) cg = playlistCanvasGroup;

            if (cg == null) panel.TryGetComponent(out cg);

            if (cg != null && cg.alpha <= 0.001f) return false;

            // Additionally check LayoutElement.ignoreLayout (just in case)
            if (panel.TryGetComponent(out LayoutElement le) && le.ignoreLayout) return false;

            return true;
        }

        [ContextMenu("Update Layout")]
        public void UpdateLayout()
        {
            if (layoutRoot == null || leftColumn == null || rightColumn == null) return;

            var parentRect = transform.parent as RectTransform;
            if (parentRect == null) return;

            var screenWidth = parentRect.rect.width;
            var screenHeight = parentRect.rect.height;
            isLandscape = screenWidth > screenHeight;

            if (showDebugLogs)
                Debug.Log($"[LayoutManager] Updating. Parent: {screenWidth}x{screenHeight}, Mode: {(isLandscape ? "Landscape" : "Portrait")}");

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
            var nextY = -mainHeight;

            if (IsPanelVisible(eqPanel))
            {
                PositionPanel(eqPanel, nextY);
                nextY -= eqHeight;
            }

            // VisualizationPanel in LeftColumn - square below Main/EQ (might overlap EQ if no space)
            if (visualizationPanel != null && IsPanelVisible(visualizationPanel))
            {
                // Move to LeftColumn if not already there
                if (visualizationPanel.parent != leftColumn)
                    visualizationPanel.SetParent(leftColumn, false);
                
                // Position directly below Main (will overlap EQ if present)
                PositionPanel(visualizationPanel, -mainHeight);
                visualizationPanel.sizeDelta = new Vector2(nativeWidth, nativeWidth);
            }

            // RightColumn - always stretched from nextY to bottom (for Playlist)
            ConfigureColumn(rightColumn, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, nextY));
            
            var unscaledScreenHeight = screenHeight / currentScale;
            var playlistHeight = unscaledScreenHeight + nextY;
            
            rightColumn.sizeDelta = new Vector2(nativeWidth, Mathf.Max(0, playlistHeight));
            
            if (playlistPanel != null)
                SetPanelStretch(playlistPanel);

            // OverlayWindows - stretch to full screen in unscaled units, centering anchors
            if (overlayWindows != null)
            {
                if (overlayWindows.parent != layoutRoot)
                    overlayWindows.SetParent(layoutRoot, false);

                overlayWindows.anchorMin = new Vector2(0.5f, 0.5f);
                overlayWindows.anchorMax = new Vector2(0.5f, 0.5f);
                overlayWindows.pivot = new Vector2(0.5f, 0.5f);
                
                // layoutRoot in Portrait is at (0.5, 1) - X is already centered, Y needs to be lowered
                overlayWindows.anchoredPosition = new Vector2(0, -unscaledScreenHeight / 2f);
                overlayWindows.sizeDelta = new Vector2(nativeWidth, unscaledScreenHeight);
            }

            if (showDebugLogs)
                Debug.Log($"[LayoutManager] Portrait: NextY={nextY}, PlaylistHeight={playlistHeight}");
        }

        private void ApplyLandscapeLayout(float screenWidth, float screenHeight)
        {
            var targetHalfWidth = screenWidth / 2f;
            currentScale = targetHalfWidth / nativeWidth;
            
            var neededHeight = mainHeight;
            if (IsPanelVisible(eqPanel))
                neededHeight += eqHeight;

            var scaledNeededHeight = neededHeight * currentScale;

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

            var scaledLeftWidth = nativeWidth * currentScale;

            ConfigureColumn(leftColumn, new Vector2(0f, 1f), new Vector2(0f, 1f), Vector2.zero);
            leftColumn.sizeDelta = new Vector2(nativeWidth, neededHeight);

            PositionPanel(mainPanel, 0);
            if (IsPanelVisible(eqPanel))
                PositionPanel(eqPanel, -mainHeight);

            // Right column - always full height
            var remainingWidthScaled = screenWidth - scaledLeftWidth;
            var remainingWidthUnscaled = remainingWidthScaled / currentScale;
            var unscaledScreenHeight = screenHeight / currentScale;

            ConfigureColumn(rightColumn, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(nativeWidth, 0));
            rightColumn.sizeDelta = new Vector2(remainingWidthUnscaled, unscaledScreenHeight);

            // Playlist - always stretch (if visible)
            if (playlistPanel != null && IsPanelVisible(playlistPanel))
                SetPanelStretch(playlistPanel);

            // VisualizationPanel - square centered in RightColumn (overlaps Playlist if visible)
            if (visualizationPanel != null && IsPanelVisible(visualizationPanel))
            {
                // Move to RightColumn if not already there
                if (visualizationPanel.parent != rightColumn)
                    visualizationPanel.SetParent(rightColumn, false);
                
                visualizationPanel.anchorMin = new Vector2(0.5f, 0.5f);
                visualizationPanel.anchorMax = new Vector2(0.5f, 0.5f);
                visualizationPanel.pivot = new Vector2(0.5f, 0.5f);
                visualizationPanel.anchoredPosition = Vector2.zero;
                visualizationPanel.sizeDelta = new Vector2(nativeWidth, nativeWidth);
            }

            if (showDebugLogs)
                Debug.Log($"[LayoutManager] Landscape: RightColumnWidth={remainingWidthUnscaled}");

            // OverlayWindows - stretch to full screen in unscaled units, centering anchors
            if (overlayWindows != null)
            {
                if (overlayWindows.parent != layoutRoot)
                    overlayWindows.SetParent(layoutRoot, false);

                overlayWindows.anchorMin = new Vector2(0.5f, 0.5f);
                overlayWindows.anchorMax = new Vector2(0.5f, 0.5f);
                overlayWindows.pivot = new Vector2(0.5f, 0.5f);
                
                // layoutRoot in Landscape is at (0, 1) - Origin is in top-left corner.
                // To hit screen center (USW/2, -USH/2), we need to subtract anchor offset (nativeWidth/2, 0)
                var unscaledScreenWidth = screenWidth / currentScale;
                var offsetX = (unscaledScreenWidth / 2f) - (nativeWidth / 2f);
                var offsetY = -unscaledScreenHeight / 2f;
                
                overlayWindows.anchoredPosition = new Vector2(offsetX, offsetY);
                overlayWindows.sizeDelta = new Vector2(unscaledScreenWidth, unscaledScreenHeight);
            }
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
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            for (int i = 0; i < 4; i++)
                Gizmos.DrawLine(corners[i], corners[(i + 1) % 4]);
        }
    }
}
