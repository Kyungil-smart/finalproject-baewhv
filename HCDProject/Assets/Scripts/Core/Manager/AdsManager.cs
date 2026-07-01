using System;
using UnityEngine;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;

public class AdsManager : BaseManager<AdsManager>
{
    private string adId = "ca-app-pub-3940256099942544/5224354917";

    private RewardedAd _rewardedAd;

    private bool isKidApp = false;

    private bool isAdUsed = false;

    private bool isShowing = false;

    public bool IsAdUsed => isAdUsed;
    public event Action<bool> OnAdReroll;

    public bool CanReroll => !isAdUsed && _rewardedAd != null && _rewardedAd.CanShowAd();

    private void SetAdUsed(bool value)
    {
        if (isAdUsed != value)
        {
            isAdUsed = value;
            OnAdReroll?.Invoke(isAdUsed);
        }
    }

    private void Start()
    {
        RequestConfiguration requestConfiguration = new RequestConfiguration
        {
            TagForChildDirectedTreatment = isKidApp ? TagForChildDirectedTreatment.True : TagForChildDirectedTreatment.False,
            TagForUnderAgeOfConsent = isKidApp ? TagForUnderAgeOfConsent.True : TagForUnderAgeOfConsent.False,
            MaxAdContentRating = isKidApp ? MaxAdContentRating.G : MaxAdContentRating.T
        };

        MobileAds.SetRequestConfiguration(requestConfiguration);

        MobileAds.Initialize((InitializationStatus status) =>
        {
            if (status == null)
            {
                Debug.LogError("SDK 초기화 실패");
                return;
            }
            Debug.Log("SDK 초기화 완료");

            MobileAdsEventExecutor.ExecuteInUpdate(LoadAds);
        });
    }

    private new void OnDestroy()
    {
        if (_rewardedAd != null)
        {
            _rewardedAd.Destroy();
            _rewardedAd = null;
        }
    }

    public void ResetAdChance()
    {
        SetAdUsed(false);
        Debug.Log("새로운 보상 노드 진입: 광고 리롤 기회 초기화");
    }

    public void LoadAds()
    {
        if (_rewardedAd != null)
        {
            _rewardedAd.Destroy();
            _rewardedAd = null;
        }

        AdRequest request = new AdRequest();

        RewardedAd.Load(adId, request, (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.Log($"광고 로드 실패 : {error}");
                return;
            }

            _rewardedAd = ad;
            Debug.Log("광고 로드 성공");
        });
    }

    public void ShowRewardedAd(Action rewardedAdOpen)
    {
        if (isAdUsed || isShowing)
        {
            Debug.Log("이미 광고 시청 완료");
            return;
        }

        if (_rewardedAd != null && _rewardedAd.CanShowAd())
        {
            isShowing = true;
            _rewardedAd.Show((Reward reward) =>
            {
                MobileAdsEventExecutor.ExecuteInUpdate(() =>
                {
                    isShowing = false;
                    Debug.Log("보상 획득 성공");

                    SetAdUsed(true);

                    rewardedAdOpen?.Invoke();

                    LoadAds();
                });
            });
        }
        else
        {
            Debug.Log("광고 없음");
            LoadAds();
        }
    }
}
