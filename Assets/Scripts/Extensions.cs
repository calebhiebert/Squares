using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.Networking;
using UnityEngine;

namespace Assets.Scripts
{
    public static class Extensions
    {
        public static float LookDirection(this Transform transform)
        {
            var diff = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            diff.Normalize();
            return Mathf.Atan2(diff.y, diff.x)*Mathf.Rad2Deg - 90;
        }

        public static Quaternion Vector2Quaternion(this Vector2 vector)
        {
            return Quaternion.Euler(0, 0, Mathf.Atan2(vector.y, vector.x)*Mathf.Rad2Deg - 90);
        }

        public static void AddExplosionForce(this Rigidbody2D body, float expForce, Vector3 expPosition, float expRadius)
        {
            if (body == null)
                Debug.Log("Body Was Null");

            if (body != null)
            {
                var dir = body.transform.position - expPosition;

                var calc = 1 - (dir.magnitude/expRadius);
                if (calc <= 0)
                {
                    calc = 0;
                }

                body.AddForce(dir.normalized*expForce*calc);
            }
        }

        public static Vector2 Abs(this Vector2 vector)
        {
            return new Vector2(Mathf.Abs(vector.x), Mathf.Abs(vector.y));
        }

        public static void ApplyDamage(this GameObject playerObject, int dmg, Vector2 dmgSource)
        {
            if(playerObject.tag != "Player")
                return;

            var controller = playerObject.GetComponentInParent<PlayerController>();

            ImpactSystem.Current.MakeDamageIndicator(playerObject.transform.position, dmg);

            if (NetworkMain.IsServer)
            {
                controller.NetPlayer.ExplosionForceModifier += dmg;
            }
        }
    }
}
