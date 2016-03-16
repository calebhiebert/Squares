using UnityEngine;

namespace Assets.Scripts.Small_Components
{
    public class CameraController : MonoBehaviour
    {
        public Transform Target;
        public float SmoothAmount = 12;

        void Update () {
            if (Target != null)
            {
                var pos = Vector3.Lerp(transform.position, Target.position, Time.deltaTime * SmoothAmount);

                pos.z = -10;

                transform.position = pos;
            }
        }
    }
}
