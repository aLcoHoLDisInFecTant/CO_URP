using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("背景音乐源")]
    public AudioSource musicSource;

    [Header("音效源（单通道）")]
    public AudioSource sfxSource;

    [Header("音乐剪辑列表")]
    public List<AudioClip> musicClips;
    public List<AudioClip> sfxClips;

    private Dictionary<string, AudioClip> musicDict;
    private Dictionary<string, AudioClip> sfxDict;

    // 防止频繁播放音效
    private float lastSFXTime = 0f;
    private float sfxCooldown = 0.05f; // 50ms间隔限制

    // 限制协程重复播放
    private Coroutine currentSFXCoroutine;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeDictionaries();
    }

    void OnEnable()
    {
        EventManager.StartListening("PlayBGM", OnPlayBGM);
        EventManager.StartListening("PlaySFX", OnPlaySFX);
    }

    void OnDisable()
    {
        EventManager.StopListening("PlayBGM", OnPlayBGM);
        EventManager.StopListening("PlaySFX", OnPlaySFX);
        ClearAllAudio(); // 场景切换时释放资源
    }

    void InitializeDictionaries()
    {
        musicDict = new Dictionary<string, AudioClip>();
        foreach (var clip in musicClips)
            if (clip != null)
                musicDict[clip.name] = clip;

        sfxDict = new Dictionary<string, AudioClip>();
        foreach (var clip in sfxClips)
            if (clip != null)
                sfxDict[clip.name] = clip;
    }

    // 播放背景音乐
    public void PlayMusic(string name, bool loop = true)
    {
        if (!musicDict.TryGetValue(name, out var clip))
        {
            Debug.LogWarning($"[AudioManager] 未找到背景音乐：{name}");
            return;
        }

        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }

    // 播放音效（含冷却限制）
    public void PlaySFX(string name)
    {
        if (Time.unscaledTime - lastSFXTime < sfxCooldown)
            return;

        lastSFXTime = Time.unscaledTime;

        if (!sfxDict.TryGetValue(name, out var clip))
        {
            Debug.LogWarning($"[AudioManager] 未找到音效：{name}");
            return;
        }

        sfxSource.PlayOneShot(clip);
    }

    // 播放限时音效（会中断上一段）
    public void PlaySFXForDuration(string name, float duration)
    {
        if (!sfxDict.TryGetValue(name, out var clip))
        {
            Debug.LogWarning($"[AudioManager] 未找到音效：{name}");
            return;
        }

        if (currentSFXCoroutine != null)
            StopCoroutine(currentSFXCoroutine);

        currentSFXCoroutine = StartCoroutine(PlaySFXPartial(clip, duration));
    }

    private IEnumerator PlaySFXPartial(AudioClip clip, float duration)
    {
        sfxSource.clip = clip;
        sfxSource.Play();
        yield return new WaitForSeconds(duration);
        sfxSource.Stop();
        sfxSource.clip = null;
    }

    // 停止背景音乐
    public void StopMusic()
    {
        musicSource.Stop();
        musicSource.clip = null;
    }

    // 设置音量
    public void SetMusicVolume(float volume)
    {
        musicSource.volume = Mathf.Clamp01(volume);
    }

    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = Mathf.Clamp01(volume);
    }

    // 清理所有音频资源（推荐在SetActive(false)或场景切换时使用）
    public void ClearAllAudio()
    {
        StopMusic();
        if (sfxSource != null)
        {
            sfxSource.Stop();
            sfxSource.clip = null;
        }

        if (currentSFXCoroutine != null)
        {
            StopCoroutine(currentSFXCoroutine);
            currentSFXCoroutine = null;
        }
    }

    // 事件回调：播放BGM
    private void OnPlayBGM(object data)
    {
        if (data == null) return;
        string bgmName = data.ToString().Trim();
        Debug.Log($"[AudioManager] 收到事件 PlayBGM：{bgmName}");
        PlayMusic(bgmName);
    }

    // 事件回调：播放SFX
    private void OnPlaySFX(object data)
    {
        if (data == null) return;
        string sfxName = data.ToString().Trim();
        Debug.Log($"[AudioManager] 收到事件 PlaySFX：{sfxName}");
        PlaySFX(sfxName);
    }
}
