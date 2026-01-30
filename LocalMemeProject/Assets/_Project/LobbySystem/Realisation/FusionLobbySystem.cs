using System;
using System.Collections.Generic;
using Dreamers.UI.UIService.Interfaces;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

namespace _Project.LobbySystem.Realisation
{
    public class FusionLobbySystem : MonoBehaviour, INetworkRunnerCallbacks
    {
        [SerializeField] private NetworkRunner networkRunnerPrefab;
        [SerializeField] private NetworkPrefabRef playerPrefab;

        [SerializeField] private NetworkObject _gameStateMachinePrefab;
        [SerializeField] private NetworkObject _deckSystemPrefab;
        [SerializeField] private NetworkObject _themeManagerPrefab;
        [SerializeField] private NetworkObject _timerPrefab;
        [SerializeField] private NetworkObject _textSubmissionManagerPrefab;
        [SerializeField] private NetworkObject _resultManagerPrefab;
        [SerializeField] private NetworkObject _votingManagerPrefab;
       
        
        private NetworkObject _fsmInstance;
        private NetworkObject _deckSystemObject;
        private NetworkObject _themeManager;
        private NetworkObject _timerObject;
        private NetworkObject _textSubmissionManager;
        private NetworkObject _resultManager;
        private NetworkObject _votingManager;
     

        private NetworkRunner _currentRunner;

        public Dictionary<PlayerRef, NetworkObject> spawnedCharacters = new();

        private string _testRoomName = "secretRoom";

        [Networked, OnChangedRender(nameof(OnNickNameChanged))]
        public NetworkString<_16> NickName { get; set; }


        [SerializeField] private PlayerListManager playerListManager;
        private IUIService _uiService;

        public void Init(IUIService uiService)
        {
            _uiService = uiService;
            playerListManager.Init(_uiService);
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_SetNickName(string newName, RpcInfo info = default)
        {
            NickName = newName;
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log($"[Spawner] Игрок {player.PlayerId} подключился");
            
            if (runner.IsServer) // Только сервер имеет право создавать объекты
            {
                // Создаем объект для зашедшего игрока
                // inputAuthority: player — отдает управление этому игроку
                NetworkObject networkPlayer = runner.Spawn(playerPrefab, Vector3.zero, Quaternion.identity, player);

                spawnedCharacters.Add(player, networkPlayer);
                
                Debug.Log($"[Spawner] Заспавнен объект для игрока {player.PlayerId}");
            }
        }

        

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            if (spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
            {
                runner.Despawn(networkObject);
                spawnedCharacters.Remove(player);
            }
        }

        public void SetNickname(string nickname)
        {
            RPC_SetNickName(nickname);
        }

        private void OnNickNameChanged()
        {
            _uiService.Get<UIMainMenu>().NicknameInputField.text = NickName.ToString();
        }

        public void JoinRoom()
        {
            string roomName = _testRoomName;
            if (string.IsNullOrWhiteSpace(roomName))
            {
                Debug.LogError("Room name cannot be empty!");
                return;
            }

            StartGame(GameMode.Client, roomName);
        }

        public void CreateRoom()
        {
            string roomName = _testRoomName;
            if (string.IsNullOrWhiteSpace(_testRoomName))
            {
                Debug.LogError("Room name cannot be empty!");
                return;
            }

            StartGame(GameMode.Host, roomName);
        }

        public async void StartGame(GameMode mode, string roomName)
        {
            if (_currentRunner != null)
            {
                Debug.LogWarning("A runner is already active.");
                return;
            }

            _uiService.Show<UILoadingScreen>(2);

            _currentRunner = Instantiate(networkRunnerPrefab);
            _currentRunner.AddCallbacks(this);
            _currentRunner.name = $"Runner - {mode}";

            // Настройка аргументов для запуска игры
            var startGameArgs = new StartGameArgs()
            {
                GameMode = mode,
                SessionName = roomName, // Это ключ к созданию/подключению к приватной комнате [web:15, web:5]
                SceneManager = _currentRunner.gameObject.AddComponent<NetworkSceneManagerDefault>()
            };

            // Запуск сетевой сессии
            var result = await _currentRunner.StartGame(startGameArgs);

            if (result.Ok)
            {
                if (!_currentRunner.IsServer)
                {
                    _uiService.Hide<UILoadingScreen>();
                    return;
                } 
                
               
                
                if (_fsmInstance == null)
                {
                    _fsmInstance = _currentRunner.Spawn(_gameStateMachinePrefab);
                }  
                if (_deckSystemObject == null)
                {
                    _deckSystemObject = _currentRunner.Spawn(_deckSystemPrefab);
                }
                if (_themeManager == null)
                {
                    _themeManager = _currentRunner.Spawn(_themeManagerPrefab);
                }
                if (_timerObject == null)
                {
                    _timerObject = _currentRunner.Spawn(_timerPrefab);
                }
                if (_textSubmissionManager == null)
                {
                    _textSubmissionManager = _currentRunner.Spawn(_textSubmissionManagerPrefab);
                }
                if (_resultManager == null)
                {
                    _resultManager = _currentRunner.Spawn(_resultManagerPrefab);
                }
                if (_votingManager == null)
                {
                    _votingManager = _currentRunner.Spawn(_votingManagerPrefab);
                }
                
                _uiService.Hide<UILoadingScreen>();
                Debug.Log($"Game started in {mode} mode with room name '{roomName}'.");
            }
            else
            {
                Debug.LogError($"Failed to start game: {result.ShutdownReason}");
                // Если запуск не удался, уничтожаем созданный экземпляр Runner
                _currentRunner.RemoveCallbacks();
                Destroy(_currentRunner.gameObject);
                _currentRunner = null;
                _uiService.Hide<UILoadingScreen>();
            }
        }
        

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
        }

        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
        }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
        }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request,
            byte[] token)
        {
        }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
        }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
        {
        }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key,
            ArraySegment<byte> data)
        {
        }

        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
        {
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {
        }

        public void OnConnectedToServer(NetworkRunner runner)
        {
        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
        }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {
        }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
        }

        public void OnSceneLoadDone(NetworkRunner runner)
        {
        }

        public void OnSceneLoadStart(NetworkRunner runner)
        {
        }
    }
}