/********************************************************************
     Copyright (c) XTech Cloud
     All rights reserved.
*********************************************************************/

using UnityEngine;
using UnityEngine.EventSystems;

namespace XTC.MegeXR.Core
{
    public abstract class XBasePointer
    {
        public enum RaycastMode
        {
            /// Casts a ray from the camera through the target of the pointer.
            /// This is ideal for reticles that are always rendered on top.
            /// The object that is selected will always be the object that appears
            /// underneath the reticle from the perspective of the camera.
            /// This also prevents the reticle from appearing to "jump" when it starts/stops hitting an object.
            ///
            /// Recommended for reticles that are always rendered on top such as the GvrReticlePointer
            /// prefab which is used for cardboard apps.
            ///
            /// Note: This will prevent the user from pointing around an object to hit something that is out of sight.
            /// This isn't a problem in a typical use case.
            ///
            /// When used with the standard daydream controller,
            /// the hit detection will not account for the laser correctly for objects that are closer to the
            /// camera than the end of the laser.
            /// In that case, it is recommended to do one of the following things:
            ///
            /// 1. Hide the laser.
            /// 2. Use a full-length laser pointer in Direct mode.
            /// 3. Use the Hybrid raycast mode.
            Camera,
            /// Cast a ray directly from the pointer origin.
            ///
            /// Recommended for full-length laser pointers.
            Direct,
            /// Default method for casting ray.
            ///
            /// Combines the Camera and Direct raycast modes.
            /// Uses a Direct ray up until the CameraRayIntersectionDistance, and then switches to use
            /// a Camera ray starting from the point where the two rays intersect.
            ///
            /// Recommended for use with the standard settings of the GvrControllerPointer prefab.
            /// This is the most versatile raycast mode. Like Camera mode, this prevents the reticle
            /// appearing jumpy. Additionally, it still allows the user to target objects that are close
            /// to them by using the laser as a visual reference.
            Hybrid,
        }


        /// Represents a ray segment for a series of intersecting rays.
        /// This is useful for Hybrid raycast mode, which uses two sequential rays.
        public struct PointerRay
        {
            /// The ray for this segment of the pointer.
            public Ray ray;

            /// The distance along the pointer from the origin of the first ray to this ray.
            public float distanceFromStart;

            /// Distance that this ray extends to.
            public float distance;
        }

        /// Determines which raycast mode to use for this raycaster.
        ///   • Camera - Ray is cast from the camera through the pointer.
        ///   • Direct - Ray is cast forward from the pointer.
        ///   • Hybrid - Begins with a Direct ray and transitions to a Camera ray.
        public RaycastMode raycastMode = RaycastMode.Hybrid;

        /// Determines the eventCamera for _GvrPointerPhysicsRaycaster_ and _GvrPointerGraphicRaycaster_.
        /// Additionaly, this is used to control what camera to use when calculating the Camera ray for
        /// the Hybrid and Camera raycast modes.
        public Camera overridePointerCamera;

        /// This is used to determine if the enterRadius or the exitRadius should be used for the raycast.
        /// It is set by GvrPointerInputModule and doesn't need to be controlled manually.
        public bool ShouldUseExitRadiusForRaycast { get; set; }

        /// If ShouldUseExitRadiusForRaycast is true, returns the exitRadius.
        /// Otherwise, returns the enterRadius.
        public float CurrentPointerRadius
        {
            get
            {
                float enterRadius, exitRadius;
                GetPointerRadius(out enterRadius, out exitRadius);
                if (ShouldUseExitRadiusForRaycast)
                {
                    return exitRadius;
                }
                else
                {
                    return enterRadius;
                }
            }
        }

        public UnityEngine.Events.UnityAction<XBasePointer, RaycastResult> OnPointerEnterCallback;
        public UnityEngine.Events.UnityAction<XBasePointer, RaycastResult> OnPointerHoverCallback;
        public UnityEngine.Events.UnityAction<XBasePointer, RaycastResult> OnPointerClickCallback;
        public UnityEngine.Events.UnityAction<XBasePointer, GameObject> OnPointerExitCallback;

        /// Called when the pointer is facing a valid GameObject. This can be a 3D
        /// or UI element.
        ///
        /// **raycastResult** is the hit detection result for the object being pointed at.
        /// **isInteractive** is true if the object being pointed at is interactive.
        public abstract void OnPointerEnter(RaycastResult raycastResult, bool isInteractive);

        /// Called every frame the user is still pointing at a valid GameObject. This
        /// can be a 3D or UI element.
        ///
        /// **raycastResult** is the hit detection result for the object being pointed at.
        /// **isInteractive** is true if the object being pointed at is interactive.
        public abstract void OnPointerHover(RaycastResult raycastResultResult, bool isInteractive);

        /// Called when the pointer no longer faces an object previously
        /// intersected with a ray projected from the camera.
        /// This is also called just before **OnInputModuleDisabled**
        /// previousObject will be null in this case.
        ///
        /// **previousObject** is the object that was being pointed at the previous frame.
        public abstract void OnPointerExit(GameObject previousObject);

        /// Called when a click is initiated.

        public abstract void OnPointerClickDown();

        /// Called when click is finished.
        public abstract void OnPointerClickUp();

        /// Return the radius of the pointer. It is used by GvrPointerPhysicsRaycaster when
        /// searching for valid pointer targets. If a radius is 0, then a ray is used to find
        /// a valid pointer target. Otherwise it will use a SphereCast.
        /// The *enterRadius* is used for finding new targets while the *exitRadius*
        /// is used to see if you are still nearby the object currently pointed at
        /// to avoid a flickering effect when just at the border of the intersection.
        ///
        /// NOTE: This is only works with GvrPointerPhysicsRaycaster. To use it with uGUI,
        /// add 3D colliders to your canvas elements.
        public abstract void GetPointerRadius(out float enterRadius, out float exitRadius);

        public abstract Transform PointerTransform { get; }

        /// Returns the max distance from the pointer that raycast hits will be detected.
        public abstract float MaxPointerDistance { get; }

        protected abstract void preSetup();
        protected abstract void postSetup();
        protected abstract void update();

        /// Returns the end point of the pointer when it is MaxPointerDistance away from the origin.
        public virtual Vector3 MaxPointerEndPoint
        {
            get
            {
                Transform pointerTransform = PointerTransform;
                if (pointerTransform == null)
                {
                    return Vector3.zero;
                }

                Vector3 maxEndPoint = GetPointAlongPointer(MaxPointerDistance);
                return maxEndPoint;
            }
        }

        /// If true, the pointer will be used for generating input events by _GvrPointerInputModule_.
        public virtual bool IsAvailable
        {
            get
            {
                Transform pointerTransform = PointerTransform;
                if (pointerTransform == null)
                {
                    return false;
                }

                return pointerTransform.gameObject.activeInHierarchy;
            }
        }


        /// When using the Camera raycast mode, this is used to calculate
        /// where the ray from the pointer will intersect with the ray from the camera.
        public virtual float CameraRayIntersectionDistance
        {
            get
            {
                return MaxPointerDistance;
            }
        }

        public Camera PointerCamera
        {
            get
            {
                if (overridePointerCamera != null)
                {
                    return overridePointerCamera;
                }

                return Camera.main;
            }
        }

        /// Returns a point in worldspace a specified distance along the pointer.
        /// What this point will be is different depending on the raycastMode.
        ///
        /// Because raycast modes differ, use this function instead of manually calculating a point
        /// projected from the pointer.
        public Vector3 GetPointAlongPointer(float distance)
        {
            PointerRay pointerRay = GetRayForDistance(distance);
            return pointerRay.ray.GetPoint(distance - pointerRay.distanceFromStart);
        }

        /// Returns the ray used for projecting points out of the pointer for the given distance.
        /// In Hybrid raycast mode, the ray will be different depending upon the distance.
        /// In Camera or Direct raycast mode, the ray will always be the same.
        public PointerRay GetRayForDistance(float distance)
        {
            PointerRay result = new PointerRay();

            if (raycastMode == RaycastMode.Hybrid)
            {
                float directDistance = CameraRayIntersectionDistance;
                if (distance < directDistance)
                {
                    result = CalculateHybridRay(this, RaycastMode.Direct);
                }
                else
                {
                    result = CalculateHybridRay(this, RaycastMode.Camera);
                }
            }
            else
            {
                result = CalculateRay(this, raycastMode);
            }

            return result;
        }


        /// Calculates the ray for a given Raycast mode.
        /// Will throw an exception if the raycast mode Hybrid is passed in.
        /// If you need to calculate the ray for the direct or camera segment of the Hybrid raycast,
        /// use CalculateHybridRay instead.
        public static PointerRay CalculateRay(XBasePointer pointer, RaycastMode mode)
        {
            PointerRay result = new PointerRay();

            if (pointer == null || !pointer.IsAvailable)
            {
                Debug.LogError("Cannot calculate ray when the pointer isn't available.");
                return result;
            }

            Transform pointerTransform = pointer.PointerTransform;

            if (pointerTransform == null)
            {
                Debug.LogError("Cannot calculate ray when pointerTransform is null.");
                return result;
            }

            result.distance = pointer.MaxPointerDistance;

            switch (mode)
            {
                case RaycastMode.Camera:
                    Camera camera = pointer.PointerCamera;
                    if (camera == null)
                    {
                        Debug.LogError("Cannot calculate ray because pointer.PointerCamera is null." +
                          "To fix this, either tag a Camera as \"MainCamera\" or set overridePointerCamera.");
                        return result;
                    }

                    Vector3 rayPointerStart = pointerTransform.position;
                    Vector3 rayPointerEnd = rayPointerStart +
                      (pointerTransform.forward * pointer.CameraRayIntersectionDistance);

                    Vector3 cameraLocation = camera.transform.position;
                    Vector3 finalRayDirection = rayPointerEnd - cameraLocation;
                    finalRayDirection.Normalize();

                    Vector3 finalRayStart = cameraLocation + (finalRayDirection * camera.nearClipPlane);

                    result.ray = new Ray(finalRayStart, finalRayDirection);
                    break;
                case RaycastMode.Direct:
                    result.ray = new Ray(pointerTransform.position, pointerTransform.forward);
                    break;
                default:
                    throw new UnityException("Invalid RaycastMode " + mode + " passed into CalculateRay.");
            }

            return result;
        }

        /// Calculates the ray for the segment of the Hybrid raycast determined by the raycast mode
        /// passed in. Throws an exception if Hybrid is passed in.
        public static PointerRay CalculateHybridRay(XBasePointer pointer, RaycastMode hybridMode)
        {
            PointerRay result;

            switch (hybridMode)
            {
                case RaycastMode.Direct:
                    result = CalculateRay(pointer, hybridMode);
                    result.distance = pointer.CameraRayIntersectionDistance;
                    break;
                case RaycastMode.Camera:
                    result = CalculateRay(pointer, hybridMode);
                    PointerRay directRay = CalculateHybridRay(pointer, RaycastMode.Direct);
                    result.ray.origin = directRay.ray.GetPoint(directRay.distance);
                    result.distanceFromStart = directRay.distance;
                    result.distance = pointer.MaxPointerDistance - directRay.distance;
                    break;
                default:
                    throw new UnityException("Invalid RaycastMode " + hybridMode + " passed into CalculateHybridRay.");
            }

            return result;
        }

        public void Setup()
        {
            preSetup();
            XPointerInputModule.OnPointerCreated(this);
            postSetup();
        }

        public void Update()
        {
          update();
        }

#if UNITY_EDITOR
        public void DrawGizmos()
        {
            if (Application.isPlaying)
            {
                switch (raycastMode)
                {
                    case RaycastMode.Camera:
                        // Camera line.
                        Gizmos.color = Color.green;
                        PointerRay pointerRay = CalculateRay(this, RaycastMode.Camera);
                        Gizmos.DrawLine(pointerRay.ray.origin, pointerRay.ray.GetPoint(pointerRay.distance));
                        Camera camera = PointerCamera;

                        // Pointer to intersection dotted line.
                        Vector3 intersection =
                          PointerTransform.position + (PointerTransform.forward * CameraRayIntersectionDistance);
                        UnityEditor.Handles.DrawDottedLine(PointerTransform.position, intersection, 1.0f);
                        break;
                    case RaycastMode.Direct:
                        // Direct line.
                        Gizmos.color = Color.blue;
                        pointerRay = CalculateRay(this, RaycastMode.Direct);
                        Gizmos.DrawLine(pointerRay.ray.origin, pointerRay.ray.GetPoint(pointerRay.distance));
                        break;
                    case RaycastMode.Hybrid:
                        // Direct line.
                        Gizmos.color = Color.blue;
                        pointerRay = CalculateHybridRay(this, RaycastMode.Direct);
                        Gizmos.DrawLine(pointerRay.ray.origin, pointerRay.ray.GetPoint(pointerRay.distance));

                        // Camera line.
                        Gizmos.color = Color.green;
                        pointerRay = CalculateHybridRay(this, RaycastMode.Camera);
                        Gizmos.DrawLine(pointerRay.ray.origin, pointerRay.ray.GetPoint(pointerRay.distance));

                        // Camera to intersection dotted line.
                        camera = PointerCamera;
                        if (camera != null)
                        {
                            UnityEditor.Handles.DrawDottedLine(camera.transform.position, pointerRay.ray.origin, 1.0f);
                        }
                        break;
                    default:
                        break;
                }
            }
        }
#endif // UNITY_EDITOR


    }//class
}//namespace XVP.VR
