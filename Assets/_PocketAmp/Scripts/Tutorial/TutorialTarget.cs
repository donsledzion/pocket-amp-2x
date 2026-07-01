using UnityEngine;

namespace SoftAware.PocketAmp.Tutorial
{
    public enum TutorialTargetType
    {
        None,
        OptionsButton,
        SkinsLibraryButton,
        WebToggle,
        SearchField,
        FirstSkinItem,
        DownloadButton,
        CloseButton
    }

    [RequireComponent(typeof(RectTransform))]
    public class TutorialTarget : MonoBehaviour
    {
        [SerializeField] private TutorialTargetType targetType;
        [SerializeField] private RectTransform spawnPoint;
        
        public TutorialTargetType TargetType => targetType;
        public RectTransform RectTransform { get; private set; }
        public RectTransform SpawnPoint => spawnPoint;

        private void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
        }

        private void OnEnable()
        {
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.RegisterTarget(this);
            }
        }

        private void OnDisable()
        {
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.UnregisterTarget(this);
            }
        }
    }
}
