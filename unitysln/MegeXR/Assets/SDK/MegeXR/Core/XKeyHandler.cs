/********************************************************************
     Copyright (c) XTech Cloud
     All rights reserved.
*********************************************************************/

using UnityEngine.Events;

namespace XTC.MegeXR.Core
{
    [System.Serializable]
    public class XKeyEvent : UnityEvent<int>
    {
    }

    public class XKeyHandler
    {
        public enum Key
        {
            RETURN,
            OK,
            HOME
        }

        public static XKeyEvent onKeyDown = new XKeyEvent();
        public static XKeyEvent onKeyUp = new XKeyEvent();
        public static XKeyEvent onKeyHold = new XKeyEvent();

        public static void DownKey(int _keyCode)
        {
            if (null != onKeyDown)
                onKeyDown.Invoke(_keyCode);
        }

        public static void UpKey(int _keyCode)
        {
            if (null != onKeyUp)
                onKeyUp.Invoke(_keyCode);
        }

        public static void HoldKey(int _keyCode)
        {
            if (null != onKeyHold)
                onKeyHold.Invoke(_keyCode);
        }
    }//class
}//namespace