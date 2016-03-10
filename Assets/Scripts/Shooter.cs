using UnityEngine;
using System.Collections;
using System.Security.Policy;

public class Shooter : MonoBehaviour
{
    // the projectile to spawn
    public GameObject projectile;

    // the transform to take projectile rotation from
    public Transform rotationFrom;

    // how much recoil to apply to the shooter
    public float recoil;

    // Called once per frame
	void Update () {

        // if the left mouse button is clicked
	    if (Input.GetMouseButtonDown(0))
	    {
            // spawn a new bullet
	        var bullet = Instantiate(projectile, transform.position, rotationFrom.rotation) as GameObject;

            // set the bullet not to collide with the square that spawned it
            Physics2D.IgnoreCollision(GetComponent<BoxCollider2D>(), bullet.GetComponent<BoxCollider2D>());

            // get the bullet's rigidbody
	        var rigidbody = GetComponent<Rigidbody2D>();

            // calculate and apply the recoil
            float myAngleInRadians = (rotationFrom.rotation.eulerAngles.z - 90) * Mathf.Deg2Rad;
            Vector2 angleVector = new Vector2(Mathf.Cos(myAngleInRadians), -Mathf.Sin(myAngleInRadians));
	        angleVector *= recoil;
	        angleVector.y *= -1;
            rigidbody.AddForce(angleVector);
	    }
	}
}
