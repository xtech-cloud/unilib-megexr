/******************************************************************

** auth : xmh
** date : 2016/5/16 11:38:13
** desc : show fps on the basis of debugconfig.txt
** Ver. : V1.0.0

******************************************************************/
using UnityEngine;
using IVR;

class FPSController : MonoBehaviour
{
    public GameObject FPS;
    public GameObject OutLog;
    void Start()
    {
        InvokeshowFps();
        Invoke("InvokeshowFps", 1.0f);
    }

    void InvokeshowFps()
    {
        if (FPS != null)
        {
            FPS.SetActive(DebugConfig.Instance.IsDebug());
        }
        if (OutLog != null)
        {
            OutLog.SetActive(DebugConfig.Instance.IsDebug());
        }
    }
    void Update()
    {
        
    }
    void OnApplicationPause(bool pause)
    {
        if (!pause)
        {
             Invoke("InvokeshowFps", 1.0f);
        }
    }
}