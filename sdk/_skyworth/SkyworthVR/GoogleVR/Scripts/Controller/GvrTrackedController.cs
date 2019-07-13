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
using System.Collections;

/// Represents an object tracked by controller input.
///
/// Updates the position and rotation of an object to approximate the controller by using
/// a _GvrBaseArmModel_ and propagates the _GvrBaseArmModel_ to all _IGvrArmModelReceivers_
/// underneath this object.
///
/// Manages the active status of the tracked controller based on controller connection status.
public class GvrTrackedController : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Arm model used to control the pose (position and rotation) of the object, " +
      "and to propagate to children that implement IGvrArmModelReceiver.")]
    private GvrBaseArmModel armModel;

    [SerializeField]
    [Tooltip("Is the object's active status determined by the controller connection status.")]
    private bool isDeactivatedWhenDisconnected = true;

    /// Arm model used to control the pose (position and rotation) of the object, and to propagate to
    /// children that implement IGvrArmModelReceiver.
    public GvrBaseArmModel ArmModel
    {
        get
        {
            return armModel;
        }
        set
        {
            if (armModel == value)
            {
                return;
            }

            armModel = value;
            PropagateArmModel();
        }
    }

    /// Is the object's active status determined by the controller connection status.
    public bool IsDeactivatedWhenDisconnected
    {
        get
        {
            return isDeactivatedWhenDisconnected;
        }
        set
        {
            if (isDeactivatedWhenDisconnected == value)
            {
                return;
            }

            isDeactivatedWhenDisconnected = value;

            if (isDeactivatedWhenDisconnected)
            {
                OnControllerStateChanged(GvrControllerInput.State, GvrControllerInput.State);
            }
        }
    }

    private GvrBasePointer mBasePointer;
    public void PropagateArmModel()
    {
        IGvrArmModelReceiver[] receivers =
          GetComponentsInChildren<IGvrArmModelReceiver>(true);

        for (int i = 0; i < receivers.Length; i++)
        {
            IGvrArmModelReceiver receiver = receivers[i];
            receiver.ArmModel = armModel;
        }
    }

    void Awake()
    {
        SVR.AtwAPI.BeginTrace("track-awake");
        mBasePointer = GetComponentInChildren<GvrBasePointer>();
//#if NOLOSDK
        GvrControllerInput.OnConterollerChanged += GvrControllerInput_OnConterollerChanged;
//#else
//        GvrControllerInput.OnStateChanged += OnControllerStateChanged;
//#endif
        UpdatePose();
        GvrControllerInput.OnGvrPointerEnable += GvrControllerInput_OnGvrPointerEnable;
        SVR.AtwAPI.EndTrace();
    }

    private void GvrControllerInput_OnGvrPointerEnable(bool obj)
    {
        if (GvrControllerInput.SvrState == SvrControllerState.GvrController)
        {
            gameObject.SetActive(obj);
            GvrPointerInputModule.Pointer = obj ? mBasePointer : null;
        }
            
    }

    private void GvrControllerInput_OnConterollerChanged(SvrControllerState state, SvrControllerState oldState)
    {
        SvrControllerState target = SvrControllerState.NoloLeftContoller | SvrControllerState.NoloRightContoller;
        if ((state & target) != 0)
        {
            gameObject.SetActive(false);
            if (GvrPointerInputModule.Pointer == mBasePointer)
            {
                GvrPointerInputModule.Pointer = null;
            }
        }
        else
        {
            //OnControllerStateChanged(GvrControllerInput.State, GvrControllerInput.State);
            if (isDeactivatedWhenDisconnected && enabled)
            {
                gameObject.SetActive(state == SvrControllerState.GvrController);
                if (gameObject.activeSelf)
                {
                    GvrPointerInputModule.Pointer = GetComponentInChildren<GvrBasePointer>();
                }
                else
                {
                    if (GvrPointerInputModule.Pointer == mBasePointer)
                    {
                        GvrPointerInputModule.Pointer = null;
                    }
                }
                    
            }
        }
    }

    void OnEnable()
    {
        SVR.AtwAPI.BeginTrace("track-onEnable");
        // Update the position using OnPostControllerInputUpdated.
        // This way, the position and rotation will be correct for the entire frame
        // so that it doesn't matter what order Updates get called in.
        GvrControllerInput.OnPostControllerInputUpdated += OnPostControllerInputUpdated;

        /// Force the pose to update immediately in case the controller isn't updated before the next
        /// time a frame is rendered.
        UpdatePose();

        /// Check the controller state immediately whenever this script is enabled.
//#if NOLOSDK
        GvrControllerInput_OnConterollerChanged(GvrControllerInput.SvrState, GvrControllerInput.SvrState);
        //#else
        //        OnControllerStateChanged(GvrControllerInput.State, GvrControllerInput.State);
        //#endif
        SVR.AtwAPI.EndTrace();
    }

    void OnDisable()
    {
        GvrControllerInput.OnPostControllerInputUpdated -= OnPostControllerInputUpdated;
    }

    void Start()
    {
        PropagateArmModel();
//#if NOLOSDK
        GvrControllerInput_OnConterollerChanged(GvrControllerInput.SvrState, GvrControllerInput.SvrState);
//#else
//        OnControllerStateChanged(GvrControllerInput.State, GvrControllerInput.State);
//#endif
    }

    void OnDestroy()
    {
        GvrControllerInput.OnGvrPointerEnable -= GvrControllerInput_OnGvrPointerEnable;
//#if NOLOSDK
        GvrControllerInput.OnConterollerChanged -= GvrControllerInput_OnConterollerChanged;
//#else
//        GvrControllerInput.OnStateChanged -= OnControllerStateChanged;
//#endif
    }

    private void OnPostControllerInputUpdated()
    {
        UpdatePose();
    }

    private void OnControllerStateChanged(GvrConnectionState state, GvrConnectionState oldState)
    {
        if (isDeactivatedWhenDisconnected && enabled)
        {
            gameObject.SetActive(state == GvrConnectionState.Connected);
            if (gameObject.activeSelf)
                GvrPointerInputModule.Pointer = GetComponentInChildren<GvrBasePointer>();
        }
    }

    private void UpdatePose()
    {
        if (armModel == null)
        {
            return;
        }

        transform.localPosition = ArmModel.ControllerPositionFromHead;
        transform.localRotation = ArmModel.ControllerRotationFromHead;
    }

#if UNITY_EDITOR
    /// If the "armModel" serialized field is changed while the application is playing
    /// by using the inspector in the editor, then we need to call the PropagateArmModel
    /// to ensure all children IGvrArmModelReceiver are updated.
    /// Outside of the editor, this can't happen because the arm model can only change when
    /// a Setter is called that automatically calls PropagateArmModel.
    void OnValidate()
    {
        if (Application.isPlaying && isActiveAndEnabled)
        {
            PropagateArmModel();
            OnControllerStateChanged(GvrControllerInput.State, GvrControllerInput.State);
        }
    }
#endif  // UNITY_EDITOR

}
