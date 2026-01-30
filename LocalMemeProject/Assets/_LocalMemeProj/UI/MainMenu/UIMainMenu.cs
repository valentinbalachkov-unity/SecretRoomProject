using Dreamers.UI.UIService.Realization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMainMenu : UIBaseWindow
{
    public Button CreateLobbyButton => _createLobbyButton;
    public Button JoinLobbyButton => _joinLobbyButton;
    public TMP_InputField NicknameInputField => _nicknameInputField;
    public TMP_InputField LobbyCodeInputField => _lobbyCodeInputField;
    
    [SerializeField] private Button _createLobbyButton;
    [SerializeField] private Button _joinLobbyButton;

    [SerializeField] private TMP_InputField _nicknameInputField;
    [SerializeField] private TMP_InputField _lobbyCodeInputField;
    
}
