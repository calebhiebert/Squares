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
        private BoxCollider2D _collider;

        public CircleCollider2D PoundCollider;
        public LayerMask PoundMask;
        public ParticleSystem[] ParticleSystems;
        public ParticleSystem[] FinisherSystems;

        public float PoundForceRadius;
        public float PoundImpactForce;
        public float PoundForce;

        [SerializeField]
        private bool _pounding;

        public override void Start()
        {
            base.Start();
            _rigidbody = GetComponentInParent<Rigidbody2D>();
            _collider = GetComponentInParent<BoxCollider2D>();

            Owner.AttachedPlayer.OnCollisionEnter += OnCollision;

            foreach (var sys in FinisherSystems)
            {
                sys.startColor = Owner.LighterColor;
            }

            foreach (var sys in ParticleSystems)
            {
                sys.startColor = Owner.LighterColor;
            }
        }

        private void OnCollision(Collision2D col)
        {
            if (_pounding)
            {
                EndPound();

                if (col.gameObject.tag == "Player")
                {
                    var p = col.gameObject.GetComponentInParent<PlayerController>();

                    Debug.Log("Direct hit to the face on " + p.NetPlayer.Name);

                    if(Owner.IsLocal)
                        AudioSource.PlayClipAtPoint(NetworkMain.Current.Ding, transform.position);

                    if (NetworkMain.IsServer)
                        p.NetPlayer.ExplosionForceModifier += 150;
                }
            }
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

            foreach (var d in Physics2D.OverlapCircleAll(transform.position, PoundForceRadius))
            {
                if (d.attachedRigidbody != null && d.attachedRigidbody != _rigidbody && d.gameObject.tag == "Player")
                {
                    d.GetComponentInParent<PlayerController>().AddForce(PoundImpactForce, transform.position, PoundForceRadius);
                }
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
            Client.Current.SendMessage(Client.Current.CreateMessage(NetObject.Type.PlayerGroundPound), NetDeliveryMethod.Unreliable);
        }

        public bool CanPound
        {
            get { return !PoundCollider.IsTouchingLayers(PoundMask) && !_pounding; }
        }
    }
}
