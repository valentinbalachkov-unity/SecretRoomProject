using System;
using Fusion;

namespace _Project.GameSystem.Realisation
{
    public class GameStateMachine : NetworkBehaviour
    {
        [Networked] public GameState CurrentGameState { get; set; }
        [Networked] public int CurrentRoundsCount { get; set; }
        
        public event Action<GameState, int> OnStateChanged;
        
        private ChangeDetector _changes;
        
        public override void Spawned()
        {
            _changes = GetChangeDetector(ChangeDetector.Source.SimulationState);
        }
        
        public override void Render()
        {
            foreach (var change in _changes.DetectChanges(this))
            {
                if (change == nameof(CurrentGameState))
                    OnStateChanged?.Invoke(CurrentGameState, CurrentRoundsCount);
            }
        }
        
       
        
      
    }
}