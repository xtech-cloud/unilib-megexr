
/********************************************************************
     Copyright (c) XTech Cloud
     All rights reserved.
*********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using XTC.MegeXR.Core;

namespace XTC.MegeXR.Decorator
{
    public class GazeFacadeDecorator : MonoBehaviour
    {
        public enum ExitAction
        {
            NONE,
            CLEAR,
            FILL
        }
        public bool visible = true;
        public float duration = 1f;
        public Image fillImage = null;
        public ExitAction exitAction = ExitAction.NONE;


        void Start()
        {
            string objID = string.Format("{0}", this.gameObject.GetInstanceID());

            XReticleAgent agent = new XReticleAgent();
            agent.uuid = objID;
            agent.duration = duration;
            agent.visible = visible;
            agent.onMangerUpdate = onManger;
            agent.onPointEnter = onPointEnter;
            agent.onPointExit = onPointExit;
            XReticlePool.Register(agent);
        }

        void OnDestory()
        {
            string objID = string.Format("{0}", this.gameObject.GetInstanceID());
            XReticlePool.Cancel(objID);
        }

        void onManger(float _manger)
        {
            if (null != fillImage)
                fillImage.fillAmount = _manger / 360f;
        }

        void onPointEnter()
        {
            if (null != fillImage)
                fillImage.fillAmount = 0;
        }

        void onPointExit()
        {
            if (null != fillImage)
            {
                if(ExitAction.CLEAR == exitAction)
                    fillImage.fillAmount = 0;
                else if(ExitAction.FILL == exitAction)
                    fillImage.fillAmount = 1;
            }
        }
    }
}//namespace
