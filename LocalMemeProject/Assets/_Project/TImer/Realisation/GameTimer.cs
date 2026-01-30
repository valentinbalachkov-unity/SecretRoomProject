using Fusion;
using UnityEngine;
using System;
using _Project.GameSystem.Realisation;

public class GameTimer : NetworkBehaviour
{
    // Сетевой таймер (автоматически синхронизируется)
    [Networked] private TickTimer Timer { get; set; }
    
    // Событие окончания таймера (локальное, для UI)
    public static event Action<float> OnTimerUpdated;
    public static event Action OnTimerExpired;

    private ChangeDetector _changes;
    private float _lastRemainingTime;
    
    public override void Spawned()
    {
        _changes = GetChangeDetector(ChangeDetector.Source.SimulationState);
    }

    /// <summary>
    /// Запустить таймер на N секунд. Вызывает ТОЛЬКО хост.
    /// </summary>
    public void StartTimer(float seconds)
    {
        if (!Object.HasStateAuthority)
        {
            return;
        }

        // TickTimer.CreateFromSeconds создаёт таймер на основе симуляционного времени
        Timer = TickTimer.CreateFromSeconds(Runner, seconds);
        Debug.Log($"[GameTimer] Таймер запущен на {seconds} секунд");
    }

    /// <summary>
    /// Остановить/сбросить таймер.
    /// </summary>
    public void StopTimer()
    {
        if (!Object.HasStateAuthority) return;
        Timer = TickTimer.None;
    }

    /// <summary>
    /// Проверка, истёк ли таймер.
    /// </summary>
    public bool IsExpired => Timer.Expired(Runner);

    /// <summary>
    /// Оставшееся время в секундах.
    /// </summary>
    public float RemainingTime => Timer.RemainingTime(Runner) ?? 0f;

    public override void FixedUpdateNetwork()
    {
        // Проверяем истечение таймера ТОЛЬКО на хосте
        if (Object.HasStateAuthority && Timer.IsRunning && Timer.Expired(Runner))
        {
            Debug.Log("[GameTimer] Таймер истёк!");
            
            // Сбрасываем таймер, чтобы событие не вызывалось повторно
            Timer = TickTimer.None;

            // Вызываем переход к следующему состоянию
            var stateMachine = FindFirstObjectByType<GameStateUIPresenter>();
            if (stateMachine != null)
            {
                if (stateMachine.GameStateMachine.CurrentGameState == GameState.WriteTextState)
                {
                    // Принудительно отправить все тексты
                    stateMachine.ForceSubmitAll();
                
                    // Небольшая задержка перед переходом (чтобы RPC успели обработаться)
                    Runner.StartCoroutine(DelayedAdvance(stateMachine));
                }
                else if (stateMachine.GameStateMachine.CurrentGameState == GameState.SelectPictureState)
                {
                    stateMachine.ForceFinishSelection();
                    Runner.StartCoroutine(DelayedAdvance(stateMachine));
                }
                else if (stateMachine.GameStateMachine.CurrentGameState == GameState.VotingState)
                {
                   FindFirstObjectByType<VotingManager>().FinishVoting();
                }
                else
                {
                    stateMachine.HostAdvance();
                }
            }
        }
    }
    private System.Collections.IEnumerator DelayedAdvance(GameStateUIPresenter sm)
    {
        yield return new WaitForSeconds(0.5f);
        sm.HostAdvance();
    }

    public override void Render()
    {
        // Обновление UI на клиентах (каждый кадр)
        if (Timer.IsRunning)
        {
            float remaining = RemainingTime;

            // Уведомляем UI только если время изменилось (экономим вызовы)
            if (Mathf.Abs(remaining - _lastRemainingTime) > 0.01f)
            {
                OnTimerUpdated?.Invoke(remaining);
                _lastRemainingTime = remaining;
            }
        }
        
        // Отслеживаем момент истечения для клиентов (для визуальных эффектов)
        foreach (var change in _changes.DetectChanges(this))
        {
            if (change == nameof(Timer))
            {
                if (Timer.IsRunning)
                {
                    Debug.Log($"[GameTimer Client] Таймер запущен, осталось {RemainingTime}с");
                }
                else if(Equals(Timer, TickTimer.None))
                {
                    OnTimerExpired?.Invoke();
                }
            }
        }
    }
}
