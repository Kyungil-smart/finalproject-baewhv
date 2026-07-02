using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SoundManager : BaseManager<SoundManager>
{
    [SerializeField] private AudioMixer mixer;
    
    [SerializeField] private AudioSource[] bgm = new AudioSource[2];
    [SerializeField] private AudioSource sfx;

    private AsyncOperationHandle<AudioClip> _currentBgm;
    
    private int currentBgmIndex = 0;
    private Coroutine bgmRoutine;

    private void Start()
    {
        PlayBgmSound("Title");
    }

    public void PlayBgmSound(string soundName, float bgmDuration = 1f)
    {
        Addressables.LoadAssetAsync<AudioClip>(soundName).Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                AudioClip clip = handle.Result;

                if (bgm[currentBgmIndex].clip == clip && bgm[currentBgmIndex].isPlaying) return;

                if (bgmRoutine != null) StopCoroutine(bgmRoutine);
                bgmRoutine = StartCoroutine(BgmRoutine(clip, handle, bgmDuration));
            }
        };
    }

    private IEnumerator BgmRoutine(AudioClip clip, AsyncOperationHandle<AudioClip> handle , float duration)
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
        bgm[currentBgmIndex].clip = null;
        
        if (_currentBgm.IsValid()) Addressables.Release(_currentBgm);
        currentBgmIndex = nextBgmIndex;
        _currentBgm = handle;
    }
    
    public void PlaySfxSound(string soundName)
    {
        Addressables.LoadAssetAsync<AudioClip>(soundName).Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                sfx.PlayOneShot(handle.Result);

                StartCoroutine(ReleaseSfxRoutine(handle, handle.Result.length));
            }
        };
    }

    private IEnumerator ReleaseSfxRoutine(AsyncOperationHandle<AudioClip> handle, float length)
    {
        yield return new WaitForSecondsRealtime(length);
        
        if (handle.IsValid()) Addressables.Release(handle);
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