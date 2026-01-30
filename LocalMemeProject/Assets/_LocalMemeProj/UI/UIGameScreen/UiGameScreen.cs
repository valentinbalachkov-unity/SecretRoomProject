using System;
using System.Collections.Generic;
using System.Linq;
using _LocalMemeProj.UI.UIGameScreen;
using _Project.DeckSystem.Realisation;
using _Project.GameSystem.Realisation;
using _Project.LobbySystem.Realisation;
using Dreamers.UI.UIService.Realization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class UiGameScreen : UIBaseWindow
{
    public Button submitButton;
    public Button applyPictureButton;

    [HideInInspector] public UIGameCard currentCard;
    
    [SerializeField] private TMP_Text _themeText;
    [SerializeField] private TMP_Text _textForPicture;

    [SerializeField] private TMP_Text _testCountCards;
    [SerializeField] private TMP_Text _testScore;
    
    [SerializeField] private TMP_Text _timerText;
    [SerializeField] private GameObject _timerPanel;
    
    [SerializeField] private Transform _cardContent;
    [SerializeField] private UIGameCard _uiGameCard;

    [SerializeField] private TMP_InputField _tmpInputField;
    
    private TextSubmissionManager _submissionManager;
    private List<UIGameCard> _cardsListInScene = new();
    
    private bool _hasSubmitted = false;
    private GameStateUIPresenter _gameStateUIPresenter;
    private CardsConfig _cardsConfig;

    public void Init(GameStateUIPresenter gameStateUIPresenter)
    {
        _gameStateUIPresenter = gameStateUIPresenter;
        _cardsConfig = Resources.Load<CardsConfig>("CardsConfig");
    }

    public void SetUIState(GameState gameState)
    {
        switch (gameState)
        {
            case GameState.WriteTextState:
                _tmpInputField.gameObject.SetActive(true);
                applyPictureButton.gameObject.SetActive(false);
                submitButton.gameObject.SetActive(true);
                _textForPicture.text = "";
                break;
            case GameState.SelectPictureState:
                applyPictureButton.interactable = true;
                _tmpInputField.gameObject.SetActive(false);
                applyPictureButton.gameObject.SetActive(true);
                submitButton.gameObject.SetActive(false);
                break;
            case GameState.ShowResultState:
                break;
            case GameState.VotingState:
                break;
        }
    }

    private void Update()
    {
        _testCountCards.text = $"Карт на руке: {NetworkingController.Local.playerController.CardsInHand.ToString()}";
        _testScore.text = $"Голосов получено: {NetworkingController.Local.playerController.Score.ToString()}";
    }

    public void UpdateUI(string theme)
    {
        _themeText.text = theme;
    }

    public void UpdateCardsHand(string currentHand)
    {
        var cardInScene = Instantiate(_uiGameCard, _cardContent);
        var cardData = _cardsConfig.cardDataList.FirstOrDefault(x => x.uid == currentHand);
        cardInScene.Init(cardData);
        cardInScene.OnClick = OnCardSelect;
        _cardsListInScene.Add(cardInScene);
    }

    public void RemoveCard(string cardId)
    {
        var card = _cardsListInScene.FirstOrDefault(x => x.CurrentData.uid == cardId);
        if (card != null)
        {
            Destroy(card.gameObject);
        }

        _cardsListInScene.Remove(card);
    }

    private void OnCardSelect(UIGameCard uiCard)
    {
        if (_gameStateUIPresenter.GameStateMachine.CurrentGameState == GameState.SelectPictureState)
        {
            foreach (var card in _cardsListInScene)
            {
                card.SelectCard(false, "");
            }
            
            uiCard.SelectCard(true, NetworkingController.Local.playerController.ReceivedText.Value);
            currentCard = uiCard;
        }
        
       
    }

    public void UpdateTimerDisplay(float remainingSeconds)
    {
        _timerPanel.SetActive(true);
        
        // Форматируем в минуты:секунды
        int minutes = Mathf.FloorToInt(remainingSeconds / 60f);
        int seconds = Mathf.FloorToInt(remainingSeconds % 60f);
        
        _timerText.text = $"{minutes:00}:{seconds:00}";
        
        // Опционально: красный цвет в последние 10 секунд
        if (remainingSeconds < 10f)
        {
            _timerText.color = Color.red;
        }
        else
        {
            _timerText.color = Color.white;
        }
    }

    public void OnTimerFinished()
    {
        _timerText.text = "00:00";
        // Можно добавить звук/анимацию
        Debug.Log("[UI] Время вышло!");
    }

    public void ShowTextForPicture(string text)
    {
        _textForPicture.text = text;
    }
    
    public void OnSubmitButtonClicked(bool isForce = false)
    {
        if (_hasSubmitted) return;

        if (_submissionManager == null)
        {
            _submissionManager = FindFirstObjectByType<TextSubmissionManager>();
        }

        string text = _tmpInputField.text;

        if (string.IsNullOrWhiteSpace(text))
        {
            Debug.LogWarning("Текст пустой!");
            if(!isForce) return;
            text = GameConfig.ExampleText[Random.Range(0, GameConfig.ExampleText.Count)];
        }

        // Отправить текст через менеджер
        NetworkingController.Local.playerController.SendTextToManager(text);
        _hasSubmitted = true;

        // Заблокировать UI
        _tmpInputField.interactable = false;
        submitButton.gameObject.SetActive(false);

        Debug.Log($"[TextInputUI] Отправлен текст: '{text}'");
    }
    
    // Принудительная отправка (вызывается по RPC от хоста)
    public void ForceSubmit()
    {
        if (_hasSubmitted) return;

        Debug.Log("[TextInputUI] Принудительная отправка!");
        OnSubmitButtonClicked(true);
    }

    public void Clear()
    {
        foreach (var card in _cardsListInScene)
        {
            Destroy(card.gameObject);
        }
        
        _cardsListInScene.Clear();
    }
    
    // Сброс UI для новой фазы
    public void ResetForNewRound()
    {
        _tmpInputField.text = "";
        _tmpInputField.interactable = true;
        submitButton.gameObject.SetActive(true);
        _hasSubmitted = false;
    }
}
