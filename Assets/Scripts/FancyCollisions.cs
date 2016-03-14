using UnityEngine;
using System.Collections;

public class FancyCollisions : MonoBehaviour {
    void OnCollisionEnter2D(Collision2D other)
    {
        var rigidbody = GetComponent<Rigidbody2D>();
        var force = rigidbody.velocity.magnitude;
        ImpactSystem.current.MakeImpact(ImpactSystem.current.hit, other.contacts[0].point, force);
    }
}
