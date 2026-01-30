using System;
using _Project.GameSystem.Realisation;
using _Project.LobbySystem.Realisation;
using Dreamers.Core.LinearSwap.Realization;
using Dreamers.UI.UIService.Interfaces;
using UniRx;
using UnityEngine;

public class UIResultController : BaseSwappableWindowController<UiResultScreen>
{
    private CompositeDisposable _disposable = new();
    private FusionLobbySystem _fusionLobbySystem;
  


    public UIResultController(IUIService uiService, GameStateUIPresenter gameStateUIPresenter,
        FusionLobbySystem fusionLobbySystem) : base(uiService)
    {
        _fusionLobbySystem = fusionLobbySystem;

        _view.ContinueButton.OnClickAsObservable().Subscribe(_ =>
        {
            NetworkingController.Local.playerController.SetReady();
            _view.ContinueButton.interactable = false;

        }).AddTo(_disposable);
        _view.VoteButton.OnClickAsObservable().Subscribe(_ =>
        {
            if(_view.currentPlayerForVote == null) return;
            
            NetworkingController.Local.playerController.SendVote(_view.currentPlayerForVote.Object.InputAuthority);
            _view.VoteButton.interactable = false;
        }).AddTo(_disposable);
    }

    protected override void OnShowEvent(object sender, EventArgs e)
    {
        GameTimer.OnTimerUpdated += _view.UpdateTimerDisplay;
        GameTimer.OnTimerExpired += _view.OnTimerFinished;

        _view.Clear();
        _view.ShowResults();
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