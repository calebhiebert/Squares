using UnityEngine;
using System.Collections;

// This class will rotate an object to point at the mouse
public class MousePointer : MonoBehaviour {
	
	// LateUpdate is called once per frame after Update
	void LateUpdate () {

        // convert the mouse's screen position into world units and calculate the difference between the target
        Vector3 diff = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.parent.position;

        diff.Normalize();
        
        // calculate the rotation in degrees
        float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;

        // nullify the rotation from the parent cube
        rot_z -= transform.parent.rotation.eulerAngles.z;

        // set the rotation
        transform.localRotation = Quaternion.Euler(0f, 0f, rot_z - 90);
    }
}
