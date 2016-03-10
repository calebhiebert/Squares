using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

    Rigidbody2D _rigidBody;
    CircleCollider2D _jumpCollider;
    public LayerMask jumpMask;
    public float moveForce, jumpForce;
    public Color color;

	// Use this for initialization
	void Start () {
        _rigidBody = GetComponent<Rigidbody2D>();
        _jumpCollider = GetComponent<CircleCollider2D>();
	}
	
	// Update is called once per frame
	void Update () {
        float horizontal = Input.GetAxis("Horizontal");

        _rigidBody.AddForce(new Vector2(horizontal * moveForce, 0));

        if(Input.GetKeyDown(KeyCode.Space) && _jumpCollider.IsTouchingLayers(jumpMask))
        {
            _rigidBody.AddForce(new Vector2(0, jumpForce));
        }

        foreach(var spr in GetComponentsInChildren<SpriteRenderer>())
        {
            spr.color = color;
        }
	}
}
