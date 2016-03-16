using Lidgren.Network;
using UnityEngine;

namespace Assets.Scripts.Networking
{
    public class NetPlayer : NetObject
    {
        // event delegates
        public delegate void MovementDataEvent(Vector2 netPosition, Quaternion netRotation, Vector2 netVelocity, float netAngularVelocity);

        public delegate void MouseUpdateEvent(Vector2 globalMousePos);

        public event MovementDataEvent OnMovementData;

        public event MouseUpdateEvent OnMouseUpdate;

        // player controller for this netobject
        public PlayerController AttachedPlayer { get; private set; }

        // information package for this netobject
        public TransformData NetData;

        // the id of this player on the network
        public byte NetId;

        // the name of this player
        public string Name;

        // do we have control of this netplayer
        public bool IsLocal;

        public struct TransformData
        {
            // transform data
            public Vector2 NetPosition;
            public Quaternion NetRotation;

            // physics data
            public Vector2 NetVelocity;
            public float NetAngularVelocity;

            // mouse data
            public Vector2 NetGlobalMouse;
        }

        public NetPlayer(byte netId, PlayerController attachedPlayer)
        {
            NetId = netId;
            AttachedPlayer = attachedPlayer;
        }

        public NetOutgoingMessage PackMovementData(NetOutgoingMessage msg)
        {
            // write player position
            msg.Write(AttachedPlayer.transform.position);

            // write player position
            msg.Write(AttachedPlayer.transform.rotation.eulerAngles.z);

            // get rigidbody
            var rigidBody = AttachedPlayer.GetComponentInChildren<Rigidbody2D>();

            //write physics details
            msg.Write(rigidBody.velocity);
            msg.Write(rigidBody.angularVelocity);

            return msg;
        }

        public void UnpackMovementData(NetIncomingMessage msg)
        {
            // read position
            NetData.NetPosition = msg.ReadVector2();

            // read rotation
            NetData.NetRotation = Quaternion.Euler(0, 0, msg.ReadFloat());

            // read velocity
            NetData.NetVelocity = msg.ReadVector2();

            // read angular velocity
            NetData.NetAngularVelocity = msg.ReadFloat();

            if (OnMovementData != null)
                OnMovementData(NetData.NetPosition, NetData.NetRotation, NetData.NetVelocity, NetData.NetAngularVelocity);
        }

        public void UnpackMouse(NetIncomingMessage msg)
        {
            NetData.NetGlobalMouse = msg.ReadVector2();

            if (OnMouseUpdate != null)
                OnMouseUpdate(NetData.NetGlobalMouse);
        }

        public NetOutgoingMessage PackData(NetOutgoingMessage msg)
        {
            msg.Write(NetId);
            msg.Write(AttachedPlayer.transform.position);
            msg.Write(Name);

            return msg;
        }

        public NetOutgoingMessage PackControls(NetOutgoingMessage msg)
        {
            var c = Controls.Poll();

            msg.Write(c.Left);
            msg.Write(c.Right);

            return msg;
        }

        public void UnpackControls(NetIncomingMessage msg)
        {
            AttachedPlayer.CurrentControls.Left = msg.ReadBoolean();
            AttachedPlayer.CurrentControls.Right = msg.ReadBoolean();
        }
    }
}
