using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SvrTrackDevices : MonoBehaviour {

    public SvrControllerState deviceType;
    private GameObject vrCamera;
    private float camerayaw;
    private float noloyaw;
    private float resetyaw;
    private float presetyaw;
    private float resultyaw;
    private GvrBasePointer mPointer;
    public static List<GameObject> mTracks = new List<GameObject>();
    private void Awake()
    {
        GvrControllerInput.OnConterollerChanged += GvrControllerInput_OnConterollerChanged;
        GvrControllerInput_OnConterollerChanged(GvrControllerInput.SvrState, GvrControllerInput.SvrState);
        mTracks.Add(gameObject);
        GvrControllerInput.OnGvrPointerEnable += GvrControllerInput_OnGvrPointerEnable;
    }

    private void GvrControllerInput_OnGvrPointerEnable(bool obj)
    {

        if ((GvrControllerInput.SvrState & deviceType) != 0)
        {
            gameObject.SetActive(obj);
        }
    }

    private void GvrControllerInput_OnConterollerChanged(SvrControllerState state, SvrControllerState oldState)
    {
       

        if ((state & deviceType) != 0)
        {
            gameObject.SetActive(true);
            if (mPointer == null) mPointer = GetComponentInChildren<GvrBasePointer>();
            switch (Svr.SvrSetting.NoloHandedness)
            {
                case Svr.SvrNoloHandedness.Left:
                    if (deviceType ==  SvrControllerState.NoloLeftContoller)
                    {
                        mPointer.gameObject.SetActive(true);
                        GvrPointerInputModule.Pointer = mPointer;
                    }
                    else
                    {
                        mPointer.gameObject.SetActive(false);
                        if (GvrPointerInputModule.Pointer == mPointer)
                        {
                            GvrPointerInputModule.Pointer = null;
                        }
                    }
                    break;
                case Svr.SvrNoloHandedness.Right:
                    if (deviceType == SvrControllerState.NoloRightContoller)
                    {
                        mPointer.gameObject.SetActive(true);
                        GvrPointerInputModule.Pointer = mPointer;
                    }
                    else
                    {
                        mPointer.gameObject.SetActive(false);
                        if (GvrPointerInputModule.Pointer == mPointer)
                        {
                            GvrPointerInputModule.Pointer = null;
                        }
                    }
                    break;
                default:
                    break;
            }


        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    void Start()
    {

    }
    void Update()
    {
       
        UpdatePose();
    }

    private void OnDestroy()
    {
        GvrControllerInput.OnGvrPointerEnable -= GvrControllerInput_OnGvrPointerEnable;
        GvrControllerInput.OnConterollerChanged -= GvrControllerInput_OnConterollerChanged;
        mTracks.Clear();
    }

    void UpdatePose()
    {

        var pose = GvrControllerInput.GetPosition(deviceType);
        var roation = GvrControllerInput.GetOrientation(deviceType);
        if (!GvrHead.TrackPosition)
        {
            Vector3 headPos = GvrControllerInput.GetPosition(SvrControllerState.NoloHead);
            transform.localPosition = pose + new Vector3(-headPos.x, -headPos.y, -headPos.z);
        }
        else
            transform.localPosition = pose;
        transform.localRotation = roation;
    }
}
