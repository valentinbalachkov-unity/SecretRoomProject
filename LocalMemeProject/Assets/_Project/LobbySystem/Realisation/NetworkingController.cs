using System;
using _Project.GameSystem.Realisation;
using Fusion;
using UnityEngine;

namespace _Project.LobbySystem.Realisation
{
    public class NetworkingController : NetworkBehaviour
    {
        // Событие, на которое подпишется UI или LogicManager, чтобы "читать" (Reader)
        public static event Action<LobbyPacket> OnPacketReceived;


        public PlayerController playerController;

        public static NetworkingController Local { get; private set; }

        [Networked] public GameMessageType CurrentGameState { get; set; }
        
        [SerializeField] private NetworkObject _loadingManagerPrefab;
        private NetworkObject _loadingManager;

        public override void Spawned()
        {
            // Если этот объект принадлежит МНЕ (локальному клиенту)
            if (Object.HasInputAuthority)
            {
                OnPacketReceived += FindFirstObjectByType<GameStateUIPresenter>().OnPackageReceived;
                Local = this;
                SendGameState(GameMessageType.LobbyState);
            }
        }
        
        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (Local == this)
            {
                OnPacketReceived -= FindFirstObjectByType<GameStateUIPresenter>().OnPackageReceived;
                Local = null;
            }
        }

        // ------------------- WRITER (Отправка) -------------------

        // Публичный метод для отправки любых данных
        public void SendPacket(GameMessageType type, string textContent = "", int intContent = 0)
        {
            if (Object.HasInputAuthority) // Отправлять может только владелец объекта
            {
                RPC_BroadcastData(type, textContent, intContent);
            }
        }

        public void SendGameState(GameMessageType state)
        {
            if (state == GameMessageType.GameState)
            {
                if (_loadingManager == null)
                {
                    _loadingManager = Runner.Spawn(_loadingManagerPrefab);
                    _loadingManager.GetComponent<LoadingManager>().Init(FindFirstObjectByType<GameStateUIPresenter>().UIService);
                }  
            }
            SendPacket(state);
        }

        // ------------------- NETWORK (Транспорт) -------------------

        // RPC отправляет данные от клиента на Сервер (Input -> State), 
        // а затем Сервер рассылает всем (State -> All).
        // Это паттерн "Broadcast" через сервер.
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_BroadcastData(GameMessageType type, string txt, int num, RpcInfo info = default)
        {
            // Сервер получил сообщение и пересылает его ВСЕМ клиентам
            RPC_RelayToAll(type, txt, num, info.Source);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_RelayToAll(GameMessageType type, string txt, int num, PlayerRef sender)
        {
            // ------------------- READER (Чтение) -------------------

            // Упаковываем данные обратно в красивую структуру
            var packet = new LobbyPacket
            {
                senderId = sender.PlayerId,
                type = type,
                stringData = txt,
                intData = num
            };

            Debug.Log($"[Reader] Получено сообщение: {type} от {sender.PlayerId}");

            // Вызываем событие, чтобы UI или другие системы отреагировали
            OnPacketReceived?.Invoke(packet);
        }
    }
}

public enum GameMessageType
{
    LobbyState = 0,
    GameState = 1
}

public enum GameState
{
    WriteTextState = 1,
    SelectPictureState = 2,
    ShowResultState = 3,
    VotingState = 4,
    EndGameState = 5
}

// Структура для удобной работы внутри C# кода (Writer/Reader)
public struct LobbyPacket
{
    public int senderId;          // ID игрока, отправившего данные
    public GameMessageType type; // Тип сообщения
    public string stringData;     // Текст или URL
    public int intData;           // ID состояния или числовые данные
}