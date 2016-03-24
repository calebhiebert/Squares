using Assets.Scripts.Networking;
using UnityEngine;

namespace Assets.Scripts.Bullet_Weapon
{
    public class Projectile : MonoBehaviour
    {
        public float Force;
        public float ExplosionForce;
        public float ExplosionRadius;

        public NetPlayer Owner;

        public ParticleSystem Trail;

        void Start () {
	        GetComponent<Rigidbody2D>().AddRelativeForce(Vector2.up * Force);

            Destroy(gameObject, 5.0f);
        }

        void OnCollisionEnter2D(Collision2D other)
        {
            ImpactSystem.Current.MakeImpact(ImpactSystem.Current.Explode, transform.position, 1, Owner.LighterColor);

            Trail.transform.SetParent(null);
            Destroy(Trail.gameObject, 2);

            Destroy(gameObject);
            
            if (other.gameObject.tag == "Player")
            {
                var p = other.gameObject.GetComponentInParent<PlayerController>();

                if(Owner.IsLocal)
                    AudioSource.PlayClipAtPoint(NetworkMain.Current.Ding, transform.position);

                    p.gameObject.ApplyDamage(50, transform.position);
            }

            foreach (var obj in Physics2D.OverlapCircleAll(transform.position, ExplosionRadius))
            {
                if (obj.attachedRigidbody != null && obj.gameObject.tag == "Player")
                {
                    obj.GetComponentInParent<PlayerController>().AddForce(Force, transform.position, ExplosionRadius);

                    if (obj.attachedRigidbody != Owner.AttachedPlayer.GetComponent<Rigidbody2D>())
                    {
                        obj.gameObject.ApplyDamage(10, transform.position);
                    }
                }

            }
        }
    }
}
