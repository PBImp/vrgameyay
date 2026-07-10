using UnityEngine;

namespace MapTools
{
    public enum PlaneEdge
    {
        Left,
        Right,
        Top,
        Bottom
    }

    [ExecuteAlways]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(BoxCollider))]
    public class SnapPlane : MonoBehaviour
    {
        [Header("Root plane size (ignored while Parent Plane is set)")]
        public float width = 2f;
        public float height = 2f;
        public float thickness = 0.1f;

        [Header("Attachment")]
        [Tooltip("Leave empty for a free/root plane. Otherwise this plane's Bottom edge is glued to the chosen edge of the parent.")]
        public SnapPlane parentPlane;
        public PlaneEdge parentEdge = PlaneEdge.Top;

        [Tooltip("Rotation around the shared edge. 0 = flush/coplanar continuation, 90 = perpendicular.")]
        public float hingeAngle;

        [Tooltip("Size of this plane extending away from the shared edge (only used while attached).")]
        public float extrusion = 2f;

        [Header("Climbing")]
        public bool climbable = true;
        public float slipPercentage;

        private bool applying;

        private void OnValidate()
        {
            Apply();
        }

        private void Update()
        {
            if (!Application.isPlaying)
            {
                Apply();
            }
        }

        public void Apply()
        {
            if (applying)
            {
                return;
            }

            applying = true;
            try
            {
                if (parentPlane != null && parentPlane != this)
                {
                    parentPlane.Apply();
                    AttachToParent();
                }

                EnsureMesh();
                transform.localScale = new Vector3(width, height, thickness);
                SyncSurfaceComponent();
            }
            finally
            {
                applying = false;
            }
        }

        private void AttachToParent()
        {
            (Vector3 p0, Vector3 p1) = parentPlane.GetEdgeWorld(parentEdge);
            Vector3 edgeDir = (p1 - p0).normalized;
            Vector3 edgeMid = (p0 + p1) * 0.5f;

            width = Vector3.Distance(p0, p1);
            height = extrusion;

            Vector3 outwardLocal = parentEdge switch
            {
                PlaneEdge.Left => Vector3.left,
                PlaneEdge.Right => Vector3.right,
                PlaneEdge.Top => Vector3.up,
                _ => Vector3.down,
            };
            Vector3 outward = parentPlane.transform.TransformDirection(outwardLocal);

            Vector3 childUp = Quaternion.AngleAxis(hingeAngle, edgeDir) * outward;
            Vector3 childNormal = Vector3.Cross(edgeDir, childUp);

            transform.rotation = Quaternion.LookRotation(childNormal, childUp);

            Vector3 scale = new Vector3(width, height, thickness);
            Vector3 localBottomMid = new Vector3(0f, -0.5f, 0f);
            transform.position = edgeMid - transform.rotation * Vector3.Scale(localBottomMid, scale);
        }

        public (Vector3, Vector3) GetEdgeWorld(PlaneEdge edge)
        {
            Vector3 a;
            Vector3 b;
            switch (edge)
            {
                case PlaneEdge.Left:
                    a = new Vector3(-0.5f, -0.5f, 0f);
                    b = new Vector3(-0.5f, 0.5f, 0f);
                    break;
                case PlaneEdge.Right:
                    a = new Vector3(0.5f, -0.5f, 0f);
                    b = new Vector3(0.5f, 0.5f, 0f);
                    break;
                case PlaneEdge.Top:
                    a = new Vector3(-0.5f, 0.5f, 0f);
                    b = new Vector3(0.5f, 0.5f, 0f);
                    break;
                default:
                    a = new Vector3(-0.5f, -0.5f, 0f);
                    b = new Vector3(0.5f, -0.5f, 0f);
                    break;
            }

            return (transform.TransformPoint(a), transform.TransformPoint(b));
        }

        private void SyncSurfaceComponent()
        {
            var surface = GetComponent<GorillaLocomotion.Surface>();
            if (climbable)
            {
                if (surface == null)
                {
                    surface = gameObject.AddComponent<GorillaLocomotion.Surface>();
                }

                surface.slipPercentage = slipPercentage;
            }
            else if (surface != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(surface);
                }
                else
                {
                    DestroyImmediate(surface);
                }
            }
        }

        private void EnsureMesh()
        {
            // Width/height/thickness scale assumes the unit cube's -0.5..0.5 bounds,
            // so any pre-existing mesh (e.g. Unity's default Plane) must be replaced,
            // not just filled in when missing.
            var meshFilter = GetComponent<MeshFilter>();
            if (meshFilter.sharedMesh != BuiltinCubeMesh())
            {
                meshFilter.sharedMesh = BuiltinCubeMesh();
            }

            var meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer.sharedMaterial == null)
            {
                meshRenderer.sharedMaterial = Resources.GetBuiltinResource<Material>("Default-Material.mat");
            }

            var boxCollider = GetComponent<BoxCollider>();
            boxCollider.center = Vector3.zero;
            boxCollider.size = Vector3.one;
        }

        private static Mesh cachedCubeMesh;

        private static Mesh BuiltinCubeMesh()
        {
            if (cachedCubeMesh == null)
            {
                var temp = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cachedCubeMesh = temp.GetComponent<MeshFilter>().sharedMesh;
                if (Application.isPlaying)
                {
                    Destroy(temp);
                }
                else
                {
                    DestroyImmediate(temp);
                }
            }

            return cachedCubeMesh;
        }
    }
}
