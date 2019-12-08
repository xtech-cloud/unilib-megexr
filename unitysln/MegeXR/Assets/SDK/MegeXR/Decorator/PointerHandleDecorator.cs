
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

    public class PointerHandleDecorator : MonoBehaviour
    {
        public XPointerHandler handler { get; private set; }

        void Awake()
        {
            Setup();
        }

        void OnDestory()
        {

        }

        public void Setup()
        {
            if(null != handler)
                return;
            handler = new XPointerHandler();
            handler.owner = this.gameObject;
            handler.Setup();
        }
    }
}//namespace
