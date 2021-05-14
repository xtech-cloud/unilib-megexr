using UnityEditor;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

[InitializeOnLoad]
public class SvrLoader
{
    static SvrLoader()
    {
#if SVR && UNITY_2019_3_OR_NEWER
        string packagepath = "file:../Assets/SkyworthVR/svr_unity(ver.2019&beyond)_sdk_plugin.tgz";
        AddRequest addrequest = Client.Add(packagepath);
#endif
        Svr.SVRLoad.OnVrSetAction = () => 
        {
#if UNITY_2019_3_OR_NEWER
#else
            PlayerSettings.SetVirtualRealitySDKs(BuildTargetGroup.Android, new string[] { "None" });
#endif
        };

    }
}
