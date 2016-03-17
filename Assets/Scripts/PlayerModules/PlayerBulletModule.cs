using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.Networking;
using Lidgren.Network;
using UnityEngine;

namespace Assets.Scripts.PlayerModules
{
    class PlayerBulletModule : PlayerModule
    {
        public GameObject BulletPrefab;

        public float ReloadTime;

        private float _lastShot;

        public override void OnOwnerUpdate()
        {
            if (Input.GetMouseButtonDown(0))
            {
                SendBulletMessage();
            }
        }

        public void CreateBullet(Vector2 origin, Vector2 direction)
        {
            var bullet = (GameObject) Instantiate(BulletPrefab, origin, direction.Vector2Quaternion());
            Physics2D.IgnoreCollision(bullet.GetComponent<BoxCollider2D>(), GetComponentInParent<BoxCollider2D>());

            _lastShot = Time.time;
        }

        private void SendBulletMessage()
        {
            var msg = Client.Current.CreateMessage(NetObject.Type.PlayerShootBullet);

            var origin = (Vector2)transform.position;
            var direction = (Vector2)(Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.parent.position).normalized;

            msg.Write(origin);
            msg.Write(direction);

            Client.Current.SendMessage(msg, NetDeliveryMethod.Unreliable);
        }

        public bool CanShoot
        {
            get { return Time.time > _lastShot + ReloadTime; }
        }
    }
}
