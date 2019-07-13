// Copyright 2018 Skyworth VR. All rights reserved.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class SvrPlayProgressBarPanel : MonoBehaviour
{
    [SerializeField]
    private Text CurrentTimeText;
    [SerializeField]
    private Text TotalTimeText;
    [SerializeField]
    private Slider PlayPBSlider;

    public delegate void SeekToTime(long time);
    public SeekToTime OnSeekToTime;
    /// <summary>
    /// If the volume is changed by the UI or not.
    /// </summary>
    private bool IsVoluntary;
    private long TotalTime;

    private void Start()
    {
        IsVoluntary = false;
        TotalTime = 0;

        PlayPBSlider.onValueChanged.AddListener(ValueChanged);
    }

    private void ValueChanged(float f)
    {
        if (IsVoluntary)
        {
            IsVoluntary = false;
            return;
        }

        long times = (long)(f * TotalTime);
        int seconds = (int)(times / 1000);
        CurrentTimeText.text = SecondsToHMS(seconds);
        if (OnSeekToTime != null)
            OnSeekToTime(times);
    }

    public void SetTotalTime(long totalTime)
    {
        TotalTime = totalTime;

        int seconds = (int)(totalTime / 1000);
        TotalTimeText.text = SecondsToHMS(seconds);
    }

    public void SetCurrentTime(long currentTime)
    {
        int seconds = (int)(currentTime / 1000);
        CurrentTimeText.text = SecondsToHMS(seconds);

        float t = 0;
        if (TotalTime != 0)
            t = (float)currentTime / TotalTime;

        IsVoluntary = true;
        PlayPBSlider.value = t;
    }

    public string SecondsToHMS(int seconds)
    {
        TimeSpan ts = new TimeSpan(0, 0, Convert.ToInt32(seconds));
        int hour = ts.Hours;
        int minute = ts.Minutes;
        int second = ts.Seconds;

        return string.Format("{0:#00}:{1:#00}:{2:#00}", hour, minute, second);
    }
}
