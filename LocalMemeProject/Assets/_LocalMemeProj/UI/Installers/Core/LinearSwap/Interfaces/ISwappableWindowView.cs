using System;
using Dreamers.UI.UIService.Interfaces;

namespace Dreamers.Core.LinearSwap.Scripts.Interfaces
{
    public interface ISwappableWindowView : IUIWindow
    {
        public event EventHandler NextButtonPressed;
    }
}