// Copyright 2017 Google Inc. All rights reserved.
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

public class SvrAndroidServiceControllerProvider : IControllerProvider
{

    private const int SVR_CONTROLLER_BUTTON_CLICK = 1 << 0;
    private const int SVR_CONTROLLER_BUTTON_APP = 1 << 1;
    private const int SVR_CONTROLLER_BUTTON_RETURN = 1 << 2;
    private const int SVR_CONTROLLER_BUTTON_HOME = 1 << 3;
    private const int SVR_CONTROLLER_BUTTON_TRIGGER = 1 << 8;

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

    internal SvrAndroidServiceControllerProvider()
    {
        SvrLog.Log("SvrAndroidServiceControllerProvider");
        Init();
    }

    ~SvrAndroidServiceControllerProvider()
    {

    }

    public void ReadState(ControllerState outState)
    {

        if (error)
        {
            outState.connectionState = GvrConnectionState.Error;
            outState.apiStatus = GvrControllerApiStatus.Error;
            outState.errorDetails = errorDetails;
            return;
        }
        
        outState.connectionState = ConvertConnectionState((int)outState.svrControllerIndex);
        outState.apiStatus = ConvertControllerApiStatus((int)outState.svrControllerIndex);
        
        if (outState.connectionState == GvrConnectionState.Connected || outState.connectionState == GvrConnectionState.ConnectedNotRecent)
        {
            rightRawOri = SvrControllerOrientation((int)outState.svrControllerIndex);
            rightrawOriQua.Set(rightRawOri[0], rightRawOri[1], rightRawOri[2], rightRawOri[3]);

            pose3d.Set(Vector3.zero, rightrawOriQua);
            pose3d.SetRightHanded(pose3d.Matrix);
            rightlastRawOrientation = pose3d.Orientation;
            if ((!initialRightRecenterDone || outState.recentered) && !rightlastRawOrientation.Equals(Quaternion.identity))
            {
                if (Camera.main != null)
                {
                    initialRightRecenterDone = true;
                    isRightReadyRecentered = false;
                    rightYawRotation = Camera.main.transform.rotation.eulerAngles - pose3d.Orientation.eulerAngles;
                    rightYawRotation = Vector3.up * rightYawRotation.y;
                    Vector3 RootRotation = Vector3.zero;
                    if (Camera.main.transform.root)
                    {
                        if (Camera.main.transform.root.rotation != Quaternion.identity)
                        {
                            RootRotation = Camera.main.transform.root.rotation.eulerAngles.y * Vector3.up;
                        }
                        rightYawRotation = Vector3.up * rightYawRotation.y - RootRotation;
                    }
                }
            }
            outState.orientation = /*Quaternion.Euler(rightYawRotation) */ rightlastRawOrientation;
            float[] rawOri = SvrControllerOrientation((int)outState.svrControllerIndex);
            float[] rawAccel = SvrControllerAccel((int)outState.svrControllerIndex);
            float[] rawGyro = SvrControllerGyro((int)outState.svrControllerIndex);

            outState.accel = new Vector3(rawAccel[0], rawAccel[1], rawAccel[2]);
            outState.gyro = new Vector3(-rawGyro[0], -rawGyro[1], rawGyro[2]);

            SvrControllerUpdateState(outState, (int)outState.svrControllerIndex);
        }
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

        outState.appButtonDown = SvrControllerButtonDown(index, SVR_CONTROLLER_BUTTON_APP);
        outState.appButtonState = SvrControllerButtonState(index, SVR_CONTROLLER_BUTTON_APP);
        outState.appButtonUp = SvrControllerButtonUp(index, SVR_CONTROLLER_BUTTON_APP);

        outState.homeButtonDown = SvrControllerButtonDown(index, SVR_CONTROLLER_BUTTON_HOME);
        outState.homeButtonState = SvrControllerButtonState(index, SVR_CONTROLLER_BUTTON_HOME);
        outState.homeButtonUp = SvrControllerButtonUp(index, SVR_CONTROLLER_BUTTON_HOME);

        outState.clickButtonDown = SvrControllerButtonDown(index, SVR_CONTROLLER_BUTTON_CLICK);
        outState.clickButtonState = SvrControllerButtonState(index, SVR_CONTROLLER_BUTTON_CLICK);
        outState.clickButtonUp = SvrControllerButtonUp(index, SVR_CONTROLLER_BUTTON_CLICK);

        outState.triggerButtonDown = SvrControllerButtonDown(index, SVR_CONTROLLER_BUTTON_TRIGGER);
        outState.triggerButtonState = SvrControllerButtonState(index, SVR_CONTROLLER_BUTTON_TRIGGER);
        outState.triggerButtonUp = SvrControllerButtonUp(index, SVR_CONTROLLER_BUTTON_TRIGGER);

        outState.isTouching = SvrControllerIsTouching(index);
        outState.touchDown = SvrControllerTouchDown(index);
        outState.touchUp = SvrControllerTouchUp(index);

        outState.recentered = SvrControllerRecentered(index);

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

        outState.errorDetails = "";

    }

    public void Init()
    {
        androidService = Svr.Controller.SvrController.Controller;
        error = androidService == null;
    }

    private GvrConnectionState ConvertConnectionState(int index)
    {
        //UnityEngine.Profiling.Profiler.BeginSample("A");
        int connectionState = androidService.Call<int>("getConnectionState", index);
        SvrLog.LogFormat("ConvertConnectionState:{0},{1}", index, connectionState);
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
        }
        catch (Exception)
        {

        }
        
    }

    public void Release()
    {
        //androidService.Call("release");
    }
    //#endif  // !UNITY_HAS_GOOGLEVR || (!UNITY_ANDROID && !UNITY_EDITOR)
}
