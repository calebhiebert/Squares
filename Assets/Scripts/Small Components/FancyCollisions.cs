using Assets.Scripts.Networking;
using UnityEngine;

namespace Assets.Scripts.Small_Components
{
    public class FancyCollisions : MonoBehaviour
    {

        private NetPlayer _owner;

        void Start()
        {
            _owner = GetComponentInParent<PlayerController>().NetPlayer;
        }

        void OnCollisionEnter2D(Collision2D other)
        {
            var rigidbody = GetComponent<Rigidbody2D>();
            var force = rigidbody.velocity.magnitude;
            ImpactSystem.Current.MakeImpact(ImpactSystem.Current.Hit, other.contacts[0].point, force, _owner.LighterColor);
        }
    }
}
