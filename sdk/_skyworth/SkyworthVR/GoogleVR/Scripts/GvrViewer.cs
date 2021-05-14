// Copyright 2014 Google Inc. All rights reserved.
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
using System.Collections.Generic;
#if UNITY_2019_3_OR_NEWER
using UnityEngine.XR;
#endif

using Gvr.Internal;
using System.Collections;

/// The GvrViewer object communicates with the head-mounted display.
/// Is is repsonsible for:
/// -  Querying the device for viewing parameters
/// -  Retrieving the latest head tracking data
/// -  Providing the rendered scene to the device for distortion correction (optional)
///
/// There should only be one of these in a scene.  An instance will be generated automatically
/// by this script at runtime, or you can add one via the Editor if you wish to customize
/// its starting properties.
[AddComponentMenu("GoogleVR/GvrViewer")]
public class GvrViewer : MonoBehaviour
{
    /// The singleton instance of the GvrViewer class.
    public static GvrViewer Instance
    {
        get
        {
#if UNITY_EDITOR
            if (instance == null && !Application.isPlaying)
            {
                instance = UnityEngine.Object.FindObjectOfType<GvrViewer>();
            }
#endif
            if (instance == null)
            {
                Debug.LogError("No GvrViewer instance found.  Ensure one exists in the scene, or call "
                    + "GvrViewer.Create() at startup to generate one.\n"
                    + "If one does exist but hasn't called Awake() yet, "
                    + "then this error is due to order-of-initialization.\n"
                    + "In that case, consider moving "
                    + "your first reference to GvrViewer.Instance to a later point in time.\n"
                    + "If exiting the scene, this indicates that the GvrViewer object has already "
                    + "been destroyed.");
            }
            return instance;
        }
    }

    private static GvrViewer instance = null;

    /// Generate a GvrViewer instance.  Takes no action if one already exists.
    public static void Create()
    {
        if (instance == null && UnityEngine.Object.FindObjectOfType<GvrViewer>() == null)
        {
            Svr.SvrLog.Log("Creating GvrViewer object");
            var go = new GameObject("GvrViewer", typeof(GvrViewer));
            go.transform.localPosition = Vector3.zero;
            // sdk will be set by Awake().
        }
    }

    /// The StereoController instance attached to the main camera, or null if there is none.
    /// @note Cached for performance.
    public static StereoController Controller
    {
        get
        {
            //#if !UNITY_HAS_GOOGLEVR || UNITY_EDITOR
            Camera camera = Camera.main;
            // Cache for performance, if possible.
            if (camera != currentMainCamera || currentController == null)
            {
                currentMainCamera = camera;
                currentController = camera.GetComponent<StereoController>();
            }
            return currentController;
            //#else
            //      return null;
            //#endif  // !UNITY_HAS_GOOGLEVR || UNITY_EDITOR
        }
    }
    private static StereoController currentController;
    private static Camera currentMainCamera;

    /// Determine whether the scene renders in stereo or mono.
    /// Supported only for versions of Unity *without* the GVR integration.
    /// VRModeEnabled will be a no-op for versions of Unity with the GVR integration.
    /// _True_ means to render in stereo, and _false_ means to render in mono.
    public bool VRModeEnabled
    {
        get
        {
#if !UNITY_HAS_GOOGLEVR || UNITY_EDITOR
            return vrModeEnabled ;
#else
      return vrModeEnabled && Svr.SvrSetting.IsVRDevice ;//UnityEngine.VR.VRSettings.enabled;
#endif  // !UNITY_HAS_GOOGLEVR || UNITY_EDITOR
        }
        set
        {
#if !UNITY_HAS_GOOGLEVR || UNITY_EDITOR
            if (value != vrModeEnabled && device != null)
            {
                device.SetVRModeEnabled(value);
            }
            vrModeEnabled = value;
#endif  // !UNITY_HAS_GOOGLEVR || UNITY_EDITOR
        }
    }

    // Ignore private field is assigned but its value is never used compile warning.
#pragma warning disable 414
    [SerializeField]
    private bool vrModeEnabled = true;
#pragma warning restore 414

    /// Methods for performing lens distortion correction.
    public enum DistortionCorrectionMethod
    {
        None,    ///< No distortion correction
        Native,  ///< Use the native C++ plugin
        Unity,   ///< Perform distortion correction in Unity (recommended)
    }

    /// Determines the distortion correction method used by the SDK to render the
    /// #StereoScreen texture on the phone.  If _Native_ is selected but not supported
    /// by the device, the _Unity_ method will be used instead.
    public DistortionCorrectionMethod DistortionCorrection
    {
        get
        {
            return distortionCorrection;
        }
        set
        {
            if (device != null && device.RequiresNativeDistortionCorrection())
            {
                value = DistortionCorrectionMethod.Native;
            }
            if (value != distortionCorrection && device != null)
            {
                device.SetDistortionCorrectionEnabled(value == DistortionCorrectionMethod.Native
                    && NativeDistortionCorrectionSupported);
                device.UpdateScreenData();
            }
            distortionCorrection = value;
        }
    }
    [SerializeField]
#if UNITY_HAS_GOOGLEVR && UNITY_ANDROID && !UNITY_EDITOR
  private DistortionCorrectionMethod distortionCorrection = DistortionCorrectionMethod.Native;
#else
    private DistortionCorrectionMethod distortionCorrection = DistortionCorrectionMethod.Unity;
#endif  // UNITY_HAS_GOOGLEVR && UNITY_ANDROID && !UNITY_EDITOR

    /// The native SDK will apply a neck offset to the head tracking, resulting in
    /// a more realistic model of a person's head position.  This control determines
    /// the scale factor of the offset.  To turn off the neck model, set it to 0, and
    /// to turn it all on, set to 1.  Intermediate values can be used to animate from
    /// on to off or vice versa.
    public float NeckModelScale
    {
        get
        {
            return neckModelScale;
        }
        set
        {
            value = Mathf.Clamp01(value);
            if (!Mathf.Approximately(value, neckModelScale) && device != null)
            {
                device.SetNeckModelScale(value);
            }
            neckModelScale = value;
            GvrViewerInternal.Instance.NeckModelScale = value;
        }
    }
    [SerializeField]
    private float neckModelScale = 0.0f;

#if UNITY_EDITOR
    public bool autoUntiltHead
    {
        get
        {
            return _autoUntiltHead;
        }
        set
        {
            if (value != _autoUntiltHead)
            {
                _autoUntiltHead = value;
                GvrViewerInternal.Instance.autoUntiltHead = value;
            }
        }
    }
    /// Restores level head tilt in when playing in the Unity Editor after you
    /// release the Ctrl key.
    private bool _autoUntiltHead = true;

    public bool UseUnityRemoteInput
    {
        get
        {
            return useUnityRemoteInput;
        }
        set
        {
            if (value != useUnityRemoteInput)
            {
                useUnityRemoteInput = value;
                GvrViewerInternal.Instance.UseUnityRemoteInput = value;
            }
        }
    }
    /// @cond
    /// Use unity remote as the input source.
    private bool useUnityRemoteInput = false;
    /// @endcond

    /// The screen size to emulate when testing in the Unity Editor.
    public GvrProfile.ScreenSizes ScreenSize
    {
        get
        {
            return screenSize;
        }
        set
        {
            if (value != screenSize)
            {
                screenSize = value;
                if (device != null)
                {
                    device.UpdateScreenData();
                }
                GvrViewerInternal.Instance.ScreenSize = value;
            }
        }
    }
    [SerializeField]
    private GvrProfile.ScreenSizes screenSize = GvrProfile.ScreenSizes.Skyworth;

    /// The viewer type to emulate when testing in the Unity Editor.
    public GvrProfile.ViewerTypes ViewerType
    {
        get
        {
            return viewerType;
        }
        set
        {
            if (value != viewerType)
            {
                viewerType = value;
                if (device != null)
                {
                    device.UpdateScreenData();
                }
                GvrViewerInternal.Instance.ViewerType = value;
            }
        }
    }
    [SerializeField]
    private GvrProfile.ViewerTypes viewerType = GvrProfile.ViewerTypes.SkyworthHMD;
#endif

    // The VR device that will be providing input data.
    private BaseVRDevice device;

    /// Whether native distortion correction functionality is supported by the VR device.
    public bool NativeDistortionCorrectionSupported { get; private set; }

    /// Whether the VR device supports showing a native UI layer, for example for settings.
    public bool NativeUILayerSupported { get; private set; }

    /// Scales the resolution of the #StereoScreen.  Set to less than 1.0 to increase
    /// rendering speed while decreasing sharpness, or greater than 1.0 to do the
    /// opposite.
    public float StereoScreenScale
    {
        get
        {
            return stereoScreenScale;
        }
        set
        {
            value = Mathf.Clamp(value, 0.1f, 10.0f);  // Sanity.
            if (stereoScreenScale != value)
            {
                stereoScreenScale = value;
                StereoScreen = null;
                Svr.SvrLog.Log("StereoScreenScale Set StereoScreen to Null");
                GvrViewerInternal.Instance.StereoScreenScale = value;
            }
        }
    }
    [SerializeField]
    private float stereoScreenScale = 1.2f;

    /// The texture that Unity renders the scene to.  After the frame has been rendered,
    /// this texture is drawn to the screen with a lens distortion correction effect.
    /// The texture size is based on the size of the screen, the lens distortion
    /// parameters, and the #StereoScreenScale factor.
    public StereoScreen StereoScreen
    {
        get
        {
            // Don't need it except for distortion correction.
            if (distortionCorrection == DistortionCorrectionMethod.None || !VRModeEnabled)
            {
                return null;
            }
            if (stereoScreen == null && !Svr.SvrSetting.IsVR9Device)
            {
                // Create on demand.
                //Debug.Log("StereoScreen GET");
                StereoScreen = device.CreateStereoScreen();  // Note: uses set{}
            }
            return stereoScreen;
        }
        set
        {
            if (value == stereoScreen)
            {
                return;
            }
            if (stereoScreen != null)
            {
                stereoScreen.Release();
            }
            stereoScreen = value;
            if (OnStereoScreenChanged != null)
            {
                OnStereoScreenChanged(stereoScreen);
            }
        }
    }
    private static StereoScreen stereoScreen = null;
    /// A callback for notifications that the StereoScreen property has changed.
    public delegate void StereoScreenChangeDelegate(StereoScreen newStereoScreen);

    /// Emitted when the StereoScreen property has changed.
    public event StereoScreenChangeDelegate OnStereoScreenChanged;

    /// Describes the current device, including phone screen.
    public GvrProfile Profile
    {
        get
        {
            return device.Profile;
        }
    }

    /// Returns true if GoogleVR is NOT supported natively.
    /// That is, this version of Unity does not have native integration but supports
    /// the GVR SDK (5.2, 5.3), or the current VR player is the in-editor emulator.
    public static bool NoNativeGVRSupport
    {
        get
        {
#if !UNITY_HAS_GOOGLEVR || UNITY_EDITOR
            return true;
#else
      return false;
#endif  // !UNITY_HAS_GOOGLEVR || UNITY_EDITOR
        }
    }

    /// Distinguish the stereo eyes.
    public enum Eye
    {
        Left,   ///< The left eye
        Right,  ///< The right eye
        Center  ///< The "center" eye (unused)
    }

    /// When retrieving the #Projection and #Viewport properties, specifies
    /// whether you want the values as seen through the viewer's lenses (`Distorted`) or
    /// as if no lenses were present (`Undistorted`).
    public enum Distortion
    {
        Distorted,   ///< Viewing through the lenses
        Undistorted  ///< No lenses
    }
    private float mYaw = 0;
    /// The transformation of head from origin in the tracking system.
    public Pose3D HeadPose
    {
        get
        {
            //Pose3D pose3D = device.GetHeadPose();
            //Matrix4x4 mrotaiontMatrix = Matrix4x4.TRS(pose3D.Position, Quaternion.identity, Vector3.one)
            //    *Matrix4x4.TRS(Vector3.one, Quaternion.Euler(0,mYaw,0), Vector3.one)
            //    *Matrix4x4.TRS(pose3D.Position, Quaternion.identity, Vector3.one).inverse 
            //    *pose3D.Matrix;
            //MutablePose3D mutablePose3D = new MutablePose3D();
            //mutablePose3D.Set(mrotaiontMatrix);
            //return device.GetHeadPose();
            return device.GetHeadPose();
        }
    }

    /// The transformation from head to eye.
    public Pose3D EyePose(Eye eye)
    {
        return device.GetEyePose((GvrViewerInternal.Eye)eye);
    }

    /// The projection matrix for a given eye.
    /// This matrix is an off-axis perspective projection with near and far
    /// clipping planes of 1m and 1000m, respectively.  The GvrEye script
    /// takes care of adjusting the matrix for its particular camera.
    public Matrix4x4 Projection(Eye eye, Distortion distortion = Distortion.Distorted)
    {
        return device.GetProjection((GvrViewerInternal.Eye)eye, (GvrViewerInternal.Distortion)distortion);
    }

    /// The screen space viewport that the camera for the specified eye should render into.
    /// In the _Distorted_ case, this will be either the left or right half of the `StereoScreen`
    /// render texture.  In the _Undistorted_ case, it refers to the actual rectangle on the
    /// screen that the eye can see.
    public Rect Viewport(Eye eye, Distortion distortion = Distortion.Distorted)
    {
        return device.GetViewport((GvrViewerInternal.Eye)eye, (GvrViewerInternal.Distortion)distortion);
    }

    /// The distance range from the viewer in user-space meters where objects may be viewed
    /// comfortably in stereo.  If the center of interest falls outside this range, the stereo
    /// eye separation should be adjusted to keep the onscreen disparity within the limits set
    /// by this range.  If native integration is not supported, or the current VR player is the
    /// in-editor emulator, StereoController will handle this if the _checkStereoComfort_ is
    /// enabled.
    public Vector2 ComfortableViewingRange
    {
        get
        {
            return defaultComfortableViewingRange;
        }
    }
    private readonly Vector2 defaultComfortableViewingRange = new Vector2(0.4f, 100000.0f);

    /// @cond
    // Optional.  Set to a URI obtained from the Google Cardboard profile generator at
    //   https://www.google.com/get/cardboard/viewerprofilegenerator/
    // Example: Cardboard I/O 2015 viewer profile
    //public Uri DefaultDeviceProfile = new Uri("http://google.com/cardboard/cfg?p=CgZHb29nbGUSEkNhcmRib2FyZCBJL08gMjAxNR0J-SA9JQHegj0qEAAAcEIAAHBCAABwQgAAcEJYADUpXA89OghX8as-YrENP1AAYAM");
    public Uri DefaultDeviceProfile = null;
    private static bool XRLoaderStatueInit;
    private static bool mSvrXRLoader;
    /// @endcond
    public static bool SvrXRLoader
    {
        get
        {
#if UNITY_2019_3_OR_NEWER
            if (!XRLoaderStatueInit)
            {
                List<UnityEngine.XR.InputDevice> inputDevices = new List<UnityEngine.XR.InputDevice>();
                UnityEngine.XR.InputDevices.GetDevices(inputDevices);
                foreach (var item in inputDevices)
                {
                    if (item.name.Contains("Skyworth"))
                    {
                        Debug.Log("Svr XRLoader true");
                        mSvrXRLoader = true;
                        break;
                    }
                }
                XRLoaderStatueInit = true;
            }
            return mSvrXRLoader;
#else
            return false;
#endif
        }
    }
    private void InitDevice()
    {
        
        if (device != null)
        {
            device.Destroy();
        }
        Svr.SvrLog.Log("InitDevice");
        
        SVR.AtwAPI.BeginTrace("init");
        device = BaseVRDevice.GetDevice();
        SVR.AtwAPI.EndTrace();
        if (Svr.SvrSetting.IsVR9Device) return;
        device.Init();
        

        SVR.AtwAPI.BeginTrace("other");
        List<string> diagnostics = new List<string>();
        NativeDistortionCorrectionSupported = device.SupportsNativeDistortionCorrection(diagnostics);
        if (diagnostics.Count > 0)
        {
            Debug.LogWarning("Built-in distortion correction disabled. Causes: ["
                             + String.Join("; ", diagnostics.ToArray()) + "]");
        }
        diagnostics.Clear();
        NativeUILayerSupported = device.SupportsNativeUILayer(diagnostics);
        if (diagnostics.Count > 0)
        {
            Debug.LogWarning("Built-in UI layer disabled. Causes: ["
                             + String.Join("; ", diagnostics.ToArray()) + "]");
        }

        if (DefaultDeviceProfile != null)
        {
            device.SetDefaultDeviceProfile(DefaultDeviceProfile);
        }

        device.SetDistortionCorrectionEnabled(distortionCorrection == DistortionCorrectionMethod.Native
            && NativeDistortionCorrectionSupported);
        device.SetNeckModelScale(neckModelScale);

#if !UNITY_HAS_GOOGLEVR || UNITY_EDITOR
        device.SetVRModeEnabled(vrModeEnabled);
#endif  // !UNITY_HAS_GOOGLEVR || UNITY_EDITOR
        SVR.AtwAPI.EndTrace();
        SVR.AtwAPI.BeginTrace("updatedata");
        device.UpdateScreenData();
        SVR.AtwAPI.EndTrace();
    }

    /// @note Each scene load causes an OnDestroy of the current SDK, followed
    /// by and Awake of a new one.  That should not cause the underlying native
    /// code to hiccup.  Exception: developer may call Application.DontDestroyOnLoad
    /// on the SDK if they want it to survive across scene loads.
    void Awake()
    {
        SVR.AtwAPI.BeginTrace("gvrviewer-awake");

        if (instance == null)
        {
            instance = this;
        }
        if (instance != this)
        {
            Debug.LogError("There must be only one GvrViewer object in a scene.");
            UnityEngine.Object.DestroyImmediate(this);
            return;
        }
#if UNITY_IOS
    Application.targetFrameRate = 60;
#endif
        // Prevent the screen from dimming / sleeping
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        if (!VRModeEnabled)
        {
            enabled = false;
            return;
        }
        SVR.AtwAPI.UnityOnPaused(false);
        if (SvrXRLoader) return;
        GvrViewerInternal.Instance.StereoScreenScale = StereoScreenScale;

        // Set up stereo pre- and post-render stages only for:
        // - Unity without the GVR native integration
        // - In-editor emulator when the current platform is Android or iOS.
        //   Since GVR is the only valid VR SDK on Android or iOS, this prevents it from
        //   interfering with VR SDKs on other platforms.
#if !UNITY_HAS_GOOGLEVR || (UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS))
        //AddPrePostRenderStages();
#endif  // !UNITY_HAS_GOOGLEVR || UNITY_EDITOR

        SVR.AtwAPI.BeginTrace("1");
        InitDevice();
        SVR.AtwAPI.EndTrace();
        //if(stereoScreen == null)
        SVR.AtwAPI.BeginTrace("2");
        stereoScreen = device.CreateStereoScreen();
        SVR.AtwAPI.EndTrace();

        Svr.SvrLog.Log("GvrViewer Awake Resume");
        SVR.AtwAPI.EndTrace();

        Application.lowMemory += OnLowMemory;
    }

    private void OnLowMemory()
    {
        Resources.UnloadUnusedAssets();
    }

    void Start()
    {
        if (SvrXRLoader) return;
        // Set up stereo controller only for:
        // - Unity without the GVR native integration
        // - In-editor emulator when the current platform is Android or iOS.
        //   Since GVR is the only valid VR SDK on Android or iOS, this prevents it from
        //   interfering with VR SDKs on other platforms.
#if !UNITY_HAS_GOOGLEVR || (UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS))
        AddStereoControllerToCameras();
#endif  // !UNITY_HAS_GOOGLEVR || UNITY_EDITOR
    }

    //#if !UNITY_HAS_GOOGLEVR || UNITY_EDITOR
    //    void AddPrePostRenderStages()
    //    {
    //        var preRender = UnityEngine.Object.FindObjectOfType<GvrPreRender>();
    //        if (preRender == null)
    //        {
    //            var go = new GameObject("PreRender", typeof(GvrPreRender));
    //            go.SendMessage("Reset");
    //            go.transform.parent = transform;
    //        }
    //        var postRender = UnityEngine.Object.FindObjectOfType<GvrPostRender>();
    //        if (postRender == null)
    //        {
    //            var go = new GameObject("PostRender", typeof(GvrPostRender));
    //            go.SendMessage("Reset");
    //            go.transform.parent = transform;
    //        }
    //    }
    //#endif  // !UNITY_HAS_GOOGLEVR || UNITY_EDITOR

    /// Whether the viewer's trigger was pulled. True for exactly one complete frame
    /// after each pull.
    public bool Triggered { get; private set; }

    /// Whether the viewer was tilted on its side. True for exactly one complete frame
    /// after each tilt.  Whether and how to respond to this event is up to the app.
    public bool Tilted { get; private set; }

    /// Whether the viewer profile has possibly changed.  This is meant to indicate
    /// that a new QR code has been scanned, although currently it is actually set any time the
    /// application is unpaused, whether it was due to a profile change or not.  True for one
    /// frame.
    public bool ProfileChanged { get; private set; }

    /// Whether the user has pressed the "VR Back Button", which on Android should be treated the
    /// same as the normal system Back Button, although you can respond to either however you want
    /// in your app.
    public bool BackButtonPressed { get; private set; }

    private bool canRecent = true;
    public bool CanRecent
    {
        get
        {
            return canRecent;
        }
        set
        {
            canRecent = value;
            if (canRecent)
                Controller.Head.SetAdujst(Quaternion.identity);
        }
    }
    private Quaternion m_RecenterQuaternion;
    // Only call device.UpdateState() once per frame.
    private int updatedToFrame = 0;

    /// Reads the latest tracking data from the phone.  This must be
    /// called before accessing any of the poses and matrices above.
    ///
    /// Multiple invocations per frame are OK:  Subsequent calls merely yield the
    /// cached results of the first call.  To minimize latency, it should be first
    /// called later in the frame (for example, in `LateUpdate`) if possible.
    public void UpdateState()
    {
        if (updatedToFrame != Time.frameCount)
        {
            updatedToFrame = Time.frameCount;
            device.UpdateState();

            if (device.profileChanged)
            {
                if (distortionCorrection != DistortionCorrectionMethod.Native &&
                    device.RequiresNativeDistortionCorrection())
                {
                    DistortionCorrection = DistortionCorrectionMethod.Native;
                }
                if (stereoScreen != null &&
                    device.ShouldRecreateStereoScreen(stereoScreen.width, stereoScreen.height))
                {
                    Svr.SvrLog.Log("StereoScreen set to Null");
                    StereoScreen = null;
                }
            }

            DispatchEvents();
        }
    }

    private void DispatchEvents()
    {
        // Update flags first by copying from device and other inputs.
        Triggered = Input.GetMouseButtonDown(0);
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
        Triggered |= GvrControllerInput.ClickButtonDown;
#endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)

        Tilted = device.tilted;
        ProfileChanged = device.profileChanged;
        BackButtonPressed = device.backButtonPressed || Input.GetKeyDown(KeyCode.Escape);
        // Reset device flags.
        device.tilted = false;
        device.profileChanged = false;
        device.backButtonPressed = false;
    }

    /// Presents the #StereoScreen to the device for distortion correction and display.
    /// @note This function is only used if #DistortionCorrection is set to _Native_,
    /// and it only has an effect if the device supports it.
    public void PostRender(StereoScreen stereoScreen)
    {
        if (SvrXRLoader) return;
        device.PostRender(stereoScreen);
    }
    public void OnPreRender()
    {
        if (SvrXRLoader) return;
        device.PreRender(stereoScreen);
    }

    /// Resets the tracker so that the user's current direction becomes forward.
    public void Recenter()
    {
        if (SvrXRLoader)
        {
#if UNITY_2019_3_OR_NEWER && SVR
            var inputsystem = new List<XRInputSubsystem>();
            SubsystemManager.GetInstances(inputsystem);
            for (int i = 0; i < inputsystem.Count; i++)
            {
                bool statue = inputsystem[i].TryRecenter();
            }
#endif
            return;
        }
        //device.Recenter();
        if (Vector3.Dot(Vector3.up, Quaternion.Euler(GvrViewer.Instance.HeadPose.Orientation.eulerAngles.x, 0, 0) * Vector3.forward) > 0
            && Vector3.Angle(Quaternion.Euler(GvrViewer.Instance.HeadPose.Orientation.eulerAngles.x, 0, 0) * Vector3.forward,Vector3.forward) > 30)
        {
            RecenterXYZ();
        }
        else
        {
            RecenterByYaw();
        }
    }
   
    public void RecenterPreviouse()
    {
        if (SvrXRLoader) return;
        Controller.Head.SetAdujst(m_RecenterQuaternion);
    }
    public void RecenterXYZ()
    {
        m_RecenterQuaternion = GvrViewer.Instance.HeadPose.Orientation;
        Controller.Head.SetAdujst(m_RecenterQuaternion);
    }
    public void RecenterByYaw()
    {
        m_RecenterQuaternion = Quaternion.Euler(0, GvrViewer.Instance.HeadPose.Orientation.eulerAngles.y, 0);
        Controller.Head.SetAdujst(m_RecenterQuaternion);
    }
    private float mTouchTime = 0;
    private void Update()
    {
        if (SvrXRLoader) return;
        if (GvrControllerInput.Recentered)
        {
            //mYaw = 0;
            Recenter();
        }

        //if (Svr.SvrSetting.GetNoloConnected)
        //    NoloRecenter();
    }
#region NOLO RECENTER
    //recenter about
    private int leftcontrollerRecenter_PreFrame = -1;
    private int rightcontrollerRecenter_PreFrame = -1;
    private int recenterSpacingFrame = 20;

    void NoloRecenter()
    {
        //leftcontroller double click system button
        if (GvrControllerInput.GetControllerState(SvrControllerState.NoloLeftContoller).homeButtonUp 
            || GvrControllerInput.GetControllerState(SvrControllerState.NoloRightContoller).homeButtonUp)
        {
            if (Time.frameCount - leftcontrollerRecenter_PreFrame <= recenterSpacingFrame)
            {

                Recenter();
                leftcontrollerRecenter_PreFrame = -1;
            }
            else
            {
                leftcontrollerRecenter_PreFrame = Time.frameCount;
            }
        }
       
    }
#endregion !NOLO RECENTER
    /// Launch the device pairing and setup dialog.
    public void ShowSettingsDialog()
    {
        device.ShowSettingsDialog();
    }

#if !UNITY_HAS_GOOGLEVR || UNITY_EDITOR
    /// Add a StereoController to any camera that does not have a Render Texture (meaning it is
    /// rendering to the screen).
    public static void AddStereoControllerToCameras()
    {
        //for (int i = 0; i < Camera.allCameras.Length; i++)
        //{
        //    Camera camera = Camera.allCameras[i];
        //    if (camera.targetTexture == null &&
        //        camera.cullingMask != 0 &&
        //        camera.GetComponent<StereoController>() == null &&
        //        camera.GetComponent<GvrEye>() == null &&
        //        camera.GetComponent<GvrPreRender>() == null &&
        //        camera.GetComponent<GvrPostRender>() == null)
        //    {
        //        camera.gameObject.AddComponent<StereoController>();
        //    }
        //}
    }
#endif  // !UNITY_HAS_GOOGLEVR || UNITY_EDITOR

    void OnEnable()
    {
        if (SvrXRLoader) return;
#if UNITY_EDITOR
        // This can happen if you edit code while the editor is in Play mode.
        if (device == null)
        {
            InitDevice();
        }
#endif
        Recenter();
        Svr.SvrLog.Log("GvrViewer OnEnable");
    }
    void OnDisable()
    {
        //device.OnPause(true);
        Svr.SvrLog.Log("GvrViewer OnDisable");
    }

    void OnApplicationPause(bool pause)
    {
        Svr.SvrLog.LogFormat("GvrViewer OnApplicationPause,{0}", pause);
        if (SvrXRLoader)
        {
            SVR.AtwAPI.UnityOnPaused(pause);
            return;
        }
        
        
        if (pause)
            device.OnPause(true);
        else
            StartCoroutine(Resume());
    }

    IEnumerator Resume()
    {
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        device.OnPause(false);
        yield return null;
        yield return null;
        yield return null;
        Recenter();
    }
   
    private float TestYaw = 0;
    IEnumerator ResetTest()
    {
        //Recenter();
        yield return new WaitForEndOfFrame();
        //GvrControllerInput.RecenterTrackingOrigin();
        
        //mYaw = TestYaw = GvrControllerInput.Orientation.eulerAngles.y;
        //yield return new WaitForSeconds(3);
        //StartCoroutine(ResetHeadByController());
    }
    //IEnumerator ResetHeadByController()
    //{
        
    //    while (true)
    //    {
    //        if (GvrControllerInput.Orientation.eulerAngles != Vector3.zero)
    //        {

    //            Matrix4x4 matrix4X4 = Matrix4x4.TRS(Vector3.zero, GvrControllerInput.Orientation, Vector3.one).inverse
    //                * HeadPose.Matrix;
    //            mYaw = matrix4X4.rotation.eulerAngles.y;
    //            //Debug.LogFormat("ResetHeadByController:[GvrControllerInput.Orientation = {0}],[mYaw = {1}] "
    //            //    , GvrControllerInput.Orientation.eulerAngles, mYaw);
    //            break;
    //        }
    //        yield return new WaitForEndOfFrame();
    //    }

    //}
    void OnApplicationFocus(bool focus)
    {
        if (SvrXRLoader) return;

        device.OnFocus(focus);
    }

    void OnApplicationQuit()
    {
        if (SvrXRLoader) return;

        //SensorDemo.SaveHMDrotation(HeadPose.Orientation.eulerAngles.y);
        device.OnApplicationQuit();
    }
    
    void OnDestroy()
    {
        if (SvrXRLoader) return;

#if !UNITY_HAS_GOOGLEVR || UNITY_EDITOR
        VRModeEnabled = false;
#endif  // !UNITY_HAS_GOOGLEVR || UNITY_EDITOR
        if (device != null)
        {
            Debug.Log("OnDestroy");
            device.Destroy();
        }
        if (instance == this)
        {
            instance = null;
        }
    }
}
