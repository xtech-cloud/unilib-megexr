/********************************************************************
     Copyright (c) XTech Cloud
     All rights reserved.
*********************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


namespace XTC.MegeXR.Core
{
    public class XReticleCountdown
    {
        public class MangerCallbackField
        {
            public CallBack.Invoke<float> method;
        }

        public static XReticleCountdown instance {get;set;}
        
        public Transform owner { get; set; }
        public Material MaterialComp { get; private set; }
        public XReticlePointer reticlePointer  { get; set; }
        public float manger {get; private set;}


        private readonly float DEFALUTCOUNT = 1f;
        private float enterTime_ = 0;
        private float targetTime_;
        private bool isContitnue_;
        private int EventExcuteCount = 0;
        private GameObject raycastResultObj_;
        private Renderer rendererComponent_;
        private MeshRenderer meshRenderer_;
        //private MeshRenderer selfRenderer_;

        private bool visiable = true;

        // Use this for initialization
        public void Setup()
        {
            targetTime_ = DEFALUTCOUNT;

            reticlePointer.OnPointerEnterCallback = onPointerEnterCallback;
            reticlePointer.OnPointerHoverCallback = onPointerHoverCallback;
            reticlePointer.OnPointerExitCallback = onPointerExitCallback;
            reticlePointer.OnPointerClickCallback = onPointerClickCallback;

            meshRenderer_ = reticlePointer.owner.GetComponent<MeshRenderer>();
            //selfRenderer_ = owner.GetComponent<MeshRenderer>();

            rendererComponent_ = owner.GetComponent<Renderer>();
            rendererComponent_.sortingOrder = meshRenderer_.sortingOrder;

            MaterialComp = rendererComponent_.material;
            CreateReticleVertices();
        }

        public void Enable()
        {
            //.GazeModeEnable = true;
        }
        public void Disable()
        {
            //mGvrReticlePointer.GazeModeEnable = false;
        }

        public void RenderObject()
        {
            MaterialComp.SetFloat("_InnerDiameter", meshRenderer_.material.GetFloat("_InnerDiameter"));
            MaterialComp.SetFloat("_OuterDiameter", meshRenderer_.material.GetFloat("_OuterDiameter"));
            MaterialComp.SetFloat("_DistanceInMeters", meshRenderer_.material.GetFloat("_DistanceInMeters"));
            rendererComponent_.sortingOrder = meshRenderer_.sortingOrder;
        }
        public void Pause(bool pause)
        {
            targetTime_ = DEFALUTCOUNT;
            enterTime_ = 0;
            EventExcuteCount = 0;
            isContitnue_ = false;
            manger = 0;
            raycastResultObj_ = null;
        }

        private void onPointerHoverCallback(XBasePointer _pointer, RaycastResult _raycastResult)
        {
            if (!raycastResultObj_) 
                return;

            //Debug.LogFormat("kevin:{0} - {1} = {2} < {3}", Time.time, mEnterTime,(Time.time - mEnterTime), mTargetTime);
            if (Time.time - enterTime_ < targetTime_)
            {
                manger = (360.0f / targetTime_) * (Time.time - enterTime_);
            }
            else
            {
                manger = 360f;
                //Debug.LogFormat("kevin:EventExcuteCount {0}", EventExcuteCount);
                if (EventExcuteCount < 2)
                {
                    EventExcuteCount++;                    
                    if (EventExcuteCount > 1)
                    {
                        XPointerInputModule module = EventSystem.current.currentInputModule as XPointerInputModule;
                        module.Impl.MockClick();
                        if (isContitnue_)
                        {
                            enterTime_ = Time.time;
                            EventExcuteCount = 0;
                        }
                    }
                }
            }
            if(visiable)
                MaterialComp.SetFloat("_FillAmount", manger);
            else
                MaterialComp.SetFloat("_FillAmount", 0f);


            string objID = string.Format("{0}", raycastResultObj_.GetInstanceID());
            XReticleAgent reticle = XReticlePool.Find(objID);
            if(null == reticle)
                return;
            reticle.onMangerUpdate(manger);
        }
        
        private void onPointerEnterCallback(XBasePointer _pointer, RaycastResult _raycastResult)
        {
            raycastResultObj_ = null;
            enterTime_ = Time.time;
            MaterialComp.SetFloat("_FillAmount", 0);
            manger = 0;
            //EventExcuted = false;
            EventExcuteCount = 0;
            GameObject click = _raycastResult.gameObject;
            if (click)
            {
                raycastResultObj_ = click;
                targetTime_ = DEFALUTCOUNT;

                string objID = string.Format("{0}", click.GetInstanceID());

                XReticleAgent reticle = XReticlePool.Find(objID);
                if(null == reticle)
                    return;

                targetTime_ = reticle.duration;
                visiable = reticle.visible;
            }
        }

        private void onPointerClickCallback(XBasePointer _pointer, RaycastResult _raycastResult)
        {
        }


        private void onPointerExitCallback(XBasePointer _pointer, GameObject _gameobject)
        {
            targetTime_ = DEFALUTCOUNT;
            enterTime_ = 0;
            EventExcuteCount = 0;
            isContitnue_ = false;
            //EventExcuted = false;
            manger = 0;
            MaterialComp.SetFloat("_FillAmount", 0);
            raycastResultObj_ = null;
        }

        

        private void CreateReticleVertices()
        {
            Mesh mesh = new Mesh();
            owner.gameObject.AddComponent<MeshFilter>();
            owner.GetComponent<MeshFilter>().mesh = mesh;

            int segments_count = 20;
            int vertex_count = (segments_count + 1) * 2;

            #region Vertices

            Vector3[] vertices = new Vector3[vertex_count];
            Vector2[] UVS = new Vector2[vertex_count];
            const float kTwoPi = Mathf.PI * 2.0f;
            int vi = 0;
            for (int si = 0; si <= segments_count; ++si)
            {
                // Add two vertices for every circle segment: one at the beginning of the
                // prism, and one at the end of the prism.
                float angle = (float)si / (float)(segments_count) * kTwoPi;
                //Debug.Log(angle);
                float x = Mathf.Sin(angle);
                float y = Mathf.Cos(angle);

                vertices[vi++] = new Vector3(x, y, 0.0f); // Outer vertex.
                vertices[vi++] = new Vector3(x, y, 1.0f); // Inner vertex.
                UVS[vi - 2] = new Vector2(x, y);
                UVS[vi - 1] = new Vector2(x, y);
            }
            #endregion

            #region Triangles
            int indices_count = (segments_count + 1) * 3 * 2;
            int[] indices = new int[indices_count];

            int vert = 0;
            int idx = 0;
            for (int si = 0; si < segments_count; ++si)
            {
                indices[idx++] = vert + 1;
                indices[idx++] = vert;
                indices[idx++] = vert + 2;

                indices[idx++] = vert + 1;
                indices[idx++] = vert + 2;
                indices[idx++] = vert + 3;

                vert += 2;
            }
            #endregion

            mesh.vertices = vertices;
            mesh.triangles = indices;
            mesh.uv = UVS;


            mesh.RecalculateBounds();
        }
    }//class
}//namespace XVP.VR
