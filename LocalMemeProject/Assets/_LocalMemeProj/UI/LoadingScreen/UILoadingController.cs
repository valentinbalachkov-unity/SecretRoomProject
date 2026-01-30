using System;
using Dreamers.Core.LinearSwap.Realization;
using Dreamers.UI.UIService.Interfaces;

public class UILoadingController : BaseSwappableWindowController<UILoadingScreen>
{
    public UILoadingController(IUIService uiService) : base(uiService)
    {
    }

    protected override void OnShowEvent(object sender, EventArgs e)
    {
        
    }

    protected override void OnHideEvent(object sender, EventArgs e)
    {
       
    }
}
