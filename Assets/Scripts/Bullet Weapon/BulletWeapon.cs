using Assets.Scripts.Networking;
using Lidgren.Network;
using UnityEngine;

namespace Assets.Scripts.Bullet_Weapon
{
    public class BulletWeapon : MonoBehaviour
    {

        public GameObject BulletPrefab;
        public GameObject HexLoaderPrefab;

        public float ReloadTime;

        private float _lastShot = 0;
        private bool _local;

        void Start()
        {
            _local = GetComponentInParent<PlayerController>().NetPlayer.IsLocal;
        }

        void Update()
        {
            if(!_local)
                return;

            if (Input.GetMouseButtonDown(0) && Time.time > _lastShot + ReloadTime)
            {
                MakeBullet(transform.position, (Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized);
            }
        }

        public void SpawnBullet(Vector2 origin, Vector2 direction)
        {
            var bullet = Instantiate(BulletPrefab, origin, direction.Vector2Quaternion()) as GameObject;
            Physics2D.IgnoreCollision(bullet.GetComponent<BoxCollider2D>(), GetComponentInParent<BoxCollider2D>());

            _lastShot = Time.time;

            /*var loader = Instantiate(HexLoaderPrefab, transform.position,
                Quaternion.Euler(0, 0, Random.Range(-360f, 360f))) as GameObject;

            loader.GetComponent<HexLoader>().loadingTime = ReloadTime;

            loader.transform.position = Vector3.zero;

            loader.transform.SetParent(transform, false);*/
        }

        public void MakeBullet(Vector2 origin, Vector2 direction)
        {
            var bulletEvent = Client.Current.CreateMessage();

            bulletEvent.Write((byte) NetObject.Type.PlayerShootBullet, NetObject.IndentifierNumOfBits);
            bulletEvent.Write(origin);
            bulletEvent.Write(direction);

            Client.Current.SendMessage(bulletEvent, NetDeliveryMethod.ReliableUnordered);
        }
    }
}