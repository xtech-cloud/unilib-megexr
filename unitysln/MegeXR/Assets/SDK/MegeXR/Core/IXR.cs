/********************************************************************
     Copyright (c) XTech Cloud
     All rights reserved.
*********************************************************************/

using UnityEngine;

namespace XTC.MegeXR.Core
{
    public interface IXR
    {
        void Preload();
        void Release();
        void AttachReticle(Transform _reticle);
        void AttachGaze(Transform _gaze);
        void Update();
        Transform root { get; }
        Transform camera { get; }
    }
}