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
    public abstract class XBasePointerInputImpl
    {
        public PointerEventData CurrentEventData { get; protected set; }

        // Active state
        public abstract bool ShouldActivateModule();

        public abstract void DeactivateModule();

        public abstract bool IsPointerOverGameObject(int pointerId);

        public abstract void Process();

        public IPointerInputModule inputModule { get; set; }
        public XEventExecutor EventExecutor{get;set;}

        public XBasePointer Pointer
        {
            get
            {
                return pointer;
            }
            set
            {
                if (pointer == value)
                {
                    return;
                }

                tryExitPointer();

                pointer = value;
            }
        }

        public void MockClick()
        {
            GameObject currentGameObject = getCurrentGameObject();
            if(null == currentGameObject)
                return;

            EventExecutor.Execute(currentGameObject, CurrentEventData, ExecuteEvents.pointerClickHandler);

            Pointer.OnPointerClickDown();
            if (Pointer.OnPointerClickCallback != null) Pointer.OnPointerClickCallback.Invoke(Pointer, CurrentEventData.pointerCurrentRaycast);

        }

        private XBasePointer pointer;
        protected Vector2 lastPose = Vector2.zero;
        private bool isPointerHovering = false;

        protected GameObject getCurrentGameObject()
        {
            if (CurrentEventData != null)
            {
                return CurrentEventData.pointerCurrentRaycast.gameObject;
            }

            return null;
        }



        protected void raycastAll()
        {
            inputModule.RaycastResultCache.Clear();
            inputModule.eventSystem.RaycastAll(CurrentEventData, inputModule.RaycastResultCache);
        }

        protected void tryExitPointer()
        {
            if (Pointer == null)
            {
                return;
            }

            GameObject currentGameObject = getCurrentGameObject();
            if (currentGameObject)
            {
                Pointer.OnPointerExit(currentGameObject);
                if (Pointer.OnPointerExitCallback != null) Pointer.OnPointerExitCallback.Invoke(Pointer, currentGameObject);
            }
        }

        protected bool isPointerActiveAndAvailable()
        {
            return pointer != null && pointer.IsAvailable;
        }


        protected void updateCurrentObject(GameObject previousObject)
        {
            if (CurrentEventData == null)
            {
                return;
            }
            // Send enter events and update the highlight.
            GameObject currentObject = getCurrentGameObject(); // Get the pointer target
            handlePointerExitAndEnter(CurrentEventData, currentObject);

            // Update the current selection, or clear if it is no longer the current object.
            var selected = EventExecutor.GetEventHandler<ISelectHandler>(currentObject);
            if (selected == inputModule.eventSystem.currentSelectedGameObject)
            {
                EventExecutor.Execute(inputModule.eventSystem.currentSelectedGameObject, inputModule.GetBaseEventData(),
                  ExecuteEvents.updateSelectedHandler);
            }
            else
            {
                inputModule.eventSystem.SetSelectedGameObject(null, CurrentEventData);
            }

            // Execute hover event.
            if (currentObject != null && currentObject == previousObject)
            {
                EventExecutor.ExecuteHierarchy(currentObject, CurrentEventData, GvrExecuteEventsExtension.pointerHoverHandler);
            }
        }

        protected void updatePointer(GameObject previousObject)
        {
            if (CurrentEventData == null)
            {
                return;
            }

            GameObject currentObject = getCurrentGameObject(); // Get the pointer target
            bool pointerIsActiveAndAvailable = isPointerActiveAndAvailable();

            bool isInteractive = CurrentEventData.pointerPress != null ||
                                 EventExecutor.GetEventHandler<IPointerClickHandler>(currentObject) != null ||
                                 EventExecutor.GetEventHandler<IDragHandler>(currentObject) != null;

            if (isPointerHovering && currentObject != null && currentObject == previousObject)
            {
                if (pointerIsActiveAndAvailable)
                {
                    Pointer.OnPointerHover(CurrentEventData.pointerCurrentRaycast, isInteractive);
                    if (Pointer.OnPointerHoverCallback != null) Pointer.OnPointerHoverCallback.Invoke(Pointer, CurrentEventData.pointerCurrentRaycast);
                }
            }
            else
            {
                // If the object's don't match or the hovering object has been destroyed
                // then the pointer has exited.
                if (previousObject != null || (currentObject == null && isPointerHovering))
                {
                    if (pointerIsActiveAndAvailable)
                    {
                        Pointer.OnPointerExit(previousObject);
                        if (Pointer.OnPointerExitCallback != null) Pointer.OnPointerExitCallback.Invoke(Pointer, previousObject);
                    }
                    isPointerHovering = false;
                }

                if (currentObject != null)
                {
                    if (pointerIsActiveAndAvailable)
                    {
                        Pointer.OnPointerEnter(CurrentEventData.pointerCurrentRaycast, isInteractive);
                        if (Pointer.OnPointerEnterCallback != null) Pointer.OnPointerEnterCallback.Invoke(Pointer, CurrentEventData.pointerCurrentRaycast);
                    }
                    isPointerHovering = true;
                }
            }
        }

        protected void handlePointerExitAndEnter(PointerEventData currentPointerData, GameObject newEnterTarget)
        {
            // If we have no target or pointerEnter has been deleted then
            // just send exit events to anything we are tracking.
            // Afterwards, exit.
            if (newEnterTarget == null || currentPointerData.pointerEnter == null)
            {
                for (var i = 0; i < currentPointerData.hovered.Count; ++i)
                {
                    EventExecutor.Execute(currentPointerData.hovered[i], currentPointerData, ExecuteEvents.pointerExitHandler);
                }

                currentPointerData.hovered.Clear();

                if (newEnterTarget == null)
                {
                    currentPointerData.pointerEnter = newEnterTarget;
                    return;
                }
            }

            // If we have not changed hover target.
            if (newEnterTarget && currentPointerData.pointerEnter == newEnterTarget)
            {
                return;
            }

            GameObject commonRoot = inputModule.FindCommonRoot(currentPointerData.pointerEnter, newEnterTarget);

            // We already an entered object from last time.
            if (currentPointerData.pointerEnter != null)
            {
                // Send exit handler call to all elements in the chain
                // until we reach the new target, or null!
                Transform t = currentPointerData.pointerEnter.transform;

                while (t != null)
                {
                    // If we reach the common root break out!
                    if (commonRoot != null && commonRoot.transform == t)
                        break;

                    EventExecutor.Execute(t.gameObject, currentPointerData, ExecuteEvents.pointerExitHandler);
                    currentPointerData.hovered.Remove(t.gameObject);
                    t = t.parent;
                }
            }

            // Now issue the enter call up to but not including the common root.
            currentPointerData.pointerEnter = newEnterTarget;
            if (newEnterTarget != null)
            {
                Transform t = newEnterTarget.transform;

                while (t != null && t.gameObject != commonRoot)
                {
                    EventExecutor.Execute(t.gameObject, currentPointerData, ExecuteEvents.pointerEnterHandler);
                    currentPointerData.hovered.Add(t.gameObject);
                    t = t.parent;
                }
            }
        }
    }//class

}//namespace XVP.VR
