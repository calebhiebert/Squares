using UnityEngine;
using System.Collections;

public class FollowPlayer : MonoBehaviour
{

    public Transform target;
    public float smoothing;
	
	void Update () {
	    if (target != null)
	    {
	        /*var pos = Vector3.Lerp(transform.position, target.position, Time.deltaTime*smoothing);*/
	        var pos = target.position;
	        pos.z = -10;
	        transform.position = pos;
	    }
	}
}
