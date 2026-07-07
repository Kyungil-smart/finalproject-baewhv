using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class VideoManager : BaseManager<VideoManager>
{
    [SerializeField] private VideoPlayer videoPlayer;
    public bool IsPlaying { get; private set; } = false;

    protected override void Awake()
    {
        base.Awake();

        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }

        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.waitForFirstFrame = true;
        }
    }

    public void PlayVideo(VideoClip clip, Action finishedVideo)
    {
        if (clip == null)
        {
            Debug.Log("비디오 없음");
            finishedVideo?.Invoke();
            return;
        }

        if (IsPlaying)
        {
            Debug.Log("비디오 재생");
            return;
        }

        StartCoroutine(VideoRoutine(clip, finishedVideo));
    }

    private IEnumerator VideoRoutine(VideoClip clip, Action finishedVideo)
    {
        IsPlaying = true;

        videoPlayer.clip = clip;
        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }

        Service.Get<TimeManager>()?.SaveTimeScale();

        videoPlayer.Play();
        Debug.Log("궁극기 시작");

        while (videoPlayer.isPlaying)
        {
            yield return null;
        }

        videoPlayer.Stop();
        IsPlaying = false;
        Debug.Log("재생 끝");

        Service.Get<TimeManager>()?.LoadTimeScale();

        finishedVideo?.Invoke();
    }
}