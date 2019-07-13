
/********************************************************************
     Copyright (c) XTech Cloud
     All rights reserved.
*********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using XTC.MegeXR.Core;

namespace XTC.MegeXR.Decorator
{

    public class ReticlePointerDecorator : MonoBehaviour
    {
        public XBasePointer pointer { get; private set; }
        private XReticleCountdown countdown_ = null;

        void Awake()
        {
            pointer = new XReticlePointer();
            (pointer as XReticlePointer).owner = this.transform;

            Transform countdown = transform.Find("ReticleCountdown");
            countdown_ = new XReticleCountdown();
            countdown_.reticlePointer = pointer as XReticlePointer;
            countdown_.owner = countdown;
            countdown_.Setup();
            XReticleCountdown.instance = countdown_;
            pointer.Setup();
        }

        void Update()
        {
            pointer.Update();
        }

        void OnEnable()
        {
            countdown_.Enable();
        }

        void OnDisable()
        {
            countdown_.Disable();
        }

        void OnRenderObject()
        {
            countdown_.RenderObject();
        }

        void OnApplicationPause(bool pause)
        {
            countdown_.Pause(pause);
        }
    }
}//namespace
