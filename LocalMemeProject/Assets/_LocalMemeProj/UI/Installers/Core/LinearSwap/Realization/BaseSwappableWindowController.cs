using System;
using System.Collections.Generic;
using Dreamers.Core.Controllers.Scripts.Realization;
using Dreamers.Core.LinearSwap.Scripts.Interfaces;
using Dreamers.UI.UIService.Interfaces;
using Dreamers.UI.UIService.Realization;

namespace Dreamers.Core.LinearSwap.Realization
{
    public abstract class BaseSwappableWindowController<TOriginView> : BaseWindowController<TOriginView>,
        ISwappableWindowController<TOriginView>
        where TOriginView : UIWindow
    {
        private ISet<Type> _types; 
        protected BaseSwappableWindowController(IUIService uiService) : base(uiService)
        {
            _types = new HashSet<Type>();
        }

        protected void AddTypeToSwap<T>() where T : UIWindow
        {
            _types.Add(typeof(T));
        }
        
        protected virtual void Swap<T>(int index = 0) where T : UIWindow
        {
            _uiService.Hide<TOriginView>();
            _uiService.Show<T>(index);
        }

        public void SwapWindow<T>(int index = 0) where T : UIWindow
        {
            if (_types.Contains(typeof(T)))
            {
                Swap<T>(index);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            _types?.Clear();
            _types = null;
        }
    }

    public abstract class BaseSwappableWindowController<TOriginView, TNextView> : BaseSwappableWindowController<TOriginView>,
        ISwappableWindowController<TOriginView, TNextView>
        where TOriginView : UIWindow
        where TNextView : UIWindow
    {
        protected BaseSwappableWindowController(IUIService uiService) : base(uiService)
        {
            AddTypeToSwap<TNextView>();
        }
    }
    
    public abstract class BaseSwappableWindowController<TOriginView, TNextViewA, TNextViewB> 
        : BaseSwappableWindowController<TOriginView, TNextViewA>, ISwappableWindowController<TOriginView, TNextViewA, TNextViewB>
        where TOriginView : UIWindow
        where TNextViewA : UIWindow
        where TNextViewB : UIWindow
    {
        protected BaseSwappableWindowController(IUIService uiService) : base(uiService)
        {
            AddTypeToSwap<TNextViewB>();
        }
    }
    
    public abstract class BaseSwappableWindowController<TOriginView, TNextViewA, TNextViewB, TNextViewC> 
        : BaseSwappableWindowController<TOriginView, TNextViewA, TNextViewB>, 
            ISwappableWindowController<TOriginView, TNextViewA, TNextViewB, TNextViewC>
        where TOriginView : UIWindow
        where TNextViewA : UIWindow
        where TNextViewB : UIWindow
        where TNextViewC : UIWindow
    {
        protected BaseSwappableWindowController(IUIService uiService) : base(uiService)
        {
            AddTypeToSwap<TNextViewC>();
        }
    }
}