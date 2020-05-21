using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

[RequireComponent(typeof(Text))]
public class ShowInputs : MonoBehaviour,IPointerDownHandler, IPointerUpHandler {

    private Text mText = null;

	// Use this for initialization
	void Start () {
        mText = GetComponent<Text>();
        IVRTouchPad.TouchEvent_onSwipe += OnSwipe;
    }

    private string swipString = string.Empty;
	
	// Update is called once per frame
	void Update () {
        IVRHandlerInputModule inputModule = EventSystem.current.currentInputModule
            as IVRHandlerInputModule;
        if (inputModule != null)
        {
            mText.text = IVR.IVRInputHandler.GetPosition().ToString() + swipString;
        }
    }

    void OnSwipe(SwipEnum dir)
    {
        swipString = "IVRTouchpad swip : " + dir;
        //Debug.Log(swipString);
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        IVRRayPointerEventData p = eventData as IVRRayPointerEventData;
        if (p != null)
            Debug.Log("IVRTouchpad Up : " + p.TouchPadPosition);
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        IVRRayPointerEventData p = eventData as IVRRayPointerEventData;
        if (p != null)
            Debug.Log("IVRTouchpad Down : " + p.TouchPadPosition);
    }
}
