using UnityEngine;
using System.Collections;

public class NullifyParentRotation : MonoBehaviour {
	void LateUpdate ()
	{
	    transform.localRotation = Quaternion.Euler(0, 0, transform.parent.rotation.eulerAngles.z*-1);
	}
}
