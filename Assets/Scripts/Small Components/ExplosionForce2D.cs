using UnityEngine;

namespace Assets.Scripts.Small_Components
{
    public class ExplosionForce2D : MonoBehaviour
    {
        public float Power;
        public float Radius;

        public static void AddExplosionForce (Rigidbody2D body, float expForce, Vector3 expPosition, float expRadius)
        {
            var dir = (body.transform.position - expPosition);
            float calc = 1 - (dir.magnitude / expRadius);
            if (calc <= 0) {
                calc = 0;		
            }

            body.AddForce (dir.normalized * expForce * calc);
        }


    }
}
