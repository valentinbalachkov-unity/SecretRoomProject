using System.Collections.Generic;
using System.Linq;
using _Project.LobbySystem.Realisation;
using Dreamers.UI.UIService.Realization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _LocalMemeProj.UI.UILobby
{
    public class UILobby : UIBaseWindow
    {
        [SerializeField] private Button startButton;
        public Button StartButton => startButton;

        public TMP_Text waitingText;
        
        [SerializeField] private TMP_Text playersCountText;
        [SerializeField] private TMP_Text playersNicknamesText;
        [SerializeField] private Transform nickNameContent;

        private List<GameObject> _players = new();

        public void SpawnPlayers(List<PlayerController> playerControllers)
        {
            foreach (var player in _players)
            {
                Destroy(player);
            }
            _players.Clear();

            var tList = playerControllers.OrderBy(x => x.Object.InputAuthority.PlayerId).ToList();

            foreach (var player in tList)
            {
                var textUser = Instantiate(playersNicknamesText, nickNameContent);
                
                string pName = player.PlayerName.Value == "" ? "Default name" : player.PlayerName.Value;
                
                textUser.text = $"{pName}";
               
                _players.Add(textUser.gameObject);
            }
            
            playersCountText.text = $"{playerControllers.Count}/{GameConfig.MAX_PLAYERS_COUNT}";
            startButton.interactable = playerControllers.Count >= 2 && playerControllers.Count <= GameConfig.MAX_PLAYERS_COUNT;
        }

    }
}
