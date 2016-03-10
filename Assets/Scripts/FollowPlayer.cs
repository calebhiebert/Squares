using UnityEngine;
using System.Collections;

public class FollowPlayer : MonoBehaviour
{
    // the target for the camera to follow
    public Transform target;
	

    // called every frame
	void Update () {
	    if (target != null)
	    {
            // get the position of the target
	        var pos = target.position;

            // move the camera back in z space so it can see everything
	        pos.z = -10;

            // set the camera's position
	        transform.position = pos;
	    }
	}
}
