using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(AnchorDynamic))]
public class AnchorDynamicEditor : Editor {

    private float Length;
    private bool isInit = false;
    private Vector3 mInitLocalScal;
    private Vector3 pleftup, prightup, pleftdown, prightdown;
    private SerializedProperty SinitScal;
    void OnEnable()
    {
        SinitScal = serializedObject.FindProperty("initScal");
    }
    void OnSceneGUI()
    {
        AnchorDynamic ad = target as AnchorDynamic;
        if (ad.transform.parent == null)
        {
            return;
        }
        Vector3 pos = ad.transform.localPosition;
        pos.x = pos.y = 0;
        ad.transform.localPosition = pos;

        if (!isInit)
        {
            Length = ad.depth;
            mInitLocalScal = new Vector3(ad.initScal, ad.initScal, ad.initScal);
            isInit = true;
        }

        Vector3 currentPos = ad.transform.localPosition;
        float scaleDivisor = Vector3.Distance(currentPos, Vector3.zero) / Length;
        Vector3 targetScale = mInitLocalScal * scaleDivisor;
        ad.transform.localScale = targetScale;

        ad.transform.localRotation = Quaternion.identity;
        Handles.DrawLine(ad.transform.parent.position, ad.transform.position);
        if (Application.isPlaying)
        {
            return;
        }
        pleftup = ad.transform.position - ad.transform.right * ad.Width / 2 + ad.transform.up * ad.Height / 2;
        prightup = ad.transform.position + ad.transform.right * ad.Width / 2 + ad.transform.up * ad.Height / 2;
        pleftdown = ad.transform.position - ad.transform.right * ad.Width / 2 - ad.transform.up * ad.Height / 2;
        prightdown = ad.transform.position + ad.transform.right * ad.Width / 2 - ad.transform.up * ad.Height / 2;
        Handles.DrawLine(ad.transform.parent.position, pleftup);
        Handles.DrawLine(ad.transform.parent.position, prightup);
        Handles.DrawLine(ad.transform.parent.position, pleftdown);
        Handles.DrawLine(ad.transform.parent.position, prightdown);
        Handles.DrawLine(pleftup, prightup);
        Handles.DrawLine(prightup, prightdown);
        Handles.DrawLine(prightdown, pleftdown);
        Handles.DrawLine(pleftdown, pleftup);



    }
}
