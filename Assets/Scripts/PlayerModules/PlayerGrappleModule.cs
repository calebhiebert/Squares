using UnityEngine;

namespace Assets.Scripts.PlayerModules
{
    public class PlayerGrappleModule : PlayerModule
    {
        public Rope GrappleRope;

        public Transform RaycastPoint;

        public LayerMask RopeMask;

        private bool _ropd;

        public override void Start()
        {
            base.Start();
        }

        public override void OnOwnerUpdate()
        {
            if (Input.GetMouseButtonDown(1))
            {
                if (_ropd)
                {
                    GrappleRope.ClearRope();
                    _ropd = false;
                }
                else
                {
                    MakeRope();
                    _ropd = true;
                }
            }

            if (_ropd && Input.mouseScrollDelta.y > 0)
            {
                GrappleRope.RemoveSegment();
            } else if (_ropd && Input.mouseScrollDelta.y < 0)
            {
                GrappleRope.AddSegment();
            }
        }

        private void MakeRope()
        {
            var hit = Physics2D.Raycast(RaycastPoint.position,
                (Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized, 60, RopeMask);

            if (hit.collider != null)
            {
                var ropeAttachPoint = new GameObject("Grapple Hit Point");

                ropeAttachPoint.transform.position = hit.point;

                ropeAttachPoint.AddComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePosition;

                GrappleRope.MakeRope(ropeAttachPoint.GetComponent<Rigidbody2D>(), GetComponent<Rigidbody2D>());
            }
        }
    }
}
