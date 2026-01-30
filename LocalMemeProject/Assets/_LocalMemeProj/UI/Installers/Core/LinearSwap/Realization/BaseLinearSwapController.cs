using System;
using Dreamers.UI.UIService.Interfaces;
using Dreamers.UI.UIService.Realization;

namespace Dreamers.Core.LinearSwap.Realization
{
    public abstract class BaseLinearSwapController<TOriginView, TNextView> : BaseSwappableWindowController<TOriginView, TNextView>
        where TOriginView : UIBaseSwappableWindow
        where TNextView : UIWindow
    {
        protected BaseLinearSwapController(IUIService uiService) : base(uiService)
        {
        }

        protected override void OnShowEvent(object sender, EventArgs e)
        {
            _view.NextButtonPressed += OnNextButtonPressed<TNextView>;
        }

        protected override void OnHideEvent(object sender, EventArgs e)
        {
            _view.NextButtonPressed -= OnNextButtonPressed<TNextView>;
        }

        protected virtual void OnNextButtonPressed<T>(object sender, EventArgs e) where T : UIWindow
        {
            SwapWindow<T>();
        }
    }
}