using UnityEngine;
using UnityEngine.UI;

namespace SoftAware.Winamp
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

        private bool lastEqActive;
        private bool lastPlaylistActive;

        private void OnEnable()
        {
            UpdateLayout();
        }

        private void Update()
        {
            bool eqActive = IsPanelVisible(eqPanel);
            bool playlistActive = IsPanelVisible(playlistPanel);

            if (eqActive != lastEqActive || playlistActive != lastPlaylistActive || !Application.isPlaying || transform.hasChanged)
            {
                lastEqActive = eqActive;
                lastPlaylistActive = playlistActive;
                UpdateLayout();
            }
        }

        private bool IsPanelVisible(RectTransform panel)
        {
            if (panel == null) return false;
            if (!panel.gameObject.activeInHierarchy) return false;

            // Sprawdzamy CanvasGroup (alpha), ponieważ Main.cs ukrywa okna przez alpha
            CanvasGroup cg = panel.GetComponent<CanvasGroup>();
            if (cg != null && cg.alpha <= 0.001f) return false;

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

            ConfigureColumn(rightColumn, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, nextY));
            
            float unscaledScreenHeight = screenHeight / currentScale;
            float playlistHeight = unscaledScreenHeight + nextY; 
            
            rightColumn.sizeDelta = new Vector2(nativeWidth, Mathf.Max(0, playlistHeight));
            
            if (showDebugLogs)
                Debug.Log($"[WinampLayout] Portrait Playlist: TopY={nextY}, Scale={currentScale}");

            if (playlistPanel != null)
                SetPanelStretch(playlistPanel);
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

            float remainingWidthScaled = screenWidth - scaledLeftWidth;
            float remainingWidthUnscaled = remainingWidthScaled / currentScale;
            float unscaledScreenHeight = screenHeight / currentScale;

            ConfigureColumn(rightColumn, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(nativeWidth, 0));
            rightColumn.sizeDelta = new Vector2(remainingWidthUnscaled, unscaledScreenHeight);

            if (playlistPanel != null)
                SetPanelStretch(playlistPanel);
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
