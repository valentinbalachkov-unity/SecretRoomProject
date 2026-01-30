using System;
using Dreamers.Core.LinearSwap.Scripts.Interfaces;
using Dreamers.UI.UIService.Realization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Dreamers.Core.LinearSwap.Realization
{
    public abstract class UIBaseSwappableWindow : UIBaseWindow, ISwappableWindowView
    {
        public event EventHandler NextButtonPressed;
        
        [SerializeField] private Button _nextWindowButton;
        
        public override void Show()
        {
            base.Show();
            if (_nextWindowButton != null)
            {
                _nextWindowButton.onClick.AddListener(OnNextWindowButtonPressed);
            }
        }

        public override void Hide()
        {
            base.Hide();
           
            if (_nextWindowButton != null)
            {
                _nextWindowButton.onClick.RemoveListener(OnNextWindowButtonPressed);
            }
        }

        public void SetActiveNextButton(bool isActive)
        {
            _nextWindowButton.gameObject.SetActive(isActive);
        }
        
        private void OnNextWindowButtonPressed()
        {
            NextButtonPressed?.Invoke(this, EventArgs.Empty);
        }
    }
}