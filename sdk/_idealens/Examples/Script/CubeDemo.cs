using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;
using IVR;

public class CubeDemo : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        IVRHandlerInputModule input = EventSystem.current.currentInputModule as IVRHandlerInputModule;
        IVRHandlerEventData pointerData = eventData as IVRHandlerEventData;
        if (null != input && null != pointerData)
        {
            IVRManager.Instance.Show("Hold the button to move cube");
            transform.parent = input.handlerPointer.transform;
        }
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        IVRHandlerEventData pointerData = eventData as IVRHandlerEventData;
        if (null != pointerData)
        {
            transform.parent = null;
        }
    }

    void Start()
    {
        IVRH2Event.ButtonEvent_onClick += OnClick;
        IVRH2Event.ButtonEvent_onLongPress += OnLongPress;
        //IVRH2Event.ButtonEvent_onPressDown += OnPressDown;
        //IVRH2Event.ButtonEvent_onPressUp += OnPressUp;
        //IVRH2Event.ButtonEvent_onPress += OnPress;
    }

    void Update()
    {
        
    }

    void OnClick(ControllerButton btn)
    {
        Debug.Log("OnClick " + btn);
    }

    void OnLongPress(ControllerButton btn)
    {
        Debug.Log("OnLongPress " + btn);
    }

    void OnPressDown(ControllerButton btn)
    {
        Debug.Log("OnPressDown " + btn);
    }

    void OnPressUp(ControllerButton btn)
    {
        Debug.Log("OnPressUp " + btn);
    }

    void OnPress(ControllerButton btn)
    {
        Debug.Log("OnPress " + btn);
    }
}
