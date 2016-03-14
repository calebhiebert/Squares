using UnityEngine;

namespace Assets.Scripts
{
    public class FaceMouse : MonoBehaviour
    {
        private PlayerController _player;

        void Start()
        {
            _player = GetComponentInParent<PlayerController>();
        }

        void LateUpdate ()
        {
            if (_player != null && _player.NetPlayer != null && !_player.NetPlayer.Local)
            {
                var diff = _player.WorldMousePos - (Vector2)transform.position;
                diff.Normalize();
                transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(diff.y, diff.x)*Mathf.Rad2Deg - 90);
            }
            else
            {
                var diff = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
                diff.Normalize();
                transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(diff.y, diff.x)*Mathf.Rad2Deg - 90);
            }
        }
    }
}
