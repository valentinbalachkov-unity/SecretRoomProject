using Fusion;
using System.Collections.Generic;
using System.Linq;
using _Project.GameSystem.Realisation;
using _Project.LobbySystem.Realisation;
using UnityEngine;

namespace _Project.DeckSystem.Realisation
{
    public class TextSubmissionManager : NetworkBehaviour
    {
        // Маппинг: отправитель → получатель
        private Dictionary<PlayerRef, PlayerRef> _textRouting = new();

        // Хранилище отправленных текстов: отправитель → текст
        private Dictionary<PlayerRef, string> _submittedTexts = new();

        // Счётчик игроков, отправивших текст
        [Networked] public int SubmittedCount { get; private set; }

        private int _totalPlayers;
        private UiGameScreen _uiGameScreen;
        private FusionLobbySystem _fusionLobbySystem;

        public void Init(FusionLobbySystem fusionLobbySystem, UiGameScreen uiGameScreen)
        {
            _uiGameScreen = uiGameScreen;
            _fusionLobbySystem = fusionLobbySystem;
        }
       
        /// <summary>
        /// Инициализация: создать случайный маршрут передачи текстов.
        /// Вызывается ТОЛЬКО хостом в начале фазы WriteText.
        /// </summary>
        public void InitializeTextRouting()
        {
            if (!Object.HasStateAuthority) return;
            
            // ... инициализация ...
            SubmittedCount = 0;
            _submittedTexts.Clear();
            _textRouting.Clear();
            // 1. Получаем список игроков
            List<PlayerRef> players = _fusionLobbySystem.spawnedCharacters.Keys.ToList();
            _totalPlayers = players.Count;

            if (_totalPlayers < 2) return;

            // 2. ПЕРЕМЕШИВАЕМ сам список игроков (создаем случайное кольцо)
            // Используем алгоритм Фишера-Йейтса
            for (int i = 0; i < players.Count; i++)
            {
                int randomIndex = Random.Range(i, players.Count);
                (players[i], players[randomIndex]) = (players[randomIndex], players[i]);
            }

            // Теперь players - это наш случайный порядок, например [3, 1, 4, 2]

            // 3. Назначаем маршруты по кольцу
            for (int i = 0; i < players.Count; i++)
            {
                PlayerRef sender = players[i];
        
                // Получатель - следующий игрок в списке (для последнего - первый)
                PlayerRef receiver = players[(i + 1) % players.Count];

                _textRouting[sender] = receiver;
        
                // Отправляем RPC
                RPC_NotifyTextRoute(sender, receiver);
        
                Debug.Log($"[TextSubmission] {sender.PlayerId} → {receiver.PlayerId}");
            }
        }

        /// <summary>
        /// Уведомить игрока о том, кому он должен отправить текст.
        /// </summary>
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_NotifyTextRoute(PlayerRef sender, PlayerRef receiver)
        {
            // Только локальный игрок реагирует
            if (Runner.LocalPlayer != sender) return;

            Debug.Log($"[TextSubmission Client] Я ({sender.PlayerId}) отправляю текст игроку {receiver.PlayerId}");
        }

        /// <summary>
        /// Метод, который вызывает Хост, когда получил текст от игрока через его контроллер
        /// </summary>
        public void OnTextReceivedFromPlayer(PlayerRef sender, string text)
        {
            // Проверяем права (на всякий случай)
            if (!Object.HasStateAuthority) return;

            // Ваша старая логика обработки текста
            if (_submittedTexts.ContainsKey(sender)) return;

            _submittedTexts[sender] = text;
            SubmittedCount++;
            Debug.Log($"[TextSubmission] количество отправивших текст: {SubmittedCount.ToString()}");

            // Отправляем текст получателю
            if (_textRouting.TryGetValue(sender, out PlayerRef receiver))
            {
                if (_fusionLobbySystem.spawnedCharacters.ContainsKey(receiver))
                {
                    var data = _fusionLobbySystem.spawnedCharacters.FirstOrDefault(x => x.Key == receiver).Value;
                    data.GetComponent<PlayerController>().ReceivedText = _submittedTexts[sender];
                }
                
                
                RPC_DeliverTextToReceiver(receiver, text, sender);
            }

            // Проверяем, все ли отправили
            CheckAllSubmitted();
           
            // ... роутинг, проверка завершения и т.д. ...
        }
        
        /// <summary>
        /// Доставить текст конкретному игроку.
        /// </summary>
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_DeliverTextToReceiver(PlayerRef receiver, string text, PlayerRef sender)
        {
            if (Runner.LocalPlayer != receiver) return;
            
            Debug.Log($"[TextSubmission Client] Получен текст от {sender.PlayerId}: '{NetworkingController.Local.playerController.ReceivedText.Value}'");
        }

        /// <summary>
        /// Проверка готовности всех игроков.
        /// </summary>
        private void CheckAllSubmitted()
        {
            if (!Object.HasStateAuthority) return;

            if (SubmittedCount >= _totalPlayers)
            {
                Debug.Log("[TextSubmission] Все игроки отправили тексты!");

                // Переход к следующей фазе
                var stateMachine = FindFirstObjectByType<GameStateUIPresenter>();
                stateMachine?.HostAdvance();
            }
            else
            {
                Debug.Log("[TextSubmission] НЕ Все игроки отправили тексты!");
            }
        }

        /// <summary>
        /// Принудительно отправить тексты всех игроков (вызывается по таймеру).
        /// </summary>
        public void ForceSubmitAll()
        {
            if (!Object.HasStateAuthority) return;

            Debug.Log("[TextSubmission] Принудительная отправка текстов!");

            // Уведомить всех клиентов, что нужно отправить текст
            RPC_ForceSubmitClient();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_ForceSubmitClient()
        {
            // Каждый клиент отправляет свой текст
            _uiGameScreen.ForceSubmit();
        }
    }
}