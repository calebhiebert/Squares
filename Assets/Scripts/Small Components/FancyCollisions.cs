using UnityEngine;

namespace Assets.Scripts.Small_Components
{
    public class FancyCollisions : MonoBehaviour {
        void OnCollisionEnter2D(Collision2D other)
        {
            var rigidbody = GetComponent<Rigidbody2D>();
            var force = rigidbody.velocity.magnitude;
            ImpactSystem.Current.MakeImpact(ImpactSystem.Current.Hit, other.contacts[0].point, force);
        }
    }
}
