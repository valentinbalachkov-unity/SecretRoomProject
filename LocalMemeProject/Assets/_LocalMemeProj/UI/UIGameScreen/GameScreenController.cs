using System;
using _Project.GameSystem.Realisation;
using _Project.LobbySystem.Realisation;
using Dreamers.Core.LinearSwap.Realization;
using Dreamers.UI.UIService.Interfaces;
using UniRx;

namespace _LocalMemeProj.UI.UIGameScreen
{
    public class GameScreenController : BaseSwappableWindowController<UiGameScreen>
    {
       
        private CompositeDisposable _disposable = new();
        
        public GameScreenController(IUIService uiService, GameStateUIPresenter gameStateUIPresenter) : base(uiService)
        {
            _view.Init(gameStateUIPresenter);
            
            _view.submitButton.OnClickAsObservable().Subscribe(_ =>
            {
                _view.OnSubmitButtonClicked();
            }).AddTo(_disposable);
            
            _view.applyPictureButton.OnClickAsObservable().Subscribe(_ =>
            {
                if (_view.currentCard != null)
                {
                   _view.RemoveCard(_view.currentCard.CurrentData.uid);
                    NetworkingController.Local.playerController.SelectCard(_view.currentCard.CurrentData.uid);
                    _view.applyPictureButton.interactable = false;
                }
               
            }).AddTo(_disposable);
        }

        protected override void OnShowEvent(object sender, EventArgs e)
        {
            GameTimer.OnTimerUpdated += _view.UpdateTimerDisplay;
            GameTimer.OnTimerExpired += _view.OnTimerFinished;
        }

        protected override void OnHideEvent(object sender, EventArgs e)
        {
            GameTimer.OnTimerUpdated -= _view.UpdateTimerDisplay;
            GameTimer.OnTimerExpired -= _view.OnTimerFinished;
        }

        public override void Dispose()
        {
            _disposable.Dispose();
            base.Dispose();
        }

    
    }
}