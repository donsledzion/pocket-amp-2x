using UnityEngine;

namespace SoftAware
{
    [DisallowMultipleComponent]
    public class Playlist : MonoBehaviour
    {
        [SerializeField] private AudioClip[] clips;
        
        public AudioClip[] Clips => clips;
    }
}
