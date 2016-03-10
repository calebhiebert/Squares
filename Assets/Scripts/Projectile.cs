using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour
{
    private BoxCollider2D _collider;
    private Rigidbody2D _rigidbody;
    private ParticleSystem _deathSystem;

    public float _shootForce;

	void Awake ()
	{
	    _collider = GetComponent<BoxCollider2D>();
	    _rigidbody = GetComponent<Rigidbody2D>();
	}

    void Start()
    {
        _rigidbody.AddRelativeForce(new Vector2(0, _shootForce));
    }

    void Explode()
    {
        if (_deathSystem != null)
        {
            _deathSystem.Play();
        }

        Destroy(gameObject);
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        Explode();
    }
}
