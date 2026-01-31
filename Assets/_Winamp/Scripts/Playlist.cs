using UnityEngine;

namespace SoftAware
{
    [DisallowMultipleComponent]
    public class Playlist : MonoBehaviour
    {
        [SerializeField] private AudioClip[] clips;
        
        private int currentIndex;
        internal AudioClip CurrentClip => clips[currentIndex];

        private void Start()
        {
            //Debug and testing only
            if (clips.Length > 0)
                SetCurrentClip(0);
        }

        private void SetCurrentClip(int index)
        {
            if (index < 0 || index >= clips.Length) return;
            currentIndex = index;
        }

        internal AudioClip GetNextClip()
        {
            SetCurrentClip(currentIndex == clips.Length - 1 ? 0 : ++currentIndex);
            return CurrentClip;
        }

        internal AudioClip GetPreviousClip()
        {
            SetCurrentClip(currentIndex == 0 ? clips.Length - 1 : --currentIndex);
            return CurrentClip;
        }
        
    }
}
