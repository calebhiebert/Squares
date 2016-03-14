using UnityEngine;

namespace Assets
{
    public class Projectile : MonoBehaviour
    {
        public float Force;
        public float ExplosionForce;
        public float ExplosionRadius;

        public ParticleSystem Trail;

        void Start () {
	        GetComponent<Rigidbody2D>().AddRelativeForce(Vector2.up * Force);

            Destroy(gameObject, 5.0f);
        }

        void OnCollisionEnter2D(Collision2D other)
        {
            ImpactSystem.current.MakeImpact(ImpactSystem.current.explode, transform.position, 1);

            Trail.transform.SetParent(null);
            Destroy(Trail.gameObject, 2);

            Destroy(gameObject);

            foreach (var obj in Physics2D.OverlapCircleAll(transform.position, ExplosionRadius))
            {
                if(obj.attachedRigidbody != null)
                    AddExplosionForce(obj.attachedRigidbody, ExplosionForce, transform.position, ExplosionRadius);
            }
        }

        public static void AddExplosionForce(Rigidbody2D body, float expForce, Vector3 expPosition, float expRadius)
        {
            var dir = (body.transform.position - expPosition);
            float calc = 1 - (dir.magnitude / expRadius);
            if (calc <= 0)
            {
                calc = 0;
            }

            body.AddForce(dir.normalized * expForce * calc);
        }
    }
}
