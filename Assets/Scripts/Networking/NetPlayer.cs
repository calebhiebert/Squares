using Lidgren.Network;
using UnityEngine;

namespace Assets.Scripts.Networking
{
    public class NetPlayer : NetObject
    {
        // event delegates
        public delegate void MovementDataEvent(Vector2 netPosition, Quaternion netRotation, Vector2 netVelocity, float netAngularVelocity);

        public delegate void MouseUpdateEvent(Vector2 globalMousePos);

        public delegate void ControlsUpdateEvent(bool left, bool right);

        public event ControlsUpdateEvent OnControlsUpdate;

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

        // the color of this player
        public Color Color;

        // remaining lives
        public int Lives;

        // hp
        public int Hp;

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
            msg.Write((Vector2)AttachedPlayer.transform.position);

            float rot = AttachedPlayer.transform.rotation.eulerAngles.z;

            msg.Write(rot);

            // write player rotation
            
            // get rigidbody
            var rigidBody = AttachedPlayer.GetComponentInChildren<Rigidbody2D>();

            //write physics details
            msg.Write(rigidBody.velocity);

            msg.Write(rigidBody.angularVelocity);

            return msg;
        }

        public void UnpackMovementData(NetIncomingMessage msg)
        {
            var pos = msg.ReadVector2();
            var rot = msg.ReadFloat();
            var vel = msg.ReadVector2();
            var ang = msg.ReadFloat();

            NetData = new TransformData
            {
                NetPosition = pos,
                NetRotation = Quaternion.Euler(0, 0, rot),
                NetVelocity = vel,
                NetAngularVelocity = ang
            };

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
            msg.Write((Vector2)AttachedPlayer.transform.position);
            msg.Write(Name);

            msg.Write(new Vector3(Color.r, Color.g, Color.b));

            return msg;
        }

        public void SendControls(Controls c)
        {
            var cont = CreateMessage(Type.PlayerControlsUpdate);

            cont.Write(c.Left);
            cont.Write(c.Right);

            Client.Current.SendMessage(cont, NetDeliveryMethod.ReliableSequenced, 1);
        }

        public void UnpackControls(NetIncomingMessage msg)
        {
            var l = msg.ReadBoolean();
            var r = msg.ReadBoolean();

            if (OnControlsUpdate != null)
                OnControlsUpdate(l, r);
        }

        public Color LighterColor
        {
            get
            {
                return new Color(
                    Color.r + NetworkMain.Current.ColorLightnessFactor, 
                    Color.g + NetworkMain.Current.ColorLightnessFactor, 
                    Color.b + NetworkMain.Current.ColorLightnessFactor
                    );
            }
        }
    }
}
