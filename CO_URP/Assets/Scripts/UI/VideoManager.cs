using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// ��Ƶ���������������Ͳ���������Ƶ��Դ
/// </summary>
[RequireComponent(typeof(VideoPlayer), typeof(AudioSource))]
public class VideoManager : MonoBehaviour
{
    #region �ֶκ�����

    [Header("�������")]
    [Tooltip("��Ƶ���������")]
    [SerializeField] private VideoPlayer videoPlayer;
    [Tooltip("���ڲ�����Ƶ��������ƵԴ���")]
    [SerializeField] private AudioSource audioSource;
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

    /// <summary>
    /// ����Ƶ��ʼ����ʱ����
    /// ����: ��Ƶ����
    /// </summary>
    public event Action<string> OnVideoStarted;

    /// <summary>
    /// ����Ƶ�������ʱ����
    /// ����: ��Ƶ����
    /// </summary>
    public event Action<string> OnVideoFinished;

    /// <summary>
    /// ����Ƶ����ͣʱ����
    /// ����: ��Ƶ����
    /// </summary>
    public event Action<string> OnVideoPaused;

    /// <summary>
    /// ����Ƶ��ֹͣʱ����
    /// ����: ��Ƶ����
    /// </summary>
    public event Action<string> OnVideoStopped;

    /// <summary>
    /// ����Ƶ���ų���ʱ����
    /// ����1: ��Ƶ����, ����2: ������Ϣ
    /// </summary>
    public event Action<string, string> OnVideoError;

    #endregion

    #region Unity�������ں���

    private void Awake()
    {
        // ���û����Inspector��ָ������������Զ���ȡ
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        // ��ʼ����Ƶ�ֵ�
        _videoDictionary = videoLibrary.ToDictionary(data => data.name, data => data.clip);

        // ����VideoPlayer
        SetupVideoPlayer();

        if (playOnAwake)
        {
            PlayVideo(initialVideoName);
        }
    }

    private void OnEnable()
    {
        // ����VideoPlayer�������¼�
        videoPlayer.loopPointReached += OnVideoEndReached;
        videoPlayer.errorReceived += OnErrorReceived;
        videoPlayer.started += OnVideoStartedPlaying;
    }

    private void OnDisable()
    {
        // ȡ�������Է�ֹ�ڴ�й©
        videoPlayer.loopPointReached -= OnVideoEndReached;
        videoPlayer.errorReceived -= OnErrorReceived;
        videoPlayer.started -= OnVideoStartedPlaying;
    }

    #endregion

    #region �������Ʒ���

    /// <summary>
    /// �������Ʋ�����Ƶ
    /// </summary>
    /// <param name="videoName">Ҫ����Ƶ���в��ŵ���Ƶ����</param>
    /// <param name="loop">�Ƿ�ѭ������</param>
    public void PlayVideo(string videoName, bool loop = false)
    {
        if (_videoDictionary.TryGetValue(videoName, out VideoClip clipToPlay))
        {
            CurrentPlayingVideoName = videoName;

            videoPlayer.Stop(); // ��ֹͣ��ǰ����
            videoPlayer.clip = clipToPlay;
            videoPlayer.isLooping = loop;
            videoPlayer.Prepare(); // �Ƽ���Prepare��׼����ɺ���Զ�����
        }
        else
        {
            Debug.LogError($"[VideoManager] �޷��ҵ���Ϊ '{videoName}' ����Ƶ��");
        }
    }

    /// <summary>
    /// ��ͣ��ǰ���ŵ���Ƶ
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
    /// �ָ����ŵ�ǰ��ͣ����Ƶ
    /// </summary>
    public void ResumeVideo()
    {
        if (videoPlayer.isPaused)
        {
            videoPlayer.Play();
        }
    }

    /// <summary>
    /// ֹͣ������Ƶ
    /// </summary>
    public void StopVideo()
    {
        if (videoPlayer.isPlaying || videoPlayer.isPaused)
        {
            string stoppedVideoName = CurrentPlayingVideoName;
            CurrentPlayingVideoName = null;
            videoPlayer.Stop();
            // ֹͣʱ��UI�ϵĻ�������������ѡ�����
            if (targetRawImage != null)
            {
                targetRawImage.texture = null;
                targetRawImage.color = Color.clear; // ����Ϊ͸��
            }
            OnVideoStopped?.Invoke(stoppedVideoName);
        }
    }

    #endregion

    #region ˽�к��¼�������

    private void SetupVideoPlayer()
    {
        // �ر�VideoPlayer�Դ���PlayOnAwake���������Լ����߼�����
        videoPlayer.playOnAwake = false;

        // ������Ƶ���ģʽΪAudioSource
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.SetTargetAudioSource(0, audioSource);

        // �����Ƿ�ָ����RawImage��������Ⱦģʽ
        if (targetRawImage != null)
        {
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            // ȷ��RawImage�Ǽ���ģ�����ʼû������
            targetRawImage.gameObject.SetActive(true);
            targetRawImage.texture = null;
            targetRawImage.color = Color.clear; // ��ʼʱ͸��
        }
        else
        {
            // ���û��RawImage����������Ϊ��3D�����ϲ���(Camera Far/Near Plane �� Material Override)
            // ����Ĭ��ΪMaterial Override����Ҫ��VideoPlayer������ֶ�����Renderer
            // videoPlayer.renderMode = VideoRenderMode.MaterialOverride;
            Debug.Log("[VideoManager] δָ��Ŀ��RawImage����Ƶ������VideoPlayer��RenderMode���ý�����Ⱦ��");
        }

        // ׼����ɺ��Զ�����
        videoPlayer.prepareCompleted += (source) => source.Play();
    }

    private void OnVideoStartedPlaying(VideoPlayer source)
    {
        CurrentPlayingVideoName = source.clip.name;
        // �������Ⱦ��RenderTexture������Ӧ�õ�RawImage��
        if (videoPlayer.renderMode == VideoRenderMode.RenderTexture)
        {
            targetRawImage.texture = source.texture;
            targetRawImage.color = Color.white; // �ָ���͸��
        }
        OnVideoStarted?.Invoke(CurrentPlayingVideoName);
        Debug.Log($"[VideoManager] ��ʼ����: {CurrentPlayingVideoName}");
    }

    private void OnVideoEndReached(VideoPlayer source)
    {
        // ֻ���ڲ�ѭ��������£���Ƶ�������㡰��ɡ�
        if (!source.isLooping)
        {
            string finishedVideoName = CurrentPlayingVideoName;
            CurrentPlayingVideoName = null;

            // ������ɺ󣬿���ѡ������RawImage
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