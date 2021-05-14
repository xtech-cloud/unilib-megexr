﻿// Copyright 2017 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using UnityEngine;
using System;
using System.Runtime.InteropServices;
using Gvr.Internal;
using Svr;
using System.Threading;
using System.Collections.Generic;

public class SvrAndroidServiceControllerProvider : IControllerProvider
{

    private const int SVR_CONTROLLER_BUTTON_CLICK = 1 << 0;
    private const int SVR_CONTROLLER_BUTTON_APP = 1 << 1;
    private const int SVR_CONTROLLER_BUTTON_RETURN = 1 << 2;
    private const int SVR_CONTROLLER_BUTTON_HOME = 1 << 3;
    private const int SVR_CONTROLLER_BUTTON_GRIP = 1 << 4;
    private const int SVR_CONTROLLER_BUTTON_TRIGGER = 1 << 8;

    private const int SVR_CONTROLLER_BUTTON_TouchPadUp = 1 << 8;
    private const int SVR_CONTROLLER_BUTTON_TouchPadDown = 1 << 9;
    private const int SVR_CONTROLLER_BUTTON_TouchPadLeft = 1 << 10;
    private const int SVR_CONTROLLER_BUTTON_TouchPadRight = 1 << 11;

    // enum gvr_controller_connection_state:
    private const int SVR_CONTROLLER_DISCONNECTED = 0;
    private const int SVR_CONTROLLER_SCANNING = 1;
    private const int SVR_CONTROLLER_CONNECTING = 2;
    private const int SVR_CONTROLLER_CONNECTED = 3;
    private const int SVR_CONTROLLER_CONNECTEDNOTRECENT = 4;

    // enum gvr_controller_api_status
    private const int SVR_CONTROLLER_API_OK = 0;
    private const int SVR_CONTROLLER_API_UNSUPPORTED = 1;
    private const int SVR_CONTROLLER_API_NOT_AUTHORIZED = 2;
    private const int SVR_CONTROLLER_API_UNAVAILABLE = 3;
    private const int SVR_CONTROLLER_API_SERVICE_OBSOLETE = 4;
    private const int SVR_CONTROLLER_API_CLIENT_OBSOLETE = 5;
    private const int SVR_CONTROLLER_API_MALFUNCTION = 6;

    private const int SVR_CONTROLLER_INDEX_RIGHT = 0;
    private const int SVR_CONTROLLER_INDEX_LEFT = 1;

    private const int SVR_CONTROLLER_HANDEDNESS_RIGHT = 0;
    private const int SVR_CONTROLLER_HANDEDNESS_LEFT = 1;

    private static AndroidJavaObject androidService;

    private bool error = false;
    private string errorDetails = string.Empty;

    private MutablePose3D pose3d = new MutablePose3D();
    private static Vector3 rightYawRotation = Vector3.zero, leftYawRotation = Vector3.zero;
    private float rightDuration, leftDuration;
    private long currentRightFrameNumber, currentLeftFrameNumber;
    private long previouslyRightFrameNumber, previouslyLeftFrameNumber;
    private Quaternion rightrawOriQua = Quaternion.identity, leftrawOriQua = Quaternion.identity;
    private float[] rightRawOri;
    private Quaternion rightlastRawOrientation = Quaternion.identity, leftlastRawOrientation = Quaternion.identity;
    private bool initialRightRecenterDone = false, initialLeftRecenterDone = false;
    private bool isRightReadyRecentered = true, isLeftReadyRecentered = true;
    private bool hasBatteryMethods = false;
    public bool SupportsBatteryStatus
    {
        get { return hasBatteryMethods; }
    }
    #region 动态库接口
    private float[] SvrControllerOrientation(int index)
    {
        return androidService.Call<float[]>("getQuaternion", index);
    }

    private float[] SvrControllerAccel(int index)
    {
        return androidService.Call<float[]>("getAccelerometer", index);
    }

    private float[] SvrControllerGyro(int index)
    {
        return androidService.Call<float[]>("getGyro", index);
    }

    private float[] SvrControllerTouchPos(int index)
    {
        return androidService.Call<float[]>("getTouchPos", index);
    }

    private bool SvrControllerIsTouching(int index)
    {
        return androidService.Call<bool>("isTouching", index);
    }

    private bool SvrControllerTouchUp(int index)
    {
        return androidService.Call<bool>("getTouchUp", index);
    }

    private bool SvrControllerTouchDown(int index)
    {
        return androidService.Call<bool>("getTouchDown", index);
    }

    private bool SvrControllerButtonState(int index, int cb)
    {
        return androidService.Call<bool>("getButtonState", index, cb);
    }

    private bool SvrControllerButtonDown(int index, int cb)
    {
        return androidService.Call<bool>("getButtonDown", index, cb);
    }

    private bool SvrControllerButtonUp(int index, int cb)
    {
        return androidService.Call<bool>("getButtonUp", index, cb);
    }

    private bool SvrControllerRecentered(int index)
    {
        return androidService.Call<bool>("getRecentered", index);
    }

    private int SvrGetBattery(int index)
    {
        return androidService.Call<int>("getBattery", index);
    }

    #endregion
    private static IntPtr getQuaternion_methodID;
    private static IntPtr getAccelerometer_methodID;
    private static IntPtr getGyro_methodID;
    private static IntPtr getTouchPos_methodID;
    private static IntPtr isTouching_methodID;
    private static IntPtr getTouchUp_methodID;
    private static IntPtr getTouchDown_methodID;
    private static IntPtr getButtonState_methodID;
    private static IntPtr getButtonDown_methodID;
    private static IntPtr getButtonUp_methodID;
    private static IntPtr getRecentered_methodID;
    private static IntPtr getBattery_methodID;
    private static IntPtr getConnectionState_methodID;

    private static jvalue[] svrControllerIndex_head_jvalue;
    private static jvalue[] svrControllerIndex_left_jvalue;
    private static jvalue[] svrControllerIndex_right_jvalue;
    internal SvrAndroidServiceControllerProvider()
    {
        SvrLog.Log("SvrAndroidServiceControllerProvider");
        Svr.Controller.SvrController.InitController();
        Init();

        getQuaternion_methodID = AndroidJNIHelper.GetMethodID(androidService.GetRawClass(), "getQuaternion", "(I)[F", false);
        getAccelerometer_methodID = AndroidJNIHelper.GetMethodID(androidService.GetRawClass(), "getAccelerometer", "(I)[F", false);
        getGyro_methodID = AndroidJNIHelper.GetMethodID(androidService.GetRawClass(), "getGyro", "(I)[F", false);
        getTouchPos_methodID = AndroidJNIHelper.GetMethodID(androidService.GetRawClass(), "getTouchPos", "(I)[F", false);
        isTouching_methodID = AndroidJNIHelper.GetMethodID(androidService.GetRawClass(), "isTouching", "(I)Z", false);
        getTouchUp_methodID = AndroidJNIHelper.GetMethodID(androidService.GetRawClass(), "getTouchUp", "(I)Z", false);
        getTouchDown_methodID = AndroidJNIHelper.GetMethodID(androidService.GetRawClass(), "getTouchDown", "(I)Z", false);
        getButtonState_methodID = AndroidJNIHelper.GetMethodID(androidService.GetRawClass(), "getButtonState", "(II)Z", false);
        getButtonDown_methodID = AndroidJNIHelper.GetMethodID(androidService.GetRawClass(), "getButtonDown", "(II)Z", false);
        getButtonUp_methodID = AndroidJNIHelper.GetMethodID(androidService.GetRawClass(), "getButtonUp", "(II)Z", false);
        getRecentered_methodID = AndroidJNIHelper.GetMethodID(androidService.GetRawClass(), "getRecentered", "(I)Z", false);
        getBattery_methodID = AndroidJNIHelper.GetMethodID(androidService.GetRawClass(), "getBattery", "(I)I", false);
        getConnectionState_methodID = AndroidJNIHelper.GetMethodID(androidService.GetRawClass(), "getConnectionState", "(I)I", false);

        svrControllerIndex_head_jvalue = AndroidJNIHelper.CreateJNIArgArray(new object[] { (int)SvrControllerIndex.SVR_CONTROLLER_INDEX_HEAD });
        svrControllerIndex_left_jvalue = AndroidJNIHelper.CreateJNIArgArray(new object[] { (int)SvrControllerIndex.SVR_CONTROLLER_INDEX_LEFT });
        svrControllerIndex_right_jvalue = AndroidJNIHelper.CreateJNIArgArray(new object[] { (int)SvrControllerIndex.SVR_CONTROLLER_INDEX_RIGHT });
    }

    ~SvrAndroidServiceControllerProvider()
    {

    }

    public void ReadState(ControllerState outState)
    {
        UnityEngine.Profiling.Profiler.BeginSample("controller_read");
        if (error)
        {
            outState.connectionState = GvrConnectionState.Error;
            outState.apiStatus = GvrControllerApiStatus.Error;
            outState.errorDetails = errorDetails;
            return;
        }

        outState.connectionState = ConvertConnectionState((int)outState.svrControllerIndex);
        //outState.connectionState = ConvertConnectionState(outState);
        outState.apiStatus = ConvertControllerApiStatus((int)outState.svrControllerIndex);
        
        if (outState.connectionState == GvrConnectionState.Connected || outState.connectionState == GvrConnectionState.ConnectedNotRecent)
        {
            rightRawOri = SvrControllerOrientation((int)outState.svrControllerIndex);
            rightrawOriQua.Set(rightRawOri[0], rightRawOri[1], rightRawOri[2], rightRawOri[3]);

            pose3d.Set(Vector3.zero, rightrawOriQua);
            pose3d.SetRightHanded(pose3d.Matrix);
            rightlastRawOrientation = pose3d.Orientation;
            //if ((!initialRightRecenterDone || outState.recentered) && !rightlastRawOrientation.Equals(Quaternion.identity))
            //{
            //    if (Camera.main != null)
            //    {
            //        initialRightRecenterDone = true;
            //        isRightReadyRecentered = false;
            //        rightYawRotation = Camera.main.transform.rotation.eulerAngles - pose3d.Orientation.eulerAngles;
            //        rightYawRotation = Vector3.up * rightYawRotation.y;
            //        Vector3 RootRotation = Vector3.zero;
            //        if (Camera.main.transform.root)
            //        {
            //            if (Camera.main.transform.root.rotation != Quaternion.identity)
            //            {
            //                RootRotation = Camera.main.transform.root.rotation.eulerAngles.y * Vector3.up;
            //            }
            //            rightYawRotation = Vector3.up * rightYawRotation.y - RootRotation;
            //        }
            //    }
            //}
            outState.orientation = /*Quaternion.Euler(rightYawRotation) */ rightlastRawOrientation;
            float[] rawOri = SvrControllerOrientation((int)outState.svrControllerIndex);
            float[] rawAccel = SvrControllerAccel((int)outState.svrControllerIndex);
            float[] rawGyro = SvrControllerGyro((int)outState.svrControllerIndex);

            outState.accel = new Vector3(rawAccel[0], rawAccel[1], rawAccel[2]);
            outState.gyro = new Vector3(-rawGyro[0], -rawGyro[1], rawGyro[2]);
            outState.position = outState.accel;
            SvrControllerUpdateState(outState, (int)outState.svrControllerIndex);
        }
        else
        {
            outState.ClearTransientState();
        }

        UnityEngine.Profiling.Profiler.EndSample();
    }

    private void SvrControllerUpdateState(ControllerState outState, int index)
    {
        float[] touchPos = SvrControllerTouchPos(index);
        //Vector2 touchPosVec = new Vector2(touchPos[0], -touchPos[1]);
        //Vector2 offset = new Vector2(-0.5f, 0.5f);
        //touchPosVec = touchPosVec + offset;
        //outState.touchPos = touchPosVec * 2;
        outState.touchPos = new Vector2(touchPos[0], touchPos[1]); //The center of the circle is in the upper-left-corner
        //LogTool.Log("outState.touchPos：" + outState.touchPos);

        outState.apiStatus = ConvertControllerApiStatus(index);

        if (outState.connectionState == GvrConnectionState.Connected)
        {
            outState.appButtonDown = SvrControllerButtonDown(index, SVR_CONTROLLER_BUTTON_APP);
            outState.appButtonState = SvrControllerButtonState(index, SVR_CONTROLLER_BUTTON_APP);
            outState.appButtonUp = SvrControllerButtonUp(index, SVR_CONTROLLER_BUTTON_APP);



            outState.clickButtonDown = SvrControllerButtonDown(index, SVR_CONTROLLER_BUTTON_CLICK);
            outState.clickButtonState = SvrControllerButtonState(index, SVR_CONTROLLER_BUTTON_CLICK);
            outState.clickButtonUp = SvrControllerButtonUp(index, SVR_CONTROLLER_BUTTON_CLICK);


            outState.triggerButtonDown = SvrControllerButtonDown(index, SVR_CONTROLLER_BUTTON_TRIGGER);
            outState.triggerButtonState = SvrControllerButtonState(index, SVR_CONTROLLER_BUTTON_TRIGGER);
            outState.triggerButtonUp = SvrControllerButtonUp(index, SVR_CONTROLLER_BUTTON_TRIGGER);

            outState.gripButtonDown = SvrControllerButtonDown(index, SVR_CONTROLLER_BUTTON_GRIP);
            outState.gripButtonState = SvrControllerButtonState(index, SVR_CONTROLLER_BUTTON_GRIP);
            outState.gripButtonUp = SvrControllerButtonUp(index, SVR_CONTROLLER_BUTTON_GRIP);

            outState.TouchPadUpButtonState = (outState.gyro.x == SVR_CONTROLLER_BUTTON_TouchPadUp);
            outState.TouchPadLeftButtonState = (outState.gyro.x == SVR_CONTROLLER_BUTTON_TouchPadLeft);
            outState.TouchPadDownButtonState = (outState.gyro.x == SVR_CONTROLLER_BUTTON_TouchPadDown);
            outState.TouchPadRightButtonState = (outState.gyro.x == SVR_CONTROLLER_BUTTON_TouchPadRight);

            outState.isTouching = SvrControllerIsTouching(index);
            outState.touchDown = SvrControllerTouchDown(index);
            outState.touchUp = SvrControllerTouchUp(index);

            outState.triggervalue = outState.gyro.z;

            int battery = SvrGetBattery(index);
            outState.batteryValue = battery;
            if (battery >= 20 * 4)
            {
                outState.batteryLevel = GvrControllerBatteryLevel.Full;
            }
            else if (battery > 20 * 3)
            {
                outState.batteryLevel = GvrControllerBatteryLevel.AlmostFull;
            }
            else if (battery >= 20 * 2)
            {
                outState.batteryLevel = GvrControllerBatteryLevel.Medium;
            }
            else if (battery >= 20)
            {
                outState.batteryLevel = GvrControllerBatteryLevel.Low;
            }
            else
            {
                outState.batteryLevel = GvrControllerBatteryLevel.CriticalLow;
            }
        }

        outState.recentered = SvrControllerRecentered(index);

        outState.homeButtonDown = SvrControllerButtonDown(index, SVR_CONTROLLER_BUTTON_HOME);
        outState.homeButtonState = SvrControllerButtonState(index, SVR_CONTROLLER_BUTTON_HOME);
        outState.homeButtonUp = SvrControllerButtonUp(index, SVR_CONTROLLER_BUTTON_HOME);

        outState.errorDetails = "";

    }

    private Thread jniThread;
    private bool theadstart;
    private Queue<Action> ExecuteQueue = new Queue<Action>();
    public void Init()
    {
        androidService = Svr.Controller.SvrController.Controller;
        error = androidService == null;

        jniThread = new Thread(new ParameterizedThreadStart(MessageInThread));
        theadstart = true;
        //jniThread.Start();
    }

    void MessageInThread(object obj)
    {
        while (theadstart)
        {
            try
            {
                while (ExecuteQueue.Count > 0)
                {
                    ExecuteQueue.Dequeue().Invoke();
                }
            }
            catch (Exception error)
            {
                Debug.LogError("Main Thread Queue Error Info=" + error);
                ExecuteQueue.Clear();
            }

            Thread.Sleep(10);
        }

        Debug.Log("Controller Message thead destroy.");
        
    }

    private GvrConnectionState ConvertConnectionState(int index)
    {
        //UnityEngine.Profiling.Profiler.BeginSample("A");
        //int connectionState = androidService.Call<int>("getConnectionState", index);
        int connectionState = AndroidJNI.CallIntMethod(androidService.GetRawObject(), getConnectionState_methodID, GetControllerIndex(index));
        //Debug.LogFormat("ConvertConnectionState:{0},{1}", index, connectionState);
        //UnityEngine.Profiling.Profiler.EndSample();
        switch (connectionState)
        {
            case SVR_CONTROLLER_DISCONNECTED:
                return GvrConnectionState.Disconnected;
            case SVR_CONTROLLER_SCANNING:
                return GvrConnectionState.Scanning;
            case SVR_CONTROLLER_CONNECTING:
                return GvrConnectionState.Connecting;
            case SVR_CONTROLLER_CONNECTED:
                return GvrConnectionState.Connected;
            case SVR_CONTROLLER_CONNECTEDNOTRECENT:
                return GvrConnectionState.ConnectedNotRecent;
            default:
                return GvrConnectionState.Error;
        }
    }

    private jvalue[] GetControllerIndex(int index)
    {
        switch ((SvrControllerIndex)index)
        {
            case SvrControllerIndex.SVR_CONTROLLER_INDEX_RIGHT:
                return svrControllerIndex_right_jvalue;
            case SvrControllerIndex.SVR_CONTROLLER_INDEX_LEFT:
                return svrControllerIndex_left_jvalue;
            case SvrControllerIndex.SVR_CONTROLLER_INDEX_HEAD:
                return svrControllerIndex_head_jvalue;
            default:
                return JNIUtils.nulljniArgs;
        }
    }

    private GvrControllerApiStatus ConvertControllerApiStatus(int gvrControllerApiStatus)
    {
        return GvrControllerApiStatus.Ok;
    }

    public enum SvrControllerHandedness
    {
        Error = -1,
        Right = 0,
        Left = 1,
    }

    public static SvrControllerHandedness GetHandedness()
    {
        switch (androidService.Call<int>("getHandedness", 0))
        {
            case SVR_CONTROLLER_HANDEDNESS_RIGHT:
                return SvrControllerHandedness.Right;
            case SVR_CONTROLLER_HANDEDNESS_LEFT:
                return SvrControllerHandedness.Left;
            default:
                return SvrControllerHandedness.Error;
        }
    }

    public void OnPause()
    {
        try
        {
            initialRightRecenterDone = false;
            androidService.Call("release");
            //ExecuteQueue.Enqueue(() => {

            //    AndroidJNI.AttachCurrentThread();
            //    androidService.Call("release");
            //    AndroidJNI.DetachCurrentThread();
            //});
        }
        catch (Exception)
        {

        }
        
    }

    public void OnResume()
    {
        try
        {
            initialRightRecenterDone = false;
            androidService.Call("init");
            //ExecuteQueue.Enqueue(() => {

            //    AndroidJNI.AttachCurrentThread();
            //    androidService.Call("init");
            //    AndroidJNI.DetachCurrentThread();
            //});
        }
        catch (Exception)
        {

        }
        
    }
    
    public void Release()
    {
        //androidService.Call("release");
    }

    public void OnQuit()
    {
        theadstart = false;
    }

    public void ReadState(ControllerState controllerStateLeft, ControllerState controllerStateRight, ControllerState controllerStateHead)
    {
        //Debug.Log("xxxx ReadState");
        Svr.Controller.SvrController.CallBegin();
        ReadState(controllerStateRight);
        ReadState(controllerStateLeft);
        ReadState(controllerStateHead);
    }
    //#endif  // !UNITY_HAS_GOOGLEVR || (!UNITY_ANDROID && !UNITY_EDITOR)
}
