
/********************************************************************
     Copyright (c) XTech Cloud
     All rights reserved.
*********************************************************************/

using UnityEngine;
using UnityEngine.EventSystems;
using XTC.MegeXR.Core;

namespace XTC.MegeXR.Decorator
{
    public class PointerInputDecorator : XPointerInputModule
    {
        protected override void Awake()
        {
            setup();
        }

        public override bool ShouldActivateModule()
        {
            return shouldActivateModule();
        }

        public override void DeactivateModule()
        {
            deactivateModule();
        }

        public override bool IsPointerOverGameObject(int _pointerId)
        {
            return isPointerOverGameObject(_pointerId);
        }

        public override void Process()
        {
            process();
        }
    }//class
}//namespace 
