using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _LocalMemeProj.UI.UIGameScreen;
using _Project.DeckSystem.Realisation;
using _Project.LobbySystem.Realisation;
using Dreamers.UI.UIService.Realization;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UiResultScreen : UIBaseWindow
{
    public UIGameCard UIGameCard => _uiGameCard;
    public Transform Content => _content;
    public TMP_Text HeaderText => _headerText;
    public Button ContinueButton => _continueButton;
    public Button VoteButton => _voteButton;

    [HideInInspector] public PlayerController currentPlayerForVote;

    [SerializeField] private TMP_Text _timerText;
    [SerializeField] private GameObject _timerPanel;
    
    [SerializeField] private UIGameCard _uiGameCard;
    [SerializeField] private Transform _content;
    [SerializeField] private TMP_Text _headerText;
    [SerializeField] private Button _continueButton;
    [SerializeField] private Button _voteButton;
    
    private CardsConfig _cardsConfig;
    
    private List<UIGameCard> _cardList = new();
    
    public void OnChangeTheme(string theme)
    {
        HeaderText.text = theme;
    }

    public void UpdateUI(GameState gameState)
    {
        if (gameState == GameState.ShowResultState)
        {
            _continueButton.interactable = true;
            _continueButton.gameObject.SetActive(true);
            _voteButton.gameObject.SetActive(false);
        }
        else if (gameState == GameState.VotingState)
        {
            _continueButton.gameObject.SetActive(false);
            _voteButton.interactable = true;
            _voteButton.gameObject.SetActive(true);
            ShowResultsForVoting();
        }
    }

    private IEnumerator ShowResultDelay()
    {
        Clear();
        
        _cardsConfig = Resources.Load<CardsConfig>("CardsConfig");
        
        
        var allControllers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

        yield return new WaitForSeconds(0.5f);
        
        foreach (var player in allControllers)
        {
            var data = _cardsConfig.cardDataList.FirstOrDefault(x => x.uid == player.CurrentCard.ToString());
            Debug.Log($"{player.Id} id, {player.CurrentCard} current card");
        
            if (data != null)
            {
                CreateCardList(data, player.ReceivedText.Value);
            }
            else
            {
                StartCoroutine(ShowResultDelay());
            }
        }
    }
    
    public void ShowResults()
    {
        StartCoroutine(ShowResultDelay());
    }

    private void ShowResultsForVoting()
    {
        Clear();
        currentPlayerForVote = null;
        
        _cardsConfig = Resources.Load<CardsConfig>("CardsConfig");

        var allControllers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        
        foreach (var player in allControllers)
        {
            Debug.Log($"{player.Id} id, {player.CurrentCard} current card");
            
            if (player.HasInputAuthority) continue;
            
            CreateCardList(
                _cardsConfig.cardDataList.FirstOrDefault(x => x.uid == player.CurrentCard.ToString()),
                player.ReceivedText.Value, player);
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
    
    public void CreateCardList(CardData imageData, string description)
    {
        var card = Instantiate(UIGameCard, Content);
        card.SetResult(imageData, description);
        _cardList.Add(card);
    }
    
    public void CreateCardList(CardData imageData, string description, PlayerController playerController)
    {
        var card = Instantiate(UIGameCard, Content);
        card.SetResult(imageData, description, playerController);
        card.OnClick = OnCardVote;
        _cardList.Add(card);
    }

    private void OnCardVote(UIGameCard cardUI)
    {
        foreach (var card in _cardList)
        {
            card.SelectCard(false);
        }
            
        cardUI.SelectCard(true);
        currentPlayerForVote = cardUI.playerController;
        Debug.Log($"{currentPlayerForVote.Runner.LocalPlayer.PlayerId} ID SELECT");
    }

    public void Clear()
    {
        foreach (var c in _cardList)
        {
            Destroy(c.gameObject);
        }
        _cardList.Clear();
    }
}
