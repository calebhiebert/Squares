using System.Collections.Generic;
using Lidgren.Network;
using UnityEngine;

namespace Assets.Scripts.Networking
{
    public class NetObject
    {
        public static List<NetObject> NetObjects = new List<NetObject>();
        public static readonly int IndentifierNumOfBits = 5;

        public enum Type
        {
            RegisterPlayer, PlayerJump, PlayerDataPack, PlayerPositionUpdate, PlayerPhysicsUpdate, PlayerMouseUpdate, PlayerControlsUpdate, PlayerShootBullet, MapData
        }

        public NetObject()
        {
            NetObjects.Add(this);
        }

        public NetOutgoingMessage CreateMessage(Type type)
        {
            if (!ClientSafe())
                return null;

            var msg = Client.Current.CreateMessage();

            msg.Write((byte)type, IndentifierNumOfBits);

            return msg;
        }

        bool ClientSafe()
        {
            if (Client.Current == null)
            {
                Debug.LogError("Client has not been started!");
                return false;
            }

            if (Client.Current.Status != NetPeerStatus.Running)
            {
                Debug.LogError("Client is not running!");
                return false;
            }

            if (Client.Current.ConnectionStatus != NetConnectionStatus.Connected)
            {
                Debug.LogError("Client is not connected!");
                return false;
            }

            return true;
        }
    }
}
