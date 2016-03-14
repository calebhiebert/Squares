using System.Collections.Generic;
using Lidgren.Network;
using UnityEngine;

namespace Assets.Scripts
{
    public class NetworkingServer : MonoBehaviour
    {
        public static NetworkingServer Current;

        public static bool IsServer;

        public int Port;
        public float UpdatesPerSecond;
        public NetServer Server;

        public byte Idcounter = 0;

        public Dictionary<NetConnection, NetworkPlayer> Players;

        private float _lastUpdate;

        void Awake()
        {
            Current = this;
        }

        public void Host()
        {
            var config = new NetPeerConfiguration("SpaceGame");

            config.Port = Port;


            Server = new NetServer(config);

            Players = new Dictionary<NetConnection, NetworkPlayer>();

            Server.Start();

            IsServer = true;
        }

        void Update()
        {
            if(Server == null || Server.Status != NetPeerStatus.Running)
                return;

            NetIncomingMessage msg;

            while ((msg = Server.ReadMessage()) != null)
            {
                if (msg.MessageType == NetIncomingMessageType.StatusChanged)
                    Debug.Log(Server.Status);

                if (msg.MessageType == NetIncomingMessageType.DebugMessage)
                    Debug.Log(msg.ReadString());

                if (msg.MessageType == NetIncomingMessageType.Data)
                {
                    byte code = msg.ReadByte();

                    if (code == PacketType.PlayerRegistration)
                    {
                        HandlePlayerRegistration(msg);
                    }

                    else if (code == PacketType.PlayerJump)
                        HandleJump(msg);

                    else if (code == PacketType.PlayerShootBullet)
                        HandleBullet(msg);

                    else if (code == PacketType.PlayerControlsUpdate)
                        HandleControlsUpdate(msg);
                }

                Server.Recycle(msg);
            }

            if (Time.time > _lastUpdate + 1/UpdatesPerSecond)
            {
                UpdatePositions();
                _lastUpdate = Time.time;
            }
        }

        void UpdatePositions()
        {
            foreach (var pc in PlayerController.ActivePlayers)
            {
                var msg = Server.CreateMessage();

                msg.Write(PacketType.PlayerTransformUpdate);

                msg.Write(pc.NetPlayer.PlayerId);

                msg = pc.NetPlayer.MakeTransformUpdate(msg);

                Server.SendToAll(msg, null, NetDeliveryMethod.UnreliableSequenced, 5);
            }
        }

        private void HandleControlsUpdate(NetIncomingMessage msg)
        {
            GetPhysicalPlayer(msg.SenderConnection).ApplyControlsUpdate(msg);
        }

        private void HandleBullet(NetIncomingMessage msg)
        {
            var passOn = Server.CreateMessage();

            passOn.Write(PacketType.PlayerShootBullet);
            passOn.Write(Players[msg.SenderConnection].PlayerId);

            // origin
            passOn.Write(msg.ReadVector2());

            // direction
            passOn.Write(msg.ReadVector2());

            //passOn.WriteTime(true);

            Server.SendToAll(passOn, NetDeliveryMethod.ReliableUnordered);
        }

        private void HandleJump(NetIncomingMessage msg)
        {
            var p = GetPhysicalPlayer(msg.SenderConnection);

            if (!p.PlayerTransform.GetComponent<PlayerController>().CanJump) return;
            p.Jump();

            var jmp = Server.CreateMessage();
            jmp.Write(PacketType.PlayerJump);
            jmp.Write(Players[msg.SenderConnection].PlayerId);

            Server.SendToAll(jmp, null, NetDeliveryMethod.UnreliableSequenced, 0);
        }

        void HandlePlayerRegistration(NetIncomingMessage message)
        {
            if(Players.ContainsKey(message.SenderConnection))
                return;

            string name = message.ReadString();

            var player = new NetworkPlayer(name, Idcounter++);

            Debug.Log("Created player for " + player.Name + " with id " + player.PlayerId);

            var response = Server.CreateMessage();

            Players.Add(message.SenderConnection, player);

            response.Write(PacketType.PlayerRegistrationResponse);
            response.Write(name);
            response.Write(player.PlayerId);

            Server.SendMessage(response, message.SenderConnection, NetDeliveryMethod.ReliableOrdered);

            var newPlayer = Server.CreateMessage();
            newPlayer.Write(PacketType.StatusPlayerConnect);

            newPlayer.Write(name);
            newPlayer.Write(player.PlayerId);

            Server.SendToAll(newPlayer, message.SenderConnection, NetDeliveryMethod.ReliableOrdered, 0);

            foreach (var networkPlayer in Players.Values)
            {
                if (networkPlayer.PlayerId != player.PlayerId)
                {
                    var p = Server.CreateMessage();

                    p.Write(PacketType.StatusPlayerConnect);
                    p.Write(networkPlayer.Name);
                    p.Write(networkPlayer.PlayerId);

                    Server.SendMessage(p, message.SenderConnection, NetDeliveryMethod.ReliableOrdered);
                }
            }
        }

        NetworkPlayer GetPhysicalPlayer(NetConnection connection)
        {
            return NetworkingClient.Current.Players[Players[connection].PlayerId];
        }
    }
}
