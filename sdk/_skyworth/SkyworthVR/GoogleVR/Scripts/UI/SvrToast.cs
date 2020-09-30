using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SvrToast : MonoBehaviour
{
    [SerializeField]
    private Text mText;

    private GameObject root;
    private float showtime;
    internal class SystemCurrentLanguage
    {
#if UNITY_EDITOR
        public static SystemLanguage m_systemLanguage = SystemLanguage.Chinese;
#endif
        public static SystemLanguage CurrentLanguage
        {
            get
            {
#if UNITY_EDITOR
                return m_systemLanguage;
#else
                SystemLanguage systemLanguage = SystemLanguage.Chinese;
                AndroidJavaClass localclass = new AndroidJavaClass("java.util.Locale");
                AndroidJavaObject localobject = localclass.CallStatic<AndroidJavaObject>("getDefault");
                string language = localobject.Call<string>("getLanguage");
                string country = localobject.Call<string>("getCountry");

                if (language == "zh" && country == "CN")
                {
                    systemLanguage = SystemLanguage.Chinese;
                }
                else if (language == "zh" && (country == "TW" || country =="MO" || country =="HK"))
                {
                    systemLanguage = SystemLanguage.ChineseTraditional;
                }
                else if (language == "ja" && country == "JP")
                {
                    systemLanguage = SystemLanguage.Japanese;
                }
                else if (language == "ko" && country == "KR")
                {
                    systemLanguage = SystemLanguage.Korean;
                }
                else if (language == "th" && country == "TH")
                {
                    systemLanguage = SystemLanguage.Thai;
                }
                else if (language == "de" && country == "DE")
                {
                    systemLanguage = SystemLanguage.German;
                }
                else
                {
                    systemLanguage = SystemLanguage.English;
                }
                return systemLanguage;
#endif
            }
        }

    }
    // Start is called before the first frame update
    void Start()
    {
        root = transform.GetChild(0).gameObject;
        root.SetActive(false);

        setText();
    }
    private void setText()
    {
        string msg_key_down = "Currently in handle controlling mode";
        SystemLanguage systemLanguage = SystemCurrentLanguage.CurrentLanguage;
        switch (systemLanguage)
        {
            case SystemLanguage.German:
                msg_key_down = "Willkommen beim Konsolenmodus";
                break;
            case SystemLanguage.Japanese:
                msg_key_down = "現在、コントローラ状態にあります";
                break;
            case SystemLanguage.Korean:
                msg_key_down = "현재 컨트롤러 상태입니다";
                break;
            case SystemLanguage.Thai:
                msg_key_down = "ขณะนี้อยู่ในโหมดควบคุม";
                break;
            case SystemLanguage.Chinese:
                msg_key_down = "当前处于手柄模式";
                break;
            case SystemLanguage.ChineseTraditional:
                msg_key_down = "當前處於手柄模式";
                break;
            default:
                break;
        }

        mText.text = msg_key_down;
    }
    // Update is called once per frame
    void Update()
    {
        if (!Svr.SvrSetting.IsVR9Device) return;
        if (GvrControllerInput.SvrState != SvrControllerState.None &&  GetTouchDown() )
        {
            setText();
            root.SetActive(true);
            showtime = Time.time;
        }

        if (root.activeSelf && Time.time - showtime >= 2.0f)
        {
            root.SetActive(false);
        }
    }
    private static bool GetTouchDown()
    {
        if (AndroidInput.touchCountSecondary > 0)
        {
            UnityEngine.Touch touch = AndroidInput.GetSecondaryTouch(0);
            return touch.phase == TouchPhase.Began;
        }
        else
        {
            return false;
        }
    }

    private static bool GetTouchUp()
    {
        if (AndroidInput.touchCountSecondary > 0)
        {
            UnityEngine.Touch touch = AndroidInput.GetSecondaryTouch(0);
            return touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled;
        }
        else
        {
            return false;
        }
    }

    private static bool GetTouch()
    {
        if (AndroidInput.touchCountSecondary > 0)
        {
            UnityEngine.Touch touch = AndroidInput.GetSecondaryTouch(0);
            return touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary;
        }
        else
        {
            return false;
        }
    }
}
