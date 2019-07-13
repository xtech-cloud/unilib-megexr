// Copyright 2018 Skyworth VR. All rights reserved.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SvrVolumePanel : MonoBehaviour
{
    [SerializeField]
    private Slider VolumeSlider;
    [SerializeField]
    private Text VolumePercentText;

    /// <summary>
    /// If the volume is changed by the UI or not.
    /// </summary>
    private bool IsVoluntary;
    /// <summary>
    /// Have you changed the volume last time.
    /// </summary>
    private bool IsChangedVolume;
    private bool IsShow;

    public delegate void SetCurrentVolumePercent(float volumePercent);
    public SetCurrentVolumePercent OnSetVolume;

    private void Start ()
    {
        IsVoluntary = false;
        IsChangedVolume = false;
        IsShow = false;

        VolumeSlider.onValueChanged.AddListener(ChangeVolume);
    }

    /// <summary>
    /// Changed volume by the UI.
    /// Change the volume of the device at the same time.
    /// </summary>
    /// <param name="f">slider's value</param>
    public void ChangeVolume(float f)
    {
        if (IsChangedVolume)
        {
            IsChangedVolume = false;
            return;
        }

        IsVoluntary = true;

        SetCurrentVolume(f);
    }

    /// <summary>
    /// Changed volume by the device.
    /// Change the volume of the UI at the same time.
    /// </summary>
    /// <param name="f">current volume percentage</param>
    public void ChangeVolumeByDevice(float f)
    {
        IsVoluntary = false;

        SetCurrentVolume(f);
    }

    /// <summary>
    /// Update the volume in the UI.
    /// </summary>
    /// <param name="volume">current volume percentage</param>
    private void SetCurrentVolume(float volume)
    {
        if (volume < 0)
            volume = 0;
        else if (volume > 1)
            volume = 1;

        IsChangedVolume = true;
        VolumeSlider.value = volume; 
        int valuePercent = (int)(volume * 100);
        VolumePercentText.text = valuePercent + "%";
        
        IsChangedVolume = false;
        if (IsVoluntary)
        {
            if (OnSetVolume != null)
                OnSetVolume(volume);
        }
    }

    public void ShowOrHideUI()
    {
        if (IsShow)
            Hide();
        else
            Show();
    }

    private void Show()
    {
        IsShow = true;

        this.transform.localScale = Vector3.one;
    }

    private void Hide()
    {
        IsShow = false;

        this.transform.localScale = Vector3.zero;
    }
}
