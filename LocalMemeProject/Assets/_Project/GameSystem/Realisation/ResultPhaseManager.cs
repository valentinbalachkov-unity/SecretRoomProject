using System.Linq;
using _Project.LobbySystem.Realisation;
using Fusion;
using UnityEngine;

namespace _Project.GameSystem.Realisation
{
    public class ResultPhaseManager : NetworkBehaviour
    {
        [Networked] public int PlayersReadyCount { get; private set; }

        private FusionLobbySystem _fusionLobbySystem;

        public override void Spawned()
        {
            if (Object.HasStateAuthority) PlayersReadyCount = 0;
        }

        public void Init(FusionLobbySystem fusionLobbySystem)
        {
            _fusionLobbySystem = fusionLobbySystem;
            
            if (Object.HasStateAuthority) PlayersReadyCount = 0;
        }

        public void Reset()
        {
            if (Object.HasStateAuthority) PlayersReadyCount = 0;
        }

        // Вызывается из UI кнопкой "Далее" (через PlayerController)
        public void SetPlayerReady()
        {
            Debug.Log("Show result 2");
            
            if (!Object.HasStateAuthority) return;
            
            PlayersReadyCount++;
        
            Debug.Log("Show result 3");
            
            // Если все готовы
            if (PlayersReadyCount >= _fusionLobbySystem.spawnedCharacters.Count())
            {
                
                Debug.Log("Show result 4");
                FindFirstObjectByType<GameStateUIPresenter>().HostAdvance();
            }
        }
    }
}