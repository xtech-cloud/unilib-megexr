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

public class SvrAndroidServiceControllerProviderV2 : IControllerProvider
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

    //private static AndroidJavaObject androidService;

    private bool error = false;
    private string errorDetails = string.Empty;

    private MutablePose3D pose3d = new MutablePose3D();
    private static Vector3 rightYawRotation = Vector3.zero, leftYawRotation = Vector3.zero;
    private float rightDuration, leftDuration;
    private long currentRightFrameNumber, currentLeftFrameNumber;
    private long previouslyRightFrameNumber, previouslyLeftFrameNumber;
    private Quaternion rightrawOriQua = Quaternion.identity, leftrawOriQua = Quaternion.identity;
    //private float[] rightRawOri;
    private Quaternion rightlastRawOrientation = Quaternion.identity, leftlastRawOrientation = Quaternion.identity;
    private bool isResume = false, initialLeftRecenterDone = false;
    private bool isRightReadyRecentered = true, isLeftReadyRecentered = true;
    private bool hasBatteryMethods = false;
    public bool SupportsBatteryStatus
    {
        get { return hasBatteryMethods; }
    }
    #region 动态库接口
    //private float[] SvrControllerOrientation(int index)
    //{
    //    return androidService.Call<float[]>("getQuaternion", index);
    //}

    //private float[] SvrControllerAccel(int index)
    //{
    //    return androidService.Call<float[]>("getAccelerometer", index);
    //}

    //private float[] SvrControllerGyro(int index)
    //{
    //    return androidService.Call<float[]>("getGyro", index);
    //}

    //private float[] SvrControllerTouchPos(int index)
    //{
    //    return androidService.Call<float[]>("getTouchPos", index);
    //}

    //private bool SvrControllerIsTouching(int index)
    //{
    //    return androidService.Call<bool>("isTouching", index);
    //}

    private bool SvrControllerTouchUp(ControllerState controllerState)
    {
        if (controllerState.mpreControllerState == null) return false;
        if (controllerState.mControllerState == null) return false;

        return controllerState.mpreControllerState.touched && !controllerState.mControllerState.touched;
    }

    private bool SvrControllerTouchDown(ControllerState controllerState)
    {
        if (controllerState.mpreControllerState == null) return false;
        if (controllerState.mControllerState == null) return false;
        return !controllerState.mpreControllerState.touched && controllerState.mControllerState.touched;
    }

    private bool SvrControllerButtonState(ControllerState controllerState, Svr.Controller.KeyCode keyCode)
    {
        return (controllerState.mControllerState.keyCode & (uint)keyCode) != 0;
    }

    private bool SvrControllerButtonDown(ControllerState controllerState, Svr.Controller.KeyCode keyCode)
    {
        if (controllerState.mpreControllerState == null) return false;
        if (controllerState.mControllerState == null) return false;

        if ((controllerState.mpreControllerState.keyCode & (uint)keyCode) == 0 && (controllerState.mControllerState.keyCode & (uint)keyCode) != 0)
            return true;
        return false;
    }

    private bool SvrControllerButtonUp(ControllerState controllerState, Svr.Controller.KeyCode keyCode)
    {
        if (controllerState.mpreControllerState == null) return false;
        if (controllerState.mControllerState == null) return false;

        if ((controllerState.mpreControllerState.keyCode & (uint)keyCode) != 0 && (controllerState.mControllerState.keyCode & (uint)keyCode) == 0)
            return true;
        return false;
    }

    //private bool SvrControllerRecentered(int index)
    //{
    //    return androidService.Call<bool>("getRecentered", index);
    //}

    //private int SvrGetBattery(int index)
    //{
    //    return androidService.Call<int>("getBattery", index);
    //}

    #endregion
    //private static IntPtr getQuaternion_methodID;
    //private static IntPtr getAccelerometer_methodID;
    //private static IntPtr getGyro_methodID;
    //private static IntPtr getTouchPos_methodID;
    //private static IntPtr isTouching_methodID;
    //private static IntPtr getTouchUp_methodID;
    //private static IntPtr getTouchDown_methodID;
    //private static IntPtr getButtonState_methodID;
    //private static IntPtr getButtonDown_methodID;
    //private static IntPtr getButtonUp_methodID;
    //private static IntPtr getRecentered_methodID;
    //private static IntPtr getBattery_methodID;
    //private static IntPtr getConnectionState_methodID;

    //private static jvalue[] svrControllerIndex_head_jvalue;
    //private static jvalue[] svrControllerIndex_left_jvalue;
    //private static jvalue[] svrControllerIndex_right_jvalue;
    internal SvrAndroidServiceControllerProviderV2()
    {
        SvrLog.Log("SvrAndroidServiceControllerProviderV2");
        Svr.Controller.SvrControllerV2.InitController();
        Init();

        //getQuaternion_methodID = AndroidJNIHelper.GetMethodID(androidService.GetRawClass(), "getQuaternion", "(I)[F", false);
        //getAccelerometer_methodID = AndroidJNIHelper.GetMethodID(androidService.GetRawClass(), "getAccelerometer", "(I)[F", false);
        //getGyro_methodID = AndroidJNIHelper.GetMethodID(androidService.GetRawClass(), "getGyro", "(I)[F", false);
        //getTouchPos_methodID = AndroidJNIHelper.GetMethodID(androidService.GetRawClass(), "getTouchPos", "(I)[F", false);
        //isTouching_methodID = AndroidJNIHelper.GetMethodID(androidService.GetRawClass(), "isTouching", "(I)Z", false);
        //getTouchUp_methodID = AndroidJNIHelper.GetMethodID(androidService.GetRawClass(), "getTouchUp", "(I)Z", false);
        //getTouchDown_methodID = AndroidJNIHelper.GetMethodID(androidService.GetRawClass(), "getTouchDown", "(I)Z", false);
        //getButtonState_methodID = AndroidJNIHelper.GetMethodID(androidService.GetRawClass(), "getButtonState", "(II)Z", false);
        //getButtonDown_methodID = AndroidJNIHelper.GetMethodID(androidService.GetRawClass(), "getButtonDown", "(II)Z", false);
        //getButtonUp_methodID = AndroidJNIHelper.GetMethodID(androidService.GetRawClass(), "getButtonUp", "(II)Z", false);
        //getRecentered_methodID = AndroidJNIHelper.GetMethodID(androidService.GetRawClass(), "getRecentered", "(I)Z", false);
        //getBattery_methodID = AndroidJNIHelper.GetMethodID(androidService.GetRawClass(), "getBattery", "(I)I", false);
        //getConnectionState_methodID = AndroidJNIHelper.GetMethodID(androidService.GetRawClass(), "getConnectionState", "(I)I", false);

        //svrControllerIndex_head_jvalue = AndroidJNIHelper.CreateJNIArgArray(new object[] { (int)SvrControllerIndex.SVR_CONTROLLER_INDEX_HEAD });
        //svrControllerIndex_left_jvalue = AndroidJNIHelper.CreateJNIArgArray(new object[] { (int)SvrControllerIndex.SVR_CONTROLLER_INDEX_LEFT });
        //svrControllerIndex_right_jvalue = AndroidJNIHelper.CreateJNIArgArray(new object[] { (int)SvrControllerIndex.SVR_CONTROLLER_INDEX_RIGHT });
    }

    ~SvrAndroidServiceControllerProviderV2()
    {

    }

    public void ReadState(ControllerState outState)
    {
        //UnityEngine.Profiling.Profiler.BeginSample("controller_read");
        if (outState.mControllerState == null)
        {
            outState.connectionState = GvrConnectionState.Error;
            outState.apiStatus = GvrControllerApiStatus.Error;
            outState.errorDetails = errorDetails;
            return;
        }

        SvrLog.Log("ReadState V2");

        outState.connectionState = ConvertConnectionState(outState.mControllerState);
        //outState.connectionState = ConvertConnectionState(outState);
        outState.apiStatus = ConvertControllerApiStatus((int)outState.svrControllerIndex);
        
        if (outState.connectionState == GvrConnectionState.Connected || outState.connectionState == GvrConnectionState.ConnectedNotRecent)
        {
            //rightRawOri = SvrControllerOrientation(outState.mControllerState);
            rightrawOriQua.Set(outState.mControllerState.transform.rotation.x, outState.mControllerState.transform.rotation.y,
                outState.mControllerState.transform.rotation.z, outState.mControllerState.transform.rotation.w);

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
            outState.position.x =  outState.mControllerState.transform.position.x;
            outState.position.y =  outState.mControllerState.transform.position.y;
            outState.position.z =  outState.mControllerState.transform.position.z;
            //float[] rawOri = SvrControllerOrientation((int)outState.svrControllerIndex);
            //float[] rawAccel = SvrControllerAccel((int)outState.svrControllerIndex);
            //float[] rawGyro = SvrControllerGyro((int)outState.svrControllerIndex);

            //outState.accel = new Vector3(rawAccel[0], rawAccel[1], rawAccel[2]);
            //outState.gyro = new Vector3(-rawGyro[0], -rawGyro[1], rawGyro[2]);

            SvrControllerUpdateState(outState);
        }
        else
        {
            outState.ClearTransientState();
        }

        //UnityEngine.Profiling.Profiler.EndSample();
    }

    private void SvrControllerUpdateState(ControllerState outState)
    {
        //float[] touchPos = SvrControllerTouchPos(index);
        //Vector2 touchPosVec = new Vector2(touchPos[0], -touchPos[1]);
        //Vector2 offset = new Vector2(-0.5f, 0.5f);
        //touchPosVec = touchPosVec + offset;
        //outState.touchPos = touchPosVec * 2;
        if (outState.connectionState == GvrConnectionState.Connected)
        {
            outState.touchPos.x = outState.mControllerState.touch.x; //The center of the circle is in the upper-left-corner
            outState.touchPos.y = outState.mControllerState.touch.y; //The center of the circle is in the upper-left-corner
                                                                     //LogTool.Log("outState.touchPos：" + outState.touchPos);

            outState.apiStatus = GvrControllerApiStatus.Ok;

            outState.appButtonDown = SvrControllerButtonDown(outState, Svr.Controller.KeyCode.Button_Menu);
            outState.appButtonState = SvrControllerButtonState(outState, Svr.Controller.KeyCode.Button_Menu);
            outState.appButtonUp = SvrControllerButtonUp(outState, Svr.Controller.KeyCode.Button_Menu);

            outState.clickButtonDown = SvrControllerButtonDown(outState, Svr.Controller.KeyCode.Button_Enter);
            outState.clickButtonState = SvrControllerButtonState(outState, Svr.Controller.KeyCode.Button_Enter);
            outState.clickButtonUp = SvrControllerButtonUp(outState, Svr.Controller.KeyCode.Button_Enter);

            outState.triggerButtonDown = SvrControllerButtonDown(outState, Svr.Controller.KeyCode.Button_Trigger);
            outState.triggerButtonState = SvrControllerButtonState(outState, Svr.Controller.KeyCode.Button_Trigger);
            outState.triggerButtonUp = SvrControllerButtonUp(outState, Svr.Controller.KeyCode.Button_Trigger);

            outState.gripButtonDown = SvrControllerButtonDown(outState, Svr.Controller.KeyCode.Button_Grip);
            outState.gripButtonState = SvrControllerButtonState(outState, Svr.Controller.KeyCode.Button_Grip);
            outState.gripButtonUp = SvrControllerButtonUp(outState, Svr.Controller.KeyCode.Button_Grip);

            outState.TouchPadUpButtonState = SvrControllerButtonState(outState, Svr.Controller.KeyCode.Button_Up);
            outState.TouchPadLeftButtonState = SvrControllerButtonState(outState, Svr.Controller.KeyCode.Button_Left);
            outState.TouchPadDownButtonState = SvrControllerButtonState(outState, Svr.Controller.KeyCode.Button_Down);
            outState.TouchPadRightButtonState = SvrControllerButtonState(outState, Svr.Controller.KeyCode.Button_Right);

            outState.isTouching = outState.mControllerState.touched;
            outState.touchDown = SvrControllerTouchDown(outState);
            outState.touchUp = SvrControllerTouchUp(outState);

           

            int battery = (int)outState.mControllerState.battery;
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
        outState.recentered = outState.mControllerState.recented;

        outState.homeButtonDown = SvrControllerButtonDown(outState, Svr.Controller.KeyCode.Button_Home);
        outState.homeButtonState = SvrControllerButtonState(outState, Svr.Controller.KeyCode.Button_Home);
        
        outState.homeButtonUp = SvrControllerButtonUp(outState, Svr.Controller.KeyCode.Button_Home);
       
    }

    public void Init()
    {
        //androidService = Svr.Controller.SvrController.Controller;
        //error = androidService == null;
    }

    private GvrConnectionState ConvertConnectionState(Svr.Controller.ControllerState controllerState)
    {
        //UnityEngine.Profiling.Profiler.BeginSample("A");
        //int connectionState = androidService.Call<int>("getConnectionState", index);
        //int connectionState = AndroidJNI.CallIntMethod(androidService.GetRawObject(), getConnectionState_methodID, GetControllerIndex(index));
        //UnityEngine.Profiling.Profiler.EndSample();
        switch (controllerState.connectionState)
        {
            case Svr.Controller.ConnectStatus.Disconnected:
                return GvrConnectionState.Disconnected;
            case Svr.Controller.ConnectStatus.Scanning:
                return GvrConnectionState.Scanning;
            case Svr.Controller.ConnectStatus.Connecting:
                return GvrConnectionState.Connecting;
            case Svr.Controller.ConnectStatus.Connected:
                return GvrConnectionState.Connected;
            case Svr.Controller.ConnectStatus.NoRecenter:
                return GvrConnectionState.ConnectedNotRecent;
            default:
                return GvrConnectionState.Error;
        }

        
    }

    //private jvalue[] GetControllerIndex(int index)
    //{
    //    switch ((SvrControllerIndex)index)
    //    {
    //        case SvrControllerIndex.SVR_CONTROLLER_INDEX_RIGHT:
    //            return svrControllerIndex_right_jvalue;
    //        case SvrControllerIndex.SVR_CONTROLLER_INDEX_LEFT:
    //            return svrControllerIndex_left_jvalue;
    //        case SvrControllerIndex.SVR_CONTROLLER_INDEX_HEAD:
    //            return svrControllerIndex_head_jvalue;
    //        default:
    //            return JNIUtils.nulljniArgs;
    //    }
    //}

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

    //public static SvrControllerHandedness GetHandedness()
    //{
    //    //switch (androidService.Call<int>("getHandedness", 0))
    //    //{
    //    //    case SVR_CONTROLLER_HANDEDNESS_RIGHT:
    //    //        return SvrControllerHandedness.Right;
    //    //    case SVR_CONTROLLER_HANDEDNESS_LEFT:
    //    //        return SvrControllerHandedness.Left;
    //    //    default:
    //    //        return SvrControllerHandedness.Error;
    //    //}
    //}

    public void OnPause()
    {
        try
        {
            isResume = false;
            int result = Svr.Controller.SvrControllerV2.disconnect();
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
        
    }

    public void OnResume()
    {
        try
        {
            isResume = true;
            int result = Svr.Controller.SvrControllerV2.connect();
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
    }

    public void ReadState(ControllerState controllerStateLeft, ControllerState controllerStateRight, ControllerState controllerStateHead)
    {
        if (!isResume) return;
        Svr.Controller.ControllerState[] controllerState = Svr.Controller.SvrControllerV2.CallBegin();

        controllerStateRight.ClearTransientState();
        controllerStateLeft.ClearTransientState();
        controllerStateHead.ClearTransientState();
        for(int i = 0; i< controllerState.Length; i++)
        {
            var item = controllerState[i];
            if (item != null)
            {
                switch (item.handness)
                {
                    case Svr.Controller.Handness.Right:
                        controllerStateRight.mControllerState = item;
                        ReadState(controllerStateRight);
                        break;
                    case Svr.Controller.Handness.Left:
                        controllerStateLeft.mControllerState = item;
                        ReadState(controllerStateLeft);
                        break;
                    case Svr.Controller.Handness.Head:
                        controllerStateHead.mControllerState = item;
                        ReadState(controllerStateHead);
                        break;
                    default:
                        break;
                }
            }
        }

    }

    //#endif  // !UNITY_HAS_GOOGLEVR || (!UNITY_ANDROID && !UNITY_EDITOR)
}
