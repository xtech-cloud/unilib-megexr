// Copyright 2018 Skyworth VR. All rights reserved.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;

public enum VideoPlayerState { Idle, Preparing, Buffering, Ready, Play, Pause, Ended }
public enum RenderCommand { InitializePlayer, UpdateVideo, FreeTexture }
public enum VideoEvent
{
    VIDEO_READY = 0, VIDEO_ENDED,
    VIDEO_BUFFER_PROGRESS = 2, VIDEO_BUFFER_START, VIDEO_BUFFER_FINISH,
    VIDEO_PLAYING_PROGRESS = 5, VIDEO_TEXTURE_CREATED,
    VIDEO_UPDATE_SUBTITLE = 11
}
public enum ExceptionEvent
{
    PATH_ERROR, NOT_SUPPORT_FORMAT, NOT_SUPPORT_SIZE,
    OTHER, EXCEPTION_NOT_BEST_SIZE, EXCEPTION_NETWORK_ERROR = 5, EXCEPTION_NOT_GOOD_SIZE
}

[Serializable]
public class JVideoDescriptionInfo
{
    public int id;
    public string name;
    public string path; //url
    public string uri;
    public int size;
    public string createTime;//2018-01-17 HH:mm:ss
    public long time;
    public int width;
    public int height;
    public bool live;

    public JVideoDescriptionInfo(int id, string videoName, string path, string uri, long time, int size, int width, int height, DateTime createTime, bool live)
    {
        this.id = id;
        this.name = videoName;
        this.path = path;
        this.uri = uri;
        this.size = size;
        this.createTime = createTime.ToString("yyyy-MM-dd HH:mm:ss");

        this.time = time;
        this.width = width;
        this.height = height;
        this.live = live;
    }
}

public class SvrVideoPlayer : MonoBehaviour
{
    #region dll
#if UNITY_ANDROID && !UNITY_EDITOR
    private const string dllName = "svr_plugin_player";
    [DllImport(dllName)]
    private static extern void InitEnvironment(AndroidJavaObject application);
    [DllImport(dllName)]
    private static extern IntPtr GetRenderEventFunc();
    [DllImport(dllName)]
    private static extern int GetExternalSurfaceTextureId(IntPtr videoPlayerPtr);
    [DllImport(dllName)]
    private static extern int GetVideoMatrix(IntPtr videoPlayerPtr, float[] videoMatrix, int size);
    [DllImport(dllName)]
    private static extern IntPtr CreateVideoPlayer();
    [DllImport(dllName)]
    private static extern void DestroyVideoPlayer(IntPtr videoPlayerPtr);
    [DllImport(dllName)]
    private static extern int GetVideoPlayerEventBase(IntPtr videoPlayerPtr);
    [DllImport(dllName)]
    private static extern IntPtr SetDataSource(IntPtr videoPlayerPtr, string svrVideoInfo); // Used before play video every times.
    [DllImport(dllName)]
    private static extern int GetSupportResolutions(IntPtr videoPlayerPtr, int[] resolutions, int size);
    [DllImport(dllName)]
    private static extern void SetInitialResolution(IntPtr videoPlayerPtr, int initialResolution);
    [DllImport(dllName)]
    private static extern int GetPlayerState(IntPtr videoPlayerPtr);
    [DllImport(dllName)]
    private static extern int GetWidth(IntPtr videoPlayerPtr);
    [DllImport(dllName)]
    private static extern int GetHeight(IntPtr videoPlayerPtr);
    [DllImport(dllName)]
    private static extern bool PlayVideo(IntPtr videoPlayerPtr);
    [DllImport(dllName)]
    private static extern bool PauseVideo(IntPtr videoPlayerPtr);
    [DllImport(dllName)]
    private static extern bool StopVideo(IntPtr videoPlayerPtr);
    [DllImport(dllName)]
    private static extern void ResetPlayer(IntPtr videoPlayerPtr);
    [DllImport(dllName)]
    private static extern long GetDuration(IntPtr videoPlayerPtr);
    [DllImport(dllName)]
    private static extern long GetCurrentPosition(IntPtr videoPlayerPtr);
    [DllImport(dllName)]
    private static extern void SetCurrentPosition(IntPtr videoPlayerPtr, long pos);
    [DllImport(dllName)]
    private static extern int GetMaxVolume(IntPtr videoPlayerPtr);
    [DllImport(dllName)]
    private static extern int GetCurrentVolume(IntPtr videoPlayerPtr);
    [DllImport(dllName)]
    private static extern void SetCurrentVolume(IntPtr videoPlayerPtr, int value);
    [DllImport(dllName)]
    private static extern void SetOnVideoEventCallback(IntPtr videoPlayerPtr, Action<IntPtr, int, int, string> this_EventId_callback, IntPtr callback_arg);
    [DllImport(dllName)]
    private static extern void SetOnExceptionEventCallback(IntPtr videoPlayerPtr, Action<IntPtr, int, int, string> this_EventId_Message_callback, IntPtr callback_arg);
    [DllImport(dllName)]
    private static extern void SetLoop(IntPtr videoPlayerPtr, bool isLoop);
    [DllImport(dllName)]
    private static extern void ReleaseEnvironment();
    [DllImport(dllName)]
    /// Note: This is now obsolete. Use SetOnVolumeChangedEvent2(IntPtr, Action, IntPtr) instead.
    private static extern void SetOnVolumeChangedEvent(Action this_EventId_VolumeChanged_callback);
    [DllImport(dllName)]
    private static extern void SetOnVolumeChangedEvent2(IntPtr videoPlayerPtr, Action<IntPtr> volume_changed_callback, IntPtr callback_arg);
    [DllImport(dllName)]
    private static extern void SetSubtitleSource(IntPtr videoPlayerPtr, string path);
#endif
    #endregion

    private Renderer VideoScreen;
#if UNITY_ANDROID && !UNITY_EDITOR
    /// <summary>
    /// Main video player.
    /// </summary>
    private IntPtr VideoPlayerPtr;
    /// <summary>
    /// The rendering event function at the Native.
    /// </summary>
    private IntPtr RenderEventFunction;
    private int VideoPlayerEventBase;
    private int SurfaceTextureId;
    private float[] VideoMatrixRaw;
    private Matrix4x4 VideoMatrix;
    private int VideoMatrixPropertyId;
#endif
    private int MaxVolume;
    private int VideoWidth;
    private int VideoHeight;
    /// <summary>
    /// If the video is ready true, else false.
    /// </summary>
    private bool IsVideoReady;
    /// <summary>
    /// Whatever the video is ready or not, 
    /// the video button has been clicked and the video is ready to play automatically. 
    /// </summary>
    private bool ReadyPlay;
    private static Queue<Action> ExecuteQueue = new Queue<Action>();

    private Action<IntPtr> VolumeChangePtr;
    private Action<IntPtr, int, int, string> VideoEventPtr;
    private Action<IntPtr, int, int, string> ExceptionEventPtr;

    public delegate void VideoEnd();
    public delegate void VideoReady();
    public delegate void VideoVolumeChange(float volumePercent);
    public delegate void VideoBufferProgressChange(float bufferPercent);
    public delegate void VideoBufferStart();
    public delegate void VideoBufferFinish();
    public delegate void VideoProgressChange(int time);
    public delegate void VideoError(ExceptionEvent errorCode, string errMessage);
    public delegate void SubtitleChange(string value);

    public VideoReady OnReady;
    public VideoEnd OnEnd;
    public VideoVolumeChange OnVolumeChange;
    public VideoBufferProgressChange OnBufferProgressChange;
    public VideoBufferStart OnBufferStart;
    public VideoBufferFinish OnBufferFinish;
    public VideoProgressChange OnProgressChange;
    public VideoError OnVideoError;
    public SubtitleChange OnSubtitleValueChange;

    private void Awake()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        SvrGlobalVariable.InitCinemaNativeInterface();

        Init();
#endif
    }

    private void Update()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        UpdateVideoRenderTexture();
#endif

        while (ExecuteQueue.Count > 0)
        {
            ExecuteQueue.Dequeue().Invoke();
        }
    }

    private void Init()
    {
        VideoScreen = this.gameObject.GetComponent<Renderer>();
#if UNITY_ANDROID && !UNITY_EDITOR
        VideoPlayerPtr = IntPtr.Zero;
        VideoEventPtr = new Action<IntPtr, int, int, string>(OnVideoEvent);
        VolumeChangePtr = new Action<IntPtr>(OnVolumeChangedEvent);
        ExceptionEventPtr = new Action<IntPtr, int, int, string>(OnExceptionEvent);
#endif

        MaxVolume = 0;
        VideoWidth = 0;
        VideoHeight = 0;

        IsVideoReady = false;
        ReadyPlay = false;

        // Called whenever a video needs to be played.
        //CreatVideoPlayer();
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    /// <summary>
    /// Player events.
    /// </summary>
    /// <param name="play">get the player form GetVideoPlayerEventBase fuction.</param>
    /// <param name="renderCmd"></param>
    private void IssuePlayerEvent(int play, RenderCommand renderCmd)
    {
        if (RenderEventFunction != IntPtr.Zero)
            GL.IssuePluginEvent(RenderEventFunction, play + (int)renderCmd);
    }

    /// <summary>
    /// Update the video render texture.
    /// </summary>
    private void UpdateVideoRenderTexture()
    {
        if (VideoPlayerPtr != IntPtr.Zero && IsVideoReady)
        {
            // Update the video frame.
            IssuePlayerEvent(VideoPlayerEventBase, RenderCommand.UpdateVideo);
            GetVideoMatrix(VideoPlayerPtr, VideoMatrixRaw, 16);
            // Rotation the matrix .
            VideoMatrix = GvrMathHelpers.ConvertFloatArrayToMatrix(VideoMatrixRaw);
            // Set the matrix data for palyer.
            VideoScreen.sharedMaterial.SetMatrix(VideoMatrixPropertyId, VideoMatrix);
        }
    }
#endif

    /// <summary>
    /// Set the 2D stereo format for the shader.
    /// </summary>
    public void SetPlayMode2D()
    {
        VideoScreen.sharedMaterial.DisableKeyword("_STEREOMODE_LEFTRIGHT");
        VideoScreen.sharedMaterial.DisableKeyword("_STEREOMODE_TOPBOTTOM");
    }

    /// <summary>
    /// Set the Side-by-Side stereo format for the shader.
    /// </summary>
    public void SetPlayMode3DLeftRight()
    {
        VideoScreen.sharedMaterial.DisableKeyword("_STEREOMODE_TOPBOTTOM");
        VideoScreen.sharedMaterial.EnableKeyword("_STEREOMODE_LEFTRIGHT");
    }

    /// <summary>
    /// Set the Over-Under stereo format for the shader.
    /// </summary>
    public void SetPlayMode3DTopBottom()
    {
        VideoScreen.sharedMaterial.DisableKeyword("_STEREOMODE_LEFTRIGHT");
        VideoScreen.sharedMaterial.EnableKeyword("_STEREOMODE_TOPBOTTOM");
    }

    /// <summary>
    /// Before playing video you need create video player first.Must be invoked before PreparedPlayVideo.
    /// If do not need player should release.
    /// You can create many player if need be.
    /// </summary>
    public void CreatVideoPlayer()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        VideoPlayerPtr = CreateVideoPlayer();

        // Bind callback functions.
        //SetOnVolumeChangedEvent(VolumeChangePtr);
        SetOnVolumeChangedEvent2(VideoPlayerPtr, VolumeChangePtr, ToIntPtr(this));
        SetOnVideoEventCallback(VideoPlayerPtr, VideoEventPtr, ToIntPtr(this));
        SetOnExceptionEventCallback(VideoPlayerPtr, ExceptionEventPtr, ToIntPtr(this));

        // Get OPENGL thread.
        RenderEventFunction = GetRenderEventFunc();
        VideoPlayerEventBase = GetVideoPlayerEventBase(VideoPlayerPtr);

        VideoMatrixRaw = new float[16];
        VideoMatrix = Matrix4x4.identity;
        VideoMatrixPropertyId = Shader.PropertyToID("video_matrix");
#endif
    }

    /// <summary>
    /// Need to release resources after create player.
    /// </summary>
    public void Release()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (VideoPlayerPtr == IntPtr.Zero)
            return;

        IsVideoReady = false;
        ReadyPlay = false;
        DestroyVideoPlayer(VideoPlayerPtr);
        VideoPlayerPtr = IntPtr.Zero;
#endif
    }

    /// <summary>
    /// Set video data source and initialize player.
    /// You must invoked CreatVideoPlayer method before calling it.
    /// </summary>
    /// <param name="url"></param>
    public void PreparedPlayVideo(string url)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (VideoPlayerPtr == IntPtr.Zero)
            return;

        // Initialize the struct object for player.
        string name = url.Substring(url.LastIndexOf('/') + 1);
        JVideoDescriptionInfo videoInfo = new JVideoDescriptionInfo(-1, name, url, url, 0, 0, 0, 0, DateTime.Now, true);

        string json = JsonUtility.ToJson(videoInfo);
        IsVideoReady = false;

        // Set data source to initialize player.
        VideoPlayerPtr = SetDataSource(VideoPlayerPtr, json);
        IssuePlayerEvent(VideoPlayerEventBase, RenderCommand.InitializePlayer);

        // You can set the outside subtitle for player.
        //SetSubtitleSource(VideoPlayerPtr, string.Format("0.srt"));
#endif
    }

    private void CreateTextureComplete(int textureId)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        // Get render texture id.
        SurfaceTextureId = textureId;
#endif
    }

    /// <summary>
    /// When the video is ready for play, you can set the video total time and playing.
    /// </summary>
    private void VideoReadyComplete()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (VideoPlayerPtr == IntPtr.Zero)
            return;

        if (OnReady != null)
            OnReady();

        IsVideoReady = true;
        MaxVolume = GetMaxVolume();
        VideoWidth = GetVideoWidth();
        VideoHeight = GetVideoHeight();

        // Bind the render texture to the player.
        Texture2D texture = Texture2D.CreateExternalTexture(VideoWidth, VideoHeight, TextureFormat.ARGB32, false, false, new System.IntPtr(SurfaceTextureId));
        VideoScreen.sharedMaterial.mainTexture = texture;

        // Reset volume UI.
        VolumeChangedComplete();

        Play();
#endif
    }

    /// <summary>
    /// When the video play finish you can do something.
    /// </summary>
    private void VideoPlayComplete()
    {
        // Stop playing firstly.
        Stop();
        if (OnEnd != null)
            OnEnd();
    }

    /// <summary>
    /// When the volume changed by device.
    /// </summary>
    private void VolumeChangedComplete()
    {
        float currentVolumePer = GetCurrentVolumePercent();
        if (OnVolumeChange != null)
            OnVolumeChange(currentVolumePer);
    }

    /// <summary>
    /// Cache progress.
    /// </summary>
    /// <param name="bufferLenth">Buffer time</param>
    private void UpdateBufferTime(float bufferLenth)
    {
        if (OnBufferProgressChange != null)
            OnBufferProgressChange(bufferLenth);
    }

    /// <summary>
    /// Start caching video.
    /// </summary>
    private void BufferProgressStart()
    {
        if (OnBufferStart != null)
            OnBufferStart();
    }

    private void BufferProgressFinish()
    {
        if (OnBufferFinish != null)
            OnBufferFinish();
    }

    /// <summary>
    /// The player update the video playing progress every 500 milliseconds.
    /// </summary>
    /// <param name="time">milliseconds</param>
    private void UpdatePlayingProgress(int time)
    {
        if (OnProgressChange != null)
            OnProgressChange(time);
    }

    /// <summary>
    /// If you used SetSubtitleSource(IntPtr videoPlayerPtr, string path) function.
    /// You should use this callback to set your outside subtitle values.
    /// </summary>
    /// <param name="value"></param>
    private void UpdateSubtitleValues(string value)
    {
        if (OnSubtitleValueChange != null)
            OnSubtitleValueChange(value);
    }

    private void VideoExceptionEvent(int eventId, string errMessage)
    {
        if (OnVideoError != null)
            OnVideoError((ExceptionEvent)eventId, errMessage);
    }

    /// <summary>
    /// Play the video if ready.
    /// </summary>
    public void Play()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (VideoPlayerPtr == IntPtr.Zero)
            return;

        ReadyPlay = true;
        if (IsVideoReady)
            PlayVideo(VideoPlayerPtr);
#endif
    }

    public void Pause()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (VideoPlayerPtr == IntPtr.Zero)
            return;

        PauseVideo(VideoPlayerPtr);
        ReadyPlay = false;
#endif
    }

    public void Stop()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (VideoPlayerPtr == IntPtr.Zero)
            return;

        ReadyPlay = false;
        IsVideoReady = false;

        StopVideo(VideoPlayerPtr);
        ResetPlayer(VideoPlayerPtr);
        // Need to release the texture resource each time stop playing.
        IssuePlayerEvent(SurfaceTextureId * 100, RenderCommand.FreeTexture);
#endif
    }

    public void SeekToTime(long ms)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (VideoPlayerPtr == IntPtr.Zero)
            return;

        SetCurrentPosition(VideoPlayerPtr, ms);
#endif
    }

    /// <summary>
    /// Set if single loop, defualt false. When you reset the player, loop is default.
    /// </summary>
    /// <param name="isLoop"></param>
    public void SetLoop(bool isLoop)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (VideoPlayerPtr == IntPtr.Zero)
            return;

        SetLoop(VideoPlayerPtr, isLoop);
#endif
    }

    public void SetCurrentVolumePercent(float percent)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (VideoPlayerPtr == IntPtr.Zero)
            return;

        if (percent < 0)
            percent = 0;
        else if (percent > 1)
            percent = 1;

        int value = Mathf.FloorToInt(MaxVolume * percent);

        SetCurrentVolume(VideoPlayerPtr, value);
#endif
    }

    public int GetMaxVolume()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (VideoPlayerPtr == IntPtr.Zero)
            return 0;

        return GetMaxVolume(VideoPlayerPtr);
#else
        return 0;
#endif
    }

    public int GetCurrentVolume()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (VideoPlayerPtr == IntPtr.Zero)
            return 0;

        return GetCurrentVolume(VideoPlayerPtr);
#else
        return 0;
#endif
    }

    /// <summary>
    /// Convert the volume value to a percentage value.
    /// </summary>
    /// <returns>percentage volume value</returns>
    public float GetCurrentVolumePercent()
    {
        try
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (VideoPlayerPtr == IntPtr.Zero)
                return 0;

            int CurrentVolume = GetCurrentVolume();

            float CurrentVolumePercent = (float)CurrentVolume / GetMaxVolume();

            return CurrentVolumePercent;
#else
            return 0;
#endif
        }
        catch (Exception e)
        {
            Debug.Log("GetCurrentVolumePercent have exception" + e.Message);
            return 0.5f;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>milliseconds</returns>
    public long GetVideoDuration()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (VideoPlayerPtr == IntPtr.Zero)
            return 0;

        return GetDuration(VideoPlayerPtr);
#else
        return 0;
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>milliseconds</returns>
    public long GetCurrentPosition()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (VideoPlayerPtr == IntPtr.Zero)
            return 0;

        return GetCurrentPosition(VideoPlayerPtr);
#else
        return 0;
#endif
    }

    public int GetVideoWidth()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (VideoPlayerPtr == IntPtr.Zero)
            return 0;

        return GetWidth(VideoPlayerPtr);
#else
        return 0;
#endif
    }

    public int GetVideoHeight()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (VideoPlayerPtr == IntPtr.Zero)
            return 0;

        return GetHeight(VideoPlayerPtr);
#else
        return 0;
#endif
    }

    /// <summary>
    /// Get current video player states.
    /// </summary>
    /// <returns>player states</returns>
    public VideoPlayerState GetPlayerState()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (VideoPlayerPtr == IntPtr.Zero)
            return VideoPlayerState.Idle;

        return (VideoPlayerState)GetPlayerState(VideoPlayerPtr);
#else
        return VideoPlayerState.Idle;
#endif
    }

    /// <summary>
    /// Check for data leaks.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    private static IntPtr ToIntPtr(System.Object obj)
    {
        GCHandle handle = GCHandle.Alloc(obj);
        return GCHandle.ToIntPtr(handle);
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private void OnVideoEvent(IntPtr cbdata, int eventId, int callbackIntegerData, string message)
    {
        if (cbdata == IntPtr.Zero)
            return;

        try
        {
            if (eventId == (int)VideoEvent.VIDEO_READY)
            {
                ExecuteQueue.Enqueue(() => VideoReadyComplete());
            }
            else if (eventId == (int)VideoEvent.VIDEO_ENDED)
            {
                ExecuteQueue.Enqueue(() => VideoPlayComplete());
            }
            else if (eventId == (int)VideoEvent.VIDEO_BUFFER_PROGRESS)
            {
                ExecuteQueue.Enqueue(() => UpdateBufferTime(callbackIntegerData));
            }
            else if (eventId == (int)VideoEvent.VIDEO_BUFFER_START)
            {
                ExecuteQueue.Enqueue(() => BufferProgressStart());
            }
            else if (eventId == (int)VideoEvent.VIDEO_BUFFER_FINISH)
            {
                ExecuteQueue.Enqueue(() => BufferProgressFinish());
            }
            else if (eventId == (int)VideoEvent.VIDEO_PLAYING_PROGRESS)
            {
                ExecuteQueue.Enqueue(() => UpdatePlayingProgress(callbackIntegerData));
            }
            else if (eventId == (int)VideoEvent.VIDEO_TEXTURE_CREATED)
            {
                ExecuteQueue.Enqueue(() => CreateTextureComplete(callbackIntegerData));
            }
            else if (eventId == (int)VideoEvent.VIDEO_UPDATE_SUBTITLE)
            {
                ExecuteQueue.Enqueue(() => UpdateSubtitleValues(message));
            }
        }
        catch (InvalidCastException e)
        {
            Debug.LogError("GC Handle pointed to unexpected type: videoPlayer.Expected " + typeof(SvrVideoPlayer));
            throw e;
        }
    }

    private void OnExceptionEvent(IntPtr cbdata, int eventId, int percent, string message)
    {
        try
        {
            ExecuteQueue.Enqueue(() => VideoExceptionEvent(eventId, message));
        }
        catch (InvalidCastException e)
        {
            Debug.LogError("GC Handle pointed to unexpected type: videoPlayer.Expected " + typeof(SvrVideoPlayer));
            throw e;
        }
    }

    private void OnVolumeChangedEvent(IntPtr cbdata)
    {
        ExecuteQueue.Enqueue(() => VolumeChangedComplete());
    }
#endif

    private void OnApplicationPause(bool pause)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (VideoPlayerPtr == IntPtr.Zero)
            return;

        if (pause && ReadyPlay)
        {
            Pause();

            ReadyPlay = true;
        }
        else if (ReadyPlay)
        {
            Play();
        }
#endif
    }

    private void OnApplicationQuit()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (VideoPlayerPtr != IntPtr.Zero)
            Release();

        SvrGlobalVariable.Release();
#endif
    }
}
