using Fusion;
using System.Collections.Generic;
using System.Linq;
using _Project.LobbySystem.Realisation;
using UnityEngine;

namespace _Project.GameSystem.Realisation
{
    public class VotingManager : NetworkBehaviour
    {
        // Сколько игроков проголосовало
        [Networked] public int VotesCastCount { get; private set; }
    
        // Список тех, кто уже проголосовал (чтобы не голосовали дважды)
        private HashSet<PlayerRef> _voters = new HashSet<PlayerRef>();

        public override void Spawned()
        {
            
        }

        public void Init()
        {
            if (Object.HasStateAuthority)
            {
                VotesCastCount = 0;
                _voters.Clear();
            }
        }

        /// <summary>
        /// Игрок голосует за другого игрока (targetPlayer).
        /// </summary>
        public void CastVote(PlayerRef voter, PlayerRef targetPlayer)
        {
            if (!Object.HasStateAuthority) return;
            // Проверки
            if (_voters.Contains(voter)) return; // Уже голосовал
         
            if (voter == targetPlayer) return;   // Нельзя за себя
        
            PlayerListManager.Instance._playerList.TryGetValue(targetPlayer, out var playerController);

            if (playerController != null)
            {
              
                playerController.AddScore(1);
                // Фиксируем голос
                _voters.Add(voter);
                VotesCastCount++;
                Debug.Log($"[Voting] {voter.PlayerId} проголосовал за {targetPlayer.PlayerId}");
            }
            
            // Если все проголосовали - конец фазы
            if (VotesCastCount >= Runner.ActivePlayers.Count())
            {
                FinishVoting();
            }
        }

        public void FinishVoting()
        {
            if (!Object.HasStateAuthority) return;
        
            // Сброс данных перед следующим раундом (если нужно)
            _voters.Clear();
            VotesCastCount = 0;

            FindFirstObjectByType<GameStateUIPresenter>().HostAdvance();
        }
    }
}