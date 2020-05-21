using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;

[RequireComponent(typeof(UnityEngine.UI.Text))]
public class ShowInputInfo : MonoBehaviour, IOnHoverHandler, IDragHandler, IScrollHandler, 
    IPointerClickHandler, IBeginDragHandler, IPointerDownHandler, IPointerUpHandler
{

    private static ShowInputInfo mInstance;
    public static ShowInputInfo Instance
    {
        get { return mInstance; }
    }

    private bool isHover = false;
    private UnityEngine.UI.Text mText;

    // Use this for initialization
    void Start () {
        mInstance = this;
        mText = GetComponent<UnityEngine.UI.Text>();
    }

	// Update is called once per frame
	void Update () {
        mText.text = OnHoverString + "\n" + DragString + "\n" + ScrollString + "\n"
            + "Click Count: " + clickCount.ToString() +"\n" + PointerString;
    }

    public void SetShowInfo(string msg)
    {
        if (!isHover)
            mText.text += msg;
    }

    private string OnHoverString = "Try to hove me!";
    private string DragString = string.Empty;
    private string ScrollString = string.Empty;
    private int clickCount = 0;
    private string PointerString = string.Empty;

    void IOnHoverHandler.OnHover(IVRRayPointerEventData eventData)
    {
        if (eventData.HitResults.Contains(gameObject))
        {
            isHover = true;
            OnHoverString = "hit point : " + eventData.HitPoints[eventData.HitResults.IndexOf(gameObject)].ToString();
        }
        else
        {
#if UNITY_EDITOR
            OnHoverString = "Better Try this on device!";
#else
            OnHoverString = "Try to hove me!";
#endif
            isHover = false;
            DragString = string.Empty;
            ScrollString = string.Empty;
        }
    }

    private Vector2 startDragPosition = Vector3.zero;

    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        IVRRayPointerEventData pointer = eventData as IVRRayPointerEventData;
        DragString = "Drag:" + (pointer.TouchPadPosition - startDragPosition).ToString();
    }

    void IScrollHandler.OnScroll(PointerEventData eventData)
    {
        ScrollString = "Scroll:" + eventData.scrollDelta;
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        clickCount++;
    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
        IVRRayPointerEventData pointer = eventData as IVRRayPointerEventData;
        startDragPosition = pointer.TouchPadPosition;
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        IVRRayPointerEventData p = eventData as IVRRayPointerEventData;
        if (p != null)
        {
            PointerString = "IVRTouchpad Up : " + p.TouchPadPosition;
        }
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        IVRRayPointerEventData p = eventData as IVRRayPointerEventData;
        if (p != null)
        {
            PointerString = "IVRTouchpad Down : " + p.TouchPadPosition;
        }
    }
}
