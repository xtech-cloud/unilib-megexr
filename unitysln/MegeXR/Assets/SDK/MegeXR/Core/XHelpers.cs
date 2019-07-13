/********************************************************************
     Copyright (c) XTech Cloud
     All rights reserved.
*********************************************************************/

using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

using UnityEngine.XR;

namespace XTC.MegeXR.Core
{

    /// Helper functions common to GVR VR applications.
    public static class XHelpers
    {
        public static Vector2 GetViewportCenter()
        {
            int viewportWidth = Screen.width;
            int viewportHeight = Screen.height;
            if (XRSettings.enabled)
            {
                viewportWidth = XRSettings.eyeTextureWidth;
                viewportHeight = XRSettings.eyeTextureHeight;
            }

            return new Vector2(0.5f * viewportWidth, 0.5f * viewportHeight);
        }

       

        public static float GetRecommendedMaxLaserDistance(XBasePointer.RaycastMode mode)
        {
            switch (mode)
            {
                case XBasePointer.RaycastMode.Direct:
                    return 20.0f;
                case XBasePointer.RaycastMode.Hybrid:
                    return 1.0f;
                case XBasePointer.RaycastMode.Camera:
                default:
                    return 0.75f;
            }
        }

        public static float GetRayIntersection(XBasePointer.RaycastMode mode)
        {
            switch (mode)
            {
                case XBasePointer.RaycastMode.Direct:
                    return 0.0f;
                case XBasePointer.RaycastMode.Hybrid:
                    return 0.0f;
                case XBasePointer.RaycastMode.Camera:
                default:
                    return 2.5f;
            }
        }

        public static bool GetShrinkLaser(XBasePointer.RaycastMode mode)
        {
            switch (mode)
            {
                case XBasePointer.RaycastMode.Direct:
                    return false;
                case XBasePointer.RaycastMode.Hybrid:
                    return true;
                case XBasePointer.RaycastMode.Camera:
                default:
                    return false;
            }
        }

        public static Vector3 GetIntersectionPosition(Camera cam, RaycastResult raycastResult)
        {
            // Check for camera
            if (cam == null)
            {
                return Vector3.zero;
            }

            float intersectionDistance = raycastResult.distance + cam.nearClipPlane;
            Vector3 intersectionPosition = cam.transform.position + cam.transform.forward * intersectionDistance;
            return intersectionPosition;
        }

        public static Vector2 NormalizedCartesianToSpherical(Vector3 cartCoords)
        {
            cartCoords.Normalize();

            if (cartCoords.x == 0)
            {
                cartCoords.x = Mathf.Epsilon;
            }

            float polar = Mathf.Atan(cartCoords.z / cartCoords.x);

            if (cartCoords.x < 0)
            {
                polar += Mathf.PI;
            }

            float elevation = Mathf.Asin(cartCoords.y);
            return new Vector2(polar, elevation);
        }

        public static float EaseOutCubic(float min, float max, float value)
        {
            if (min > max)
            {
                Debug.LogError("Invalid values passed to EaseOutCubic, max must be greater than min. " +
                  "min: " + min + ", max: " + max);
                return value;
            }

            value = Mathf.Clamp01(value);
            value -= 1.0f;
            float delta = max - min;
            float result = delta * (value * value * value + 1.0f) + min;
            return result;
        }

        /// Converts a float array of length 16 into a column-major 4x4 matrix.
        public static Matrix4x4 ConvertFloatArrayToMatrix(float[] floatArray)
        {
            Matrix4x4 result = new Matrix4x4();

            if (floatArray == null || floatArray.Length != 16)
            {
                throw new System.ArgumentException("floatArray must not be null and have a length of 16.");
            }

            result[0, 0] = floatArray[0];
            result[1, 0] = floatArray[1];
            result[2, 0] = floatArray[2];
            result[3, 0] = floatArray[3];
            result[0, 1] = floatArray[4];
            result[1, 1] = floatArray[5];
            result[2, 1] = floatArray[6];
            result[3, 1] = floatArray[7];
            result[0, 2] = floatArray[8];
            result[1, 2] = floatArray[9];
            result[2, 2] = floatArray[10];
            result[3, 2] = floatArray[11];
            result[0, 3] = floatArray[12];
            result[1, 3] = floatArray[13];
            result[2, 3] = floatArray[14];
            result[3, 3] = floatArray[15];

            return result;
        }
    }//class
}//namespace XVP.VR
