using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoundaryRegion_Manager : MonoBehaviour
{

    public GameObject tips;
    //private bool isOutOfRange = false;
    //public float m_safety_distance = 1.5f;
    public float m_safety_radius = 2.0f;
    //private int basestationFOV = 100;
    //[SerializeField]
    //private NoloVR_TrackedDevice m_HMDDevice;
    [SerializeField]
    private bool showTrackingBoundary = false;

    private float m_safety_height;
    public bool ShowTrackingBoundary
    {
        get
        {
            return showTrackingBoundary;
        }
        set
        {
            showTrackingBoundary = value;
            if (tips != null) tips.SetActive(showTrackingBoundary);
            if (enabled != showTrackingBoundary)
                enabled = showTrackingBoundary;
        }
    }
    private bool IsOutOfRange;
    private bool PreOutofRange;
    private void Start()
    {
        enabled = showTrackingBoundary;
        if (tips != null) tips.SetActive(false);
        transform.GetChild(0).localScale = Vector3.one * m_safety_radius;
        transform.GetChild(0).gameObject.SetActive(false);
        //m_safety_height = Mathf.Tan(basestationFOV * 0.5f) * m_safety_distance * 2;
        GvrControllerInput.OnConterollerChanged += GvrControllerInput_OnConterollerChanged;
        
    }

    private void GvrControllerInput_OnConterollerChanged(SvrControllerState state, SvrControllerState oldState)
    {
        if ((state & (SvrControllerState.NoloHead)) == 0)
        {
            InRange();
        }
    }
    private void OnDestroy()
    {
        GvrControllerInput.OnConterollerChanged -= GvrControllerInput_OnConterollerChanged;
    }
    private void Update()
    {
//#if UNITY_EDITOR
//#else
        //if (!NoloVR_Plugins.API.GetPoseByDeviceType((int)NoloDeviceType.Hmd).bDeviceIsConnected) return;


        //if (CheckHorizontal(NoloVR_System.GetInstance().objects[i].gameObject))
        //{
        //    NOLO_Events.Send(NOLO_Events.EventsType.TrackingOutofRange);
        //    OutOfRange();
        //    break;
        //}
        //else
        //{
        //    NOLO_Events.Send(NOLO_Events.EventsType.TrackingInRange);
        //    InRange();
        //}
        
        if (CheckHorizontal(SvrTrackDevices.mTracks))
        {
            IsOutOfRange = true;
            if (PreOutofRange != IsOutOfRange)
            {
                //if (OnTrackingRangeEvent != null) OnTrackingRangeEvent.Invoke(EventsType.TrackingOutofRange);
                //m_Region.SetActive(true);
                PreOutofRange = IsOutOfRange;
                //RangeState = EventsType.TrackingOutofRange;
                //NOLO_Events.Send(NOLO_Events.EventsType.TrackingOutofRange);
                OutOfRange();
            }
        }
        else
        {
            IsOutOfRange = false;
            if (PreOutofRange != IsOutOfRange)
            {
                //if (OnTrackingRangeEvent != null) OnTrackingRangeEvent.Invoke(EventsType.TrackingInRange);
                //m_Region.SetActive(false);
                PreOutofRange = IsOutOfRange;
                //RangeState = EventsType.TrackingInRange;
                //NOLO_Events.Send(NOLO_Events.EventsType.TrackingInRange);
                InRange();
            }

        }
//#endif
    }
    void OnEnable()
    {
        //NOLO_Events.Listen(NOLO_Events.EventsType.TrackingOutofRange, OutOfRange);
        //NOLO_Events.Listen(NOLO_Events.EventsType.TrackingInRange, InRange);
    }
    void OnDisable()
    {
        //NOLO_Events.Remove(NOLO_Events.EventsType.TrackingOutofRange, OutOfRange);
        //NOLO_Events.Remove(NOLO_Events.EventsType.TrackingInRange, InRange);
    }

    void OutOfRange()
    {
        //do out of range
        if (tips) tips.SetActive(true);
        transform.GetChild(0).gameObject.SetActive(true);
        //isOutOfRange = true;
    }
    void InRange()
    {
        //if (isOutOfRange)
        //{
        //do in range
        if (tips) tips.SetActive(false);
        transform.GetChild(0).gameObject.SetActive(false);
        //    isOutOfRange = false;
        //}
    }

    //private bool CheckHeight(GameObject target)
    //{
    //    Vector3 vectrors = (target.transform.localPosition - transform.localPosition).normalized;
    //    float angle = Vector3.Angle(vectrors, -Vector3.forward);
    //    if (angle * 2 > basestationFOV) return false;
    //    else return true;
    //}

    //private bool CheckHorizontal(GameObject target)
    //{
    //    Vector3 rouncenteral = transform.localPosition - transform.forward * m_safety_distance;

    //    Vector3 positiveCenterPos = rouncenteral;
    //    positiveCenterPos.y = target.transform.localPosition.y;

    //    float distance = Vector3.Distance(target.transform.localPosition, positiveCenterPos);

    //    if (distance > m_safety_radius) return false;
    //    else return true;
    //}
    private bool CheckHorizontal(List<GameObject> targets)
    {

        float MaxDistance = 0;
        foreach (var item in targets)
        {
            Vector3 positiveCenterPos = Vector3.zero;
            positiveCenterPos.y = item.transform.localPosition.y;
            float distance = Vector3.Distance(item.transform.localPosition, positiveCenterPos);
            if (MaxDistance < distance) MaxDistance = distance;
        }


        //Svr.SvrLog.Log("distance:"+ MaxDistance+","+ m_safety_radius);
        return MaxDistance > (m_safety_radius - 0.5f);
    }

    private void OnValidate()
    {
        ShowTrackingBoundary = showTrackingBoundary;
    }
}
