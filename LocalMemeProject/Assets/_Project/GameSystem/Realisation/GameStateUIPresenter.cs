using System.Collections;
using _LocalMemeProj.UI.UILobby;
using _Project.Audio.Realisation;
using _Project.Config.Realisation;
using _Project.DeckSystem.Realisation;
using _Project.LobbySystem.Realisation;
using _Project.ThemeSystem.Realisation;
using Dreamers.UI.UIService.Interfaces;
using UnityEngine;
using Zenject;

namespace _Project.GameSystem.Realisation
{
    public class GameStateUIPresenter : MonoBehaviour
    {
        [Inject] private IUIService _uiService;
        public IUIService UIService => _uiService;

        [Inject] private FusionLobbySystem _fusionLobbySystem;

        private GameStateMachine _gameStateMachine;
        public GameStateMachine GameStateMachine => _gameStateMachine;


        private DeckController _deckController;
        private ThemeManager _themeManager;
        private GameTimer _gameTimer;
        private TextSubmissionManager _textSubmissionManager;
        private ResultPhaseManager _resultPhaseManager;
        private VotingManager _votingManager;

        private RoundThemesConfig _roundThemesConfig;
        private CardsConfig _cardsConfig;

        private UiGameScreen _uiGameScreen;
        private UiResultScreen _uiResultScreen;

        private LoadingManager _loadingManager;

        private int _maxRounds;
        
        public IEnumerator InitGame()
        {
            _roundThemesConfig = Resources.Load<RoundThemesConfig>("RoundThemesConfig");
            _cardsConfig = Resources.Load<CardsConfig>("CardsConfig");
            _cardsConfig.Init();
            _uiGameScreen = _uiService.Get<UiGameScreen>();
            _uiResultScreen = _uiService.Get<UiResultScreen>();
            
            _uiGameScreen.Clear();

            // Найдите FSM любым удобным способом (например, она одна на сцене/в сети)

            _gameStateMachine = FindFirstObjectByType<GameStateMachine>();
            _deckController = FindFirstObjectByType<DeckController>();
            _themeManager = FindFirstObjectByType<ThemeManager>();
            _gameTimer = FindFirstObjectByType<GameTimer>();
            _textSubmissionManager = FindFirstObjectByType<TextSubmissionManager>();
            _loadingManager = FindFirstObjectByType<LoadingManager>();
            _resultPhaseManager = FindFirstObjectByType<ResultPhaseManager>();
            _votingManager = FindFirstObjectByType<VotingManager>();

            while (_gameStateMachine == null || _deckController == null || _themeManager == null ||
                   _gameTimer == null || _textSubmissionManager == null || _loadingManager == null || _resultPhaseManager == null
                   || _votingManager == null)
            {
                yield return new WaitForSeconds(0.5f);
                _gameStateMachine = FindFirstObjectByType<GameStateMachine>();
                _deckController = FindFirstObjectByType<DeckController>();
                _themeManager = FindFirstObjectByType<ThemeManager>();
                _gameTimer = FindFirstObjectByType<GameTimer>();
                _textSubmissionManager = FindFirstObjectByType<TextSubmissionManager>();
                _loadingManager = FindFirstObjectByType<LoadingManager>();
                _resultPhaseManager = FindFirstObjectByType<ResultPhaseManager>();
                _votingManager = FindFirstObjectByType<VotingManager>();
            }

            _themeManager.Init(_roundThemesConfig, _uiService);
            _resultPhaseManager.Init(_fusionLobbySystem);
            _gameStateMachine.OnStateChanged += HandleStateChanged;
            _deckController.InitializeDeck(_cardsConfig.cardDataList, _fusionLobbySystem);
            _textSubmissionManager.Init(_fusionLobbySystem, _uiGameScreen);
            _votingManager.Init();

            if (_gameStateMachine.HasStateAuthority)
            {
                SetRoundsCount(_roundThemesConfig.roundsThemeList.Count);
            }
            
            _loadingManager.SetPlayerReady();
        }

        public void ForceSubmitAll()
        {
            _textSubmissionManager.ForceSubmitAll();
        }

        public void ForceFinishSelection()
        {
            _deckController.ForceFinishSelection();
        }

        public void OnPackageReceived(LobbyPacket lobbyPacket)
        {
            switch (lobbyPacket.type)
            {
                case GameMessageType.LobbyState:
                    _uiService.Hide<UiGameScreen>();
                    _uiService.Hide<UIScoreView>();
                    _uiService.Show<UILobby>();
                    AudioManager.Instance.PlayMusic(MusicState.Lobby);
                    break;
                case GameMessageType.GameState:
                    _uiService.Hide<UILobby>();
                    _uiService.Show<UILoadingScreen>(2);
                    _uiService.Show<UiGameScreen>();
                    AudioManager.Instance.PlayMusic(MusicState.GameRound);
                    StartCoroutine(InitGame());
                    break;
            }
        }

        private void HandleStateChanged(GameState state, int roundsLeft)
        {
            switch (state)
            {
                case GameState.WriteTextState:
                    _uiService.Hide<UiResultScreen>();
                    _uiGameScreen.ResetForNewRound();
                    _textSubmissionManager.InitializeTextRouting();
                    _deckController.GiveCardsToPlayers();
                    _themeManager.DrawTheme();
                    _resultPhaseManager.Reset();
                    _gameTimer.StartTimer(GameConfig.TIMER_PHASE_1);
                    break;
                case GameState.SelectPictureState:
                    _deckController.ResetSelection();
                    _uiGameScreen.ShowTextForPicture(NetworkingController.Local.playerController.ReceivedText.Value);
                    _gameTimer.StartTimer(GameConfig.TIMER_PHASE_2);
                    break;
                case GameState.ShowResultState:
                    _uiService.Show<UiResultScreen>(1);
                    _gameTimer.StartTimer(GameConfig.TIMER_PHASE_3);
                    break;
                case GameState.VotingState:
                    _gameTimer.StartTimer(GameConfig.TIMER_PHASE_4);
                    break;
                case GameState.EndGameState:
                    _gameStateMachine.OnStateChanged -= HandleStateChanged;
                    _uiService.HideAll();
                    AudioManager.Instance.PlayMusic(MusicState.Results);
                    _uiService.Show<UIScoreView>();
                    break;
            }
            
            _uiGameScreen.SetUIState(state);
            _uiResultScreen.UpdateUI(state);
        }
        

        public void HostAdvance()
        {
            if (!_gameStateMachine.HasStateAuthority) return;

            switch (_gameStateMachine.CurrentGameState)
            {
                case GameState.WriteTextState:
                    _gameStateMachine.CurrentGameState = GameState.SelectPictureState;
                    break;

                case GameState.SelectPictureState:
                    _gameStateMachine.CurrentGameState = GameState.ShowResultState;
                    break;

                case GameState.ShowResultState:
                    _gameStateMachine.CurrentGameState = GameState.VotingState;
                    break;

                case GameState.VotingState:
                    
                    // Ваше правило: если прошли все раунды -> Voting, иначе -1 и повтор цикла
                    if (_gameStateMachine.CurrentRoundsCount <= 1)
                    {
                        _gameStateMachine.CurrentGameState = GameState.EndGameState;
                    }
                    else
                    {
                        _gameStateMachine.CurrentRoundsCount -= 1;
                        _gameStateMachine.CurrentGameState = GameState.WriteTextState;
                    }
                    
                    break;
                case GameState.EndGameState:
                  
                    break;
            }
        }


        private void SetRoundsCount(int currentPackRoundsCount)
        {
            _maxRounds = currentPackRoundsCount;

            if (_maxRounds > GameConfig.MAX_ROUND)
            {
                _maxRounds = GameConfig.MAX_ROUND;
            }

            _gameStateMachine.CurrentRoundsCount = _maxRounds;
        }
    }
}