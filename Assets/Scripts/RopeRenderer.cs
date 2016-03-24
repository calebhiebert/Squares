using UnityEngine;

namespace Assets.Scripts
{
    public class RopeRenderer : MonoBehaviour
    {
        public Transform[] RopeSegments;

        public LineRenderer LineRenderer;

        private void Update () {
            if (RopeSegments == null || RopeSegments.Length <= 0) return;

            LineRenderer.SetVertexCount(RopeSegments.Length);

            for (var index = 0; index < RopeSegments.Length; index++)
            {
                LineRenderer.SetPosition(index, RopeSegments[index].position);
            }
        }
    }
}
