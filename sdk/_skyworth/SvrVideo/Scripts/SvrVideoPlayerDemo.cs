// Copyright 2018 Skyworth VR. All rights reserved.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StereoType { None, LeftRight, TopBottom }

public class SvrVideoPlayerDemo : MonoBehaviour
{
    public SvrVideoPlayer SvrVideoPlayer;

    [SerializeField]
    private string[] VideoUrls;
    [SerializeField]
    private SvrVideoControlPanel SvrVideoControlPanel;
    [SerializeField]
    private StereoType StereoType;

    private int videoCount;
    private int currentPlayingVideoIndex;

    private void Start()
    {
        videoCount = VideoUrls.Length;
        currentPlayingVideoIndex = 0;

        if (SvrVideoPlayer == null)
            SvrVideoPlayer = GetComponent<SvrVideoPlayer>();

        SvrVideoPlayer.OnEnd += OnEnd;
        SvrVideoPlayer.OnReady += OnReady;
        SvrVideoPlayer.OnVolumeChange += OnVolumeChange;
        SvrVideoPlayer.OnProgressChange += OnProgressChange;
        SvrVideoPlayer.OnVideoError += OnVideoError;

        PlayVideoByIndex(0);
    }

    public void PlayVideoByIndex(int index)
    {
        if (videoCount < 0)
            return;

        if (currentPlayingVideoIndex + index > videoCount - 1)
            currentPlayingVideoIndex = 0;
        else if (currentPlayingVideoIndex + index < 0)
            currentPlayingVideoIndex = videoCount - 1;
        else
            currentPlayingVideoIndex += index;

        // Use CreatVideoPlayer before PreparedPlayVideo.
        SvrVideoPlayer.CreatVideoPlayer();
        // Set video data source.
        SvrVideoPlayer.PreparedPlayVideo(VideoUrls[currentPlayingVideoIndex]);

        // Set video stereo mode.
        if (StereoType == StereoType.LeftRight)
            SvrVideoPlayer.SetPlayMode3DLeftRight();
        else if (StereoType == StereoType.TopBottom)
            SvrVideoPlayer.SetPlayMode3DTopBottom();
        else
            SvrVideoPlayer.SetPlayMode2D();

        string name = VideoUrls[currentPlayingVideoIndex].Substring(VideoUrls[currentPlayingVideoIndex].LastIndexOf('/') + 1);
        SvrVideoControlPanel.SetVideoName(name);
        SvrVideoControlPanel.SetPlayControlButtonStatus(true);
        SvrVideoControlPanel.SetVideoCurrentTime(0);
    }

    private void OnEnd()
    {
        SvrVideoControlPanel.SetPlayControlButtonStatus(false);
        SvrVideoPlayer.Release();
        // Play next video.
        PlayVideoByIndex(1);
    }

    private void OnReady()
    {
        long totalTime = SvrVideoPlayer.GetVideoDuration();
        SvrVideoControlPanel.SetVideoTotalTime(totalTime);
    }

    private void OnVolumeChange(float volumePercent)
    {
        SvrVideoControlPanel.ChangeVolumeByDevice(volumePercent);
    }

    private void OnProgressChange(int time)
    {
        SvrVideoControlPanel.SetVideoCurrentTime(time);
    }

    private void OnVideoError(ExceptionEvent errorCode, string errMessage)
    {
        Debug.LogErrorFormat("{0}:{1}", errorCode.ToString(), errMessage);
    }

    private void OnApplicationQuit()
    {
        SvrVideoPlayer.OnEnd -= OnEnd;
        SvrVideoPlayer.OnReady -= OnReady;
        SvrVideoPlayer.OnVolumeChange -= OnVolumeChange;
        SvrVideoPlayer.OnProgressChange -= OnProgressChange;
        SvrVideoPlayer.OnVideoError -= OnVideoError;
    }
}
