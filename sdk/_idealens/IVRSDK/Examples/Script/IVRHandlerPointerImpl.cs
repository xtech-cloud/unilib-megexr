using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

namespace IVR
{
    public class IVRHandlerPointerImpl : IVRHandlerPointer
    {

        public LineRenderer lineRender;
        public Transform pointer;
        public MeshRenderer pointerMesh;
        public Texture pointerNormal;
        public Texture pointerTap;
        public Texture pointerLoading;

        [Tooltip("Angular scale of pointer")]
        public float depthScaleMultiplier = 0.03f;

        /// <summary>
        /// Current scale applied to pointer
        /// </summary>
        public float currentScale { get; private set; }

        /// <summary>
        /// Current depth of pointer from camera
        /// </summary>
        private float depth;
        /// <summary>
        /// How many times position has been set this frame. Used to detect when there are no position sets in a frame.
        /// </summary>
        private int positionSetsThisFrame = 0;
        /// <summary>
        /// Position last frame.
        /// </summary>
        private Vector3 lastPosition;
        private Vector3 pointerResetPos = Vector3.zero;
        private Vector3 pointerStartPos = Vector3.zero;

        private float startTime = 0;

        void Start()
        {
            pointerStartPos = pointer.localPosition;
            IVRH2Event.ButtonEvent_onPressDown += OnPressDown;
            IVRH2Event.ButtonEvent_onPressUp += OnPressUp;
            IVRH2Event.ButtonEvent_onPress += OnPress;
            pointerMesh.material.mainTexture = pointerTap;
            pointerMesh.material.SetVector("_TintColor", new Vector4(0.5f, 0.5f, 0.5f, 0.5f * 0.8f));
        }
        void OnDestroy()
        {
            IVRH2Event.ButtonEvent_onPressDown -= OnPressDown;
            IVRH2Event.ButtonEvent_onPressUp -= OnPressUp;
            IVRH2Event.ButtonEvent_onPress -= OnPress;
        }
        /// <summary>
        /// Set position and orientation of pointer
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="normal"></param>
        public override void SetPosition(Vector3 pos, Vector3 normal)
        {
            if (!isActiveAndEnabled) return;
            pointer.position = pos;
            pointerStartPos = pointer.localPosition;
            //pointer.gameObject.SetActive(true);
            //lineRender.SetPosition(1, new Vector3(0, 0, (pos - transform.position).magnitude));

            // Set the rotation to match the normal of the surface it's on. For the other degree of freedom use
            // the direction of movement so that trail effects etc are easier
            Quaternion newRot = pointer.rotation;
            newRot.SetLookRotation(normal, (lastPosition - pointer.position).normalized);
            if (startTime == 0)
            {
                //pointer.rotation = Quaternion.Slerp(pointer.rotation, newRot, 0.1f);
                pointer.rotation = newRot;
            }

            // record depth so that distance doesn't pop when pointer leaves an object
            depth = (transform.position - pos).magnitude;

            //set scale based on depth
            currentScale = depth * depthScaleMultiplier;
            pointer.localScale = new Vector3(currentScale, currentScale, currentScale);

            positionSetsThisFrame++;
        }

        public override void Reset()
        {
            IVRInputHandler.UpdateModule();
            pointerResetPos = new Vector3(0, IVRInputHandler.GetRotation().y, 0);
        }

        public override void ResetTo(Vector3 rotation)
        {
            IVRInputHandler.UpdateModule();
            pointerResetPos = new Vector3(0, IVRInputHandler.GetRotation().y + rotation.y, 0);
        }

        public void OnApplicationPause(bool pause)
        {
            if (!pause)
            {
                StartCoroutine(Resume());
            }
        }

        private IEnumerator Resume()
        {
            IVRInputHandler.UpdateModule();
            while (IVRInputHandler.GetRotation() == Vector3.zero)
            {
                yield return null;
            }
            Reset();
        }

        void LateUpdate()
        {
            // This happens after all Updates so we know nothing set the position this frame
            if (positionSetsThisFrame == 0)
            {
                // No geometry intersections, so gazing into space. Make the cursor face directly at the camera
                Quaternion newRot = pointer.rotation;
                newRot.SetLookRotation(transform.forward, (lastPosition - pointer.position).normalized);
                if (startTime == 0)
                {
                    pointer.rotation = newRot;
                }

                pointer.localPosition = pointerStartPos;
                currentScale = pointerStartPos.z * depthScaleMultiplier;
                pointer.localScale = new Vector3(currentScale, currentScale, currentScale);
                //pointer.gameObject.SetActive(false);
                //lineRender.SetPosition(1, new Vector3(0, 0, 100));
            }
            // Keep track of cursor movement direction
            //_positionDelta = pointer.position - lastPosition;
            lastPosition = pointer.position;

            positionSetsThisFrame = 0;

            transform.localEulerAngles = pointerResetPos - IVRInputHandler.GetRotation();
        }

        void OnPressDown(ControllerButton type)
        {
            if ((type & ControllerButton.CONTROLLER_BUTTON_APP) != 0)
            {
                startTime = Time.unscaledTime;
            }
            pointerMesh.material.mainTexture = pointerTap;
            pointerMesh.material.SetVector("_TintColor", new Vector4(0.5f, 0.5f, 0.5f, 0.5f));
        }

        void OnPress(ControllerButton type)
        {
            if ((type & ControllerButton.CONTROLLER_BUTTON_APP) == 0) return;
            if (startTime == 0) return;
            float time = Time.unscaledTime - startTime - 0.5f;
            if (time < 0.5f && time > 0)
            {
                if (pointerMesh.material.mainTexture != pointerLoading)
                {
                    pointerMesh.material.mainTexture = pointerLoading;
                    pointer.localEulerAngles = new Vector3(pointer.localEulerAngles.x, 0, 0);
                }

                float radio = Mathf.Lerp(360, 0, time * 2.0f);
                //Debug.Log(radio);
                pointerMesh.material.SetFloat("_FillAmount", radio);
            }
            else if (time > 0.5f)
            {
                pointerMesh.material.mainTexture = pointerTap;
                pointerMesh.material.SetFloat("_FillAmount", 360);
            }
        }

        void OnPressUp(ControllerButton type)
        {
            if ((type & ControllerButton.CONTROLLER_BUTTON_APP) != 0)
            {
                startTime = 0;
            }
            pointerMesh.material.mainTexture = pointerTap;
            pointerMesh.material.SetFloat("_FillAmount", 360);
            pointerMesh.material.SetVector("_TintColor", new Vector4(0.5f, 0.5f, 0.5f, 0.5f * 0.8f));
        }

    }
}
