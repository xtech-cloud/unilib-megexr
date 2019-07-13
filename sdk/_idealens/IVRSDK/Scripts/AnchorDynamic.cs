using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class AnchorDynamic : MonoBehaviour
{

    private IVRInputModule module;
    private Vector3 mInitLocalPosition;
    private Vector3 mInitLocalScal;
    private float Depth;
    public float Speed = 10;
    private float mStartTime;
    private bool hoveinit;
    private bool Unhovieinit;
    private float mMoveLenght;
    private bool started = false;
    public static AnchorDynamic Instance { get; private set; }
    public float depth = 5f;
    public float initScal = 0.07998779f;
    public float Width = 100;
    public float Height = 100;
    private MeshRenderer mRender;
    private Material mDynamicMat;
    private int mShaderClipID;
    private Vector3 startforward, startright, startup, startworldpos;
    private float MaxAngleX, MaxAngleY, LengthAngle;
    void Awake()
    {
        Instance = this;
        mRender = GetComponent<MeshRenderer>();
    }
    // Use this for initialization
    void Start()
    {
        startforward = transform.forward;
        startright = transform.right;
        startup = transform.up;
        startworldpos = transform.position;
        mDynamicMat = mRender.material;
        module = GameObject.FindObjectOfType<IVRInputModule>();
        mInitLocalPosition = transform.localPosition;
        mInitLocalScal = transform.localScale;
        Depth = Mathf.Abs(mInitLocalPosition.z);
        started = true;
        IVRManager.OnResetPos += IVRManager_OnResetPos;
        mShaderClipID = Shader.PropertyToID("_Offset");

        MaxAngleX = Vector3.Angle(transform.parent.position - transform.position,
           transform.parent.position - (transform.position + transform.right * (Width / 2 - transform.localScale.x)));
        MaxAngleY = Vector3.Angle(transform.parent.position - transform.position,
            transform.parent.position - (transform.position + transform.up * (Height / 2 - transform.localScale.y)));
        LengthAngle = Vector3.Angle(transform.parent.position - (transform.position + transform.up * transform.localScale.y / 2),
            transform.parent.position - (transform.position - transform.up * transform.localScale.y / 2));

    }
    public void Hide()
    {
        mRender.enabled = false;
    }
    public void Show()
    {
        mRender.enabled = true;
    }

    private void IVRManager_OnResetPos()
    {
        transform.localPosition = mInitLocalPosition;
        transform.localScale = mInitLocalScal;
    }

    void OnDestroy()
    {
        IVRManager.OnResetPos -= IVRManager_OnResetPos;
    }
    // Update is called once per frame
    void Update()
    {


        if (module != null && module.faceposition != Vector3.zero)
        {
            if (!hoveinit)
            {
                //Debug.Log("OnPointerEnter");
                mStartTime = Time.time;
                mMoveLenght = Vector3.Distance(transform.position, module.faceposition);
                hoveinit = true;
            }

            //Debug.Log("OnPointerHover");
            float deltastep = ((Time.time - mStartTime) * Speed) / mMoveLenght;
            Vector3 endposition = transform.parent.position + transform.parent.forward * Vector3.Distance(transform.parent.position, module.faceposition);
            //Debug.Log(endposition);
            transform.position = Vector3.Lerp(transform.position, endposition, deltastep);
            //Debug.Log(Mathf.Lerp(transform.localPosition.z, Vector3.Distance(transform.position, module.faceposition), deltastep));
            //transform.localPosition.Set(0, 0, Mathf.Lerp(transform.localPosition.z, Vector3.Distance(transform.position, module.faceposition), deltastep));
        }
        else
        {
            hoveinit = false;
        }
        //else
        //{

        //    if (Mathf.Abs(Vector3.Distance(transform.localPosition, Vector3.zero) - Vector3.Distance(mInitLocalPosition, Vector3.zero)) >= 0.1f)
        //    {
        //        if (hoveinit)
        //        {
        //            mStartTime = Time.time;
        //            mMoveLenght = Vector3.Distance(transform.localPosition, mInitLocalPosition);
        //            hoveinit = false;
        //        }

        //        float deltastep = ((Time.time - mStartTime) * Speed) / mMoveLenght;
        //        if (Application.isPlaying)
        //            transform.localPosition = Vector3.Lerp(transform.localPosition, mInitLocalPosition, deltastep);
        //    }

        //}

        Vector3 currentPos = transform.localPosition;

        float scaleDivisor = Vector3.Distance(currentPos, Vector3.zero) / Depth;

        Vector3 targetScale = mInitLocalScal * scaleDivisor;
        transform.localScale = targetScale;
        Vector3 anchordirection = transform.position - transform.parent.position;
        Vector3 pUpanchordirection = Vector3.ProjectOnPlane(anchordirection, Vector3.up);
        float wAngle = Vector3.Angle(pUpanchordirection, startforward);
        Vector3 pRightanchordirection = Vector3.ProjectOnPlane(anchordirection, Vector3.right);
        float hAngle = Vector3.Angle(pRightanchordirection, startforward);

        if (hAngle > MaxAngleY || wAngle > MaxAngleX)
        {
            //Up or Down
            //if Up
            Vector3 pFowrdanchordirection = Vector3.ProjectOnPlane(anchordirection, startforward).normalized;
            float offsetV = (hAngle - MaxAngleY) / LengthAngle;

            Vector4 clipValue = mDynamicMat.GetVector(mShaderClipID);
            if (pFowrdanchordirection.y > 0)
            {
                clipValue.y = offsetV;
            }
            else
            {
                clipValue.w = offsetV;

            }

            float offseth = (wAngle - MaxAngleX) / LengthAngle;
            if (pFowrdanchordirection.x < 0)
            {
                clipValue.x = offseth;
            }
            else
            {
                clipValue.z = offseth;
            }
            mDynamicMat.SetVector(mShaderClipID, clipValue);
        }
        if (hAngle <= MaxAngleY && wAngle <= MaxAngleX)
        {
            mDynamicMat.SetVector(mShaderClipID, new Vector4(0f, 0, 0, 0));
        }

    }

    [ContextMenu("reset")]
    void reset()
    {
        transform.localPosition = new Vector3(0, 0, depth);
        transform.localScale = new Vector3(initScal, initScal, initScal);
        transform.localRotation = Quaternion.identity;
    }
    [ContextMenu("Checkout")]
    void Checkout()
    {
        GameObject o = Instantiate<GameObject>(gameObject);
        o.transform.parent = transform.parent;
        o.name = gameObject.name;
        //DestroyImmediate(gameObject);
    }

void OnDrawGizmos()
    {

        if (!Application.isPlaying)
        {
            return;
        }
        //Vector3 anchordirection = transform.position - transform.parent.position;
        //Vector3 pUpanchordirection = Vector3.ProjectOnPlane(anchordirection, startup);
        //Gizmos.DrawLine(transform.parent.position, transform.parent.position + pUpanchordirection);
        //Gizmos.DrawLine(transform.position, transform.parent.position + pUpanchordirection);
        //Vector3 pRightanchordirection = Vector3.ProjectOnPlane(anchordirection, startright);
        //Gizmos.DrawLine(transform.parent.position, transform.parent.position + pRightanchordirection);
        //Gizmos.DrawLine(transform.position, transform.parent.position + pRightanchordirection);
        //Vector3 pFowrdanchordirection = Vector3.ProjectOnPlane(anchordirection, startforward);
        //Gizmos.DrawLine(transform.parent.position, transform.parent.position + pFowrdanchordirection);
        //Gizmos.DrawLine(transform.position, transform.parent.position + pFowrdanchordirection);

        Vector3 pleftup, prightup, pleftdown, prightdown;
        pleftup = startworldpos - startright * Width / 2 + startup * Height / 2;
        prightup = startworldpos + startright * Width / 2 + startup * Height / 2;
        pleftdown = startworldpos - startright * Width / 2 - startup * Height / 2;
        prightdown = startworldpos + startright * Width / 2 - startup * Height / 2;
        Gizmos.DrawLine(transform.parent.position, pleftup);
        Gizmos.DrawLine(transform.parent.position, prightup);
        Gizmos.DrawLine(transform.parent.position, pleftdown);
        Gizmos.DrawLine(transform.parent.position, prightdown);
        Gizmos.DrawLine(pleftup, prightup);
        Gizmos.DrawLine(prightup, prightdown);
        Gizmos.DrawLine(prightdown, pleftdown);
        Gizmos.DrawLine(pleftdown, pleftup);
    }
}
