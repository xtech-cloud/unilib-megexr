using UnityEngine;
using System.Collections;
using UnityEditor;

public class CreateIVRCamera : Editor 
{
    [MenuItem("IVRSDK/Creat IVRCamera Component", false, 10)]
    public static void CreateCamera()
    {
        GameObject obj = GameObject.Find("IVRCamera");
        if (obj == null)
        {
            //Creat IVRcamera GameObject
            obj = new GameObject();
            obj.name = "IVRCamera";
            TagHelper.AddTag("IVRCamera");
            obj.tag = "IVRCamera";
            obj.AddComponent<NeedIVRManager>();

            GameObject anchor = new GameObject();
            anchor.name = "Anchor";
            anchor.transform.parent = obj.transform;
            anchor.AddComponent<AudioListener>();
            Camera anchorcamera = anchor.AddComponent<Camera>();
            anchorcamera.enabled = false;
            anchorcamera.cullingMask = 0;

            GameObject RightEye = new GameObject();
            RightEye.name = "RightEye";
            RightEye.transform.parent = obj.transform;
            Camera rightCamera = RightEye.AddComponent<Camera>();
            rightCamera.fieldOfView = 90;
#if UNITY_5
            RightEye.AddComponent<FlareLayer>();
#endif
            GameObject LeftEye = new GameObject();
            LeftEye.name = "LeftEye";
            LeftEye.transform.parent = obj.transform;
            Camera leftCamera = LeftEye.AddComponent<Camera>();
            leftCamera.fieldOfView = 90;
#if UNITY_5
            LeftEye.AddComponent<FlareLayer>();
#endif
        }
        else
        {
            RefrashCamer(obj);
        }
        Debug.Log("ivrsdk init completion");
    }
	
    public static void RefrashCamer(GameObject ivrcamera)
    {
        Transform root = ivrcamera.transform;
        Transform anchorTras = root.Find("Anchor");
        Transform RightEyeTras = root.Find("RightEye");
        Transform LeftEyeTras = root.Find("LeftEye");
        if (!ivrcamera.GetComponent<NeedIVRManager>())
        {
            ivrcamera.AddComponent<NeedIVRManager>();
        }
        if (anchorTras == null)
        {
            GameObject anchor = new GameObject();
            anchorTras = anchor.transform;
            anchor.name = "Anchor";
            anchorTras.parent = ivrcamera.transform;
            //anchor.AddComponent<AudioListener>();
            //Camera camera = anchor.AddComponent<Camera>();
            //camera.cullingMask = -1;
            //camera.enabled = false;
        }
        if(anchorTras.GetComponent<IVRCamera>())
            DestroyImmediate(anchorTras.GetComponent<IVRCamera>());
        if (!anchorTras.GetComponent<AudioListener>())
            anchorTras.gameObject.AddComponent<AudioListener>();
        Camera anchorCamera = anchorTras.GetComponent<Camera>();


        if (!anchorCamera)
        {
            anchorCamera = anchorTras.gameObject.AddComponent<Camera>();
        }
        anchorCamera.cullingMask = 0;
        anchorCamera.enabled = false;
        
        if (RightEyeTras == null)
        {
            GameObject RightEye = new GameObject();
            RightEye.name = "RightEye";
            RightEye.transform.parent = ivrcamera.transform;
            Camera rightCamera = RightEye.AddComponent<Camera>();
            rightCamera.fieldOfView = 90;
#if UNITY_5
            RightEye.AddComponent<FlareLayer>();
#endif
        }
        if (LeftEyeTras == null)
        {
            GameObject LeftEye = new GameObject();
            LeftEye.name = "LeftEye";
            LeftEye.transform.parent = ivrcamera.transform;
            Camera leftCamera = LeftEye.AddComponent<Camera>();
            leftCamera.fieldOfView = 90;
#if UNITY_5
            LeftEye.AddComponent<FlareLayer>();
#endif
        }
    }
}