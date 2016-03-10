using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour
{
    private BoxCollider2D _collider;
    private Rigidbody2D _rigidbody;

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
}
