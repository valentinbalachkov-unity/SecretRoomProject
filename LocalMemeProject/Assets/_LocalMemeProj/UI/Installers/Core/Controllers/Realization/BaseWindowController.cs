using System;
using Dreamers.Core.Controllers.Scripts.Interfaces;
using Dreamers.UI.UIService.Interfaces;
using Dreamers.UI.UIService.Realization;

namespace Dreamers.Core.Controllers.Scripts.Realization
{
    public abstract class BaseWindowController<TView> : IWindowController<TView> where TView : UIWindow
    {
        protected TView _view;
        protected IUIService _uiService;

        protected BaseWindowController(IUIService uiService)
        {
            _uiService = uiService;
            _view = _uiService.Get<TView>();
            
            _view.OnShowEvent += OnShowEvent;
            _view.OnHideEvent += OnHideEvent;
        }
        
        public virtual void Dispose()
        {
            _view.OnShowEvent -= OnShowEvent;
            _view.OnHideEvent -= OnHideEvent;

            _view = null;
            _uiService = null;
        }

        protected abstract void OnHideEvent(object sender, EventArgs e);

        protected abstract void OnShowEvent(object sender, EventArgs e);
    }
}