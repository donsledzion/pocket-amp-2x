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
        
        public TutorialTargetType TargetType => targetType;
        public RectTransform RectTransform { get; private set; }

        private void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
        }
    }
}
