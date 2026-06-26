using System;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using UnityEngine;

public class AdsController : BaseManager<AdsController>
{
    private string adId = "ca-app-pub-3940256099942544/5224354917";
    
    // 보상형 광고
    private RewardedAd _rewardedAd;
    
    // 배너형 광고
    private BannerView _bannerView;
    
    // 전면 광고 (선택이 아닌 특정 시점에 강제 실행)
    private InterstitialAd _interstitialAd;

    // 보상형 전면 광고 (강제 실행이지만 보상이 있음)
    private RewardedInterstitialAd _rewardedInterstitialAd;
    
    // 게임 로딩 광고 
    private AppOpenAd _appOpenAd;

    private bool isKidApp;
    
    private void Start()
    {
        // 광고 설정 
        RequestConfiguration requestConfiguration = new RequestConfiguration
        {
            // 아동용 광고 설정                             어린 아동용 광고가 필요할 경우           그냥 광고면 될 경우 
            TagForChildDirectedTreatment = isKidApp ? TagForChildDirectedTreatment.True : TagForChildDirectedTreatment.False,
            
            // 동의 연령 미달 광고 설정                         청소년 미성년자인가                   아닌가 
            TagForUnderAgeOfConsent = isKidApp ? TagForUnderAgeOfConsent.True : TagForUnderAgeOfConsent.False,
            
            // 콘텐츠 등급 지정           G : 전체 이용가 , PG : 12세 이상 이용 , MA : 청불등급(성인광고) , T : 설명상 전체이용가로 보이긴 함 (Content suitable for teen and older audiences.)
            MaxAdContentRating = MaxAdContentRating.G  // , PG , MA , T
        };
        
        // 방금 설정한 리퀘스트 설정을 광고에 적용
        MobileAds.SetRequestConfiguration(requestConfiguration);
        
        // 광고를 사용하기 위한 준비 작업 (구글 서버와의 통신을 위한 시간이 조금 소요됨으로) 콜백함수로 관리 
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

    private void OnDestroy()
    {
        if (_rewardedAd != null) _rewardedAd.Destroy();
    }

    // 광고를 받기위한 매서드
    public void LoadAds()
    {
        // 기존에 다 본 광고 제거 (메모리 누수 방지)
        if (_rewardedAd != null)
        {
            _rewardedAd.Destroy();
            _rewardedAd = null;
        }

        // 구글 서버에 보낼 광고 리퀘스트 (서버에 광고 달라고 요청할 전광판)
        AdRequest request = new();
        
        // 크기 지정이 필요한 배너 광고의 경우      사이즈와          위치 설정 가능  (이외에 전면광고의 경우 스마트폰 크기에 맞춰서 전체 화면으로 나온다고 함)
        _bannerView = new BannerView(adId, AdSize.Banner, AdPosition.Bottom);
        // 배너 광고는 리퀘스트에 담아주면 로드 완료 
        _bannerView.LoadAd(request);
        
        // 싱제 광고 로드 :아이디  리퀘스트   받아올 광고       광고 못받은 이유 
        RewardedAd.Load(adId, request, (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError($"광고 로드 실패 : {error}");
                return;
            }
            
            // 광고로 인한 매출을 확인 하고 싶을 때 
            // _rewardedAd.OnAdPaid += (AdValue value) =>
            // {
            //     Debug.Log($"수익 : {value.Value}");
            // };
            //
            // // 유저의 광고 클릭 횟수를 확인 하고 싶을 때
            // int clicknum = 0;
            // _rewardedAd.OnAdClicked += () =>
            // {
            //     clicknum++;
            //     Debug.Log("유저가 광고를 클릭하고 넘어감 : 유저 -1");
            // };
            //
            // // 광고가 노출됨을 카운트 하고 싶을 때 
            // _rewardedAd.OnAdImpressionRecorded += () =>
            // {
            //     Debug.Log("광고가 노출됨");
            // };
            //
            // // 광고가 열렸을때 게임의 사운드와 같은 게임 로직을 멈추기 위해 
            // _rewardedAd.OnAdFullScreenContentOpened += () =>
            // {
            //     // Service.Get<SoundManager>?.MuteSound();
            // };
            //
            // // 유저가 광고를 다 안보고 광고 창을 닫을 경우 
            // _rewardedAd.OnAdFullScreenContentClosed += () =>
            // {
            //     MobileAdsEventExecutor.ExecuteInUpdate(LoadAds);
            // };
            //
            // // 광고 표시 도중 (인터넷 끊김) 과 같은 에러로 인해 광고가 끊겼을 경우
            // _rewardedAd.OnAdFullScreenContentFailed += (AdError adError) =>
            // {
            //     MobileAdsEventExecutor.ExecuteInUpdate(LoadAds);
            // };
            
            _rewardedAd = ad;
            Debug.Log("광고 로드 성공");
        });
    }

    // 받아온 광고를 화면에 띄우기 위한 매서드
    public void ShowRewardedAd(Action rewardedAdOpen)
    {
        // 받아온 광고가 있고           화면에 띄울 수 있는 상태라면
        if (_rewardedAd != null && _rewardedAd.CanShowAd())
        {
            // 광고 틀어!
            _rewardedAd.Show((Reward reward) =>
            {
                // 구글 ads 같은 경우 외부 백그라운드 스레드이기 떄문에 메인 스레드에 있는 유니티 로직에 함부로 손대면 게임이 멈춰버림 
                // 이를 멈추지 않도록 유니티 메인 스레드와 sdk 이벤트를 동기화 시켜주는 역할 
                MobileAdsEventExecutor.ExecuteInUpdate(() =>
                {
                    // 광고에 대한 보상을 적용
                    rewardedAdOpen?.Invoke();
                    // 다음 광고를 위해 새로운 광고 로드
                    LoadAds();
                });
            });
        }
        else
        {
            Debug.Log("광고 없다");
            // 광고 없으니 로드 !
            LoadAds();
        }
    }
}
