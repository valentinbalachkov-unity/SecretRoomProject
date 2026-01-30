using System.Collections.Generic;
using System.Linq;
using _LocalMemeProj.UI.UILobby;
using Dreamers.UI.UIService.Interfaces;
using Fusion;
using UnityEngine;

namespace _Project.LobbySystem.Realisation
{
    public class PlayerListManager : MonoBehaviour
    {
        public Dictionary<PlayerRef, PlayerController> _playerList = new();
        
        public static PlayerListManager Instance;
        private void Awake() => Instance = this;

        private IUIService _uiService;
        public IUIService UIService => _uiService;


        public void Init(IUIService uiService)
        {
            _uiService = uiService;
        }
        
        public void AddPlayer(PlayerRef playerRef, PlayerController player)
        {
            if (_playerList.TryAdd(playerRef, player))
            {
                UpdatePlayerListUI();
            }
        }

        public void RemovePlayer(PlayerRef playerRef)
        {
            if (_playerList.Remove(playerRef))
            {
                UpdatePlayerListUI();
            }
        }

        public void UpdatePlayerListUI()
        {
            _uiService.Get<UILobby>().SpawnPlayers(_playerList.Values.ToList());
        }
    }
}