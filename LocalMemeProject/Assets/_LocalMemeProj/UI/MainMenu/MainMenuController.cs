using System;
using _LocalMemeProj.Network;
using _Project.LobbySystem.Realisation;
using Dreamers.Core.LinearSwap.Realization;
using Dreamers.UI.UIService.Interfaces;
using UniRx;

public class MainMenuController : BaseSwappableWindowController<UIMainMenu>
{
    private CompositeDisposable _disposable = new();

    private FusionLobbySystem _fusionLobbySystem;
    
    public MainMenuController(IUIService uiService, FusionLobbySystem fusionLobbySystem) : base(uiService)
    {
        _fusionLobbySystem = fusionLobbySystem;
        
        _view.CreateLobbyButton.OnClickAsObservable().Subscribe(_ =>
        {
            fusionLobbySystem.CreateRoom();
        }).AddTo(_disposable);
            
        _view.JoinLobbyButton.OnClickAsObservable().Subscribe(_ =>
        {
            fusionLobbySystem.JoinRoom();
        }).AddTo(_disposable);
        
         _view.NicknameInputField.onValueChanged.AddListener(fusionLobbySystem.SetNickname);
        // _view.LobbyCodeInputField.onValueChanged.AddListener(networkController.SetRoomCode);
    }
    
    

    protected override void OnShowEvent(object sender, EventArgs e)
    {
        _view.NicknameInputField.text = _fusionLobbySystem.NickName.Value;
        // _view.LobbyCodeInputField.text = _networkController.RoomName;
    }

    protected override void OnHideEvent(object sender, EventArgs e)
    {
        
    }

    public override void Dispose()
    {
        _view.LobbyCodeInputField.onValueChanged.RemoveAllListeners();
        _view.NicknameInputField.onValueChanged.RemoveAllListeners();
        _disposable.Dispose();
        base.Dispose();
    }
}
