/********************************************************************
     Copyright (c) XTech Cloud
     All rights reserved.
*********************************************************************/

using UnityEngine;
using UnityEngine.EventSystems;

namespace XTC.MegeXR.Core
{
    public class XReticlePointer : XBasePointer
    {
        /// The constants below are expsed for testing. Minimum inner angle of the reticle (in degrees).
        public const float RETICLE_MIN_INNER_ANGLE = 0.0f;

        /// Minimum outer angle of the reticle (in degrees).
        public const float RETICLE_MIN_OUTER_ANGLE = 0.25f;

        /// Angle at which to expand the reticle when intersecting with an object (in degrees).
        public const float RETICLE_GROWTH_ANGLE = 0.7f;

        /// Minimum distance of the reticle (in meters).
        public const float RETICLE_DISTANCE_MIN = 0.45f;

        /// Maximum distance of the reticle (in meters).
        public float maxReticleDistance = 20.0f;

        /// Number of segments making the reticle circle.
        public int reticleSegments = 20;

        /// Growth speed multiplier for the reticle/
        public float reticleGrowthSpeed = 8.0f;

        /// Sorting order to use for the reticle's renderer.
        /// Range values come from https://docs.unity3d.com/ScriptReference/Renderer-sortingOrder.html.
        /// Default value 32767 ensures gaze reticle is always rendered on top.
        [Range(-32767, 32767)]
        public int reticleSortingOrder = 32767;

        public Material MaterialComp { private get; set; }

        // Current inner angle of the reticle (in degrees).
        // Exposed for testing.
        public float ReticleInnerAngle { get; private set; }

        // Current outer angle of the reticle (in degrees).
        // Exposed for testing.
        public float ReticleOuterAngle { get; private set; }

        // Current distance of the reticle (in meters).
        // Getter exposed for testing.
        public float ReticleDistanceInMeters { get; private set; }

        // Current inner and outer diameters of the reticle, before distance multiplication.
        // Getters exposed for testing.
        public float ReticleInnerDiameter { get; private set; }

        public float ReticleOuterDiameter { get; private set; }

        public Transform owner { get; set; }

        public override float MaxPointerDistance { get { return maxReticleDistance; } }

        public override Transform PointerTransform
        {
            get
            {
                return owner;
            }
        }

        private float defaultDistance = 4.0f;

        public override void OnPointerEnter(RaycastResult raycastResultResult, bool isInteractive)
        {
            SetPointerTarget(raycastResultResult.worldPosition, isInteractive);
        }

        public override void OnPointerHover(RaycastResult raycastResultResult, bool isInteractive)
        {
            SetPointerTarget(raycastResultResult.worldPosition, isInteractive);
        }

        public override void OnPointerExit(GameObject previousObject)
        {
            //ReticleDistanceInMeters = maxReticleDistance;
            ReticleInnerAngle = RETICLE_MIN_INNER_ANGLE;
            ReticleOuterAngle = RETICLE_MIN_OUTER_ANGLE;
        }

        public override void OnPointerClickDown() { }

        public override void OnPointerClickUp() { }

        public override void GetPointerRadius(out float enterRadius, out float exitRadius)
        {
            float min_inner_angle_radians = Mathf.Deg2Rad * RETICLE_MIN_INNER_ANGLE;
            float max_inner_angle_radians = Mathf.Deg2Rad * (RETICLE_MIN_INNER_ANGLE + RETICLE_GROWTH_ANGLE);

            enterRadius = 2.0f * Mathf.Tan(min_inner_angle_radians);
            exitRadius = 2.0f * Mathf.Tan(max_inner_angle_radians);
        }

        public void UpdateDiameters()
        {
            ReticleDistanceInMeters =
              Mathf.Clamp(ReticleDistanceInMeters, RETICLE_DISTANCE_MIN, maxReticleDistance);

            if (ReticleInnerAngle < RETICLE_MIN_INNER_ANGLE)
            {
                ReticleInnerAngle = RETICLE_MIN_INNER_ANGLE;
            }

            if (ReticleOuterAngle < RETICLE_MIN_OUTER_ANGLE)
            {
                ReticleOuterAngle = RETICLE_MIN_OUTER_ANGLE;
            }

            float inner_half_angle_radians = Mathf.Deg2Rad * ReticleInnerAngle * 0.5f;
            float outer_half_angle_radians = Mathf.Deg2Rad * ReticleOuterAngle * 0.5f;

            float inner_diameter = 2.0f * Mathf.Tan(inner_half_angle_radians);
            float outer_diameter = 2.0f * Mathf.Tan(outer_half_angle_radians);

            ReticleInnerDiameter =
              Mathf.Lerp(ReticleInnerDiameter, inner_diameter, Time.deltaTime * reticleGrowthSpeed);
            ReticleOuterDiameter =
              Mathf.Lerp(ReticleOuterDiameter, outer_diameter, Time.deltaTime * reticleGrowthSpeed);

            MaterialComp.SetFloat("_InnerDiameter", ReticleInnerDiameter * ReticleDistanceInMeters);
            MaterialComp.SetFloat("_OuterDiameter", ReticleOuterDiameter * ReticleDistanceInMeters);
            MaterialComp.SetFloat("_DistanceInMeters", ReticleDistanceInMeters);
        }

        protected override void preSetup()
        {
            ReticleInnerAngle = RETICLE_MIN_INNER_ANGLE;
            ReticleOuterAngle = RETICLE_MIN_OUTER_ANGLE;
            ReticleDistanceInMeters = defaultDistance;

        }

        protected override void postSetup()
        {
            Renderer rendererComponent = PointerTransform.GetComponent<Renderer>();
            rendererComponent.sortingOrder = reticleSortingOrder;

            MaterialComp = rendererComponent.material;

            CreateReticleVertices();
        }

        protected override void update()
        {
            UpdateDiameters();
        }

        private bool SetPointerTarget(Vector3 target, bool interactive)
        {
            if (PointerTransform == null)
            {
                Debug.LogWarning("Cannot operate on a null pointer transform");
                return false;
            }

            Vector3 targetLocalPosition = PointerTransform.InverseTransformPoint(target);

            ReticleDistanceInMeters =
              Mathf.Clamp(targetLocalPosition.z, RETICLE_DISTANCE_MIN, maxReticleDistance);
            if (interactive)
            {
                ReticleInnerAngle = RETICLE_MIN_INNER_ANGLE + RETICLE_GROWTH_ANGLE;
                ReticleOuterAngle = RETICLE_MIN_OUTER_ANGLE + RETICLE_GROWTH_ANGLE;
            }
            else
            {
                ReticleInnerAngle = RETICLE_MIN_INNER_ANGLE;
                ReticleOuterAngle = RETICLE_MIN_OUTER_ANGLE;
            }
            return true;
        }

        private void CreateReticleVertices()
        {
            Mesh mesh = new Mesh();
            MeshFilter meshFilter = PointerTransform.gameObject.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;

            int segments_count = reticleSegments;
            int vertex_count = (segments_count + 1) * 2;

            #region Vertices

            Vector3[] vertices = new Vector3[vertex_count];

            const float kTwoPi = Mathf.PI * 2.0f;
            int vi = 0;
            for (int si = 0; si <= segments_count; ++si)
            {
                // Add two vertices for every circle segment: one at the beginning of the
                // prism, and one at the end of the prism.
                float angle = (float)si / (float)(segments_count) * kTwoPi;

                float x = Mathf.Sin(angle);
                float y = Mathf.Cos(angle);

                vertices[vi++] = new Vector3(x, y, 0.0f); // Outer vertex.
                vertices[vi++] = new Vector3(x, y, 1.0f); // Inner vertex.
            }
            #endregion

            #region Triangles
            int indices_count = (segments_count + 1) * 3 * 2;
            int[] indices = new int[indices_count];

            int vert = 0;
            int idx = 0;
            for (int si = 0; si < segments_count; ++si)
            {
                indices[idx++] = vert + 1;
                indices[idx++] = vert;
                indices[idx++] = vert + 2;

                indices[idx++] = vert + 1;
                indices[idx++] = vert + 2;
                indices[idx++] = vert + 3;

                vert += 2;
            }
            #endregion

            mesh.vertices = vertices;
            mesh.triangles = indices;
            mesh.RecalculateBounds();
        }
    }//class
}//namespace XVP.VR
