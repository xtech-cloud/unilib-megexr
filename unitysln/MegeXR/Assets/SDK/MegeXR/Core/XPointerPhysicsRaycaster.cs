﻿/********************************************************************
     Copyright (c) XTech Cloud
     All rights reserved.
*********************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace XTC.MegeXR.Core
{

    /// This script provides a raycaster for use with the GvrPointerInputModule.
    /// It behaves similarly to the standards Physics raycaster, except that it utilize raycast
    /// modes specifically for Gvr.
    ///
    /// View GvrBasePointerRaycaster.cs and GvrPointerInputModule.cs for more details.
    public class XPointerPhysicsRaycaster : XBasePointerRaycaster
    {
        /// Used to sort the raycast hits by distance.
        private class HitComparer : IComparer<RaycastHit>
        {
            public int Compare(RaycastHit lhs, RaycastHit rhs)
            {
                return lhs.distance.CompareTo(rhs.distance);
            }
        }

        /// Const to use for clarity when no event mask is set
        protected const int NO_EVENT_MASK_SET = -1;

        /// The maximum allowed value for the field maxRaycastHits.
        private const int MAX_RAYCAST_HITS_MAX = 512;

        /// Layer mask used to filter events. Always combined with the camera's culling mask if a camera is used.
        [SerializeField]
        protected LayerMask raycasterEventMask = NO_EVENT_MASK_SET;

        [SerializeField]
        [Range(1, MAX_RAYCAST_HITS_MAX)]
        /// The max number of hits that the raycaster can detect at once.
        /// They are NOT guaranteed to be ordered by distance. This value should be set to a higher number
        /// than the number of objects the pointer is expected to intersect with in a single frame.
        ///
        /// This functionality is used to prevent unnecessary memory allocation to improve performance.
        /// https://docs.unity3d.com/ScriptReference/Physics.SphereCastNonAlloc.html
        private int maxRaycastHits = 64;

        /// Buffer of raycast hits re-used each time PerformRaycast is called.
        private RaycastHit[] hits;

        /// Used to sort the hits by distance.
        private HitComparer hitComparer = new HitComparer();

        public int MaxRaycastHits
        {
            get
            {
                return maxRaycastHits;
            }
            set
            {
                maxRaycastHits = Mathf.Min(value, MAX_RAYCAST_HITS_MAX);

                if (Application.isPlaying && hits != null && hits.Length != maxRaycastHits)
                {
                    hits = new RaycastHit[maxRaycastHits];
                }
            }
        }

        /// Camera used for masking layers and determining the screen position of the raycast result.
        public override Camera eventCamera
        {
            get
            {
                XBasePointer pointer = XPointerInputModule.Pointer;
                if (pointer == null)
                {
                    return null;
                }

                return pointer.PointerCamera;
            }
        }

        /// Event mask used to determine which objects will receive events.
        public int finalEventMask
        {
            get
            {
                return (eventCamera != null) ? eventCamera.cullingMask & eventMask : NO_EVENT_MASK_SET;
            }
        }

        /// Layer mask used to filter events. Always combined with the camera's culling mask if a camera is used.
        public LayerMask eventMask
        {
            get
            {
                return raycasterEventMask;
            }
            set
            {
                raycasterEventMask = value;
            }
        }

        protected XPointerPhysicsRaycaster()
        {
        }

        protected override void Awake()
        {
            base.Awake();
            hits = new RaycastHit[maxRaycastHits];
        }

        protected override bool PerformRaycast(XBasePointer.PointerRay pointerRay, float radius,
          PointerEventData eventData, List<RaycastResult> resultAppendList)
        {

            if (eventCamera == null)
            {
                return false;
            }

            int numHits;
            if (radius > 0.0f)
            {
                numHits = Physics.SphereCastNonAlloc(pointerRay.ray, radius, hits, pointerRay.distance, finalEventMask);
            }
            else
            {
                numHits = Physics.RaycastNonAlloc(pointerRay.ray, hits, pointerRay.distance, finalEventMask);
            }

            if (numHits == 0)
            {
                return false;
            }

            if (numHits == MaxRaycastHits)
            {
                MaxRaycastHits *= 2;
                Debug.LogWarningFormat("Physics Raycast/Spherecast returned {0} hits, which is the current " +
                  "maximum and means that some hits may have been lost. Setting maxRaycastHits to {1}. " +
                  "Please set maxRaycastHits to a sufficiently high value for your scene.",
                  numHits, MaxRaycastHits);
            }

            Array.Sort(hits, 0, numHits, hitComparer);

            for (int i = 0; i < numHits; ++i)
            {
                Vector3 projection = Vector3.Project(hits[i].point - pointerRay.ray.origin, pointerRay.ray.direction);
                Vector3 hitPosition = projection + pointerRay.ray.origin;
                float resultDistance = hits[i].distance + pointerRay.distanceFromStart;

                Transform pointerTransform =
                  XPointerInputModule.Pointer.PointerTransform;
                float delta = (hitPosition - pointerTransform.position).magnitude;
                if (delta < pointerRay.distanceFromStart)
                {
                    continue;
                }

                RaycastResult result = new RaycastResult
                {
                    gameObject = hits[i].collider.gameObject,
                    module = this,
                    distance = resultDistance,
                    worldPosition = hitPosition,
                    worldNormal = hits[i].normal,
                    screenPosition = eventCamera.WorldToScreenPoint(hitPosition),
                    index = resultAppendList.Count,
                    sortingLayer = 0,
                    sortingOrder = 0
                };

                resultAppendList.Add(result);
            }

            return true;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            // Makes sure that the hits buffer is updated if maxRaycastHits is changed in the inspector
            // while testing in the editor.
            if (Application.isPlaying)
            {
                MaxRaycastHits = maxRaycastHits;
            }
        }
#endif  // UNITY_EDITOR
    }//class
}//namespace XVP.VR
