/********************************************************************
     Copyright (c) XTech Cloud
     All rights reserved.
*********************************************************************/

using System;
using UnityEngine;
using UnityEngine.EventSystems;

using UnityEngine.XR;

namespace XTC.MegeXR.Core
{

    public class XReticleInputModuleImpl : XBasePointerInputImpl
    {
        private bool isActive = false;

        public override bool ShouldActivateModule()
        {

            bool activeState = inputModule.ShouldActivate();

            if (activeState != isActive)
            {
                isActive = activeState;
            }

            return activeState;
        }

        public override void DeactivateModule()
        {
            tryExitPointer();
            inputModule.Deactivate();
            if (CurrentEventData != null)
            {
                //HandlePointerExitAndEnter(CurrentEventData, null);
                CurrentEventData = null;
            }
            inputModule.eventSystem.SetSelectedGameObject(null, inputModule.GetBaseEventData());
        }

        public override bool IsPointerOverGameObject(int pointerId)
        {
            return CurrentEventData != null && CurrentEventData.pointerEnter != null;
        }

        public override void Process()
        {
            // If the pointer is inactive, make sure it is exited if necessary.
            if (!isPointerActiveAndAvailable())
            {
                tryExitPointer();
            }
            // Save the previous Game Object
            GameObject previousObject = getCurrentGameObject();

            castRay();
            updateCurrentObject(previousObject);
            updatePointer(previousObject);
        }

        protected void castRay()
        {
            Vector2 currentPose = lastPose;
            if (isPointerActiveAndAvailable())
            {
                currentPose = XHelpers.NormalizedCartesianToSpherical(Pointer.PointerTransform.forward);
            }

            if (CurrentEventData == null)
            {
                CurrentEventData = new PointerEventData(inputModule.eventSystem);
                lastPose = currentPose;
            }

            // Store the previous raycast result.
            RaycastResult previousRaycastResult = CurrentEventData.pointerCurrentRaycast;

            // The initial cast must use the enter radius.
            if (isPointerActiveAndAvailable())
            {
                Pointer.ShouldUseExitRadiusForRaycast = false;
            }

            // Cast a ray into the scene
            CurrentEventData.Reset();
            // Set the position to the center of the camera.
            // This is only necessary if using the built-in Unity raycasters.
            RaycastResult raycastResult;
            CurrentEventData.position = XHelpers.GetViewportCenter();
            bool pointerIsActiveAndAvailable = isPointerActiveAndAvailable();
            if (pointerIsActiveAndAvailable)
            {
                raycastAll();
                raycastResult = inputModule.FindFirstRaycast(inputModule.RaycastResultCache);
                //CurrentEventData.pointerId = (int)GvrControllerHand.Dominant;
                CurrentEventData.pointerId = 2;
            }
            else
            {
                raycastResult = new RaycastResult();
                raycastResult.Clear();
            }

            // If we were already pointing at an object we must check that object against the exit radius
            // to make sure we are no longer pointing at it to prevent flicker.
            if (previousRaycastResult.gameObject != null
                && raycastResult.gameObject != previousRaycastResult.gameObject
                && pointerIsActiveAndAvailable)
            {
                Pointer.ShouldUseExitRadiusForRaycast = true;
                raycastAll();
                RaycastResult firstResult = inputModule.FindFirstRaycast(inputModule.RaycastResultCache);
                if (firstResult.gameObject == previousRaycastResult.gameObject)
                {
                    raycastResult = firstResult;
                }
            }

            if (raycastResult.gameObject != null && raycastResult.worldPosition == Vector3.zero)
            {
                raycastResult.worldPosition =
                  XHelpers.GetIntersectionPosition(CurrentEventData.enterEventCamera, raycastResult);
            }

            CurrentEventData.pointerCurrentRaycast = raycastResult;

            // Find the real screen position associated with the raycast
            // Based on the results of the hit and the state of the pointerData.
            if (raycastResult.gameObject != null)
            {
                CurrentEventData.position = raycastResult.screenPosition;
            }
            else if (isPointerActiveAndAvailable() && CurrentEventData.enterEventCamera != null)
            {
                Vector3 pointerPos = Pointer.MaxPointerEndPoint;
                CurrentEventData.position = CurrentEventData.enterEventCamera.WorldToScreenPoint(pointerPos);
            }

            inputModule.RaycastResultCache.Clear();
            CurrentEventData.delta = currentPose - lastPose;
            lastPose = currentPose;

            // Check to make sure the Raycaster being used is a XRaycaster.
            if (raycastResult.module != null
                && !(raycastResult.module is XPointerGraphicRaycaster)
                && !(raycastResult.module is XPointerPhysicsRaycaster))
            {
                Debug.LogWarning("Using Raycaster (Raycaster: " + raycastResult.module.GetType() +
                  ", Object: " + raycastResult.module.name + "). It is recommended to use " +
                  "XPointerPhysicsRaycaster or XPointerGrahpicRaycaster with XPointerInputModule.");
            }
        }
    }//class

}//namespace XVP.VR
