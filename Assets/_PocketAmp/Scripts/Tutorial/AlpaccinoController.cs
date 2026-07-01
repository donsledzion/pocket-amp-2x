using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PrimeTween;

namespace SoftAware.PocketAmp.Tutorial
{
    public enum ArrowDirection
    {
        Up,
        Down,
        Left,
        Right,
        None
    }

    public class AlpaccinoController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform rootRect;
        [SerializeField] private Image alpacaImage;
        [SerializeField] private Image arrowImage;
        [SerializeField] private TextMeshProUGUI speechText;
        [SerializeField] private CanvasGroup bubbleCanvasGroup;
        [SerializeField] private Button dismissButton;

        [Header("Sprites")]
        [SerializeField] private Sprite alpacaIdle;
        [SerializeField] private Sprite alpacaPointing;

        private Sequence currentFadeSeq;

        private void Awake()
        {
            if (dismissButton != null)
            {
                dismissButton.onClick.AddListener(OnDismissClicked);
            }
        }

        public void Show(RectTransform target, RectTransform spawnPoint, string text, ArrowDirection arrowDir)
        {
            gameObject.SetActive(true);
            speechText.text = text;

            // Change sprite if needed
            if (alpacaImage != null && alpacaPointing != null)
            {
                alpacaImage.sprite = arrowDir != ArrowDirection.None ? alpacaPointing : alpacaIdle;
            }

            SetupArrow(arrowDir);
            PositionNearTarget(target, spawnPoint, arrowDir);

            // Pop animation
            rootRect.localScale = Vector3.zero;
            Tween.Scale(rootRect, 1f, duration: 0.4f, ease: Ease.OutBack);
        }

        public void PointToTarget(RectTransform target, RectTransform spawnPoint, string text, ArrowDirection arrowDir)
        {
            if (bubbleCanvasGroup != null && gameObject.activeInHierarchy)
            {
                currentFadeSeq.Stop();
                currentFadeSeq = Sequence.Create()
                    .Chain(Tween.Alpha(bubbleCanvasGroup, 0f, 0.15f))
                    .ChainCallback(() => 
                    {
                        speechText.text = text;
                        if (alpacaImage != null && alpacaPointing != null)
                            alpacaImage.sprite = arrowDir != ArrowDirection.None ? alpacaPointing : alpacaIdle;
                        SetupArrow(arrowDir);
                        PositionNearTarget(target, spawnPoint, arrowDir);
                    })
                    .Chain(Tween.Alpha(bubbleCanvasGroup, 1f, 0.2f));
            }
            else
            {
                speechText.text = text;
                if (alpacaImage != null && alpacaPointing != null)
                    alpacaImage.sprite = arrowDir != ArrowDirection.None ? alpacaPointing : alpacaIdle;
                SetupArrow(arrowDir);
                PositionNearTarget(target, spawnPoint, arrowDir);
            }
        }

        private void SetupArrow(ArrowDirection dir)
        {
            if (arrowImage != null)
            {
                arrowImage.gameObject.SetActive(dir != ArrowDirection.None);
            }
        }

        private void PositionNearTarget(RectTransform targetRect, RectTransform spawnPoint, ArrowDirection arrowDir)
        {
            if (targetRect == null && spawnPoint != null)
            {
                rootRect.position = spawnPoint.position;
                if (arrowImage != null) arrowImage.gameObject.SetActive(false);
                return;
            }
            else if (targetRect == null) 
            {
                if (arrowImage != null) arrowImage.gameObject.SetActive(false);
                return; // Default position
            }
            
            Vector3 finalPos = spawnPoint != null ? spawnPoint.position : targetRect.position;

            if ((finalPos - targetRect.position).sqrMagnitude < 0.01f && arrowDir != ArrowDirection.None)
            {
                float offsetX = (targetRect.rect.width * 0.5f + 150f) * targetRect.lossyScale.x;
                float offsetY = (targetRect.rect.height * 0.5f + 150f) * targetRect.lossyScale.y;

                if (arrowDir == ArrowDirection.Left) finalPos += Vector3.right * offsetX;
                else if (arrowDir == ArrowDirection.Right) finalPos += Vector3.left * offsetX;
                else if (arrowDir == ArrowDirection.Up) finalPos += Vector3.down * offsetY;
                else if (arrowDir == ArrowDirection.Down) finalPos += Vector3.up * offsetY;
            }

            if (arrowDir != ArrowDirection.None && arrowImage != null)
            {
                Transform parent = rootRect.parent;
                Vector3 localTarget = parent.InverseTransformPoint(targetRect.position);
                Vector3 localFinal = parent.InverseTransformPoint(finalPos);

                Vector3 direction = localTarget - localFinal;
                Vector2 dir2D = new Vector2(direction.x, direction.y).normalized;

                Vector3 center = alpacaImage != null ? alpacaImage.rectTransform.localPosition : Vector3.zero;
                float radiusX = alpacaImage != null ? alpacaImage.rectTransform.rect.width * 0.5f : 0f;
                float radiusY = alpacaImage != null ? alpacaImage.rectTransform.rect.height * 0.5f : 0f;

                Vector3 edgeOffset = new Vector3(dir2D.x * radiusX, dir2D.y * radiusY, 0);
                arrowImage.rectTransform.localPosition = center + edgeOffset;

                Vector3 edgeLocalPos = localFinal + center + edgeOffset;
                Vector3 trueDirection = localTarget - edgeLocalPos;

                if (trueDirection.sqrMagnitude > 0.001f)
                {
                    float angle = Mathf.Atan2(trueDirection.y, trueDirection.x) * Mathf.Rad2Deg;
                    arrowImage.rectTransform.localRotation = Quaternion.Euler(0, 0, angle);

                    float distance = trueDirection.magnitude;
                    arrowImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, distance);
                }
            }

            Tween.Position(rootRect, finalPos, duration: 0.5f, ease: Ease.OutQuad);
        }

        public void Dismiss(RectTransform startButtonRect)
        {
            Vector3 targetPos = startButtonRect != null ? startButtonRect.position : rootRect.position;

            Sequence.Create()
                .Chain(Tween.Position(rootRect, targetPos, duration: 0.5f, ease: Ease.InBack))
                .Group(Tween.Scale(rootRect, 0f, duration: 0.5f, ease: Ease.InBack))
                .OnComplete(() => gameObject.SetActive(false));
        }

        private void OnDismissClicked()
        {
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.Dismiss();
            }
            else
            {
                Dismiss(null);
            }
        }
    }
}
