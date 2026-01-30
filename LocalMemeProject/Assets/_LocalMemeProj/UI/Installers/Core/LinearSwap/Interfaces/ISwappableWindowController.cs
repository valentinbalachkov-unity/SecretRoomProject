using Dreamers.Core.Controllers.Scripts.Interfaces;
using Dreamers.UI.UIService.Interfaces;
using Dreamers.UI.UIService.Realization;

namespace Dreamers.Core.LinearSwap.Scripts.Interfaces
{
    public interface ISwappableWindowController<TOriginView> : IWindowController<TOriginView>
        where TOriginView : IUIWindow
    {
        public void SwapWindow<T>(int index = 0) where T : UIWindow;
    }
    
    public interface ISwappableWindowController<TOriginView, TNextView> : IWindowController<TOriginView>
        where TOriginView : IUIWindow
        where TNextView : IUIWindow
    {
        
    }
    
    public interface ISwappableWindowController<TOriginView, TNextViewA, TNextViewB> : ISwappableWindowController<TOriginView, TNextViewA>
        where TOriginView : IUIWindow
        where TNextViewA : IUIWindow
        where TNextViewB : IUIWindow
    {
        
    }
    
    public interface ISwappableWindowController<TOriginView, TNextViewA, TNextViewB, TNextViewC> 
        : ISwappableWindowController<TOriginView, TNextViewA, TNextViewB>
        where TOriginView : IUIWindow
        where TNextViewA : IUIWindow
        where TNextViewB : IUIWindow
        where TNextViewC : IUIWindow
    {
        
    }
}
