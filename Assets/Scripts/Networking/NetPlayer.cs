using Lidgren.Network;
using UnityEngine;

namespace Assets.Scripts.Networking
{
    public class NetPlayer : NetObject
    {
        // event delegates
        public delegate void TransformUpdateEvent(Vector2 netPosition, Quaternion netRotation);

        public delegate void PhysicsUpdateEvent(Vector2 netVelocity, float netAngularVelocity);

        public delegate void MouseUpdateEvent(Vector2 globalMousePos);

        public event TransformUpdateEvent OnTransformUpdate;

        public event PhysicsUpdateEvent OnPhysicsUpdate;

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

        public void UnpackPosition(NetIncomingMessage msg)
        {
            NetData.NetPosition = msg.ReadVector2();
            NetData.NetRotation = Quaternion.Euler(0, 0, msg.ReadFloat());

            if (OnTransformUpdate != null)
                OnTransformUpdate(NetData.NetPosition, NetData.NetRotation);
        }

        public NetOutgoingMessage PackPosition(NetOutgoingMessage msg)
        {
            msg.Write((Vector2)AttachedPlayer.transform.position);
            msg.Write(AttachedPlayer.transform.rotation.eulerAngles.z);

            return msg;
        }

        public void UnpackPhysics(NetIncomingMessage msg)
        {
            NetData.NetVelocity = msg.ReadVector2();
            NetData.NetAngularVelocity = msg.ReadFloat();

            if (OnPhysicsUpdate != null)
                OnPhysicsUpdate(NetData.NetVelocity, NetData.NetAngularVelocity);
        }

        public NetOutgoingMessage PackPhysics(NetOutgoingMessage msg)
        {
            var rigidBody = AttachedPlayer.GetComponentInChildren<Rigidbody2D>();

            msg.Write(rigidBody.velocity);
            msg.Write(rigidBody.angularVelocity);

            return msg;
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
