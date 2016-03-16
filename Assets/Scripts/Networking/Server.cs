using System;
using System.Collections.Generic;
using Lidgren.Network;
using UnityEngine;

namespace Assets.Scripts.Networking
{
    public class Server : NetServer
    {
        public static Server Current;

        private Dictionary<NetConnection, NetPlayer> _players; 

        private NetIncomingMessage _incoming;

        private Client _attachedClient;

        private byte _idCounter = 0;

        public Server(NetPeerConfiguration config, Client attachedClient) : base(config)
        {
            Start();
            _players = new Dictionary<NetConnection, NetPlayer>();
            _attachedClient = attachedClient;
        }

        public void Update()
        {
            if ((_incoming = ReadMessage()) == null) return;

            switch (_incoming.MessageType)
            {
                case NetIncomingMessageType.DebugMessage:
                    Debug.Log("[S][INFO] " + _incoming.ReadString());
                    break;

                case NetIncomingMessageType.ErrorMessage:
                    Debug.Log("[S][ERROR]" + _incoming.ReadString());
                    break;

                case NetIncomingMessageType.StatusChanged:
                    Debug.Log("[S][STATUS] " + Status);
                    break;

                case NetIncomingMessageType.WarningMessage:
                    Debug.Log("[S][WARN] " + _incoming.ReadString());
                    break;

                case NetIncomingMessageType.Data:
                    HandleData(_incoming);
                    break;
            }
        }

        private void HandleData(NetIncomingMessage msg)
        {
            var type = (NetObject.Type) msg.ReadByte(NetObject.IndentifierNumOfBits);

            switch (type)
            {
                case NetObject.Type.PlayerControlsUpdate:
                    HandleControlsUpdate(msg);
                    break;
                case NetObject.Type.RegisterPlayer:
                    RegisterPlayer(msg);
                    break;
                case NetObject.Type.PlayerJump:
                    break;
                case NetObject.Type.PlayerDataPack:
                    break;
                case NetObject.Type.PlayerPositionUpdate:
                    break;
                case NetObject.Type.PlayerPhysicsUpdate:
                    break;
                case NetObject.Type.PlayerMouseUpdate:
                    break;
                case NetObject.Type.PlayerShootBullet:
                    break;
                case NetObject.Type.MapData:
                    HandleMapRequest(msg);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Recycle(_incoming);
        }

        private void HandleControlsUpdate(NetIncomingMessage msg)
        {
            GetClientPlayer(msg.SenderConnection).UnpackControls(msg);
        }

        private void HandleMapRequest(NetIncomingMessage msg)
        {
            var response = CreateMessage();

            response.Write((byte)NetObject.Type.MapData, NetObject.IndentifierNumOfBits);

            response.Write(NetworkMain.Current.GameSceneName);

            SendMessage(response, msg.SenderConnection, NetDeliveryMethod.ReliableOrdered);
        }

        private void RegisterPlayer(NetIncomingMessage msg)
        {
            var connection = msg.SenderConnection;

            if(_players.ContainsKey(connection))
                return;

            var name = msg.ReadString();

            // spawn the player
            var controller = NetworkMain.Current.SpawnPlayer(_idCounter++, Vector2.zero, name);

            /* Respond to player request */
            var registrationResponse = CreateMessage();

            registrationResponse.Write((byte)NetObject.Type.RegisterPlayer, NetObject.IndentifierNumOfBits);

            // send the player its fresh new id
            registrationResponse.Write(controller.NetPlayer.NetId);

            // tell the player where to spawn
            registrationResponse.Write(Vector2.zero);

            // send the player's name back for convinience purposes
            registrationResponse.Write(name);

            SendMessage(registrationResponse, msg.SenderConnection, NetDeliveryMethod.ReliableOrdered);

            _attachedClient.Players.Add(controller.NetPlayer.NetId, controller.NetPlayer);

            _players.Add(connection, GetClientPlayer(controller.NetPlayer.NetId));

            /* Send the new connection data about all currently connected players */
            foreach (var player in _players.Values)
            {
                if (player.NetId != controller.NetPlayer.NetId)
                {
                    var m = CreateMessage();

                    m.Write((byte)NetObject.Type.PlayerDataPack, NetObject.IndentifierNumOfBits);

                    m = player.PackData(m);

                    SendMessage(m, connection, NetDeliveryMethod.ReliableOrdered);
                }
            }

            /* Send the newly created player to all other connected players */
            var connectNotice = CreateMessage();

            connectNotice.Write((byte) NetObject.Type.PlayerDataPack);

            connectNotice = controller.NetPlayer.PackData(connectNotice);

            SendToAll(connectNotice, connection, NetDeliveryMethod.ReliableOrdered, 0);
        }

        private NetPlayer GetClientPlayer(NetConnection connection)
        {
            return _attachedClient.Players[_players[connection].NetId];
        }

        private NetPlayer GetClientPlayer(byte netId)
        {
            return _attachedClient.Players[netId];
        }
    }
}
