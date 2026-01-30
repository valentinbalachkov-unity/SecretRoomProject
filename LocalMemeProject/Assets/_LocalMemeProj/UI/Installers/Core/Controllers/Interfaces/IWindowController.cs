using System;
using Dreamers.UI.UIService.Interfaces;

namespace Dreamers.Core.Controllers.Scripts.Interfaces
{
    public interface IWindowController<TView> : IDisposable 
        where TView : IUIWindow
    {
        
    }
}
