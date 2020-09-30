// Copyright 2017 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// Draws a circular reticle in front of any object that the user points at.
/// The circle dilates if the object is clickable.
public class GvrReticlePointer : GvrBasePointer
{
    [SerializeField]
    private float default_distance = 4.0f;
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

    public override float MaxPointerDistance { get { return maxReticleDistance; } }
    private GvrBasePointer mBasePointer;
    public UnityEngine.Events.UnityAction<RaycastResult> OnPointerEnterEvent;
    public UnityEngine.Events.UnityAction<RaycastResult> OnPointerHoverEvent;
    public UnityEngine.Events.UnityAction<GameObject> OnPointerExitEvent;
    public override void OnPointerEnter(RaycastResult raycastResultResult, bool isInteractive)
    {
        SetPointerTarget(raycastResultResult.worldPosition, isInteractive);
        if (OnPointerEnterEvent != null) OnPointerEnterEvent.Invoke(raycastResultResult);

        //Debug.Log("LEFT:"+GvrViewer.Controller.Eyes[0].cam.WorldToScreenPoint(raycastResultResult.worldPosition).ToString("F4"));
        //Debug.Log("RIGHT:"+ GvrViewer.Controller.Eyes[1].cam.WorldToScreenPoint(raycastResultResult.worldPosition).ToString("F4"));
    }

    public override void OnPointerHover(RaycastResult raycastResultResult, bool isInteractive)
    {
        SetPointerTarget(raycastResultResult.worldPosition, isInteractive);
        if (OnPointerHoverEvent != null) OnPointerHoverEvent.Invoke(raycastResultResult);
        //if (Svr.SvrSetting.IsVR9Device)
        //    SVR.AtwAPI.SetScreenPoint(GvrViewer.Controller.Eyes[0].cam.WorldToScreenPoint(raycastResultResult.worldPosition).x,
        //        GvrViewer.Controller.Eyes[1].cam.WorldToScreenPoint(raycastResultResult.worldPosition).x);
    }

    public override void OnPointerExit(GameObject previousObject)
    {
        //ReticleDistanceInMeters = maxReticleDistance;
        ReticleInnerAngle = RETICLE_MIN_INNER_ANGLE;
        ReticleOuterAngle = RETICLE_MIN_OUTER_ANGLE;
        if (OnPointerExitEvent != null) OnPointerExitEvent.Invoke(previousObject);
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

    void Awake()
    {
        SVR.AtwAPI.BeginTrace("pointer-awake");
        mBasePointer = GetComponentInChildren<GvrBasePointer>();
        GvrControllerInput.OnGvrPointerEnable += GvrControllerInput_OnGvrPointerEnable;
//#if NOLOSDK
        GvrControllerInput.OnConterollerChanged += GvrControllerInput_OnConterollerChanged;
//#else
//        GvrControllerInput.OnStateChanged += OnControllerStateChanged;
//#endif
        ReticleInnerAngle = RETICLE_MIN_INNER_ANGLE;
        ReticleOuterAngle = RETICLE_MIN_OUTER_ANGLE;
        ReticleDistanceInMeters = default_distance;
        if (Svr.SvrSetting.IsVR9Device)
            GetComponent<Renderer>().enabled = false;
        SVR.AtwAPI.EndTrace();
    }

    private void GvrControllerInput_OnGvrPointerEnable(bool obj)
    {
        if (GvrControllerInput.SvrState == SvrControllerState.None)
        {
            if (Svr.SvrSetting.IsVR9Device)
                SVR.AtwAPI.ShowDualSurface(obj);
            else
                GetComponent<Renderer>().enabled = obj;

        }
            
    }

    private void GvrControllerInput_OnConterollerChanged(SvrControllerState state, SvrControllerState oldState)
    {
        SVR.AtwAPI.BeginTrace("Input_Changed");
        SVR.AtwAPI.BeginTrace("1");
        SvrControllerState target = SvrControllerState.NoloLeftContoller | SvrControllerState.NoloRightContoller | SvrControllerState.GvrController;
        SVR.AtwAPI.EndTrace();
        //Svr.SvrLog.Log("GvrControllerInput_OnConterollerChanged "+ state);
        if ((state & target) != 0)
        {
            gameObject.SetActive(false);
            SVR.AtwAPI.ShowDualSurface(false);
            //Svr.SvrLog.Log("ShowDualSurface false");
            if (GvrPointerInputModule.Pointer == mBasePointer)
            {
                GvrPointerInputModule.Pointer = null;
            }
        }
        else
        {
            SVR.AtwAPI.BeginTrace("2");
            //OnControllerStateChanged(GvrControllerInput.State, GvrControllerInput.State);
            if (enabled)
            {
                gameObject.SetActive(true);
                //SVR.AtwAPI.ShowDualSurface(true);

                if (Svr.SvrSetting.IsVR9Device)
                {
                    SVR.AtwAPI.ShowDualSurface(true);
                }
                else
                {
                    Renderer renderer = GetComponent<Renderer>();
                    if (renderer != null)
                        renderer.enabled = true;
                }
                //Svr.SvrLog.Log("ShowDualSurface true");
                if (gameObject.activeSelf)
                {
                    GvrPointerInputModule.Pointer = mBasePointer;
                }
                else
                {
                    if (GvrPointerInputModule.Pointer == mBasePointer)
                    {
                        GvrPointerInputModule.Pointer = null;
                    }
                }

            }
            SVR.AtwAPI.EndTrace();
        }

        SVR.AtwAPI.EndTrace();
    }

    private void OnEnable()
    {
        SVR.AtwAPI.BeginTrace("pointer-onEnable");
        //#if NOLOSDK
        GvrControllerInput_OnConterollerChanged(GvrControllerInput.SvrState, 0);
        //#else
        //        OnControllerStateChanged(GvrControllerInput.State, GvrControllerInput.State);
        //#endif
        SVR.AtwAPI.EndTrace();

    }
    private void OnControllerStateChanged(GvrConnectionState state, GvrConnectionState oldState)
    {
        if (enabled)
        {
            gameObject.SetActive(state != GvrConnectionState.Connected);
            SVR.AtwAPI.ShowDualSurface(state != GvrConnectionState.Connected);
            if (gameObject.activeSelf)
            {
                GvrPointerInputModule.Pointer = GetComponentInChildren<GvrBasePointer>();
            }
        }
    }
    private void OnDisable()
    {
        SVR.AtwAPI.ShowDualSurface(false);
    }
    void OnDestroy()
    {
        GvrControllerInput.OnGvrPointerEnable -= GvrControllerInput_OnGvrPointerEnable;
//#if NOLOSDK
        GvrControllerInput.OnConterollerChanged -= GvrControllerInput_OnConterollerChanged;
//#else
//        GvrControllerInput.OnStateChanged -= OnControllerStateChanged;
//#endif
    }
    protected override void Start()
    {
        base.Start();
        Renderer rendererComponent = GetComponent<Renderer>();
        rendererComponent.sortingOrder = reticleSortingOrder;

        MaterialComp = rendererComponent.material;

        CreateReticleVertices();

    }

    void Update()
    {
        UpdateDiameters();
    }

    private bool SetPointerTarget(Vector3 target, bool interactive)
    {
        if (base.PointerTransform == null)
        {
            Debug.LogWarning("Cannot operate on a null pointer transform");
            return false;
        }

        Vector3 targetLocalPosition = base.PointerTransform.InverseTransformPoint(target);

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
        gameObject.AddComponent<MeshFilter>();
        GetComponent<MeshFilter>().mesh = mesh;

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
#if !UNITY_5_5_OR_NEWER
    // Optimize() is deprecated as of Unity 5.5.0p1.
    mesh.Optimize();
#endif  // !UNITY_5_5_OR_NEWER
    }
}
