using System;
using System.Collections;
using System.Linq.Expressions;
using System.Security.AccessControl;
using System.Collections.Generic;
using Lidgren.Network;
using UnityEngine;

namespace Assets.Scripts
{
    public class PlayerController : MonoBehaviour
    {
        public static List<PlayerController> ActivePlayers = new List<PlayerController>();

        public float SmoothingAmount;

        public float HorizontalMoveForce;
        public float JumpForce;

        public LayerMask JumpMask;
        public ParticleSystem JumpSystem;
        public TextMesh NameDisplay;

        private Rigidbody2D _rigidbody;
        private BoxCollider2D _collider;
        private readonly Queue<HistoryEntry> _historyQueue = new Queue<HistoryEntry>(); 

        private int _jumpAllowance;

        void Start ()
        {
            _collider = GetComponent<BoxCollider2D>();
            _rigidbody = GetComponent<Rigidbody2D>();

            if (NetPlayer != null)
            {
                NetPlayer.PlayerTransform = transform;
                NameDisplay.text = NetPlayer.Name;

                NetPlayer.OnNameChange += s => NameDisplay.text = s;

                NetPlayer.OnTransformUpdate += OnTransformUpdate;

                NetPlayer.OnJump += Jump;

                NetPlayer.OnControlsUpdate += controls => CurrentControls = controls;
            }

            ActivePlayers.Add(this);
        }

        private void OnTransformUpdate(Vector2 pos, float rot, Vector2 vel, float angVel, Vector2 mouse)
        {
            if(!NetworkingServer.IsServer)
            {
                TargetPosition = pos;
                TargetRotation = transform.rotation = Quaternion.Euler(0, 0, rot);
                TargetVelocity = vel;
                TargetAngularVelocity = angVel;
            }

            WorldMousePos = mouse;
        }

        void Update()
        {
            if (_collider.IsTouchingLayers(JumpMask))
                _jumpAllowance = 1;

            if (NetPlayer != null && NetPlayer.Local)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    var jumpMsg = NetworkingClient.Current.Client.CreateMessage();
                    jumpMsg.Write(PacketType.PlayerJump);
                    NetworkingClient.Current.Client.SendMessage(jumpMsg, NetDeliveryMethod.Unreliable);
                }
            }

            /*var he = new HistoryEntry
            {
                pos = transform.position,
                rot = transform.rotation,
                vel = _rigidbody.velocity,
                angVel = _rigidbody.angularVelocity
            };

            _historyQueue.Enqueue(he);*/
        }

        void FixedUpdate()
        {
            if (!NetworkingServer.IsServer)
            {
                transform.position = Vector2.Lerp(transform.position, TargetPosition, Time.deltaTime * SmoothingAmount);
                transform.rotation = Quaternion.Lerp(transform.rotation, TargetRotation, Time.deltaTime*SmoothingAmount * 2);

                _rigidbody.velocity = Vector2.Lerp(_rigidbody.velocity, TargetVelocity, Time.deltaTime*SmoothingAmount);
                _rigidbody.angularVelocity = Mathf.Lerp(_rigidbody.angularVelocity, TargetAngularVelocity,
                    Time.deltaTime*SmoothingAmount * 2);
            }

            if (CurrentControls != null)
                Move(CurrentControls);
        }

        private void Jump()
        {
            _rigidbody.AddForce(new Vector2(0, JumpForce));
            _jumpAllowance--;

            if(JumpSystem != null)
                JumpSystem.Play();
        }

        void Move(Controls controls)
        {
            float horizontal = (controls.Left ? -HorizontalMoveForce : 0) + (controls.Right ? HorizontalMoveForce : 0);
            _rigidbody.AddForce(new Vector2(horizontal, 0));
        }

        void OnDestroy()
        {
            ActivePlayers.Remove(this);
        }

        public struct HistoryEntry
        {
            public Vector2 pos;
            public Quaternion rot;
            public Vector2 vel;
            public float angVel;
        }

        public NetworkPlayer NetPlayer { get; set; }

        public Controls CurrentControls { get; set; }

        public Vector2 TargetPosition { get; set; }
        public Vector2 TargetVelocity { get; set; }
        public Quaternion TargetRotation { get; set; }
        public float TargetAngularVelocity { get; set; }

        public Vector2 WorldMousePos { get; set; }

        public bool CanJump
        {
            get { return _jumpAllowance > 0; }
        }
    }
}
