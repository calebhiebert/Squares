using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.Networking;
using JetBrains.Annotations;
using Lidgren.Network;
using UnityEngine;

namespace Assets.Scripts.PlayerModules
{
    public class PlayerGroundPoundModule : PlayerModule
    {
        private Rigidbody2D _rigidbody;

        public CircleCollider2D PoundCollider;
        public LayerMask PoundMask;
        public ParticleSystem[] ParticleSystems;
        public ParticleSystem[] FinisherSystems;

        public float PoundForce;

        [SerializeField]
        private bool _pounding;

        public override void Start()
        {
            base.Start();
            _rigidbody = GetComponentInParent<Rigidbody2D>();
        }

        public override void OnOwnerUpdate()
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                SendPoundCommand();
            }
        }

        public override void OnEveryoneUpdate()
        {
            if (_pounding)
            {
                _rigidbody.AddForce(new Vector2(0, -PoundForce));
            }

            if (_pounding && GetComponentInParent<BoxCollider2D>().IsTouchingLayers(PoundMask))
            {
                EndPound();
            }
        }

        void EndPound()
        {
            _pounding = false;

            foreach (var system in ParticleSystems)
            {
                system.Stop();
            }

            foreach (var system in FinisherSystems)
            {
                system.Play();
            }
        }

        public void DoPound()
        {
            _pounding = true;

            foreach (var system in ParticleSystems)
            {
                system.Play();
            }
        }

        private void SendPoundCommand()
        {
            DoPound();

            Client.Current.SendMessage(Client.Current.CreateMessage(NetObject.Type.PlayerGroundPound), NetDeliveryMethod.Unreliable);
        }

        public bool CanPound
        {
            get { return !PoundCollider.IsTouchingLayers(PoundMask) && !_pounding; }
        }
    }
}
