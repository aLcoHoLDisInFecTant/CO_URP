using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// ��Ƶ���������������Ͳ���������Ƶ��Դ
/// </summary>
[RequireComponent(typeof(VideoPlayer))]
public class VideoManager : MonoBehaviour
{
    #region �ֶκ�����

    [Header("�������")]
    [Tooltip("��Ƶ���������")]
    [SerializeField] private VideoPlayer videoPlayer;
    [Tooltip("����ѡ�����Ҫ��UI�ϲ��ţ���ָ����RawImage")]
    [SerializeField] private RawImage targetRawImage;

    [Header("��Ƶ��")]
    [Tooltip("�ڴ˴��������Ҫ�������Ƶ")]
    [SerializeField] private List<VideoData> videoLibrary = new List<VideoData>();

    [Header("��������")]
    [Tooltip("�Ƿ��ڽű�����ʱ�Զ�������Ƶ")]
    [SerializeField] private bool playOnAwake = false;
    [Tooltip("����Զ����ţ�Ҫ���ŵ���Ƶ����")]
    [SerializeField] private string initialVideoName;

    // ˽���ֵ䣬����ͨ�����ƿ��ٲ�����Ƶ���������
    private Dictionary<string, VideoClip> _videoDictionary;

    // ��ǰ���ŵ���Ƶ����
    public string CurrentPlayingVideoName { get; private set; }

    #endregion

    #region �¼�

    public event Action<string> OnVideoStarted;
    public event Action<string> OnVideoFinished;
    public event Action<string> OnVideoPaused;
    public event Action<string> OnVideoStopped;
    public event Action<string, string> OnVideoError;

    #endregion

    #region Unity�������ں���

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

    #region �������Ʒ���

    public void PlayVideo(string videoName, bool loop = false)
    {
        Debug.Log("������Ƶ" + videoName);
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
            Debug.LogError($"[VideoManager] �޷��ҵ���Ϊ '{videoName}' ����Ƶ��");
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
    /// ��ղ�������׼����һ�β��ţ��ʺ���SetActive(false)ǰ����
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
        Debug.Log("[VideoManager] ������������");
    }

    #endregion

    #region ˽�к��¼�������

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
            Debug.Log("[VideoManager] δָ��Ŀ��RawImage����Ƶ������VideoPlayer��RenderMode���ý�����Ⱦ��");
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
        Debug.Log($"[VideoManager] ��ʼ����: {CurrentPlayingVideoName}");
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
            Debug.Log($"[VideoManager] �������: {finishedVideoName}");
        }
    }

    private void OnErrorReceived(VideoPlayer source, string message)
    {
        OnVideoError?.Invoke(source.clip.name, message);
        Debug.LogError($"[VideoManager] ������Ƶ '{source.clip.name}' ʱ����: {message}");
    }

    #endregion
}
