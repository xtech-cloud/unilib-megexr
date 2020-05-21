using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class DrawLine : MonoBehaviour {

    public float lineWidth = 0.01f;
    public List<Vector3> mTouchPosList;
    public GameObject mPointer;
    public Shader shader;

    private bool bTouch = false;
    private Mesh ml;
    private Material lmat;
    private List<GameObject> mTouchPointList;
    private List<GameObject> mTouchPointPool;

    // Use this for initialization
    void Start () {
        mTouchPosList = new List<Vector3>();
        mTouchPointList = new List<GameObject>();
        mTouchPointPool = new List<GameObject>();
        ml = new Mesh();
        lmat = new Material(shader);
        lmat.color = new Color(0, 0, 0, 0.3f);
    }
	
	// Update is called once per frame
	void Update () {
#if !UNITY_EDITOR
        IVRHandlerInputModule inputModule = EventSystem.current.currentInputModule
            as IVRHandlerInputModule;
        Vector3 touchPosition = Vector3.zero;
        if (inputModule != null)
        {
            touchPosition = IVR.IVRInputHandler.GetPosition();
            touchPosition.y = -touchPosition.y;
        }
        if (touchPosition == Vector3.zero)
        {
            if (bTouch) bTouch = false;
            //Draw();
        }
        else
        {
            touchPosition = touchPosition + new Vector3(-147, 147, 0);
            if (!bTouch) { bTouch = true; Reset(); }
            int iTouchPostCount = mTouchPosList.Count;
            if (iTouchPostCount == 0 || mTouchPosList[iTouchPostCount-1] != touchPosition)
            {
                if (iTouchPostCount > 0)
                {
                    AddLine(ml, MakeQuad(mTouchPosList[iTouchPostCount - 1],
                        touchPosition, lineWidth), false);
                }
                mTouchPosList.Add(touchPosition);
                AddPointer(touchPosition);
            }
        }
        Graphics.DrawMesh(ml, transform.localToWorldMatrix, lmat, 0);
#endif //!UNITY_EDITOR
    }

    void Reset()
    {
        mTouchPosList.Clear();
        ml = new Mesh();
        if (mTouchPointList.Count > 0)
        {
            foreach(var go in mTouchPointList)
            {
                RemovePointer(go);
            }
            mTouchPointList.Clear();
        }
    }

    void AddPointer(Vector3 pos)
    {
        if (mTouchPointPool.Count == 0)
        {
            GameObject p = Instantiate(mPointer);
            p.transform.parent = transform;
            p.transform.localScale = Vector3.one;
            p.transform.localPosition = pos;
            mTouchPointList.Add(p);
        }
        else
        {
            mTouchPointPool[0].SetActive(true);
            mTouchPointPool[0].transform.localPosition = pos;
            mTouchPointList.Add(mTouchPointPool[0]);
            mTouchPointPool.RemoveAt(0);
        }
    }

    void RemovePointer(GameObject p)
    {
        p.SetActive(false);
        mTouchPointPool.Add(p);
    }

    Vector3[] MakeQuad(Vector3 s, Vector3 e, float w)
    {
        w = w / 2;
        Vector3[] q = new Vector3[4];

        Vector3 n = Vector3.Cross(s, e);
        Vector3 l = Vector3.Cross(n, e - s);
        l.Normalize();

        q[0] = transform.InverseTransformPoint(s + l * w);
        q[1] = transform.InverseTransformPoint(s + l * -w);
        q[2] = transform.InverseTransformPoint(e + l * w);
        q[3] = transform.InverseTransformPoint(e + l * -w);

        return q;
    }

    void AddLine(Mesh m, Vector3[] quad, bool tmp)
    {
        int vl = m.vertices.Length;

        Vector3[] vs = m.vertices;
        if (!tmp || vl == 0) vs = resizeVertices(vs, 4);
        else vl -= 4;

        vs[vl] = quad[0];
        vs[vl + 1] = quad[1];
        vs[vl + 2] = quad[2];
        vs[vl + 3] = quad[3];

        int tl = m.triangles.Length;

        int[] ts = m.triangles;
        if (!tmp || tl == 0) ts = resizeTraingles(ts, 6);
        else tl -= 6;
        ts[tl] = vl;
        ts[tl + 1] = vl + 1;
        ts[tl + 2] = vl + 2;
        ts[tl + 3] = vl + 1;
        ts[tl + 4] = vl + 3;
        ts[tl + 5] = vl + 2;

        m.vertices = vs;
        m.triangles = ts;
        m.RecalculateBounds();
    }

    Vector3[] resizeVertices(Vector3[] ovs, int ns)
    {
        Vector3[] nvs = new Vector3[ovs.Length + ns];
        for (int i = 0; i < ovs.Length; i++) nvs[i] = ovs[i];
        return nvs;
    }

    int[] resizeTraingles(int[] ovs, int ns)
    {
        int[] nvs = new int[ovs.Length + ns];
        for (int i = 0; i < ovs.Length; i++) nvs[i] = ovs[i];
        return nvs;
    }
}
