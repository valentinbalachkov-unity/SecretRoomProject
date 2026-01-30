using Dreamers.UI.UIService.Interfaces;
using UnityEngine;

namespace Dreamers.UI.UIService.Realization
{
    public class UIRoot : MonoBehaviour, IUIRoot
    {
        public Camera Camera
        {
            get => _camera;
            set
            {
                canvas.worldCamera = value;
                canvas.planeDistance = CanvasPlaneDistance;
                
                _camera = value;
            }
        }

        public LayerContainer Container => container;

        public Transform PoolContainer => poolContainer;
        
        [SerializeField] private Canvas canvas;
        [SerializeField] private LayerContainer container;
        [SerializeField] private Transform poolContainer;
        
        private Camera _camera;
        
        private const float CanvasPlaneDistance = 100f;
    }
}
