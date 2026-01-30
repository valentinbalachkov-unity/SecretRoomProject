using UnityEngine;

namespace Dreamers.MainCamera
{
    public class CameraView : MonoBehaviour
    {
        public Camera Camera => camera;
        
        [SerializeField] private Camera camera;
    }
}