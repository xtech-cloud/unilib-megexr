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
    public class IdealensVR : IXR
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

        public void Update()
        {
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                XKeyHandler.DownKey((int)XKeyHandler.Key.RETURN);
            }
            if(Input.GetKeyDown(KeyCode.Joystick1Button0))
            {
                XKeyHandler.DownKey((int)XKeyHandler.Key.OK);
            }
            if(Input.GetKeyDown(KeyCode.Home))
            {
                XKeyHandler.DownKey((int)XKeyHandler.Key.HOME);
            }

            if(Input.GetKeyUp(KeyCode.Escape))
            {
                XKeyHandler.UpKey((int)XKeyHandler.Key.RETURN);
            }
            if(Input.GetKeyUp(KeyCode.Joystick1Button0))
            {
                XKeyHandler.UpKey((int)XKeyHandler.Key.OK);
            }
            if(Input.GetKeyUp(KeyCode.Home))
            {
                XKeyHandler.UpKey((int)XKeyHandler.Key.HOME);
            }

            if(Input.GetKey(KeyCode.Escape))
            {
                XKeyHandler.HoldKey((int)XKeyHandler.Key.RETURN);
            }
            if(Input.GetKey(KeyCode.Joystick1Button0))
            {
                XKeyHandler.HoldKey((int)XKeyHandler.Key.OK);
            }
            if(Input.GetKey(KeyCode.Home))
            {
                XKeyHandler.HoldKey((int)XKeyHandler.Key.HOME);
            }
        }


        private void preload()
        {
            GameObject go = Resources.Load<GameObject>("IdealensVR");
            GameObject objRoot = GameObject.Instantiate<GameObject>(go);
            objRoot.name = "Idealens";
            root_ = objRoot.transform;

            camera_ = root_.transform.Find("Head/Anchor");
        }
    }//class
}//namespace

