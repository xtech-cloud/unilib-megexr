/********************************************************************
     Copyright (c) XTech Cloud
     All rights reserved.
*********************************************************************/

using UnityEngine;
using XTC.Logger;

namespace XTC.MegeXR.Core
{
    public static class Engine
    {
        public static IXR xr { get; private set; }
        public static Transform reticle {get; private set;}
        public static Transform canvas3D {get; private set;}

        public static void InjectXR(IXR _xr)
        {
            Log.Info("Engine.InjectXR", _xr);
            xr = _xr;
        }

        public static void InjectReticle(Transform _reticle)
        {
            Log.Info("Engine.InjectReticle", _reticle);
            reticle = _reticle;
        }

        public static void InjectCanvas3D(Transform _canvas3D)
        {
            Log.Info("Engine.InjectCanvas3D", _canvas3D);
            canvas3D = _canvas3D;
        }

        public static void Preload()
        {
            Log.Info("Engine.Preload", "Preload ...");
            preload();
        }

        public static void Initialize()
        {
            Log.Info("Engine.Initialize", "Initialize ...");
            initialize();
        }

        public static void Run()
        {
            Log.Info("Engine.Run", "Run ...");
            run();
        }

        public static void Update()
        {
            update();
        }

        public static void Stop()
        {
            Log.Info("Engine.Stop", "Stop ...");
            stop();
        }

        public static void Release()
        {
            Log.Info("Engine.Release", "Release ...");
            release();
        }

        private static void preload()
        {
            xr.Preload();
            xr.AttachReticle(reticle);
            reticle.localPosition = Vector3.zero;
            reticle.localRotation = Quaternion.identity;
            reticle.localScale = Vector3.one;

        }

        private static void initialize()
        {
            canvas3D.GetComponent<Canvas>().worldCamera = xr.camera.GetComponent<Camera>();
        }

        private static void run()
        {
        }

        private static void update()
        {
            canvas3D.position = xr.camera.position;
            xr.Update();
        }

        private static void stop()
        {
           
        }

        private static void release()
        {
            xr.Release();
        }
    }//class
}//namespace