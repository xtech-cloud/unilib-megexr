using UnityEngine;
using System.Collections;

public class IVRH2EventDemo : MonoBehaviour {

    private GameObject mCube;

    void Start()
    {
#if UNITY_EDITOR
        Debug.LogWarning("Try this demo on device!");
#endif
        mCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mCube.transform.position = new Vector3(0, 0, 6);
        //Because the IVRHandlerInputModule is not used, IVRInputHandler should be actived when start
        IVR.IVRInputHandler.Activate();
        //This is important, the current handler status should be checked by invoke IVRInputHandler.CheckUpdate(), it's recommended to check this every frame
        //If IVRHandlerInputModule is used, this will not be a concern
        //Be aware, IVRInputHandler.IsConnected() could only be updated by IVRInputHandler.CheckState()
        StartCoroutine(CheckUpdate());
    }

    void OnApplicationQuit()
    {
        //Because the IVRHandlerInputModule is not used, IVRInputHandler should be deactived when quit
        IVR.IVRInputHandler.Deactivate();
    }

    void OnApplicationPause(bool Pause)
    {
        //Because the IVRHandlerInputModule is not used, IVRInputHandler should be paused when application pause
        IVR.IVRInputHandler.OnPause(Pause);
    }

    IEnumerator CheckUpdate()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            IVR.IVRInputHandler.CheckState();
        }
    }

    void Update()
    {
        //Because the IVRHandlerInputModule is not used, IVRH2Event and IVRInputHandler should be update every frame
        IVR.IVRH2Event.Update(Time.unscaledTime);
        //If IVRInputHandler.IsConnected() return false, IVRInputHandler.UpdateModule() will not get the newest data.
        //IVRInputHandler.IsConnected() could only be updated by IVRInputHandler.CheckState()
        if (IVR.IVRInputHandler.IsConnected())
        {
            IVR.IVRInputHandler.UpdateModule();
            mCube.transform.eulerAngles = IVR.IVRInputHandler.GetRotation();
        }
        else
        {
            Debug.Log("Hanlder is not connected! ");
        }
    }

    void OnEnable()
    {
        //IVRH2Event will only respond for the input on handler of H2.
        IVR.IVRH2Event.ButtonEvent_onClick += OnClick;
        IVR.IVRH2Event.ButtonEvent_onSwipe += OnSwipe;
    }

    void OnDisable()
    {
        IVR.IVRH2Event.ButtonEvent_onClick -= OnClick;
        IVR.IVRH2Event.ButtonEvent_onSwipe -= OnSwipe;
    }

    void OnClick(IVR.ControllerButton btn)
    {
        if ((btn & IVR.ControllerButton.CONTROLLER_BUTTON_APP) != 0)
        {
            mCube.SetActive(!mCube.activeInHierarchy);
        }
        else if ((btn & IVR.ControllerButton.CONTROLLER_BUTTON_TP_CLICK) != 0)
        {
            mCube.transform.localScale = Vector3.one * 1.5f;
        }
        else if ((btn & IVR.ControllerButton.CONTROLLER_BUTTON_TRIGGER) != 0)
        {
            mCube.transform.localScale = Vector3.one * 1f;
        }
    }

    void OnSwipe(SwipEnum dir)
    {
        Vector3 pos = mCube.transform.position;
        switch (dir)
        {
            case SwipEnum.MOVE_BACK:
                pos.x -= 1;
                break;
            case SwipEnum.MOVE_DOWN:
                pos.y -= 1;
                break;
            case SwipEnum.MOVE_FOWRAD:
                pos.x += 1;
                break;
            case SwipEnum.MOVE_UP:
                pos.y += 1;
                break;
        }
        mCube.transform.position = pos;
    }

}
