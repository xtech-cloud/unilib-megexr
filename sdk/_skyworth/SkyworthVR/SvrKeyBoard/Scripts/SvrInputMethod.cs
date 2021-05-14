/*
 * @Author: xieminghui
 * @Date: 2020-12-30 14:47:37
 * @Description: Description
 * @LastEditors: xieminghui
 * @LastEditTime: 2020-12-31 15:45:25
 * @Copyright: Copyright 2020 Skyworth VR. All rights reserved.
 */
using Svr.Keyboard;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Svr.Keyboard
{
    public class SvrInputMethod : MonoBehaviour
    {
        public static SvrInputMethod Instacne { get; private set; }
        private KeyboardEvent mKeyboardEvent;
        private RawImage mRawImage;
        private InputMethodApi mInputMethodApi;
        private GLTexture mOriginTexture;
        public string text { get; set; }
        public Action<string, int, int> OnTextChange;
        public Action OnKeyboardClose;
        public bool active { get; private set; }
        private float length;
        private Vector3 selfPosition;
        private Vector3 onHoverPosition;
        private GameObject mMoveRoot;
        private GameObject mKeyboadMoveLayer;
        private Transform mKeyboardSourceRoot;
        private bool isInternalShow = false;
        /// <summary>
        /// 是否使用系统键盘
        /// </summary>
        public static bool system = false;
        /// <summary>
        /// 键盘移动边界
        /// </summary>
        public Vector2 MoveBound = new Vector2(20, 10);
        /// <summary>
        /// 在UI下面的位置
        /// </summary>
        public float m_below_pos = -2.0f;
        public float m_horizontal_offset = 0;
        private Transform mTarget = null;
        private static float UI_INPUTFOWARD = -0.5f;
        private static float KEYBOARDFOLLOW = 4.0f;
        private void Awake()
        {
            Instacne = this;

            mRawImage = transform.GetChild(0).GetComponent<RawImage>();
            mRawImage.gameObject.SetActive(false);
            mKeyboardEvent = transform.GetChild(0).GetComponent<KeyboardEvent>();
#if UNITY_ANDROID && !UNITY_EDITOR
            InputMethodApi.debug = false;
            mInputMethodApi = new InputMethodApi(system);
            mInputMethodApi.OnTextureCreated = InputMethodApi_TextureCreat;
            mInputMethodApi.OnTextChange = InputMethodApi_OnTextChange;
            mInputMethodApi.OnShowKeyboard = InputMethodApi_OnShowKeyboard;
            mInputMethodApi.OnHidEvent = InputMethodApi_OnHidEvent;
            mInputMethodApi.OnMoveStart = InputMethodApi_OnMoveStart;
            //mInputMethodApi.OnMoveEnd = InputMethodApi_OnMoveEnd;
#endif
            mKeyboardEvent.OnGvrPointerHover += MKeyboardEvent_OnGvrPointerHover;
            mKeyboardEvent.OnPointerDown += MKeyboardEvent_OnPointerDown;
            mKeyboardEvent.OnPointerUp += MKeyboardEvent_OnPointerUp;
        }

        private void InputMethodApi_OnMoveEnd()
        {
            ResetRoot();
        }

        private void ResetRoot()
        {
            if (mMoveRoot)
            {
                Vector3 position = transform.position;
                Quaternion rotation = transform.rotation;
                transform.SetParent(mKeyboardSourceRoot);
                transform.position = position;
                transform.rotation = rotation;
                Destroy(mMoveRoot);
                mMoveRoot = null;
                Destroy(mKeyboadMoveLayer);
                mKeyboadMoveLayer = null;


            }
        }
        private void InputMethodApi_OnMoveStart()
        {
            mMoveRoot = new GameObject();

            mMoveRoot.transform.position = GvrPointerInputModule.CurrentRaycastResult.worldPosition;
            //Quaternion quaternion = Quaternion.identity;
            //quaternion.SetLookRotation(mMoveRoot.transform.position - Camera.main.transform.position);
            //mMoveRoot.transform.rotation = quaternion;

            length = Vector3.Distance(mMoveRoot.transform.position, Camera.main.transform.position);
            mKeyboardSourceRoot = transform.parent;
            transform.SetParent(mMoveRoot.transform);
            selfPosition = mMoveRoot.transform.position;

            Vector3 keyboardLayerPos;
            if (mTarget == null)
                keyboardLayerPos = transform.position;
            else
            {
                keyboardLayerPos = mTarget.position;
            }
            Quaternion keyboardLayerRotation;
            if (mTarget == null)
                keyboardLayerRotation = transform.rotation;
            else
            {
                keyboardLayerRotation = mTarget.rotation;
            }

            mKeyboadMoveLayer = GameObject.CreatePrimitive(PrimitiveType.Quad);
            mKeyboadMoveLayer.name = "svr_keyboard_layer";
            mKeyboadMoveLayer.transform.rotation = keyboardLayerRotation;
            DestroyImmediate(mKeyboadMoveLayer.GetComponent<Renderer>());
            mKeyboadMoveLayer.transform.position = keyboardLayerPos;
            mKeyboadMoveLayer.transform.localScale = new Vector3(MoveBound.x, MoveBound.y, 1);
        }

        private void InputMethodApi_OnHidEvent()
        {
            HidKeyboard();
            if (OnKeyboardClose != null) OnKeyboardClose();
        }

        private void InputMethodApi_OnShowKeyboard()
        {
            isInternalShow = true;
            ShowKeyboard();
        }

        private void InputMethodApi_OnTextChange(string arg1, int arg2, int arg3)
        {
            text = arg1;
            if (OnTextChange != null) OnTextChange(arg1, arg2, arg3);
        }

        private void MKeyboardEvent_OnPointerUp(UnityEngine.EventSystems.PointerEventData obj)
        {
            Vector2 keyboardposition = ConverToKeyboardPosition(obj.pointerCurrentRaycast.worldPosition);
            if (mInputMethodApi != null)
                mInputMethodApi.performClickUp((int)keyboardposition.x, (int)keyboardposition.y);
        }

        private void MKeyboardEvent_OnPointerDown(UnityEngine.EventSystems.PointerEventData obj)
        {
            Vector2 keyboardposition = ConverToKeyboardPosition(obj.pointerCurrentRaycast.worldPosition);
            if (mInputMethodApi != null)
                mInputMethodApi.performClickDown((int)keyboardposition.x, (int)keyboardposition.y);
        }

        private void MKeyboardEvent_OnGvrPointerHover(UnityEngine.EventSystems.PointerEventData obj)
        {
            Vector2 keyboardposition = ConverToKeyboardPosition(obj.pointerCurrentRaycast.worldPosition);
            if (mInputMethodApi != null)
                mInputMethodApi.updateFocusPosition((int)keyboardposition.x, (int)keyboardposition.y);
        }

        private void InputMethodApi_TextureCreat(GLTexture gLTexture)
        {
            Texture2D texture = Texture2D.CreateExternalTexture(gLTexture.Width, gLTexture.Hight, TextureFormat.ARGB32, false, false, new IntPtr(gLTexture.TextureID));
            mOriginTexture = gLTexture;
            mRawImage.texture = texture;

            SetKeyboardSize(gLTexture.Width, gLTexture.Hight);
        }
        private Vector2 ConverToKeyboardPosition(Vector3 worldPosition)
        {
            Vector3 localPos = mRawImage.rectTransform.worldToLocalMatrix.MultiplyPoint(worldPosition);
            Vector2 currentSize = mRawImage.rectTransform.sizeDelta;
            Vector2 keyboardsize = Vector2.zero;
            keyboardsize.x = localPos.x + currentSize.x * 0.5f;
            keyboardsize.y = -localPos.y + currentSize.y * 0.5f;
            if (mOriginTexture != null)
            {
                Vector2 sieze = Vector2.zero;
                sieze.x = keyboardsize.x * (mOriginTexture.Width / currentSize.x);
                sieze.y = keyboardsize.y * (mOriginTexture.Hight / currentSize.y);
                return sieze;
            }
            else
                return keyboardsize;
        }
        private void SetKeyboardSize(int o_withd, int o_hight)
        {
            Vector2 currentSize = mRawImage.rectTransform.sizeDelta;
            currentSize.y = currentSize.x * o_hight / o_withd;
            mRawImage.rectTransform.sizeDelta = currentSize;
        }
        // Update is called once per frame
        void Update()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            mInputMethodApi.Update();
#endif
            if (mOriginTexture != null)
                mRawImage.material.SetMatrix("video_matrix", mOriginTexture.getTransformMatrix());

            if (GvrControllerInput.ClickButtonDown && GvrPointerInputModule.CurrentRaycastResult.gameObject == null)
            {
                HidKeyboard();
            }
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.DownArrow))
                InputMethodApi_OnMoveStart();
            if (Input.GetKeyDown(KeyCode.UpArrow))
                InputMethodApi_OnMoveEnd();
#endif
            if (mMoveRoot)
            {
                //Vector3 pos = (Camera.main.transform.forward * length);
                //mMoveRoot.transform.position = pos; 
                //Quaternion quaternion = Quaternion.identity;
                //quaternion.SetLookRotation(mMoveRoot.transform.position - Camera.main.transform.position);
                //mMoveRoot.transform.rotation = quaternion;
                if (GvrPointerInputModule.CurrentRaycastResult.gameObject != null)
                {
                    Vector3 currentPos = GvrPointerInputModule.CurrentRaycastResult.worldPosition;
                    mMoveRoot.transform.position = Vector3.Lerp(mMoveRoot.transform.position, currentPos, Time.deltaTime * KEYBOARDFOLLOW);
                }
            }
#if UNITY_EDITOR
            if (Input.GetMouseButtonUp(0))
#else
            if (GvrControllerInput.ClickButtonUp || GvrControllerInput.TriggerButtonUp)
#endif
            {
                if (mMoveRoot != null)
                    InputMethodApi_OnMoveEnd();
            }
        }
        /// <summary>
        /// 显示键盘
        /// </summary>
        /// <param name="text"></param>
        public void ShowKeyboard(Transform target = null, string text = null)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (text != null)
                mInputMethodApi.show(text);
            else
                mInputMethodApi.show();
#endif
            if (mRawImage)
                mRawImage.gameObject.SetActive(true);
            if (!isInternalShow && !active)
                SetKeyboadPosition(target);
            active = true;
        }
        private void SetKeyboadPosition(Transform target)
        {
            mTarget = target;
            if (target == null)
            {
                //用摄像机计算键盘位置
                Vector3 targetPos = transform.position;
                targetPos.y = Camera.main.transform.position.y;
                float angle = Vector3.Angle(Camera.main.transform.forward, (targetPos - Camera.main.transform.position));
                if (angle < 10) return;
                int direct = 1;
                Debug.Log(Vector3.Dot(Camera.main.transform.forward, transform.right));
                if (Vector3.Dot(Camera.main.transform.forward, transform.right) < 0)
                    direct = -1;
                transform.RotateAround(Camera.main.transform.position, Vector3.up, direct * angle);
            }
            else
            {
                //使用target计算键盘位置
                transform.rotation = target.rotation;
                transform.position = target.position + (transform.up * m_below_pos) + (transform.forward * UI_INPUTFOWARD) + (transform.right * m_horizontal_offset);
            }
        }
        /// <summary>
        /// 隐藏键盘
        /// </summary>
        public void HidKeyboard()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (mInputMethodApi != null)
                mInputMethodApi.hide();
            if (mInputMethodApi != null)
                mInputMethodApi.performClickUp(-1, -1);
#endif
            active = false;
            if (OnKeyboardClose != null) OnKeyboardClose();
            if (mRawImage)
                mRawImage.gameObject.SetActive(active);
            ResetRoot();
        }

        public void setSelection(int pos)
        {

#if UNITY_ANDROID && !UNITY_EDITOR
            mInputMethodApi.setTextSelection(pos,pos);
#endif
        }

        public void OnDisable()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if(mInputMethodApi !=null)
                mInputMethodApi.hide();
#endif

            active = false;

        }

        public void OnDestroy()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if(mInputMethodApi !=null)
                mInputMethodApi.Release();
#endif
        }
        public void Release()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if(mInputMethodApi !=null)
                mInputMethodApi.Release();
#endif
        }

        public void OnApplicationPause(bool pause)
        {
            if (!pause)
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                if (mInputMethodApi != null)
                    mInputMethodApi.Connect(system);
#endif
            }
            else
            {
                HidKeyboard();
                if (mInputMethodApi != null)
                    mInputMethodApi.Disconnect();
            }
        }
    }
}