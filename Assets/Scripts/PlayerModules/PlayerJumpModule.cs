using Assets.Scripts.Networking;
using Lidgren.Network;
using UnityEngine;

namespace Assets.Scripts.PlayerModules
{
    public class PlayerJumpModule : PlayerModule
    {
        public int NumberOfJumps;
        public float JumpForce;

        public ParticleSystem JumpSystem;

        private Rigidbody2D _rigidbody;

        private int _jumpAllowance;

        public override void Start ()
        {
            base.Start();

            _rigidbody = GetComponent<Rigidbody2D>();
        }

        public override void OnOwnerUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                var jumpSignal = Client.Current.CreateMessage(NetObject.Type.PlayerJump);
                Client.Current.SendMessage(jumpSignal, NetDeliveryMethod.Unreliable);
            }
        }

        public void DoJump()
        {
            _rigidbody.AddForce(new Vector2(0, JumpForce));
            _jumpAllowance--;

            JumpSystem.Play();
        }

        public bool CanJump
        {
            get { return _jumpAllowance > 0; }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            _jumpAllowance = NumberOfJumps;
        }
    }
}
