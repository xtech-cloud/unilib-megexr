/********************************************************************
     Copyright (c) XTech Cloud
     All rights reserved.
*********************************************************************/

using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace XTC.MegeXR.Core
{

    /// This script provides shared functionality used by all Gvr raycasters.
    public abstract class XBasePointerRaycaster : BaseRaycaster
    {
        private XBasePointer.PointerRay lastRay;

        protected XBasePointer.RaycastMode CurrentRaycastModeForHybrid { get; private set; }

        protected XBasePointerRaycaster()
        {
        }

        public XBasePointer.PointerRay GetLastRay()
        {
            return lastRay;
        }

        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            XBasePointer pointer = XPointerInputModule.Pointer;
            if (pointer == null || !pointer.IsAvailable)
            {
                return;
            }

            if (pointer.raycastMode == XBasePointer.RaycastMode.Hybrid)
            {
                RaycastHybrid(pointer, eventData, resultAppendList);
            }
            else
            {
                RaycastDefault(pointer, eventData, resultAppendList);
            }
        }

        protected abstract bool PerformRaycast(XBasePointer.PointerRay pointerRay, float radius,
          PointerEventData eventData, List<RaycastResult> resultAppendList);

        private void RaycastHybrid(XBasePointer pointer, PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            CurrentRaycastModeForHybrid = XBasePointer.RaycastMode.Direct;
            lastRay = XBasePointer.CalculateHybridRay(pointer, CurrentRaycastModeForHybrid);
            float radius = pointer.CurrentPointerRadius;
            bool foundHit = PerformRaycast(lastRay, radius, eventData, resultAppendList);

            if (!foundHit)
            {
                CurrentRaycastModeForHybrid = XBasePointer.RaycastMode.Camera;
                lastRay = XBasePointer.CalculateHybridRay(pointer, CurrentRaycastModeForHybrid);
                PerformRaycast(lastRay, radius, eventData, resultAppendList);
            }
        }

        private void RaycastDefault(XBasePointer pointer, PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            lastRay = XBasePointer.CalculateRay(pointer, pointer.raycastMode);
            float radius = pointer.CurrentPointerRadius;
            PerformRaycast(lastRay, radius, eventData, resultAppendList);
        }
    }//class
}//namespace XVP.VR
