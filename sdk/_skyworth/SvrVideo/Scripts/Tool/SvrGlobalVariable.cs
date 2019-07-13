// Copyright 2018 Skyworth VR. All rights reserved.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SvrGlobalVariable
{
#if UNITY_ANDROID && !UNITY_EDITOR
    static bool IsInitCurActivity = false;
    static AndroidJavaObject AndroidInterface;
    static AndroidJavaObject CurActivity;
    static AndroidJavaObject JApplication;

    static void InitCurrentActivity()
    {
        IsInitCurActivity = true;

        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        if (unityPlayer != null)
            CurActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
    }

    public static AndroidJavaObject GetJApplication()
    {
        if (!IsInitCurActivity)
            InitCurrentActivity();

        if (JApplication != null)
            return JApplication;
        else
        {
            if (CurActivity != null)
            {
                JApplication = CurActivity.Call<AndroidJavaObject>("getApplication");
                return JApplication;
            }
            else
                return null;
        }
    }

    /// <summary>
    /// Need used before create video player.
    /// </summary>
    public static void InitCinemaNativeInterface()
    {
        AndroidJavaClass nativeInterfaceClass = new AndroidJavaClass("com.ssnwt.vr.playermanager.jni.NativeInterface");
        if (nativeInterfaceClass == null)
        {
            return;
        }

        AndroidInterface = nativeInterfaceClass.CallStatic<AndroidJavaObject>("getInstance");
        if (AndroidInterface == null)
        {
            return;
        }

        AndroidInterface.Call("init", GetJApplication());
    }

    /// <summary>
    /// When application quit need use this.
    /// </summary>
    public static void Release()
    {
        if (AndroidInterface != null)
            AndroidInterface.Call("release");
    }
#endif
}