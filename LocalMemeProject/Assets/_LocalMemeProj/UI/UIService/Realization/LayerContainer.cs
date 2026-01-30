using UnityEngine;

namespace Dreamers.UI.UIService.Realization
{
    public class LayerContainer : MonoBehaviour
    {
        public Transform[] Layers => layers;
        
        [SerializeField] private Transform[] layers;
    }
}