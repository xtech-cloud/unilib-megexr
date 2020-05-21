using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
using UnityEngine.UI;

public class OutLog : MonoBehaviour
{
    static List<string> mLines = new List<string>();
    static List<string> mWriteTxt = new List<string>();
    private string outpath;
    private string FileName = "outLog.txt";
    private Text lable;
    void Start()
    {
        lable = GetComponent<Text>();
        //Application.persistentDataPath Unity中只有这个路径是既可以读也可以写的。
        if (string.IsNullOrEmpty(Application.persistentDataPath))
        {
            Debug.LogError("无法获得Android路径");
        }
#if UNITY_EDITOR
        outpath = Application.dataPath + "/Log";
#else
        outpath = Application.persistentDataPath + "/Log";
#endif
        if (!Directory.Exists(outpath))
        {
            Directory.CreateDirectory(outpath);
        }
        outpath += "/" + FileName;
        //每次启动客户端删除之前保存的Log
        if (System.IO.File.Exists(outpath))
        {
            File.Delete(outpath);
        }

        //在这里做一个Log的监听
        Application.RegisterLogCallback(HandleLog);
        //一个输出
        GameObject.DontDestroyOnLoad(gameObject);

        Debug.Log("QualitySettings.antiAliasing " + QualitySettings.antiAliasing + "  ");
    }

    void Update()
    {
        //因为写入文件的操作必须在主线程中完成，所以在Update中哦给你写入文件。
        try
        {
            if (mWriteTxt.Count > 0)
            {
                string[] temp = mWriteTxt.ToArray();
                foreach (string t in temp)
                {
                    using (StreamWriter writer = new StreamWriter(outpath, true, Encoding.UTF8))
                    {
                        writer.WriteLine("[" + DateTime.Now + "]:" + t);
                    }
                    mWriteTxt.Remove(t);
                }
            }
        }
        catch (Exception e)
        {
            mWriteTxt.Clear();
            Debug.Log(e.Message);
        }

        if (lable == null) return;
        if (mLines.Count > 0)
        {
            lable.text = "error:" + mLines.Count;
        }
        else
        {
            lable.text = "";
        }

    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        string msg = "";
        if (type == LogType.Error || type == LogType.Exception)
        {

            if (!logString.Contains("[EGL]"))
            {
                Log(logString, stackTrace);
            }

            msg = "[ERROR]";
            msg += logString;
            msg += "\n" + stackTrace;


        }
        else
        {
            msg = logString + "\n\t" + stackTrace;
        }
        mWriteTxt.Add(msg);
    }

    //这里我把错误的信息保存起来，用来输出在手机屏幕上
    static public void Log(params object[] objs)
    {
        string text = "";
        for (int i = 0; i < objs.Length; ++i)
        {
            if (i == 0)
            {
                text += objs[i].ToString();
            }
            else
            {
                text += ", " + objs[i].ToString();
            }
        }
        if (Application.isPlaying)
        {
            if (mLines.Count > 20)
            {
                mLines.RemoveAt(0);
            }
            mLines.Add(text);

        }
    }

    void OnGUI()
    {


        //for (int i = 0, imax = mLines.Count; i < imax; ++i)
        //{
        //    GUILayout.Label(mLines[i]);
        //}
    }
}