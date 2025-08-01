using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 视频管理器，负责管理和播放所有视频资源
/// </summary>
[RequireComponent(typeof(VideoPlayer))]
public class VideoManager : MonoBehaviour
{
    #region 字段和属性

    [Header("组件引用")]
    [Tooltip("视频播放器组件")]
    [SerializeField] private VideoPlayer videoPlayer;
    [Tooltip("（可选）如果要在UI上播放，请指定此RawImage")]
    [SerializeField] private RawImage targetRawImage;

    [Header("视频库")]
    [Tooltip("在此处添加所有要管理的视频")]
    [SerializeField] private List<VideoData> videoLibrary = new List<VideoData>();

    [Header("播放设置")]
    [Tooltip("是否在脚本启动时自动播放视频")]
    [SerializeField] private bool playOnAwake = false;
    [Tooltip("如果自动播放，要播放的视频名称")]
    [SerializeField] private string initialVideoName;

    // 私有字典，用于通过名称快速查找视频，提高性能
    private Dictionary<string, VideoClip> _videoDictionary;

    // 当前播放的视频名称
    public string CurrentPlayingVideoName { get; private set; }

    #endregion

    #region 事件

    public event Action<string> OnVideoStarted;
    public event Action<string> OnVideoFinished;
    public event Action<string> OnVideoPaused;
    public event Action<string> OnVideoStopped;
    public event Action<string, string> OnVideoError;

    #endregion

    #region Unity生命周期函数

    private void Awake()
    {
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }

        _videoDictionary = videoLibrary.ToDictionary(data => data.name, data => data.clip);

        SetupVideoPlayer();

        if (playOnAwake)
        {
            PlayVideo(initialVideoName);
        }
    }

    private void OnEnable()
    {
        videoPlayer.loopPointReached += OnVideoEndReached;
        videoPlayer.errorReceived += OnErrorReceived;
        videoPlayer.started += OnVideoStartedPlaying;
    }

    private void OnDisable()
    {
        videoPlayer.loopPointReached -= OnVideoEndReached;
        videoPlayer.errorReceived -= OnErrorReceived;
        videoPlayer.started -= OnVideoStartedPlaying;
    }

    #endregion

    #region 公共控制方法

    public void PlayVideo(string videoName, bool loop = false)
    {
        Debug.Log("播放视频" + videoName);
        if (_videoDictionary.TryGetValue(videoName, out VideoClip clipToPlay))
        {
            CurrentPlayingVideoName = videoName;

            videoPlayer.Stop();
            videoPlayer.clip = clipToPlay;
            videoPlayer.isLooping = loop;
            videoPlayer.Prepare();
        }
        else
        {
            Debug.LogError($"[VideoManager] 无法找到名为 '{videoName}' 的视频。");
        }
    }

    public void PauseVideo()
    {
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
            OnVideoPaused?.Invoke(CurrentPlayingVideoName);
        }
    }

    public void ResumeVideo()
    {
        if (videoPlayer.isPaused)
        {
            videoPlayer.Play();
        }
    }

    public void StopVideo()
    {
        if (videoPlayer.isPlaying || videoPlayer.isPaused)
        {
            string stoppedVideoName = CurrentPlayingVideoName;
            CurrentPlayingVideoName = null;
            videoPlayer.Stop();
            if (targetRawImage != null)
            {
                targetRawImage.texture = null;
                targetRawImage.color = Color.clear;
            }
            OnVideoStopped?.Invoke(stoppedVideoName);
        }
    }

    /// <summary>
    /// 清空播放器并准备下一次播放，适合在SetActive(false)前调用
    /// </summary>
    public void ResetPlayer()
    {
        StopVideo();
        videoPlayer.clip = null;
        if (targetRawImage != null)
        {
            targetRawImage.texture = null;
            targetRawImage.color = Color.clear;
        }
        CurrentPlayingVideoName = null;
        Debug.Log("[VideoManager] 播放器已重置");
    }

    #endregion

    #region 私有和事件处理方法

    private void SetupVideoPlayer()
    {
        videoPlayer.playOnAwake = false;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None;

        if (targetRawImage != null)
        {
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            targetRawImage.gameObject.SetActive(true);
            targetRawImage.texture = null;
            targetRawImage.color = Color.clear;
        }
        else
        {
            Debug.Log("[VideoManager] 未指定目标RawImage。视频将根据VideoPlayer的RenderMode设置进行渲染。");
        }

        videoPlayer.prepareCompleted += (source) => source.Play();
    }

    private void OnVideoStartedPlaying(VideoPlayer source)
    {
        CurrentPlayingVideoName = source.clip.name;
        if (videoPlayer.renderMode == VideoRenderMode.RenderTexture)
        {
            targetRawImage.texture = source.texture;
            targetRawImage.color = Color.white;
        }
        OnVideoStarted?.Invoke(CurrentPlayingVideoName);
        Debug.Log($"[VideoManager] 开始播放: {CurrentPlayingVideoName}");
    }

    private void OnVideoEndReached(VideoPlayer source)
    {
        if (!source.isLooping)
        {
            string finishedVideoName = CurrentPlayingVideoName;
            CurrentPlayingVideoName = null;

            if (targetRawImage != null)
            {
                targetRawImage.texture = null;
                targetRawImage.color = Color.clear;
            }

            OnVideoFinished?.Invoke(finishedVideoName);
            Debug.Log($"[VideoManager] 播放完成: {finishedVideoName}");
        }
    }

    private void OnErrorReceived(VideoPlayer source, string message)
    {
        OnVideoError?.Invoke(source.clip.name, message);
        Debug.LogError($"[VideoManager] 播放视频 '{source.clip.name}' 时出错: {message}");
    }

    #endregion
}
