using UnityEngine;
using System.Collections;

// Projectile: This class goes on every bullet object when it is spawned
public class Projectile : MonoBehaviour
{
    // the rigidbody attached to the bullet
    private Rigidbody2D _rigidbody;

    // a particle system to be played when the bullet is removed
    public ParticleSystem _deathSystem;

    // the amount of force that will be applied to the bullet when it is spawned
    public float _shootForce;

    // called when the bullet is spawned
	void Awake ()
	{
        // get components from bullet
	    _rigidbody = GetComponent<Rigidbody2D>();
	}

    void Start()
    {
        // add the force to move the bullet
        _rigidbody.AddRelativeForce(new Vector2(0, _shootForce));
    }

    // called when the bullet dies
    void Explode()
    {
        // if a death particle system is defined, play it
        if (_deathSystem != null)
        {
            _deathSystem.Play();
        }

        // destroy the bullet
        Destroy(gameObject);

        // destroy the particle trail after a delay
        Destroy(GetComponentInChildren<ParticleSystem>().gameObject, 1.0f);

        // destroy the death system after a delay
        Destroy(_deathSystem.gameObject, 1.0f);

        // detach the death system from the bullet
        _deathSystem.gameObject.transform.SetParent(null, true);

        // detach the particle trail from the bullet so it stays after the bullet is deleted
        GetComponentInChildren<ParticleSystem>().gameObject.transform.SetParent(null, true);
    }

    // called when the bullet touches something
    void OnCollisionEnter2D(Collision2D other)
    {
        // call the death method
        Explode();
    }
}
