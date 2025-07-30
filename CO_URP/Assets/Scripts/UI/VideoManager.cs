using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 视频管理器，负责管理和播放所有视频资源
/// </summary>
[RequireComponent(typeof(VideoPlayer), typeof(AudioSource))]
public class VideoManager : MonoBehaviour
{
    #region 字段和属性

    [Header("组件引用")]
    [Tooltip("视频播放器组件")]
    [SerializeField] private VideoPlayer videoPlayer;
    [Tooltip("用于播放视频声音的音频源组件")]
    [SerializeField] private AudioSource audioSource;
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

    /// <summary>
    /// 当视频开始播放时触发
    /// 参数: 视频名称
    /// </summary>
    public event Action<string> OnVideoStarted;

    /// <summary>
    /// 当视频播放完成时触发
    /// 参数: 视频名称
    /// </summary>
    public event Action<string> OnVideoFinished;

    /// <summary>
    /// 当视频被暂停时触发
    /// 参数: 视频名称
    /// </summary>
    public event Action<string> OnVideoPaused;

    /// <summary>
    /// 当视频被停止时触发
    /// 参数: 视频名称
    /// </summary>
    public event Action<string> OnVideoStopped;

    /// <summary>
    /// 当视频播放出错时触发
    /// 参数1: 视频名称, 参数2: 错误信息
    /// </summary>
    public event Action<string, string> OnVideoError;

    #endregion

    #region Unity生命周期函数

    private void Awake()
    {
        // 如果没有在Inspector中指定组件，尝试自动获取
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        // 初始化视频字典
        _videoDictionary = videoLibrary.ToDictionary(data => data.name, data => data.clip);

        // 配置VideoPlayer
        SetupVideoPlayer();

        if (playOnAwake)
        {
            PlayVideo(initialVideoName);
        }
    }

    private void OnEnable()
    {
        // 订阅VideoPlayer的内置事件
        videoPlayer.loopPointReached += OnVideoEndReached;
        videoPlayer.errorReceived += OnErrorReceived;
        videoPlayer.started += OnVideoStartedPlaying;
    }

    private void OnDisable()
    {
        // 取消订阅以防止内存泄漏
        videoPlayer.loopPointReached -= OnVideoEndReached;
        videoPlayer.errorReceived -= OnErrorReceived;
        videoPlayer.started -= OnVideoStartedPlaying;
    }

    #endregion

    #region 公共控制方法

    /// <summary>
    /// 根据名称播放视频
    /// </summary>
    /// <param name="videoName">要在视频库中播放的视频名称</param>
    /// <param name="loop">是否循环播放</param>
    public void PlayVideo(string videoName, bool loop = false)
    {
        if (_videoDictionary.TryGetValue(videoName, out VideoClip clipToPlay))
        {
            CurrentPlayingVideoName = videoName;

            videoPlayer.Stop(); // 先停止当前播放
            videoPlayer.clip = clipToPlay;
            videoPlayer.isLooping = loop;
            videoPlayer.Prepare(); // 推荐先Prepare，准备完成后会自动播放
        }
        else
        {
            Debug.LogError($"[VideoManager] 无法找到名为 '{videoName}' 的视频。");
        }
    }

    /// <summary>
    /// 暂停当前播放的视频
    /// </summary>
    public void PauseVideo()
    {
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
            OnVideoPaused?.Invoke(CurrentPlayingVideoName);
        }
    }

    /// <summary>
    /// 恢复播放当前暂停的视频
    /// </summary>
    public void ResumeVideo()
    {
        if (videoPlayer.isPaused)
        {
            videoPlayer.Play();
        }
    }

    /// <summary>
    /// 停止播放视频
    /// </summary>
    public void StopVideo()
    {
        if (videoPlayer.isPlaying || videoPlayer.isPaused)
        {
            string stoppedVideoName = CurrentPlayingVideoName;
            CurrentPlayingVideoName = null;
            videoPlayer.Stop();
            // 停止时，UI上的画面会残留，可以选择清空
            if (targetRawImage != null)
            {
                targetRawImage.texture = null;
                targetRawImage.color = Color.clear; // 设置为透明
            }
            OnVideoStopped?.Invoke(stoppedVideoName);
        }
    }

    #endregion

    #region 私有和事件处理方法

    private void SetupVideoPlayer()
    {
        // 关闭VideoPlayer自带的PlayOnAwake，由我们自己的逻辑控制
        videoPlayer.playOnAwake = false;

        // 设置音频输出模式为AudioSource
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.SetTargetAudioSource(0, audioSource);

        // 根据是否指定了RawImage来设置渲染模式
        if (targetRawImage != null)
        {
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            // 确保RawImage是激活的，但初始没有纹理
            targetRawImage.gameObject.SetActive(true);
            targetRawImage.texture = null;
            targetRawImage.color = Color.clear; // 开始时透明
        }
        else
        {
            // 如果没有RawImage，可以设置为在3D物体上播放(Camera Far/Near Plane 或 Material Override)
            // 这里默认为Material Override，需要在VideoPlayer组件上手动设置Renderer
            // videoPlayer.renderMode = VideoRenderMode.MaterialOverride;
            Debug.Log("[VideoManager] 未指定目标RawImage。视频将根据VideoPlayer的RenderMode设置进行渲染。");
        }

        // 准备完成后自动播放
        videoPlayer.prepareCompleted += (source) => source.Play();
    }

    private void OnVideoStartedPlaying(VideoPlayer source)
    {
        CurrentPlayingVideoName = source.clip.name;
        // 如果是渲染到RenderTexture，则将其应用到RawImage上
        if (videoPlayer.renderMode == VideoRenderMode.RenderTexture)
        {
            targetRawImage.texture = source.texture;
            targetRawImage.color = Color.white; // 恢复不透明
        }
        OnVideoStarted?.Invoke(CurrentPlayingVideoName);
        Debug.Log($"[VideoManager] 开始播放: {CurrentPlayingVideoName}");
    }

    private void OnVideoEndReached(VideoPlayer source)
    {
        // 只有在不循环的情况下，视频结束才算“完成”
        if (!source.isLooping)
        {
            string finishedVideoName = CurrentPlayingVideoName;
            CurrentPlayingVideoName = null;

            // 播放完成后，可以选择隐藏RawImage
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