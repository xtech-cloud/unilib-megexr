/********************************************************************
     Copyright (c) XTech Cloud
     All rights reserved.
*********************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace XTC.MegeXR.Core
{
    public class XReticleAgent
    {
        public string uuid = "";
        public CallBack.Invoke<float> onMangerUpdate;
        public CallBack.Invoke onPointEnter;
        public CallBack.Invoke onPointExit;
		public bool visible = true;
		public float duration = 1.0f;
    }
}//namespace