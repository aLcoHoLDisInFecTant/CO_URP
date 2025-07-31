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

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeDictionaries();

        // ✅ 注册事件监听
        EventManager.StartListening("PlayBGM", OnPlayBGM);
        EventManager.StartListening("PlaySFX", OnPlaySFX);
    }

    void OnDestroy()
    {
        // ✅ 注销事件监听
        EventManager.StopListening("PlayBGM", OnPlayBGM);
        EventManager.StopListening("PlaySFX", OnPlaySFX);
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

    // ✅ 响应事件：播放BGM
    private void OnPlayBGM(object data)
    {
        if (data == null) return;
        string bgmName = data.ToString().Trim();
        Debug.Log($"[AudioManager] 收到事件 PlayBGM：{bgmName}");
        PlayMusic(bgmName);
    }

    // ✅ 响应事件：播放SFX
    private void OnPlaySFX(object data)
    {
        if (data == null) return;
        string sfxName = data.ToString().Trim();
        Debug.Log($"[AudioManager] 收到事件 PlaySFX：{sfxName}");
        PlaySFX(sfxName);
    }

    // ✅ 主接口：播放背景音乐
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

    // ✅ 主接口：播放音效
    public void PlaySFX(string name)
    {
        if (!sfxDict.TryGetValue(name, out var clip))
        {
            Debug.LogWarning($"[AudioManager] 未找到音效：{name}");
            return;
        }

        sfxSource.PlayOneShot(clip);
    }

    // ✅ 停止播放背景音乐
    public void StopMusic()
    {
        musicSource.Stop();
    }

    // ✅ 设置音量
    public void SetMusicVolume(float volume)
    {
        musicSource.volume = Mathf.Clamp01(volume);
    }

    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = Mathf.Clamp01(volume);
    }

    // ✅ 播放部分音效（限时）
    public void PlaySFXForDuration(string name, float duration)
    {
        if (!sfxDict.TryGetValue(name, out var clip))
        {
            Debug.LogWarning($"[AudioManager] 未找到音效：{name}");
            return;
        }

        StartCoroutine(PlaySFXPartial(clip, duration));
    }

    private IEnumerator PlaySFXPartial(AudioClip clip, float duration)
    {
        sfxSource.clip = clip;
        sfxSource.Play();
        yield return new WaitForSeconds(duration);
        sfxSource.Stop();
    }
}
