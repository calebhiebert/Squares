using System;
using System.Collections;
using System.Collections.Generic;
using Lidgren.Network;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace Assets.Scripts
{
    public class NetworkingClient : MonoBehaviour
    {
        public static NetworkingClient Current;

        public GameObject playerPrefab;

        public float UpdatesPerSecond;
        public int Port;

        private NetClient _client;
        private NetworkPlayer _localPlayer;

        private float _lastSend;
        private bool _sceneLoaded;

        public void Connect()
        {
            NetPeerConfiguration config = new NetPeerConfiguration("SpaceGame")
            {
                SimulatedMinimumLatency = 0.020f,
                SimulatedLoss = 0.01f,
                SimulatedDuplicatesChance = 0.01f
            };

            _client = new NetClient(config);

            _client.Start();

            ServerConnection = _client.Connect(Host, Port);

            Debug.Log(ServerConnection);

            Players = new Dictionary<byte, NetworkPlayer>();
        }

        void Awake()
        {
            Current = this;
            DontDestroyOnLoad(gameObject);
        }

        void Update()
        {
            if (_client == null || _client.Status != NetPeerStatus.Running)
                return;

            NetIncomingMessage msg;

            while ((msg = _client.ReadMessage()) != null)
            {

                if(msg.MessageType == NetIncomingMessageType.StatusChanged)
                    Register(Name);

                if(msg.MessageType == NetIncomingMessageType.DebugMessage)
                    Debug.Log(msg.ReadString());

                if (msg.MessageType == NetIncomingMessageType.Data)
                {
                    byte code = msg.ReadByte();

                    if (code == PacketType.PlayerRegistrationResponse)
                        HandleRegistrationResponse(msg);

                    else if (code == PacketType.StatusPlayerConnect)
                        HandlePlayerConnection(msg);

                    else if(code == PacketType.PlayerTransformUpdate)
                        HandleTransformUpate(msg);

                    else if (code == PacketType.StatusPlayerDisconnect)
                        HandleDisconnect(msg);

                    else if (code == PacketType.PlayerJump)
                        Players[msg.ReadByte()].Jump();

                    else if (code == PacketType.PlayerShootBullet)
                        HandleBullet(msg);
                }

                _client.Recycle(msg);
            }

            if (Time.time > _lastSend + 1/UpdatesPerSecond)
            {
                SendUpdates();
                _lastSend = Time.time;
            }
        }

        void OnGUI()
        {
            if (Client != null && ServerConnection != null)
            {
                GUI.Label(new Rect(10, 10, 100, 20), "Ping: " + (int) (ServerConnection.AverageRoundtripTime*1000));
            }
        }

        private void HandleBullet(NetIncomingMessage msg)
        {
            var id = msg.ReadByte();
            var origin = msg.ReadVector2();
            var direction = msg.ReadVector2();

            Players[id].PlayerTransform.GetComponentInChildren<BulletWeapon>().SpawnBullet(origin, direction);
        }

        void OnDestroy()
        {
            if (_client != null)
            {
                NetOutgoingMessage byeMsg = _client.CreateMessage();

                byeMsg.Write(PacketType.StatusPlayerDisconnect);

                _client.SendMessage(byeMsg, NetDeliveryMethod.ReliableUnordered);

                _client.Shutdown("bye");
            }
        }

        private void SendUpdates()
        {
            if(_localPlayer == null)
                return;

            var controls = Client.CreateMessage();

            controls.Write(PacketType.PlayerControlsUpdate);

            controls = _localPlayer.MakeControlsUpdate(controls);

            _client.SendMessage(controls, NetDeliveryMethod.UnreliableSequenced, 1);
        }

        private void Register(string name)
        {
            var message = _client.CreateMessage();

            message.Write(PacketType.PlayerRegistration);

            message.Write(name);

            _client.SendMessage(message, ServerConnection, NetDeliveryMethod.ReliableOrdered);

            Debug.Log("Sent Registration");
        }

        private void HandleRegistrationResponse(NetIncomingMessage msg)
        {
            var name = msg.ReadString();
            var id = msg.ReadByte();

            var player = new NetworkPlayer(name, id, true);

            _localPlayer = player;

            Players.Add(id, player);

            StartCoroutine(LoadSceneForPlayer(SceneManager.LoadSceneAsync("Game"), player));
        }

        private void HandlePlayerConnection(NetIncomingMessage msg)
        {
            var name = msg.ReadString();
            var id = msg.ReadByte();

            var player = new NetworkPlayer(name, id, false);

            Players.Add(id, player);

            if(_sceneLoaded)
                CreateNewPlayer(player);
        }

        private void HandleDisconnect(NetIncomingMessage msg)
        {
            var id = msg.ReadByte();

            foreach (var pc in PlayerController.ActivePlayers)
            {
                if (pc.NetPlayer.PlayerId == id)
                {
                    Destroy(pc.gameObject);
                    break;
                }
            }
        }

        private void CreateNewPlayer(NetworkPlayer np)
        {
            var playerGameObject = Instantiate(playerPrefab);

            playerGameObject.GetComponent<PlayerController>().NetPlayer = np;

            if(np.Local)
                Camera.main.GetComponent<CameraController>().Target = playerGameObject.transform;
        }

        private void HandleTransformUpate(NetIncomingMessage msg)
        {
            var playerId = msg.ReadByte();

            var player = Players[playerId];

            if (player != null)
            {
                player.ApplyTransformUpdate(msg);
            }
        }

        private IEnumerator LoadSceneForPlayer(AsyncOperation operation, NetworkPlayer player)
        {
            while (!operation.isDone)
            {
                yield return null;
            }

            _sceneLoaded = true;

            foreach (var p in Players.Values)
            {
                CreateNewPlayer(p);
            }
        }

        public NetworkPlayer LocalPlayer
        {
            get { return _localPlayer; }
        }

        public NetClient Client
        {
            get { return _client; }
        }

        public string Name { get; set; }

        public string Host { get; set; }

        public NetConnection ServerConnection { get; private set; }

        public Dictionary<byte, NetworkPlayer> Players { get; private set; }
    }
}
