using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("±≥æ∞“Ù¿÷")]
    public AudioSource musicSource;

    [Header("“Ù–ß‘¥£®ø…∏¥”√£©")]
    public AudioSource sfxSource;

    [Header("“Ù∆µºÙº≠ø‚")]
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
    }

    void InitializeDictionaries()
    {
        musicDict = new Dictionary<string, AudioClip>();
        foreach (var clip in musicClips)
            musicDict[clip.name] = clip;

        sfxDict = new Dictionary<string, AudioClip>();
        foreach (var clip in sfxClips)
            sfxDict[clip.name] = clip;
    }

    public void PlayMusic(string name, bool loop = true)
    {
        if (!musicDict.ContainsKey(name))
        {
            Debug.LogWarning($"[AudioManager] Œ¥’“µΩ“Ù¿÷: {name}");
            return;
        }

        musicSource.clip = musicDict[name];
        musicSource.loop = loop;
        musicSource.Play();
    }

    public void PlaySFX(string name)
    {
        if (!sfxDict.ContainsKey(name))
        {
            Debug.LogWarning($"[AudioManager] Œ¥’“µΩ“Ù–ß: {name}");
            return;
        }

        sfxSource.PlayOneShot(sfxDict[name]);
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void SetMusicVolume(float volume)
    {
        musicSource.volume = Mathf.Clamp01(volume);
    }

    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = Mathf.Clamp01(volume);
    }

    public void PlaySFXForDuration(string name, float duration)
    {
        if (!sfxDict.ContainsKey(name))
        {
            Debug.LogWarning($"[AudioManager] Œ¥’“µΩ“Ù–ß: {name}");
            return;
        }

        StartCoroutine(PlaySFXPartial(sfxDict[name], duration));
    }

    private IEnumerator PlaySFXPartial(AudioClip clip, float duration)
    {
        sfxSource.clip = clip;
        sfxSource.Play();
        yield return new WaitForSeconds(duration);
        sfxSource.Stop();
    }

}
