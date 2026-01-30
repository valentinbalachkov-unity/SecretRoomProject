using UnityEngine;
using LayerContainer = Dreamers.UI.UIService.Realization.LayerContainer;

namespace Dreamers.UI.UIService.Interfaces
{
    public interface IUIRoot
    {
        Camera Camera { set; }
        LayerContainer Container { get; }
        Transform PoolContainer { get; }
    }
}
