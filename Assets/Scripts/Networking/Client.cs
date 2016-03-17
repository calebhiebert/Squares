using System;
using System.Collections.Generic;
using Assets.Scripts.Bullet_Weapon;
using Assets.Scripts.PlayerModules;
using Assets.Scripts.Small_Components;
using Lidgren.Network;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Networking
{
    public class Client : NetClient
    {
        public static Client Current;

        private NetIncomingMessage _incoming;

        private Dictionary<byte, NetPlayer> _players;

        private NetPlayer _localPlayer;

        public Client(NetPeerConfiguration config) : base(config)
        {
            Start();

            _players = new Dictionary<byte, NetPlayer>();
        }

        public void Update()
        {
            if ((_incoming = ReadMessage()) != null)
            {
                switch (_incoming.MessageType)
                {
                        case NetIncomingMessageType.DebugMessage:
                            Debug.Log("[C][INFO] " + _incoming.ReadString());
                        break;

                        case NetIncomingMessageType.ErrorMessage:
                            Debug.LogError("[C][ERROR]" + _incoming.ReadString());
                        break;

                        case NetIncomingMessageType.StatusChanged:
                            Debug.Log("[C][STATUS] " + Status);
                        if(ServerConnection != null)
                            SendMapRequest();
                        break;

                        case NetIncomingMessageType.WarningMessage:
                            Debug.LogWarning("[C][WARN] " + _incoming.ReadString());
                        break;

                        case NetIncomingMessageType.Data:
                            HandleData(_incoming);
                        break;
                }
            }
        }

        public Dictionary<byte, NetPlayer> Players
        {
            get { return _players; }
        }

        private void Register(string playerName, Color color)
        {
            var msg = CreateMessage(NetObject.Type.RegisterPlayer);

            msg.Write(playerName);

            msg.Write(new Vector3(color.r, color.g, color.b));

            SendMessage(msg, NetDeliveryMethod.ReliableOrdered);
        }

        private void SendMapRequest()
        {
            var mr = CreateMessage();
            mr.Write((byte) NetObject.Type.MapData, NetObject.IndentifierNumOfBits);
            SendMessage(mr, NetDeliveryMethod.ReliableOrdered);
        }

        private void HandleData(NetIncomingMessage msg)
        {
            NetObject.Type type = (NetObject.Type) msg.ReadByte(NetObject.IndentifierNumOfBits);

            switch (type)
            {
                case NetObject.Type.RegisterPlayer:
                    HandleRegistrationResponse(msg);        
                    break;
                case NetObject.Type.PlayerJump:
                    HandleJump(msg);
                    break;
                case NetObject.Type.PlayerDataPack:
                    HandlePlayerData(msg);
                    break;
                case NetObject.Type.PlayerMouseUpdate:
                    break;
                case NetObject.Type.PlayerControlsUpdate:
                    break;
                case NetObject.Type.PlayerShootBullet:
                    HandleBullet(msg);
                    break;
                case NetObject.Type.MapData:
                    HandleMapData(msg);
                    break;
                case NetObject.Type.PlayerMovementUpdate:
                    HandleMovementUpdate(msg);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void HandleJump(NetIncomingMessage msg)
        {
            if(NetworkMain.IsServer)
                return;

            var id = msg.ReadByte();

            if(_players.ContainsKey(id))
                _players[id].AttachedPlayer.GetComponent<PlayerJumpModule>().DoJump();
        }

        private void HandleMovementUpdate(NetIncomingMessage msg)
        {
            if(NetworkMain.IsServer)
                return;

            var id = msg.ReadByte();

            if(_players.ContainsKey(id))
                _players[id].UnpackMovementData(msg);
        }

        private void HandleRegistrationResponse(NetIncomingMessage msg)
        {
            var id = msg.ReadByte();
            var pos = msg.ReadVector2();
            var name = msg.ReadString();
            var color = msg.ReadVector3();

            Debug.Log(string.Format("id: {0} pos: {1} name: {2}", id, pos, name));

            if (NetworkMain.IsServer)
            {
                Debug.Log("I am server");

                _localPlayer = _players[id];

                _localPlayer.IsLocal = true;

                Camera.main.GetComponent<CameraController>().Target = _localPlayer.AttachedPlayer.transform;
            }
            else
            {
                Debug.Log("I am client");

                var controller = NetworkMain.Current.SpawnPlayer(id, pos, name, new Color(color.x, color.y, color.z));

                controller.NetPlayer.IsLocal = true;

                _players.Add(controller.NetPlayer.NetId, controller.NetPlayer);

                _localPlayer = controller.NetPlayer;

                Camera.main.GetComponent<CameraController>().Target = controller.transform;
            }
        }

        private void HandlePlayerData(NetIncomingMessage msg)
        {
            if(NetworkMain.IsServer)
                return;

            var id = msg.ReadByte();
            var pos = msg.ReadVector2();
            var name = msg.ReadString();
            var color = msg.ReadVector3();

            var controller = NetworkMain.Current.SpawnPlayer(id, pos, name, new Color(color.x, color.y, color.z));

            _players.Add(id, controller.NetPlayer);
        }

        private void HandleBullet(NetIncomingMessage msg)
        {
            if(NetworkMain.IsServer)
                return;

            var id = msg.ReadByte();
            var origin = msg.ReadVector2();
            var direction = msg.ReadVector2();

            _players[id].AttachedPlayer.GetComponentInChildren<PlayerBulletModule>().CreateBullet(origin, direction);
        }

        private void HandleMapData(NetIncomingMessage msg)
        {
            var data = msg.ReadString();

            if(SceneManager.GetActiveScene().name == data)
                return;

            NetworkMain.OnSceneLoadComplete += OnSceneLoad;

            NetworkMain.Current.LoadMap(data);
        }

        private void OnSceneLoad(string sceneName)
        {
            Register(NetworkMain.Current.PlayerName, NetworkMain.Current.PlayerColor);

            NetworkMain.OnSceneLoadComplete -= OnSceneLoad;
        }

        public NetOutgoingMessage CreateMessage(NetObject.Type type)
        {
            var msg = CreateMessage();
            msg.Write((byte) type, NetObject.IndentifierNumOfBits);

            return msg;
        }
    }
}
