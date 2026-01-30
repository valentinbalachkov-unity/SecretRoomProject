using System;
using UIWindow = Dreamers.UI.UIService.Realization.UIWindow;

namespace Dreamers.UI.UIService.Interfaces 
{
    public interface IUIService
    {
        T Show<T>(int layer = 0) where T : UIWindow;
        void Hide<T>(Action onEnd = null) where T : UIWindow;
        void HideAll(Action onEnd = null);
        T Get<T>() where T : UIWindow;
        void LoadWindows(string root);
        void DeleteWindows();
    }
}
