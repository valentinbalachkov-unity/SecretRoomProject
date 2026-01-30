using Fusion;
using System.Linq;
using UnityEngine;
using System.Collections;
using _Project.GameSystem.Realisation;
using _Project.LobbySystem.Realisation;
using Dreamers.UI.UIService.Interfaces;

public class LoadingManager : NetworkBehaviour
{
    // Глобальное состояние загрузки (видно всем)
    [Networked] public NetworkBool IsLoadingComplete { get; private set; }

    private ChangeDetector _changes;
    private IUIService _uiService;
    private FusionLobbySystem _fusionLobbySystem;

    public void Init(IUIService uiService)
    {
        _uiService = uiService;
        ShowLoadingScreen(true);
    }

    public override void Spawned()
    {
        _fusionLobbySystem = FindAnyObjectByType<FusionLobbySystem>();
        
        _changes = GetChangeDetector(ChangeDetector.Source.SimulationState);
        IsLoadingComplete = false;

        // Начинаем проверку готовности (только хост)
        if (Object.HasStateAuthority)
        {
            StartCoroutine(CheckPlayersReadyRoutine());
        }
    }

    public void SetPlayerReady()
    {
        StartCoroutine(ReportReadyDelayed());
    }

    private IEnumerator ReportReadyDelayed()
    {
        yield return new WaitForSeconds(0.5f); // Имитация/задержка инициализации
        
        Debug.Log("AAAAAAAAAAAAAAAAAa");
        
        NetworkingController.Local.playerController.SetPlayerLoaded();
    }

    // Логика Хоста: проверка всех игроков
    private IEnumerator CheckPlayersReadyRoutine()
    {
        Debug.Log("[LoadingManager] Ожидание игроков...");

        bool allReady = false;

        while (!allReady)
        {
            yield return new WaitForSeconds(0.5f); // Проверка раз в полсекунды

            int readyCount = 0;
            int totalPlayers = Runner.ActivePlayers.Count();

            if (_fusionLobbySystem != null)
            {
                foreach (var player in _fusionLobbySystem.spawnedCharacters.Keys)
                {
                    if (_fusionLobbySystem.spawnedCharacters[player].GetComponent<PlayerController>().IsLoaded)
                    {
                        readyCount++;
                    }
                }
            }
            else
            {
                _fusionLobbySystem = FindFirstObjectByType<FusionLobbySystem>();
            }

            // Если все активные игроки готовы
            if (readyCount > 0 && readyCount == totalPlayers)
            {
                allReady = true;
            }

            Debug.Log($"[LoadingManager] Готово игроков: {readyCount}/{totalPlayers}");
        }

        // Все готовы -> Завершаем загрузку
        Debug.Log("[LoadingManager] Все игроки готовы! Запуск игры.");
        IsLoadingComplete = true; // Синхронизируется всем клиентам

        // Запускаем игровой цикл
        var fsm = FindFirstObjectByType<GameStateUIPresenter>();
        if (fsm != null)
        {
            fsm.GameStateMachine.CurrentGameState = GameState.WriteTextState;
        }
    }

    public override void Render()
    {
        // Клиенты следят за изменением IsLoadingComplete
        foreach (var change in _changes.DetectChanges(this))
        {
            if (change == nameof(IsLoadingComplete))
            {
                if (IsLoadingComplete)
                {
                    ShowLoadingScreen(false);
                }
            }
        }
    }

    private void ShowLoadingScreen(bool show)
    {
        if (_uiService == null)
        {
            _uiService = FindFirstObjectByType<GameStateUIPresenter>().UIService;
        }
        
        if (show)
        {
            _uiService.Show<UILoadingScreen>(2);
        }
        else
        {
            _uiService.Hide<UILoadingScreen>();
        }
       
    }
}