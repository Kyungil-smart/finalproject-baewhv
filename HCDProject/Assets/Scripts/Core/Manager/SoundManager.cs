using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : BaseManager<SoundManager>
{
    [SerializeField] private AudioMixer mixer;
    
    [SerializeField] private AudioSource[] bgm = new AudioSource[2];
    [SerializeField] private AudioSource sfx;
    [SerializeField] private List<AudioClip> _bgmClip = new List<AudioClip>();
    [SerializeField] private List<AudioClip> _sfxClip = new List<AudioClip>();
    
    private Dictionary<string, AudioClip> _sound = new Dictionary<string, AudioClip>();
    
    private int currentBgmIndex = 0;
    private Coroutine bgmRoutine;

    private void Start()
    {
        Init();
        
        PlayBgmSound("HCD_Title");
    }

    public void Init()
    {
        foreach (var clip in _bgmClip)
        {
            if (clip == null) continue;
            RegisterClip(clip.name, clip);
        }

        foreach (var clip in _sfxClip)
        {
            if (clip == null) continue;
            RegisterClip(clip.name, clip);
        }
    }

    private void RegisterClip(string clipName, AudioClip clip)
    {
        if (!_sound.ContainsKey(clipName)) _sound.Add(clipName, clip);
    }

    private AudioClip GetClip(string soundName)
    {
        if (_sound.TryGetValue(soundName, out AudioClip clip)) return clip;
        
        return null;
    }

    public void PlayBgmSound(string soundName, float bgmDuration = 1f)
    {
        AudioClip clip = GetClip(soundName);
        
        if (clip == null) return;
        if (bgm[currentBgmIndex].clip == clip && bgm[currentBgmIndex].isPlaying) return;
        
        if (bgmRoutine != null) StopCoroutine(bgmRoutine);
        bgmRoutine = StartCoroutine(BgmRoutine(clip, bgmDuration));
    }

    private IEnumerator BgmRoutine(AudioClip clip, float duration)
    {
        int nextBgmIndex = (currentBgmIndex + 1) % 2;
        
        bgm[nextBgmIndex].clip = clip;
        bgm[nextBgmIndex].volume = 0;
        bgm[nextBgmIndex].Play();

        float time = 0;
        float startVolume = bgm[nextBgmIndex].volume;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            float percent = time / duration;
            
            bgm[currentBgmIndex].volume = Mathf.Lerp(startVolume, 0, percent);
            bgm[nextBgmIndex].volume = Mathf.Lerp(0, 1f, percent);
            yield return null;
        }
        
        bgm[currentBgmIndex].Stop();
        currentBgmIndex = nextBgmIndex;
    }
    
    public void PlaySfxSound(string soundName)
    {
        AudioClip clip = GetClip(soundName);
        if (clip == null) return;
        sfx.PlayOneShot(clip);
    }

    public void SetMasterVolume(float value)
    {
        float volume = value <= 0 ? -80f : Mathf.Log10(value) * 20;
        mixer.SetFloat("MasterVolume", volume);
    }

    public void SetBgmVolume(float value)
    {
        float volume = value <= 0 ? -80f : Mathf.Log10(value) * 20;
        mixer.SetFloat("BgmVolume", volume);
    }

    public void SetSfxVolume(float value)
    {
        float volume = value <= 0 ? -80f : Mathf.Log10(value) * 20;
        mixer.SetFloat("SfxVolume", volume);
    }
}