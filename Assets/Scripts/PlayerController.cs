using System;
using System.Collections;
using System.Linq.Expressions;
using System.Security.AccessControl;
using System.Collections.Generic;
using Assets.Scripts.Networking;
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
        public int CopycatFrameDelay;

        public LayerMask JumpMask;
        public ParticleSystem JumpSystem;
        public TextMesh NameDisplay;

        private Rigidbody2D _rigidbody;
        private BoxCollider2D _collider;
        private Queue<HistoryEntry> _historyQueue = new Queue<HistoryEntry>();
        private Controls _lastFrameControls = Controls.Poll();

        private int _jumpAllowance;

        private GameObject _dummy;

        void Start ()
        {
            _collider = GetComponent<BoxCollider2D>();
            _rigidbody = GetComponent<Rigidbody2D>();

            NameDisplay.text = NetPlayer.Name;

            NetPlayer.OnTransformUpdate += NetPosReceived;

            NetPlayer.OnPhysicsUpdate += NetPhysicsReceived;

            NetPlayer.OnMouseUpdate += NetMouseReceived;

            ActivePlayers.Add(this);

            CurrentControls = new Controls
            {
                Left = false,
                Right = false,
                WorldMouseCoord = Vector2.zero
            };

            _dummy = new GameObject("Dummy");
            _dummy.AddComponent<SpriteRenderer>().sprite = GetComponentInChildren<SpriteRenderer>().sprite;
            _dummy.GetComponent<SpriteRenderer>().color = Color.gray;
        }

        private void NetMouseReceived(Vector2 globalMousePos)
        {
            WorldMousePos = globalMousePos;
        }

        private void NetPhysicsReceived(Vector2 netVelocity, float netAngularVelocity)
        {
            TargetVelocity = netVelocity;
            TargetAngularVelocity = netAngularVelocity;
        }

        private void NetPosReceived(Vector2 netPosition, Quaternion netRotation)
        {
            TargetPosition = netPosition;
            TargetRotation = netRotation;
        }

        void Update()
        {
            if (_collider.IsTouchingLayers(JumpMask))
                _jumpAllowance = 1;

            var he = new HistoryEntry
            {
                pos = transform.position,
                rot = transform.rotation,
                vel = _rigidbody.velocity,
                angVel = _rigidbody.angularVelocity
            };

            _historyQueue.Enqueue(he);

            if (_historyQueue.Count > CopycatFrameDelay)
            {
                var dq = _historyQueue.Dequeue();

                _dummy.transform.position = dq.pos;
                _dummy.transform.rotation = dq.rot;
            }
        }

        void FixedUpdate()
        {
            if (!NetworkMain.IsServer)
            {
                transform.position = Vector2.Lerp(transform.position, TargetPosition, Time.deltaTime * SmoothingAmount);
                transform.rotation = Quaternion.Lerp(transform.rotation, TargetRotation, Time.deltaTime*SmoothingAmount * 2);

                _rigidbody.velocity = Vector2.Lerp(_rigidbody.velocity, TargetVelocity, Time.deltaTime*SmoothingAmount);
                _rigidbody.angularVelocity = Mathf.Lerp(_rigidbody.angularVelocity, TargetAngularVelocity,
                    Time.deltaTime*SmoothingAmount * 2);
            }

            if (NetPlayer.IsLocal)
            {
                CurrentControls = Controls.Poll();

                if (_lastFrameControls.Right != CurrentControls.Right || _lastFrameControls.Left != CurrentControls.Left)
                {
                    var controls = NetPlayer.CreateMessage(NetObject.Type.PlayerControlsUpdate);

                    controls = NetPlayer.PackControls(controls);

                    Client.Current.SendMessage(controls, NetDeliveryMethod.ReliableSequenced, 1);

                    _lastFrameControls = CurrentControls;
                }
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

        public NetPlayer NetPlayer { get; set; }

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
