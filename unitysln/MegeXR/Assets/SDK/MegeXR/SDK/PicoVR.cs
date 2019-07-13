/********************************************************************
     Copyright (c) HsiaV
     All rights reserved.
*********************************************************************/

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using XTC.MegeXR.Core;

namespace XTC.MegeXR.SDK
{
    public class PicoVR : IXR
    {
        private Transform root_ = null;

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

        }


        private void preload()
        {
            GameObject go = Resources.Load<GameObject>("Pvr_UnitySDK");
            GameObject objRoot = GameObject.Instantiate<GameObject>(go);
            objRoot.name = "Pico";
            root_ = objRoot.transform;

            camera_ = root_.transform.Find("Head");
        }
    }//class
}//namespace

