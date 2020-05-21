/******************************************************************

** auth : xmh
** date : 2016/5/31 13:58:53
** desc : 描述
** Ver. : V1.0.0

******************************************************************/
using UnityEngine;
using System.Runtime.InteropServices;

namespace IVR
{
    public enum EyeTye
    { 
        Left,Right
    }
    public class IVROverlay : MonoBehaviour
    {
        public EyeTye eyeType;
        //public Camera LeftAnchor;
        //public Camera RightAnchor;
        private Camera mCamera;
        public enum OverlayType
        {
            None,			// Disabled the overlay
            Underlay,		// Eye buffers blend on top
            Overlay,		// Blends on top of the eye buffer
            OverlayShowLod	// Blends on top and colorizes texture level of detail
        };

#pragma warning disable 414		// The private field 'x' is assigned but its value is never used
        Matrix4x4 toOculusMatrix = Matrix4x4.Scale(new Vector3(0.5f, -0.5f, -0.5f));
        OverlayType currentOverlayType = OverlayType.Overlay;
        int texId = 0;
#pragma warning restore 414		// The private field 'x' is assigned but its value is never used

//#if UNITY_ANDROID && !UNITY_EDITOR
    //[DllImport("idealseePlugin")]
    //private static extern void IVR_SetOverlay(int texId, int eye);
//#endif
        private AndroidJavaObject mActivity;
        void Awake()
        {
            Debug.Log("Overlay Awake");
            var javaVrActivityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            mActivity = javaVrActivityClass.GetStatic<AndroidJavaObject>("currentActivity");
            // Getting the NativeTextureID/PTR synchronizes with the multithreaded renderer, which
            // causes a problem on the first frame if this gets called after the OVRDisplay initialization,
            // so do it in Awake() instead of Start().
            //texId = this.GetComponent<Renderer>().material.mainTexture.GetNativeTextureID();
            mCamera = GetComponent<Camera>();
        }

        void OnRenderObject()
        {
            //if (Input.GetKey(KeyCode.Joystick1Button0))
            //{
            //    currentOverlayType = OverlayType.None;
            //}
            //else if (Input.GetKey(KeyCode.Joystick1Button1))
            //{
            //    currentOverlayType = OverlayType.OverlayShowLod;
            //}
            //else
            //{
            //    currentOverlayType = OverlayType.Overlay;
            //}

            //int eyeNum = (Camera.current.depth == (int)RenderEventType.RightEyeEndFrame) ? 1 : 0;
            //Matrix4x4 mv_Left = LeftAnchor.worldToCameraMatrix * this.transform.localToWorldMatrix * toOculusMatrix;

            //IVR_SetOverlay(LeftAnchor.targetTexture.GetNativeTextureID(), 0);
            System.IntPtr texutre =  mCamera.targetTexture.GetNativeTexturePtr();
            mActivity.Interface_javaobjcall("SetEyeTexture", texutre.ToInt32(), (int)eyeType);
            //Matrix4x4 mv_Right = RightAnchor.worldToCameraMatrix * this.transform.localToWorldMatrix * toOculusMatrix;
            //IVR_SetOverlay(RightAnchor.targetTexture.GetNativeTextureID(), 1);
            //mActivity.Interface_javaobjcall("SetEyeTexture", RightAnchor.targetTexture.GetNativeTextureID(), 1);

        }

        //void OnRenderObject()
        //{
            // The overlay must be specified every eye frame, because it is positioned relative to the
            // current head location.  If frames are dropped, it will be time warped appropriately,
            // just like the eye buffers.

            //if (currentOverlayType == OverlayType.None)
            //{
            //    GetComponent<Renderer>().enabled = true;	// use normal renderer
            //    return;
            //}

            //GetComponent<Renderer>().enabled = false;		// render with the overlay plane instead of the normal renderer

//#if UNITY_ANDROID && !UNITY_EDITOR
//        int eyeNum = (Camera.current.depth == (int)RenderEventType.RightEyeEndFrame) ? 1 : 0;
//        Matrix4x4 mv = Camera.current.worldToCameraMatrix * this.transform.localToWorldMatrix * toOculusMatrix;

//        OVR_TW_SetOverlayPlane (texId, eyeNum, (int)currentOverlayType,
//                            mv [0, 0], mv [0, 1], mv [0, 2], mv [0, 3],
//                            mv [1, 0], mv [1, 1], mv [1, 2], mv [1, 3],
//                            mv [2, 0], mv [2, 1], mv [2, 2], mv [2, 3],
//                            mv [3, 0], mv [3, 1], mv [3, 2], mv [3, 3]);
//#endif
        //}
    }
}
