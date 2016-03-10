using UnityEngine;
using System.Collections;

public class PlayerCamera : MonoBehaviour {

    public Transform target;
    public float smoothing;

	// Use this for initialization
	void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {
	    if(target != null)
        {
            var pos = Vector3.Lerp(target.position, transform.position, smoothing * Time.deltaTime);
            pos.z = -10;
            transform.position = pos;
        }
	}
}
