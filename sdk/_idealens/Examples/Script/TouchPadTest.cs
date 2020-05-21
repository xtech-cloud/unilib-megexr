using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TouchPadTest : MonoBehaviour {

    public Image m_Pointer;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
#if !UNITY_EDITOR
        Vector3 pos = Vector3.zero;
        IVRHandlerInputModule inputModule = EventSystem.current.currentInputModule
            as IVRHandlerInputModule;
        if (inputModule != null)
        {
            pos = IVR.IVRInputHandler.GetPosition();
            pos.y = -pos.y;
        }
#else
        Vector3 pos = Input.mousePosition;
#endif //!UNITY_EDITOR
        if (pos == Vector3.zero)
        {
            m_Pointer.enabled = false;
        }
        else
        {
            m_Pointer.enabled = true;
            m_Pointer.transform.localPosition = pos + new Vector3(-147, 147, 0);
            //m_Pointer.rectTransform.anchoredPosition3D = pos + new Vector3(-26, 26, 0);
        }

    }
}
