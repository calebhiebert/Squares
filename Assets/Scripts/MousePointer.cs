using UnityEngine;
using System.Collections;

public class MousePointer : MonoBehaviour {
	
	// Update is called once per frame
	void Update () {
        Vector3 diff = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.parent.position;
        diff.Normalize();

        float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;

        rot_z -= transform.parent.rotation.eulerAngles.z;

        transform.localRotation = Quaternion.Euler(0f, 0f, rot_z - 90);
    }
}
