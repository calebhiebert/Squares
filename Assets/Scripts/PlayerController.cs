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

        public float HorizontalMoveForce;
        public int CopycatFrameDelay;

        public TextMesh NameDisplay;

        private Rigidbody2D _rigidbody;
        private Queue<HistoryEntry> _historyQueue = new Queue<HistoryEntry>();
        private Controls _lastFrameControls = new Controls().Poll();

        private GameObject _dummy;

        private void Start ()
        {
            _rigidbody = GetComponent<Rigidbody2D>();

            NameDisplay.text = NetPlayer.Name;

            ActivePlayers.Add(this);

            CurrentControls = new Controls
            {
                Left = false,
                Right = false,
                WorldMouseCoord = Vector2.zero
            };

            NetPlayer.OnMovementData += OnMovementUpdate;

            _dummy = new GameObject("Dummy");
            _dummy.AddComponent<SpriteRenderer>().sprite = GetComponentInChildren<SpriteRenderer>().sprite;
            _dummy.GetComponent<SpriteRenderer>().color = Color.gray;
        }

        private void OnMovementUpdate(Vector2 netPosition, Quaternion netRotation, Vector2 netVelocity, float netAngularVelocity)
        {
            transform.position = netPosition;
            transform.rotation = netRotation;
            _rigidbody.velocity = netVelocity;
            _rigidbody.angularVelocity = netAngularVelocity;

            _dummy.transform.position = netPosition;
            _dummy.transform.rotation = netRotation;
        }

        private void FixedUpdate()
        {
            if (NetPlayer.IsLocal)
            {
                CurrentControls = CurrentControls.Poll();

                if (_lastFrameControls.Right != CurrentControls.Right || _lastFrameControls.Left != CurrentControls.Left)
                {
                    NetPlayer.SendControls(CurrentControls);
                    _lastFrameControls = CurrentControls;
                }
            }

            if (NetworkMain.IsServer)
            {
                if (transform.position.y <= -13)
                {
                    Kill();
                }
            }

            if (CurrentControls != null)
                Move(CurrentControls);
        }

        private void Kill()
        {
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            _rigidbody.velocity = Vector2.zero;
            _rigidbody.angularVelocity = 0;
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

        public void Remove()
        {
            ImpactSystem.Current.MakeImpact(ImpactSystem.Current.Disconnect, transform.position, 0,
                NetPlayer.LighterColor);

            Destroy(_dummy.gameObject);

            Destroy(gameObject);
        }

        public NetPlayer NetPlayer { get; set; }

        public Controls CurrentControls { get; set; }

        public Vector2 WorldMousePos { get; set; }
    }
}
