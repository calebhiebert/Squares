using UnityEngine;

namespace Assets.Scripts.Small_Components
{
    /// <summary>
    /// Place this on an object to make it ignore its parent's rotation
    /// </summary>
    public class NullifyParentRotation : MonoBehaviour {

        /// <summary>
        /// Late Update
        /// called after update, to make sure the parent has had time to move
        /// </summary>
        void LateUpdate ()
        {
            transform.localRotation = Quaternion.Euler(0, 0, transform.parent.rotation.eulerAngles.z*-1);
        }
    }
}
