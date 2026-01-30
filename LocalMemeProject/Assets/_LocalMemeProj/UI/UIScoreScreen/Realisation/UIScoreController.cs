using System;
using _Project.LobbySystem.Realisation;
using Dreamers.Core.LinearSwap.Realization;
using Dreamers.UI.UIService.Interfaces;
using UniRx;

namespace _LocalMemeProj.UI.UIScoreScreen.Realisation
{
    public class UIScoreController : BaseSwappableWindowController<UIScoreView>
    {
        private CompositeDisposable _disposable = new();
        
        public UIScoreController(IUIService uiService) : base(uiService)
        {
            _view.continueButton.OnClickAsObservable().Subscribe(_ =>
            {
                NetworkingController.Local.SendGameState(GameMessageType.LobbyState);
            }).AddTo(_disposable);
        }

        protected override void OnShowEvent(object sender, EventArgs e)
        {
            _view.continueButton.gameObject.SetActive(NetworkingController.Local.Runner.IsServer);
           _view.UpdateList();
        }

        protected override void OnHideEvent(object sender, EventArgs e)
        {
            
        }

        public override void Dispose()
        {
            _disposable.Dispose();
            base.Dispose();
        }
    }
}