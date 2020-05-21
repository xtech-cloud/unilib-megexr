using UnityEngine;
using System.Collections;

public class IVRK2EventDemo : MonoBehaviour {

    private GameObject mCube;

    void Start()
    {
#if UNITY_EDITOR
        Debug.LogWarning("Try this demo on device!");
#endif
        mCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mCube.transform.position = new Vector3(0, 0, 6);

    }

    void Update()
    {
        //Because the IVRInputModule is not used, IVRK2Event should be update every frame
        IVR.IVRK2Event.Update(Time.unscaledTime);
    }

    void OnEnable()
    {
        //IVRK2Event will only respond for the input on touchpad of K2.
        IVR.IVRK2Event.TouchEvent_onSingleTap += OnSingleTap;
        IVR.IVRK2Event.TouchEvent_onSwipe += OnSwipe;
    }

    void OnDisable()
    {
        IVR.IVRK2Event.TouchEvent_onSingleTap -= OnSingleTap;
        IVR.IVRK2Event.TouchEvent_onSwipe -= OnSwipe;
    }

    void OnSingleTap()
    {
        mCube.SetActive(!mCube.activeInHierarchy);
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
