using _Project.DeckSystem.Realisation;
using _Project.GameSystem.Realisation;
using Fusion;
using UnityEngine;

namespace _Project.LobbySystem.Realisation
{
    public class PlayerController : NetworkBehaviour
    {
        [Networked] public int Score { get; set; }
        
        [Networked] public NetworkString<_32> PlayerName { get; set; }
        [Networked] public NetworkString<_32> ReceivedText { get; set; }
        public string testText;
        private ChangeDetector _changes;

        // Количество карт на руке (видно всем)
        [Networked] public int CardsInHand { get; set; }

        [Networked] public NetworkString<_32> CurrentCard { get; set; }

        [Networked] public NetworkBool IsLoaded { get; set; }

        private TextSubmissionManager _submissionManager;

        // Если нужно хранить конкретные карты (например, uid), используйте массив
        // Fusion 1 поддерживает массивы фиксированного размера через [Networked]
        [Networked, Capacity(GameConfig.MAX_CARDS_IN_HAND)] // Макс n карт на руке
        public NetworkArray<string> CardIds => default; // Можно хранить индексы или хеши uid

        public override void Spawned()
        {
            _submissionManager = FindFirstObjectByType<TextSubmissionManager>();
            _changes = GetChangeDetector(ChangeDetector.Source.SimulationState);

            // 1. Регистрируемся в менеджере (добавляем строчку в таблицу UI)
            PlayerListManager.Instance.AddPlayer(Object.InputAuthority, this);

            // 2. Если это НАШ локальный игрок, мы должны задать ему имя
            if (Object.HasInputAuthority)
            {
                RPC_SetNickName(PlayerListManager.Instance.UIService.Get<UIMainMenu>().NicknameInputField.text);
            }
        }
        
        public void AddScore(int amount)
        {
            if (Object.HasStateAuthority)
            {
                Score += amount;
            }
        }

        /// <summary>
        /// Игрок выбирает карту (вызывается из UI локально)
        /// </summary>
        public void SelectCard(string cardUid)
        {
            if (Object.HasInputAuthority)
            {
                RPC_SelectCard(cardUid);
            }
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_SelectCard(string cardUid)
        {
            // Сообщаем DeckManager'у
            var deckManager = FindFirstObjectByType<DeckController>();
            if (deckManager != null)
            {
                deckManager.OnCardSelectedByPlayer(Object.InputAuthority, cardUid);
            }
        }
        
        public void SetReady()
        {
            if (Object.HasInputAuthority)
            {
                RPC_SetReady();
            }
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_SetReady()
        {
            // Сообщаем DeckManager'у
            var resultManager = FindFirstObjectByType<ResultPhaseManager>();
            if (resultManager != null)
            {
                resultManager.SetPlayerReady();
            }
        }

        // Метод для DeckManager (сброс)
        public void ClearCurrentCard()
        {
            if (Object.HasStateAuthority)
            {
                CurrentCard = "";
            }
        }

        // Метод для DeckManager (таймер)
        public void SetCardSelection(string uid)
        {
            if (Object.HasStateAuthority)
            {
                RPC_SelectCard(uid);
            }
        }

        // Вспомогательный: взять карту из руки
        public string GetRandomCardFromHand()
        {
            // Если рука пуста - вернуть заглушку, иначе первую карту
            if (CardsInHand > 0)
            {
                // Логика получения UID по индексу из CardIds
                // (Зависит от того, как вы храните UID карт)

                var r = Random.Range(0, CardsInHand);
                
                var card = CardIds[r];

                CardsInHand--;
                PlayerListManager.Instance.UIService.Get<UiGameScreen>().RemoveCard(card);
                
                return card;
            }

            return "p1";
        }
        
        /// <summary>
        /// Вызывается из UI кнопки "Голосовать"
        /// </summary>
        public void SendVote(PlayerRef targetPlayer)
        {
            // Проверка: вызывать может только владелец этого игрока
            if (Object.HasInputAuthority)
            {
                RPC_SendVote(targetPlayer);
            }
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_SendVote(PlayerRef targetPlayer)
        {
            // Этот код выполняется НА СЕРВЕРЕ
    
            // Находим менеджер голосования (он должен быть в сцене)
            var votingManager = FindFirstObjectByType<VotingManager>();
    
            if (votingManager != null)
            {
                // 1. voter = Object.InputAuthority (кто отправил RPC)
                // 2. targetPlayer = аргумент из RPC (за кого)
                votingManager.CastVote(Object.InputAuthority, targetPlayer);
            }
            else
            {
                Debug.LogError("VotingManager не найден!");
            }
        }


        /// <summary>
        /// Метод для отправки текста (вызывается из UI)
        /// </summary>
        public void SendTextToManager(string text)
        {
            if (Object.HasInputAuthority)
            {
                RPC_SendText(text);
            }
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_SendText(string text)
        {
            // Этот код выполняется на СЕРВЕРЕ (Хосте)

            if (_submissionManager != null)
            {
                // Теперь хост вызывает метод менеджера локально
                _submissionManager.OnTextReceivedFromPlayer(Object.InputAuthority, text);
            }
            else
            {
                _submissionManager = FindFirstObjectByType<TextSubmissionManager>();
                _submissionManager.OnTextReceivedFromPlayer(Object.InputAuthority, text);
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            // Удаляемся из списка при выходе
            PlayerListManager.Instance.RemovePlayer(Object.InputAuthority);
        }

        // Метод, который выполняется на сервере для смены имени
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void RPC_SetNickName(string nickName, RpcInfo info = default)
        {
            PlayerName = nickName;
        }

        public override void Render()
        {
            // Проверяем изменения каждый кадр
            foreach (var change in _changes.DetectChanges(this))
            {
                if (change == nameof(PlayerName))
                {
                    OnNameChanged();
                }
            }
        }

        private static void OnNameChanged()
        {
            PlayerListManager.Instance.UpdatePlayerListUI();
        }

        // Метод для добавления карты (вызывает хост)
        public void AddCard(string cardId)
        {
            if (!Object.HasStateAuthority) return;

            if (CardsInHand < CardIds.Length)
            {
                CardIds.Set(CardsInHand, cardId);
                CardsInHand++;
            }
        }

        // Метод для сброса руки (например, в конце раунда)
        public void ClearHand()
        {
            if (!Object.HasStateAuthority) return;

            CardsInHand = 0;
            // CardIds очищать не обязательно, просто игнорируем индексы >= CardsInHand
        }

        // Вызывать этот метод локально, когда сцена/ресурсы загружены
        public void SetPlayerLoaded()
        {
            if (Object.HasInputAuthority)
            {
                // Отправляем RPC хосту, чтобы он обновил переменную
                RPC_SetLoaded(true);
            }
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_SetLoaded(bool isLoaded)
        {
            IsLoaded = isLoaded;
            Debug.Log($"[PlayerController] Игрок {Object.InputAuthority.PlayerId} загрузился!");
        }
    }
}