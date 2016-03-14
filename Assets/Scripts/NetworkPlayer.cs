using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class NetworkPlayer
    {
        public delegate void NameChangeEvent(string name);
        public delegate void TransformUpdateEvent(Vector2 pos, float rot, Vector2 vel, float angVel, Vector2 mouse);
        public delegate void JumpEvent();
        public delegate void ControlsUpdateEvent(Controls c);

        public event NameChangeEvent OnNameChange;
        public event JumpEvent OnJump;
        public event TransformUpdateEvent OnTransformUpdate;
        public event ControlsUpdateEvent OnControlsUpdate;

        public Transform PlayerTransform;

        private string _name;

        public byte PlayerId;
        public bool Local;

        public NetworkPlayer(string name, byte playerId)
        {
            _name = name;
            PlayerId = playerId;
        }

        public NetworkPlayer(string name, byte playerId, bool local)
        {
            _name = name;
            PlayerId = playerId;
            Local = local;
        }

        public NetOutgoingMessage MakeTransformUpdate(NetOutgoingMessage message)
        {
            message.Write((Vector2)PlayerTransform.position);
            message.Write(PlayerTransform.rotation.eulerAngles.z);

            var rigidbody = PlayerTransform.GetComponent<Rigidbody2D>();

            message.Write(rigidbody.velocity);
            message.Write(rigidbody.angularVelocity);

            Vector2 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            message.Write(mouse);

            return message;
        }

        public void ApplyTransformUpdate(NetIncomingMessage message)
        {
            if (OnTransformUpdate != null)
            {
                OnTransformUpdate(
                    message.ReadVector2(), 
                    message.ReadFloat(), 
                    message.ReadVector2(), 
                    message.ReadFloat(),
                    message.ReadVector2());
            }
        }

        public void Jump()
        {
            if (OnJump != null)
                OnJump();
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;

                if (OnNameChange != null)
                    OnNameChange(value);
            }
        }

        public NetOutgoingMessage MakeControlsUpdate(NetOutgoingMessage msg)
        {
            var c = Controls.Poll();

            msg.Write(c.Left);
            msg.Write(c.Right);
            msg.Write(c.WorldMouseCoord);

            return msg;
        }

        public void ApplyControlsUpdate(NetIncomingMessage msg)
        {
            Controls c = new Controls
            {
                Left = msg.ReadBoolean(),
                Right = msg.ReadBoolean(),
                WorldMouseCoord = msg.ReadVector2()
            };

            if (OnControlsUpdate != null)
            {
                OnControlsUpdate(c);
            }
        }

        public int TimestepsBehind
        {
            get
            {
                var t = NetworkingClient.Current.ServerConnection.AverageRoundtripTime;
                return (int) (t/Time.fixedDeltaTime);
            }
        }

        public float Ping
        {
            get { return NetworkingClient.Current.ServerConnection.AverageRoundtripTime; }
        }
    }
}
