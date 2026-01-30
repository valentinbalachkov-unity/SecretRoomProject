using System;

namespace Dreamers.UI.UIService.Realization
{
    public abstract class UIBaseWindow : UIWindow
    {
        public override void Show()
        {
            OnShowEvent?.Invoke(this, EventArgs.Empty);
        }

        public override void Hide()
        {
            OnHideEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}