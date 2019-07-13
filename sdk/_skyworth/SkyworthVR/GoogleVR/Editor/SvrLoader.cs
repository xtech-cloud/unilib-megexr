using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;


[InitializeOnLoad]
public class SvrLoader
{

    static SvrLoader()
    {
#if UNITY_2018_1_OR_NEWER
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7;
#else
        PlayerSettings.Android.targetDevice = AndroidTargetDevice.ARMv7;
#endif


    }

   
    
}
