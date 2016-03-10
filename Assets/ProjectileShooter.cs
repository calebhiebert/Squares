using UnityEngine;
using System.Collections;
using System.Security.Policy;

public class ProjectileShooter : MonoBehaviour
{
    // the projectile to spawn
    public GameObject projectile;
    public Transform rotationFrom;
    public float recoil;

	void Start () {
	    
	}

	void Update () {
	    if (Input.GetMouseButtonDown(0))
	    {
	        var bullet = Instantiate(projectile, transform.position, rotationFrom.rotation) as GameObject;
            Physics2D.IgnoreCollision(GetComponent<BoxCollider2D>(), bullet.GetComponent<BoxCollider2D>());

	        var rigidbody = GetComponent<Rigidbody2D>();

            float myAngleInRadians = rotationFrom.rotation.eulerAngles.z * Mathf.Deg2Rad;
            Vector2 angleVector = new Vector2(
                Mathf.Cos(myAngleInRadians),
                -Mathf.Sin(myAngleInRadians));

            rigidbody.AddForce(angleVector * recoil);
	    }
	}
}
