using System;
using System.Collections.Generic;
using Assets.Scripts.PlayerModules;
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

        private float _lastUpdate = 0;

        public Server(NetPeerConfiguration config, Client attachedClient) : base(config)
        {
            Start();
            _players = new Dictionary<NetConnection, NetPlayer>();
            _attachedClient = attachedClient;
        }

        public void Update()
        {
            if ((_incoming = ReadMessage()) != null)
            {
                switch (_incoming.MessageType)
                {
                    case NetIncomingMessageType.DebugMessage:
                        Debug.Log("[S][INFO] " + _incoming.ReadString());
                        break;

                    case NetIncomingMessageType.ErrorMessage:
                        Debug.Log("[S][ERROR]" + _incoming.ReadString());
                        break;

                    case NetIncomingMessageType.StatusChanged:
                        var status = (NetConnectionStatus)_incoming.ReadByte();
                        Debug.Log("[S][STATUS][ " + _incoming.SenderConnection + "] " + status);

                        if (status == NetConnectionStatus.Disconnected)
                            HandleClientDisconnect(_players[_incoming.SenderConnection], _incoming.SenderConnection);

                        break;

                    case NetIncomingMessageType.WarningMessage:
                        Debug.Log("[S][WARN] " + _incoming.ReadString());
                        break;

                    case NetIncomingMessageType.Data:
                        HandleData(_incoming);
                        break;
                }
            }

            if (Time.time > _lastUpdate + 1/NetworkMain.Current.ServerUpdatesPerSecond)
            {
                _lastUpdate = Time.time;
                SendUpdates();
            }
        }

        private void HandleClientDisconnect(NetPlayer netPlayer, NetConnection nc)
        {
            var disconnect = CreateMessage(NetObject.Type.PlayerDisconnect);

            disconnect.Write(netPlayer.NetId);

            SendToAll(disconnect, NetDeliveryMethod.ReliableOrdered);

            _players.Remove(nc);
        }

        private void SendUpdates()
        {
            foreach (var player in _players.Values)
            {
                /* Send Positions To Everyone */
                SendMovementUpdate(player);
            }
        }

        private void SendMovementUpdate(NetPlayer player)
        {
            var he = player.AttachedPlayer.LastUpdateEntry;

            var diff = ((Vector2)player.AttachedPlayer.transform.position - he.pos).Abs();

            if (diff.x > 0 || diff.y > 0)
            {
                var msg = CreateMessage(NetObject.Type.PlayerMovementUpdate);

                msg.Write(player.NetId);

                msg = player.PackMovementData(msg);

                SendToAll(msg, null, NetDeliveryMethod.UnreliableSequenced, 2);
            }

            player.AttachedPlayer.UpdateHistoryEntry();
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
                    HandleJump(msg);
                    break;
                case NetObject.Type.PlayerDataPack:
                    break;
                case NetObject.Type.PlayerMouseUpdate:
                    break;
                case NetObject.Type.PlayerShootBullet:
                    HandleBullet(msg);
                    break;
                case NetObject.Type.MapData:
                    HandleMapRequest(msg);
                    break;
                case NetObject.Type.PlayerGroundPound:
                    HandleGroundPound(msg);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Recycle(_incoming);
        }

        private void HandleGroundPound(NetIncomingMessage msg)
        {
            var player = GetClientPlayer(msg.SenderConnection);
            var module = player.AttachedPlayer.GetComponentInChildren<PlayerGroundPoundModule>();

            if (module.CanPound)
            {
                module.DoPound();

                var poundMsg = CreateMessage(NetObject.Type.PlayerGroundPound);

                poundMsg.Write(player.NetId);

                SendToAll(poundMsg, NetDeliveryMethod.ReliableUnordered);

                SendMovementUpdate(player);
            }
        }

        private void HandleBullet(NetIncomingMessage msg)
        {
            var player = GetClientPlayer(msg.SenderConnection);
            var bulletModule = player.AttachedPlayer.GetComponentInChildren<PlayerBulletModule>();

            var origin = msg.ReadVector2();
            var direction = msg.ReadVector2();

            if (bulletModule.CanShoot)
            {
                bulletModule.CreateBullet(origin, direction);

                var bulletMsg = CreateMessage(NetObject.Type.PlayerShootBullet);

                bulletMsg.Write(player.NetId);

                bulletMsg.Write(origin);
                bulletMsg.Write(direction);

                SendToAll(bulletMsg, NetDeliveryMethod.ReliableUnordered);

                ChangeTimeScale(0.5f, 1.0f);
            }
        }

        private void HandleJump(NetIncomingMessage msg)
        {
            var player = GetClientPlayer(msg.SenderConnection);

            var jumpModule = player.AttachedPlayer.GetComponentInChildren<PlayerJumpModule>();

            if (jumpModule.CanJump)
            {
                jumpModule.DoJump();

                var jumpMsg = CreateMessage(NetObject.Type.PlayerJump);

                jumpMsg.Write(player.NetId);

                SendToAll(jumpMsg, NetDeliveryMethod.Unreliable);

                SendMovementUpdate(player);
            }
        }

        private void HandleControlsUpdate(NetIncomingMessage msg)
        {
            GetClientPlayer(msg.SenderConnection).UnpackControls(msg);
        }

        private void HandleMapRequest(NetIncomingMessage msg)
        {
            var response = CreateMessage(NetObject.Type.MapData);

            response.Write(NetworkMain.Current.GameSceneName);

            SendMessage(response, msg.SenderConnection, NetDeliveryMethod.ReliableOrdered);
        }

        private void RegisterPlayer(NetIncomingMessage msg)
        {
            var connection = msg.SenderConnection;

            if (_players.ContainsKey(connection))
                return;

            var name = msg.ReadString();

            var vec = msg.ReadVector3();

            var color = new Color(vec.x, vec.y, vec.z);

            // spawn the player
            var controller = NetworkMain.Current.SpawnPlayer(_idCounter++, Vector2.zero, name, color);

            /* Respond to player request */
            var registrationResponse = CreateMessage(NetObject.Type.RegisterPlayer);

            // send the player its fresh new id
            registrationResponse.Write(controller.NetPlayer.NetId);

            // tell the player where to spawn
            registrationResponse.Write(Vector2.zero);

            // send the player's name back for convinience purposes
            registrationResponse.Write(name);

            // send the player's color back for convinience purposes
            registrationResponse.Write(color);

            SendMessage(registrationResponse, msg.SenderConnection, NetDeliveryMethod.ReliableOrdered);

            _attachedClient.Players.Add(controller.NetPlayer.NetId, controller.NetPlayer);

            _players.Add(connection, GetClientPlayer(controller.NetPlayer.NetId));

            /* Send the new connection data about all currently connected players */
            foreach (var player in _players.Values)
            {
                if (player.NetId != controller.NetPlayer.NetId)
                {
                    var m = CreateMessage(NetObject.Type.PlayerDataPack);

                    m = player.PackData(m);

                    SendMessage(m, connection, NetDeliveryMethod.ReliableOrdered);
                }
            }

            /* Send the newly created player to all other connected players */
            var connectNotice = CreateMessage(NetObject.Type.PlayerDataPack);

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

        public NetOutgoingMessage CreateMessage(NetObject.Type type)
        {
            var msg = CreateMessage();
            msg.Write((byte) type, NetObject.IndentifierNumOfBits);

            return msg;
        }

        public void ChangeTimeScale(float newTimeScale, float forTime)
        {
            var ts = CreateMessage(NetObject.Type.GameTimeScale);

            ts.Write(newTimeScale);

            ts.Write(forTime);

            SendToAll(ts, NetDeliveryMethod.ReliableUnordered);
        }
    }
}
