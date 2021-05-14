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

using System.Collections.Generic;
using UnityEngine;
#if UNITY_2019_3_OR_NEWER
using UnityEngine.XR;
#endif

/// This script provides head tracking support for a camera.
///
/// Attach this script to any game object that should match the user's head motion.
/// By default, it continuously updates the local transform to GvrViewer.HeadView.
/// A target object may be specified to provide an alternate reference frame for the motion.
///
/// This script will typically be attached directly to a _Camera_ object, or to its
/// parent if you need to offset the camera from the origin.
/// Alternatively it can be inserted as a child of the _Camera_ but parent of the
/// GvrEye camera.  Do this if you already have steering logic driving the
/// mono Camera and wish to have the user's head motion be relative to that.  Note
/// that in the latter setup, head tracking is visible only when VR Mode is enabled.
///
/// In some cases you may need two instances of GvrHead, referring to two
/// different targets (one of which may be the parent), in order to split where
/// the rotation is applied from where the positional offset is applied.  Use the
/// #trackRotation and #trackPosition properties in this case.
[AddComponentMenu("GoogleVR/GvrHead")]
public class GvrHead : MonoBehaviour
{
    /// <summary>
    /// The update type being used by the tracked pose driver
    /// </summary>
    public enum UpdateType
    {
        /// <summary>
        /// Sample input at both update, and directly before rendering. For smooth head pose tracking, 
        /// we recommend using this value as it will provide the lowest input latency for the device. 
        /// This is the default value for the UpdateType option
        /// </summary>
        UpdateAndBeforeRender,
        /// <summary>
        /// Only sample input during the update phase of the frame.
        /// </summary>
        Update,
        /// <summary>
        /// Only sample input directly before rendering
        /// </summary>
        BeforeRender,
    }

    [SerializeField]
    UpdateType m_UpdateType = UpdateType.UpdateAndBeforeRender;
    /// <summary>
    /// The update type being used by the tracked pose driver
    /// </summary>
    public UpdateType updateType
    {
        get { return m_UpdateType; }
        set { m_UpdateType = value; }
    }
    /// Determines whether to apply the user's head rotation to this gameobject's
    /// orientation.  True means to update the gameobject's orientation with the
    /// user's head rotation, and false means don't modify the gameobject's orientation.
    public bool trackRotation = true;

    /// Determines whether to apply ther user's head offset to this gameobject's
    /// position.  True means to update the gameobject's position with the user's head offset,
    /// and false means don't modify the gameobject's position.
    public bool trackPosition = true;

    public static bool TrackPosition { get; private set; }
    /// The user's head motion will be applied in this object's reference frame
    /// instead of the head object's parent.  A good use case is for head-based
    /// steering.  Normally, turning the parent object (i.e. the body or vehicle)
    /// towards the direction the user is looking would carry the head along with it,
    /// thus creating a positive feedback loop.  Use an external target object as a
    /// fixed point of reference for the direction the user is looking.  Often, the
    /// grandparent or higher ancestor is a suitable target.
    public Transform target;

    /// Determines whether the head tracking is applied during `LateUpdate()` or
    /// `Update()`.  The default is false, which means it is applied during `LateUpdate()`
    /// to reduce latency.
    ///
    /// However, some scripts may need to use the camera's direction to affect the gameplay,
    /// e.g by casting rays or steering a vehicle, during the `LateUpdate()` phase.
    /// This can cause an annoying jitter because Unity, during this `LateUpdate()`
    /// phase, will update the head object first on some frames but second on others.
    /// If this is the case for your game, try switching the head to apply head tracking
    /// during `Update()` by setting this to true.
    public bool updateEarly = false;

    /// Returns a ray based on the heads position and forward direction, after making
    /// sure the transform is up to date.  Use to raycast into the scene to determine
    /// objects that the user is looking at.
    public Ray Gaze
    {
        get
        {
            UpdateHead();
            return new Ray(transform.position, transform.forward);
        }
    }

    public delegate void HeadUpdatedDelegate(GameObject head);

    /// Called after the head pose has been updated with the latest sensor data.
    public event HeadUpdatedDelegate OnHeadUpdated;

    void Awake()
    {
        TrackPosition = trackPosition;
        GvrViewer.Create();
        SvrTrackDevices.mTracks.Add(gameObject);
    }

    private Quaternion m_TargetRotation;
    private Quaternion m_PreviousRotation;
    private bool m_Recentering;
    private bool readCount;
    public static Quaternion orientation = new Quaternion();
    private Quaternion mAdjustOritation = Quaternion.identity;
    public Quaternion CurrentAdjustOritation { get { return mAdjustOritation; } }
#if UNITY_2019_3_OR_NEWER
    static internal List<XRNodeState> nodeStates = new List<XRNodeState>();
#endif
    //private int NeedToRecenter = 5;
    //private bool HavedRecenter = false;
    void Update()
    {
        if (GvrViewer.SvrXRLoader && (m_UpdateType == UpdateType.Update ||
                m_UpdateType == UpdateType.UpdateAndBeforeRender))
        {
            UpdateHead();
        }
    }

    //// Normally, update head pose now.
    void LateUpdate()
    {
        if (!GvrViewer.SvrXRLoader)
        {
            UpdateHead();
        }
    }
    protected virtual void FixedUpdate()
    {
        if (GvrViewer.SvrXRLoader && (m_UpdateType == UpdateType.Update ||
               m_UpdateType == UpdateType.UpdateAndBeforeRender))
        {
            UpdateHead();
        }
    }
    protected virtual void OnEnable()
    {
        Application.onBeforeRender += OnBeforeRender;
    }

    protected virtual void OnDisable()
    {
        // remove delegate registration            
        Application.onBeforeRender -= OnBeforeRender;
    }

    protected virtual void OnBeforeRender()
    {
        if (GvrViewer.SvrXRLoader && (m_UpdateType == UpdateType.BeforeRender ||
                m_UpdateType == UpdateType.UpdateAndBeforeRender))
        {
            UpdateHead();
        }
    }
    public void SetAdujst(Quaternion rotation)
    {
        mAdjustOritation = Quaternion.Inverse(rotation);
    }
    // Compute new head pose.
    private void UpdateHead()
    {
        if (!enabled)
            return;
#if UNITY_ANDROID && !UNITY_EDITOR
        if (trackRotation)
        {
            if (GvrViewer.SvrXRLoader) {
#if UNITY_2019_3_OR_NEWER
                InputTracking.GetNodeStates(nodeStates);
                foreach (XRNodeState nodeState in nodeStates)
                {
                    //Debug.Log("nodeState xxxx "+ nodeState.nodeType);
                    if (nodeState.nodeType == XRNode.CenterEye)
                    {
                        nodeState.TryGetRotation(out orientation);
                        break;
                    }
                }
#endif
            }
            else {
                GvrViewer.Instance.UpdateState();
                //orientation = (mAdjust * Matrix4x4.TRS(Vector3.zero, GvrViewer.Instance.HeadPose.Orientation, Vector3.one)).rotation;
                orientation = (mAdjustOritation * GvrViewer.Instance.HeadPose.Orientation);
            }
            if (target == null)
            {
                transform.localRotation = orientation;
            }
            else
            {
                transform.rotation = target.rotation * orientation;
            }

        }
        else
        {
            if (!GvrViewer.SvrXRLoader)
                GvrViewer.Instance.UpdateState();
        }
#endif
        if (trackPosition)
        {
            //Vector3 pos = GvrViewer.Instance.HeadPose.Position;
            Vector3 pos = GvrControllerInput.GetPosition(SvrControllerState.NoloHead);
            //#if NOLOSDK
            //            pos = NoloVR_Controller.GetDevice(NoloDeviceType.Hmd).GetPose().pos;
            //#endif
            if (target == null)
            {
                transform.localPosition = pos;
            }
            else
            {
                transform.position = target.position + target.rotation * pos;
            }
        }
        else
        {
            transform.localPosition = Vector3.zero;
        }

        if (OnHeadUpdated != null)
        {
            OnHeadUpdated(gameObject);
        }

        //if (NeedToRecenter == 0)
        //{
        //    if (!HavedRecenter)
        //    {
        //        GvrViewer.Instance.Recenter();
        //        GvrControllerInput.RecenterController();
        //        HavedRecenter = true;
        //    }
        //}
        //else
        //{
        //    NeedToRecenter--;
        //}
    }

    void OnApplicationPause(bool pause)
    {
        //if (NeedToRecenter == 0)
        //{
        //    NeedToRecenter = 1;
        //    HavedRecenter = false;
        //}
    }
}
