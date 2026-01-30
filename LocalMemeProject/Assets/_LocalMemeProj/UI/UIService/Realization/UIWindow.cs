using System;
using UnityEngine;
using Interfaces_IUIWindow = Dreamers.UI.UIService.Interfaces.IUIWindow;

namespace Dreamers.UI.UIService.Realization
{
    public abstract class UIWindow : MonoBehaviour, Interfaces_IUIWindow
    {
        public EventHandler OnShowEvent { get; set; }
        public EventHandler OnHideEvent { get; set; }
        public abstract void Show();
        public abstract void Hide();
        protected virtual void OnShowEnd() { }
        protected virtual void OnHideEnd() { }
    }
}
