using System;
using System.Collections.Generic;
using Assets.Scripts.Bullet_Weapon;
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

        private void Register(string playerName)
        {
            var msg = CreateMessage();

            msg.Write((byte) NetObject.Type.RegisterPlayer, NetObject.IndentifierNumOfBits);

            msg.Write(playerName);

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
                    break;
                case NetObject.Type.PlayerDataPack:
                    HandlePlayerData(msg);
                    break;
                case NetObject.Type.PlayerPositionUpdate:
                    break;
                case NetObject.Type.PlayerPhysicsUpdate:
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
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void HandleRegistrationResponse(NetIncomingMessage msg)
        {
            var id = msg.ReadByte();
            var pos = msg.ReadVector2();
            var name = msg.ReadString();

            Debug.Log(string.Format("id: {0} pos: {1} name: {2}", id, pos, name));

            if (NetworkMain.IsServer)
            {
                Debug.Log("I am server");

                _localPlayer = _players[id];

                _localPlayer.IsLocal = true;
            }
            else
            {
                Debug.Log("I am client");

                var controller = NetworkMain.Current.SpawnPlayer(id, pos, name);

                controller.NetPlayer.IsLocal = true;

                _players.Add(controller.NetPlayer.NetId, controller.NetPlayer);

                _localPlayer = controller.NetPlayer;

                if (Camera.main != null)
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

            var controller = NetworkMain.Current.SpawnPlayer(id, pos, name);

            _players.Add(id, controller.NetPlayer);
        }

        private void HandleBullet(NetIncomingMessage msg)
        {
            var id = msg.ReadByte();
            var origin = msg.ReadVector2();
            var direction = msg.ReadVector2();

            _players[id].AttachedPlayer.GetComponentInChildren<BulletWeapon>().SpawnBullet(origin, direction);
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
            Register("Robby");

            NetworkMain.OnSceneLoadComplete -= OnSceneLoad;
        }
    }
}
