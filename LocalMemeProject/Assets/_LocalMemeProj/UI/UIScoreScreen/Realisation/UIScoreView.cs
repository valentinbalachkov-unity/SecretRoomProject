using System.Collections.Generic;
using _LocalMemeProj.UI.UIScoreScreen.Realisation;
using _Project.LobbySystem.Realisation;
using Dreamers.UI.UIService.Realization;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class UIScoreView : UIBaseWindow
{
    public Button continueButton;
    [SerializeField] private Transform _content;
    [SerializeField] private ScoreElementView _scoreElementView;

    private List<ScoreElementView> _elements = new();

    public void UpdateList()
    {
        foreach (var element in _elements)
        {
            Destroy(element.gameObject);
        }
        
        _elements.Clear();
        
        foreach (var key in PlayerListManager.Instance._playerList.Keys)
        {
            var controller = PlayerListManager.Instance._playerList[key];
            if (controller != null)
            {
                var element = Instantiate(_scoreElementView, _content);
                element.UpdateElement(controller.PlayerName.ToString(), controller.Score);
                _elements.Add(element);
            }
        }
    }
    
}
