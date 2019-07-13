/********************************************************************
     Copyright (c) XTech Cloud
     All rights reserved.
*********************************************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace XTC.MegeXR.Core
{
    public interface IPointerInputModule
    {
        EventSystem eventSystem { get; }
        List<RaycastResult> RaycastResultCache { get; }

        bool ShouldActivate();
        void Deactivate();
        BaseEventData GetBaseEventData();

        GameObject FindCommonRoot(GameObject g1, GameObject g2);
        RaycastResult FindFirstRaycast(List<RaycastResult> candidates);
        
    }

    public abstract class XPointerInputModule : BaseInputModule, IPointerInputModule
    {

        public static XPointerInputModule instance { get; set; }

        public XBasePointerInputImpl Impl { get; private set; }

        public static XBasePointer Pointer
        {
            get
            {
                return instance.Impl.Pointer;
            }
            set
            {
                instance.Impl.Pointer = value;
            }
        }

        public static void OnPointerCreated(XBasePointer createdPointer)
        {
            if (instance.Impl.Pointer == null)
            {
                instance.Impl.Pointer = createdPointer;
            }
        }

        protected void setup()
        {
            //base.Awake();
            instance = this;
            Impl = new XReticleInputModuleImpl();
            Impl.inputModule = this;
            Impl.EventExecutor = new XEventExecutor();
            //UpdateImplProperties();
        }

        #region  bridge of BaseInputModule

        protected bool shouldActivateModule()
        {
            return Impl.ShouldActivateModule();
        }

        protected void deactivateModule()
        {
            Impl.DeactivateModule();
        }

        protected bool isPointerOverGameObject(int pointerId)
        {
            return Impl.IsPointerOverGameObject(pointerId);
        }

        protected void process()
        {
            //UpdateImplProperties();
            Impl.Process();
        }



        #endregion

        #region  Implement IPointerInputModule

        public new EventSystem eventSystem
        {
            get
            {
                return base.eventSystem;
            }
        }

        public List<RaycastResult> RaycastResultCache
        {
            get
            {
                return m_RaycastResultCache;
            }
        }

        public new BaseEventData GetBaseEventData()
        {
            return base.GetBaseEventData();
        }

        public bool ShouldActivate()
        {
            return base.ShouldActivateModule();
        }

        public void Deactivate()
        {
            base.DeactivateModule();
        }

        public new GameObject FindCommonRoot(GameObject g1, GameObject g2)
        {
            return BaseInputModule.FindCommonRoot(g1, g2);
        }

        public new RaycastResult FindFirstRaycast(List<RaycastResult> candidates)
        {
            return BaseInputModule.FindFirstRaycast(candidates);
        }

        #endregion
    }//class
}//namespace XVP.VR
