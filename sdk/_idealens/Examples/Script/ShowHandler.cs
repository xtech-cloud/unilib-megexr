using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class ShowHandler : MonoBehaviour, IPointerClickHandler
{

    public Image TouchPad;
    public Image HomeButton;
    public Image AppButton;
    public Image MinusButton;
    public Image AddButton;
    public Image TriggerButton;

    public Color ClickColor;
    public Color TriggerPushColor;
    public Color TriggerClickColor;
	
	// Update is called once per frame
	void Update () {
        IVRHandlerInputModule inputModule = EventSystem.current.currentInputModule
            as IVRHandlerInputModule;
        IVRInputModule test = EventSystem.current.currentInputModule as IVRInputModule;
        //Debug.Log(test);
        if (inputModule != null)
        {
            TouchPad.color = IVR.IVRInputHandler.GetKey(IVR.ControllerButton.CONTROLLER_BUTTON_TP_CLICK) ?
                 ClickColor : Color.white;
            HomeButton.color = IVR.IVRInputHandler.GetKey(IVR.ControllerButton.CONTROLLER_BUTTON_HOME) ?
                 ClickColor : Color.white;
            AppButton.color = IVR.IVRInputHandler.GetKey(IVR.ControllerButton.CONTROLLER_BUTTON_APP) ?
                 ClickColor : Color.white;
            MinusButton.color = IVR.IVRInputHandler.GetKey(IVR.ControllerButton.CONTROLLER_BUTTON_VOL_DOWN) ?
                 ClickColor : Color.white;
            AddButton.color = IVR.IVRInputHandler.GetKey(IVR.ControllerButton.CONTROLLER_BUTTON_VOL_UP) ?
                 ClickColor : Color.white;
            TriggerButton.color = IVR.IVRInputHandler.GetKey(IVR.ControllerButton.CONTROLLER_BUTTON_TRIGGER) ? ClickColor : Color.white;
            if (IVR.IVRInputHandler.GetKey(IVR.ControllerButton.CONTROLLER_BUTTON_HOME))
            {
                IVR.IVRInputHandler.SendVibrateCmd(1);
            }
            if (IVR.IVRInputHandler.GetKey(IVR.ControllerButton.CONTROLLER_BUTTON_APP))
            {
                IVR.IVRInputHandler.SendVibrateCmd(2);
            }
            if (IVR.IVRInputHandler.GetKey(IVR.ControllerButton.CONTROLLER_BUTTON_TP_CLICK))
            {
                IVR.IVRInputHandler.SendVibrateCmd(3);
            }
        }
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        PointerEventData ped = eventData as IVRRayPointerEventData;
        Debug.Log(ped);
    }
}
