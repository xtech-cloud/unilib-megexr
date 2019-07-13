/********************************************************************
     Copyright (c) XTech Cloud
     All rights reserved.
*********************************************************************/

using UnityEngine;
using UnityEngine.EventSystems;

namespace XTC.MegeXR.Core
{
    public class XPointerHandler
    {
        public GameObject owner {get;set;}

        public void Setup()
        {
            setup();
        }
        
        public void setup()
        {
            EventTrigger et = owner.GetComponent<EventTrigger>();
            if(null == et)
                owner.AddComponent<EventTrigger>();
        }

        public void AddEnterEvent(UnityEngine.Events.UnityAction<BaseEventData> _event)
        {
            EventTrigger et = owner.GetComponent<EventTrigger>();
            EventTrigger.Entry onPointEnter =  new EventTrigger.Entry();
			onPointEnter.eventID = EventTriggerType.PointerEnter;
			if(null != _event) onPointEnter.callback.AddListener(_event);
			et.triggers.Add(onPointEnter);
        }

        public void RemoveEnterEvent(UnityEngine.Events.UnityAction<BaseEventData> _event)
        {
            EventTrigger et = owner.GetComponent<EventTrigger>();
            EventTrigger.Entry onPointEnter =  new EventTrigger.Entry();
			onPointEnter.eventID = EventTriggerType.PointerEnter;
			if(null != _event) onPointEnter.callback.RemoveListener(_event);
			et.triggers.Add(onPointEnter);
        }

        public void AddExitEvent(UnityEngine.Events.UnityAction<BaseEventData> _event)
        {
            EventTrigger et = owner.GetComponent<EventTrigger>();

            EventTrigger.Entry onPointExit =  new EventTrigger.Entry();
			onPointExit.eventID = EventTriggerType.PointerExit;
			if(null != _event) onPointExit.callback.AddListener(_event);
			et.triggers.Add(onPointExit);
        }

        public void RemoveExitEvent(UnityEngine.Events.UnityAction<BaseEventData> _event)
        {
            EventTrigger et = owner.GetComponent<EventTrigger>();

            EventTrigger.Entry onPointExit =  new EventTrigger.Entry();
			onPointExit.eventID = EventTriggerType.PointerExit;
			if(null != _event) onPointExit.callback.RemoveListener(_event);
			et.triggers.Add(onPointExit);
        }

        public void AddClickEvent(UnityEngine.Events.UnityAction<BaseEventData> _event)
        {
            EventTrigger et = owner.GetComponent<EventTrigger>();

            EventTrigger.Entry onPointClick =  new EventTrigger.Entry();
			onPointClick.eventID = EventTriggerType.PointerClick;
			if(null != _event) onPointClick.callback.AddListener(_event);
			et.triggers.Add(onPointClick);
        }

        public void RemoveClickEvent(UnityEngine.Events.UnityAction<BaseEventData> _event)
        {
            EventTrigger et = owner.GetComponent<EventTrigger>();

            EventTrigger.Entry onPointClick =  new EventTrigger.Entry();
			onPointClick.eventID = EventTriggerType.PointerClick;
			if(null != _event) onPointClick.callback.RemoveListener(_event);
			et.triggers.Add(onPointClick);
        }
    }//class
}//namespace