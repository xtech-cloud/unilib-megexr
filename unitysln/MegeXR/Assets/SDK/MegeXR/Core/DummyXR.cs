/********************************************************************
     Copyright (c) XTech Cloud
     All rights reserved.
*********************************************************************/

using UnityEngine;
using XTC.Logger;

namespace XTC.MegeXR.Core
{
    public class DummyXR : IXR
    {
        private Transform root_ = null;

        private float rotationY = 0f;
        private float rotationX = 0f;
        private float sensitivityX = 0.25f;
        private float sensitivityY = 0.25f;
        private float minmumY = -60f;
        private float maxmunY = 60f;

        public Transform root
        {
            get
            {
                return root_;
            }
        }

        public Transform camera_ = null;

        public Transform camera
        {
            get
            {
                return camera_;
            }
        }

        public void Preload()
        {
            preload();
        }

        public void Release()
        {

        }

        public void AttachReticle(Transform _reticle)
        {
            _reticle.SetParent(camera_);
        }

        public void AttachGaze(Transform _gaze)
        {
            _gaze.SetParent(camera_);
        }

        public bool IsOkDown()
        {
            return Input.GetMouseButtonDown(0) || Input.GetButtonDown("Submit");
        }

        public bool IsOkUp()
        {
            return Input.GetMouseButtonUp(0) || Input.GetButtonUp("Submit");
        }

        public bool IsOkHold()
        {
            return Input.GetMouseButton(0) || Input.GetButtonUp("Submit");
        }

        public void Update()
        {
            if (0 == Input.touchCount)
            {
                float x = Input.GetAxis("Mouse X");
                float y = Input.GetAxis("Mouse Y");
                Vector3 Angle = new Vector3(y, -x, 0);
                camera_.transform.eulerAngles -= Angle;
            }
            else
            {
                Touch touch = Input.GetTouch(0);
                Vector2 deltaPos = touch.deltaPosition;

                rotationX = camera_.localEulerAngles.y + deltaPos.x * sensitivityX;
                rotationY += deltaPos.y * sensitivityY;
                rotationY = clamp(rotationY, maxmunY, minmumY);
                camera_.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
            }
        }

        private void preload()
        {
            this.LogInfo("preload DummyVR");

            GameObject objRoot = new GameObject("Dummy");
            root_ = objRoot.transform;

            GameObject objHead = new GameObject("Head");
            camera_ = objHead.transform;
            objHead.transform.SetParent(root_.transform);
            resetTransform(objHead.transform);

            buildHead(objHead);
        }


        private void resetTransform(Transform _transform)
        {
            _transform.localPosition = Vector3.zero;
            _transform.localRotation = Quaternion.identity;
            _transform.localScale = Vector3.one;
        }

        private void buildHead(GameObject _head)
        {
            Camera camera = _head.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.backgroundColor = new Color(49, 77, 121, 5);
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 1000;
            camera.depth = 0;
            camera.useOcclusionCulling = true;
            camera.allowHDR = false;
            camera.allowMSAA = true;
            camera.tag = "MainCamera";
        }

        private float clamp(float value, float max, float min)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }//class
}//namespace

