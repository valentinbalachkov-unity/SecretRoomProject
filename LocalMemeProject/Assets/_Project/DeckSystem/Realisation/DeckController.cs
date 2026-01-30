using System;
using Fusion;
using System.Collections.Generic;
using System.Linq;
using _Project.GameSystem.Realisation;
using _Project.LobbySystem.Realisation;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Project.DeckSystem.Realisation
{
    public class DeckController : NetworkBehaviour
    {
        // Сколько игроков уже выбрали карту
        [Networked] public int CardsSelectedCount { get; private set; }

        // PlayerRef → индекс в массиве игроков, int[] → битовая маска доступности карт
        private Dictionary<PlayerRef, NetworkArray<int>> _playerCardAvailability = new();

        // Локальный кэш для быстрого доступа (не сетевой, пересоздаётся из networked данных)
        private Dictionary<PlayerRef, bool[]> _availabilityCache = new();


        // Флаг, что фаза завершена (чтобы не переключать дважды)
        private bool _phaseFinished = false;
        
        private List<CardData> _allCards = new();
        private FusionLobbySystem _fusionLobbySystem;

        public override void Spawned()
        {
        }
        

        /// <summary>
        /// Вызывается ХОСТОМ (через RPC из PlayerController), когда игрок выбирает карту.
        /// </summary>
        public void OnCardSelectedByPlayer(PlayerRef player, string cardUid)
        {
            if (!Object.HasStateAuthority) return;
            if (_phaseFinished) return;

            // Здесь можно сохранить выбор игрока, если нужно для логики игры
            // Например: _selectedCards[player] = cardUid;

            // Увеличиваем счетчик

            NetworkObject playerNO;
            _fusionLobbySystem.spawnedCharacters.TryGetValue(player, out playerNO);

            if (playerNO != null)
            {
                var controller = playerNO.GetComponent<PlayerController>();
                controller.CurrentCard = cardUid;
                controller.CardsInHand--;
                Debug.Log($"{controller.CurrentCard} and {cardUid}");
            }
            
            
            CardsSelectedCount++;
            Debug.Log(
                $"[DeckManager] Игрок {player.PlayerId} выбрал карту. Всего: {CardsSelectedCount}/{_fusionLobbySystem.spawnedCharacters.Count()}");

            // Проверяем, все ли выбрали
            CheckAllCardsSelected();
        }

        /// <summary>
        /// Проверка: все ли игроки сделали выбор?
        /// </summary>
        private void CheckAllCardsSelected()
        {
            int totalPlayers = _fusionLobbySystem.spawnedCharacters.Count();

            if (CardsSelectedCount >= totalPlayers)
            {
                FinishSelectionPhase();
            }
        }

        /// <summary>
        /// Принудительное завершение фазы (таймер).
        /// </summary>
        public void ForceFinishSelection()
        {
            if (!Object.HasStateAuthority) return;
            if (_phaseFinished) return;

            Debug.Log("[DeckManager] Таймер истёк, принудительный выбор!");

            // Для тех, кто не выбрал, можно выбрать случайную карту

            foreach (var player in _fusionLobbySystem.spawnedCharacters.Keys)
            {
                var controller = _fusionLobbySystem.spawnedCharacters[player].GetComponent<PlayerController>();
                if (string.IsNullOrEmpty(controller.CurrentCard.ToString()))
                {
                    // Выбираем первую попавшуюся из руки (или рандом)
                    // (Предполагаем, что у контроллера есть метод GetRandomCardUidFromHand)
                    string randomCard = controller.GetRandomCardFromHand();
                    controller.SetCardSelection(randomCard); // Установит Networked св-во

                    // Увеличиваем счетчик (хотя фаза все равно завершится)
                    CardsSelectedCount++;
                }
                
            }

            FinishSelectionPhase();
        }

        private void FinishSelectionPhase()
        {
            _phaseFinished = true;
            Debug.Log("[DeckManager] Фаза выбора карт завершена!");

            // Переход к следующей фазе
            var fsm = FindFirstObjectByType<GameStateUIPresenter>();
            if (fsm != null)
            {
                fsm.HostAdvance();
            }
        }

        // Сброс счетчика перед началом новой фазы выбора (вызывать из GameStateMachine)
        public void ResetSelection()
        {
            if (Object.HasStateAuthority)
            {
                CardsSelectedCount = 0;
                _phaseFinished = false;

                var controllers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None).ToList();

                foreach (var controller in controllers)
                {
                    controller.ClearCurrentCard();
                }
            }
        }

        // Инициализация: все карты доступны всем игрокам
        public void InitializeDeck(List<CardData> cardDataList, FusionLobbySystem fusionLobbySystem)
        {
            _fusionLobbySystem = fusionLobbySystem;

            _allCards = new List<CardData>(cardDataList);

            if (!Object.HasStateAuthority) return;

            CardsSelectedCount = 0;

            foreach (var player in _fusionLobbySystem.spawnedCharacters.Keys)
            {
                ResetPlayerDeck(player);
            }
        }

        // Сброс колоды для игрока (все карты снова доступны)
        private void ResetPlayerDeck(PlayerRef player)
        {
            if (!_availabilityCache.ContainsKey(player))
            {
                _availabilityCache[player] = new bool[_allCards.Count];
            }

            for (int i = 0; i < _allCards.Count; i++)
            {
                _availabilityCache[player][i] = true; // все карты участвуют
            }
        }

        public void GiveCardsToPlayers()
        {
            if (!Object.HasStateAuthority) return;

            foreach (var character in _fusionLobbySystem.spawnedCharacters.Keys)
            {
                var controller = _fusionLobbySystem.spawnedCharacters[character].GetComponent<PlayerController>();

                if (controller.CardsInHand < GameConfig.MAX_CARDS_IN_HAND)
                {
                   
                    int count = GameConfig.MAX_CARDS_IN_HAND - controller.CardsInHand;

                    for (int i = 0; i < count; i++)
                    {
                        var card = DrawCard(character);
                        controller.AddCard(card.uid);
                    }
                }
            }
        }

        /// <summary>
        /// Выдать случайную доступную карту игроку.
        /// Вызывает ТОЛЬКО хост.
        /// </summary>
        public CardData DrawCard(PlayerRef player)
        {
            if (!Object.HasStateAuthority)
            {
                return null;
            }

            if (!_availabilityCache.ContainsKey(player))
            {
                ResetPlayerDeck(player);
            }

            bool[] availability = _availabilityCache[player];

            // Найти индексы всех доступных карт
            List<int> availableIndices = new List<int>();
            for (int i = 0; i < availability.Length; i++)
            {
                if (availability[i]) availableIndices.Add(i);
            }

            // Если колода исчерпана — сбросить
            if (availableIndices.Count == 0)
            {
                Debug.Log($"Колода исчерпана для игрока {player.PlayerId}, сброс...");
                ResetPlayerDeck(player);

                availableIndices.Clear();
                for (int i = 0; i < availability.Length; i++)
                {
                    availableIndices.Add(i);
                }
            }

            // Выбрать случайную карту
            int randomIndex = availableIndices[Random.Range(0, availableIndices.Count)];
            CardData drawnCard = _allCards[randomIndex];

            // Пометить карту как использованную для этого игрока
            availability[randomIndex] = false;

            Debug.Log($"Игрок {player.PlayerId} получил карту: {drawnCard.uid}");

            // Синхронизировать изменение через RPC (чтобы клиенты знали)
            RPC_SyncCardUsed(player, randomIndex);
            RPC_SendCardToPlayer(player, drawnCard.uid);

            return drawnCard;
        }

        // RPC для синхронизации состояния колоды всем клиентам
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_SyncCardUsed(PlayerRef player, int cardIndex)
        {
            if (!_availabilityCache.ContainsKey(player))
            {
                _availabilityCache[player] = new bool[_allCards.Count];
                for (int i = 0; i < _allCards.Count; i++)
                {
                    _availabilityCache[player][i] = true;
                }
            }

            _availabilityCache[player][cardIndex] = false;
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_SendCardToPlayer(PlayerRef targetPlayer, string cardUid)
        {
            // Проверяем, что RPC пришёл именно МНЕ
            if (Runner.LocalPlayer != targetPlayer) return;

            CardData card = GetCardByUid(cardUid); // ваш метод поиска
            PlayerListManager.Instance.UIService.Get<UiGameScreen>().UpdateCardsHand(cardUid);
            Debug.Log($"ADD card RPC {cardUid}");
        }

        private CardData GetCardByUid(string uid)
        {
            return _allCards.Find(c => c.uid == uid);
        }

        // Вызывается, когда новый игрок присоединяется
        public void OnPlayerJoinedLobby(PlayerRef player)
        {
            if (Object.HasStateAuthority)
            {
                ResetPlayerDeck(player);
            }
        }
    }
}