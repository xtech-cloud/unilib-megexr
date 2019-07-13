// Copyright 2018 Skyworth VR. All rights reserved.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SvrVideoControlPanel : MonoBehaviour
{
    public SvrVideoPlayerDemo SvrVideoPlayerDemo;

    [SerializeField]
    private Text VideoNameText;
    [SerializeField]
    private Button PlayBtn;
    [SerializeField]
    private Button PauseBtn;
    [SerializeField]
    private Button PreviousBtn;
    [SerializeField]
    private Button NextBtn;
    [SerializeField]
    private Button VolumeBtn;
    [SerializeField]
    private SvrPlayProgressBarPanel PlayPBPanel;
    [SerializeField]
    private SvrVolumePanel SvrVolumePanel;

    private void OnEnable ()
    {
        PlayPBPanel.OnSeekToTime += SvrVideoPlayerDemo.SvrVideoPlayer.SeekToTime;
        SvrVolumePanel.OnSetVolume += SvrVideoPlayerDemo.SvrVideoPlayer.SetCurrentVolumePercent;
    }

    private void OnDisable()
    {
        PlayPBPanel.OnSeekToTime -= SvrVideoPlayerDemo.SvrVideoPlayer.SeekToTime;
        SvrVolumePanel.OnSetVolume -= SvrVideoPlayerDemo.SvrVideoPlayer.SetCurrentVolumePercent;
    }

    public void SetPlayControlButtonStatus(bool isPlay)
    {
        if(isPlay)
        {
            PlayBtn.transform.localScale = Vector3.zero;
            PauseBtn.transform.localScale = Vector3.one;
        }
        else
        {
            PlayBtn.transform.localScale = Vector3.one;
            PauseBtn.transform.localScale = Vector3.zero;
        }
    }

    public void ClickPlayBtn()
    {
        SvrVideoPlayerDemo.SvrVideoPlayer.Play();
        SetPlayControlButtonStatus(true);
    }

    public void ClickPauseBtn()
    {
        SvrVideoPlayerDemo.SvrVideoPlayer.Pause();
        SetPlayControlButtonStatus(false);
    }

    public void ClickPreviousBtn()
    {
        SvrVideoPlayerDemo.SvrVideoPlayer.Stop();
        SvrVideoPlayerDemo.SvrVideoPlayer.Release();
        SvrVideoPlayerDemo.PlayVideoByIndex(-1);
    }

    public void ClickNextBtn()
    {
        SvrVideoPlayerDemo.SvrVideoPlayer.Stop();
        SvrVideoPlayerDemo.SvrVideoPlayer.Release();
        SvrVideoPlayerDemo.PlayVideoByIndex(1);
    }

    public void ClickVolumeBtn()
    {
        SvrVolumePanel.ShowOrHideUI();
    }

    public void ChangeVolumeByDevice(float volumePercent)
    {
        SvrVolumePanel.ChangeVolumeByDevice(volumePercent);
    }

    public void SetVideoName(string name)
    {
        VideoNameText.text = name;
    }

    public void SetVideoTotalTime(long time)
    {
        PlayPBPanel.SetTotalTime(time);
    }

    public void SetVideoCurrentTime(long time)
    {
        PlayPBPanel.SetCurrentTime(time);
    }
}
