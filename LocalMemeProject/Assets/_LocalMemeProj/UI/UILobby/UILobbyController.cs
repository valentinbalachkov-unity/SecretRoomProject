using System;
using System.Collections.Generic;
using _Project.LobbySystem.Realisation;
using Dreamers.Core.LinearSwap.Realization;
using Dreamers.UI.UIService.Interfaces;
using TMPro;
using UniRx;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _LocalMemeProj.UI.UILobby
{
    public class UILobbyController : BaseSwappableWindowController<UILobby>
    {
        private CompositeDisposable _disposable = new();

        private Dictionary<int, TMP_Text> _playerDict = new();
        
        public UILobbyController(IUIService uiService) : base(uiService)
        {
            _view.StartButton.OnClickAsObservable().Subscribe(_ =>
            {
                if (NetworkingController.Local.Runner.IsServer)
                {
                    NetworkingController.Local.SendGameState(GameMessageType.GameState);
                }
            }).AddTo(_disposable);
        }

        protected override void OnShowEvent(object sender, EventArgs e)
        {
            _view.StartButton.gameObject.SetActive(NetworkingController.Local.Runner.IsServer);
            _view.waitingText.gameObject.SetActive(!NetworkingController.Local.Runner.IsServer);
        }

        protected override void OnHideEvent(object sender, EventArgs e)
        {
            foreach (var item in _playerDict)
            {
                Object.Destroy(item.Value.gameObject);
            }

            _playerDict.Clear();
        }

        public override void Dispose()
        {
            _disposable.Dispose();
            base.Dispose();
        }
        
        
    }
}