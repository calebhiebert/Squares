using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class Rope : MonoBehaviour
    {
        public RopeRenderer RopeRenderer;

        public Sprite SegmentSprite;

        public float SegmentsPerUnit;

        // how thick the rope is
        public float SegmentWidth;

        // how heavy each segment of rope is
        public float RopeSegmentMass;

        // how much angular drag is applied to each rope segment
        public float AngularDrag;

        // how much drag is applied to each rope segment, usually keep this at 0, anything higher makes weird results
        public float Drag;

        // the array of generated segments
        private List<GameObject> _segments;

        private float _segmentHeight;

        /// <summary>
        /// Takes two rigid bodies, it will attach each end of the rope to these
        /// </summary>
        /// <param name="beginning"></param>
        /// <param name="end"></param>
        public void MakeRope(Rigidbody2D beginning, Rigidbody2D end)
        {
            var distance = Vector2.Distance(beginning.transform.position, end.transform.position);

            var segments = (int) (Vector2.Distance(beginning.transform.position, end.transform.position) * SegmentsPerUnit);

            // calculate how tall each segment should be
            _segmentHeight = distance / segments;

            // create a new array to hold all the segment gameobjects
            _segments = new List<GameObject>();

            /*
            create the initial segment
            this is done outside of the loop because this segment has some special properties
            */
            _segments.Add(CreateSegment(new Vector2(SegmentWidth, _segmentHeight)));

            var hinge = _segments[0].GetComponent<HingeJoint2D>();

            hinge.connectedBody = beginning;

            hinge.anchor = Vector2.zero;
            hinge.connectedAnchor = Vector2.zero;

            // create the rest of the segments
            for (var i = 1; i < segments; i++)
            {
                // create the segment, and immidiately add it to the array
                _segments.Add(CreateSegment(new Vector2(SegmentWidth, _segmentHeight)));

                var joint = _segments[i].GetComponent<HingeJoint2D>();

                // set the connected rigidbody to the last one created
                joint.connectedBody = _segments[i - 1].GetComponent<Rigidbody2D>();

                var dist = _segments[i].GetComponent<DistanceJoint2D>();

                dist.connectedBody = joint.connectedBody;

                // allow the distance to change a little, keeps the rope more stable
                dist.maxDistanceOnly = true;

                var pos = Vector2.Lerp(beginning.transform.position, end.transform.position, (float) i/segments);

                // set the new segment's position
                _segments[i].transform.position = pos;
            }

            // if there is a RopeRenderer, set it up
            if (RopeRenderer != null)
            {
                RopeRenderer.RopeSegments = new Transform[_segments.Count];

                RopeRenderer.LineRenderer.SetWidth(SegmentWidth, SegmentWidth);

                for (var i = 0; i < _segments.Count; i++)
                {
                    RopeRenderer.RopeSegments[i] = _segments[i].transform;
                }
            }

            _segments[_segments.Count - 1].AddComponent<HingeJoint2D>().connectedBody = end;
        }

        /// <summary>
        /// Creates a new gameobject and attaches all the required components
        /// </summary>
        /// <param name="segmentSize">How big the segment will be</param>
        /// <returns>The created GameObject</returns>
        private GameObject CreateSegment(Vector2 segmentSize)
        {
            // create a new GameObject
            var newRope = new GameObject("Rope Segment") {layer = 9};

            // set the parent to this object, so the unity editor wont get cluttered
            newRope.transform.SetParent(transform, false);

            /* CREATE THE BOX COLLIDER */
            var boxCollider = newRope.AddComponent<BoxCollider2D>();

            boxCollider.size = segmentSize;

            /* CREATE THE RIGIDBODY */
            var rigidBody = newRope.AddComponent<Rigidbody2D>();

            rigidBody.mass = RopeSegmentMass;

            rigidBody.angularDrag = AngularDrag;

            rigidBody.drag = Drag;

            rigidBody.interpolation = RigidbodyInterpolation2D.Interpolate;

            /* CREATE THE HINGE JOINT */
            var joint = newRope.AddComponent<HingeJoint2D>();

            joint.autoConfigureConnectedAnchor = false;

            // set the anchors so the segments are evenly spaced
            joint.anchor = new Vector2(0, segmentSize.y / 2);
            joint.connectedAnchor = new Vector2(0, -segmentSize.y / 2);

            /* CREATE THE DISTANCE JOINT */
            var distance = newRope.AddComponent<DistanceJoint2D>();

            distance.distance = segmentSize.y;

            /* CREATE SPRITE RENDERER */
            var spr = newRope.AddComponent<SpriteRenderer>();

            spr.sprite = SegmentSprite;

            // return the newly created rope
            return newRope;
        }

        public void AddSegment()
        {
            if (_segments.Count >= 3)
            {
                var newSegment = CreateSegment(new Vector2(SegmentWidth, _segmentHeight));

                var joint = newSegment.GetComponent<HingeJoint2D>();

                joint.connectedBody = _segments[0].GetComponent<Rigidbody2D>();

                var dist = newSegment.GetComponent<DistanceJoint2D>();

                dist.connectedBody = joint.connectedBody;

                dist.maxDistanceOnly = true;

                var pos = _segments[0].transform.TransformPoint(0, -_segmentHeight, 0);

                newSegment.transform.position = pos;

                _segments[1].GetComponent<HingeJoint2D>().connectedBody = newSegment.GetComponent<Rigidbody2D>();
                _segments[1].GetComponent<DistanceJoint2D>().connectedBody = newSegment.GetComponent<Rigidbody2D>();

                _segments.Insert(1, newSegment);
            }
        }

        public void RemoveSegment()
        {
            if (_segments.Count > 3)
            {
                Destroy(_segments[1]);

                var go = _segments[2];

                go.GetComponent<HingeJoint2D>().connectedBody = _segments[0].GetComponent<Rigidbody2D>();
                go.GetComponent<DistanceJoint2D>().connectedBody = _segments[0].GetComponent<Rigidbody2D>();

                _segments.RemoveAt(1);
            }
        }

        public void ClearRope()
        {
            foreach (var segment in _segments)
            {
                Destroy(segment);
            }

            _segments.Clear();

            RopeRenderer.RopeSegments = null;
        }
    }
}
