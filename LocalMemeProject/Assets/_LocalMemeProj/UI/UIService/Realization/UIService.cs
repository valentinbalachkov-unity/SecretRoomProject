using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Interfaces_IUIService = Dreamers.UI.UIService.Interfaces.IUIService;
using IUIRoot = Dreamers.UI.UIService.Interfaces.IUIRoot;
using Object = UnityEngine.Object;

namespace Dreamers.UI.UIService.Realization 
{
    public class UIService : Interfaces_IUIService
    {
        private readonly IUIRoot _uIRoot;
        private readonly IInstantiator _instantiator;
        
        private readonly Dictionary<Type,UIWindow> _viewStorage = new Dictionary<Type,UIWindow>();
        private readonly Dictionary<Type, GameObject> _initWindows= new Dictionary<Type, GameObject>();

        private const string WindowsSource = "UIWindows";
        
        public UIService(
            IInstantiator instantiator,
            IUIRoot uIRoot)
        {
            _instantiator = instantiator;
            _uIRoot = uIRoot;
            
            LoadWindows(WindowsSource);
        }

        public T Show<T>(int layer = 0) where T : UIWindow
        {
            var window = Get<T>();
            if(window != null)
            {
                window.transform.SetParent(_uIRoot.Container.Layers[layer], false);
                window.transform.localScale = Vector3.one;
                window.transform.localRotation = Quaternion.identity;
                window.transform.localPosition = Vector3.zero;

                var component = window.GetComponent<T>();
                
                //always resize to screen size
                var rect = component.transform as RectTransform;
                if (rect != null)
                {
                    rect.offsetMax = Vector2.zero;
                    rect.offsetMin = Vector2.zero;
                }
                
                component.Show();
                return component;
            }
            return null;
        }

        public T Get<T>() where T : UIWindow
        {
            var type = typeof(T);
            if (_initWindows.ContainsKey(type))
            {
                var view = _initWindows[type];            
                return view.GetComponent<T>();
            }
            return null;
        }

        public void LoadWindows(string root)
        {
            _viewStorage.Clear();
            
            var windows = Resources.LoadAll(root, typeof(UIWindow));

            foreach (var window in windows)
            {
                var windowType = window.GetType();
                _viewStorage.Add(windowType, (UIWindow) window);
            }
            
            InitWindows();
        }

        public void Hide<T>(Action onEnd = null) where T : UIWindow
        {
            var window = Get<T>();
            if(window!=null)
            {
                window.transform.SetParent(_uIRoot.PoolContainer);
                window.Hide();
                onEnd?.Invoke();
            }
        }

        public void HideAll(Action onEnd = null)
        {
            foreach (var viewsKVP in _initWindows)
            {
                viewsKVP.Value.transform.SetParent(_uIRoot.PoolContainer);
                onEnd?.Invoke();
            }
        }

        public void DeleteWindows()
        {
            foreach (var viewKVP in _viewStorage)
            {
                Object.Destroy(_initWindows[viewKVP.Key].gameObject);
                _initWindows.Remove(viewKVP.Key);
            }
        }

        private void InitWindows()
        {
            foreach (var windowKVP in _viewStorage)
            {
                Init(windowKVP.Key, _uIRoot.PoolContainer);
            }
        }
    
        private void Init(Type t, Transform parent = null)
        {
            if(_viewStorage.ContainsKey(t))
            {
                GameObject view = null;
                if(parent!=null)
                {
                    view = _instantiator.InstantiatePrefab(_viewStorage[t], parent);
                }
                else
                {
                    view = _instantiator.InstantiatePrefab(_viewStorage[t]);
                }
                _initWindows.Add(t, view);
            }
        }
    }
}
