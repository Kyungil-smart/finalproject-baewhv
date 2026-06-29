using System;
using UnityEngine;
using UnityEngine.UI;

public class AdsTest : MonoBehaviour
{
    [SerializeField] private Button testAdButton;

    private void Start()
    {
        if (testAdButton != null)
        {
            testAdButton.transform.SetAsLastSibling();

            testAdButton.onClick.RemoveAllListeners();
            testAdButton.onClick.AddListener(OnClickTestAd);
        }
        else
        {
            Debug.LogWarning("버튼 연결 X");
        }
    }

    private void OnClickTestAd()
    {
        Debug.Log("테스트 버튼 클릭: 구글 보상형 광고 재생 요청");

        Service.Get<AdsManager>()?.ShowRewardedAd(() =>
        {
            Debug.Log("구글 광고 완수 검증 완료, 보상 완료");
        });
    }
}
