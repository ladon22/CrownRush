using GoogleMobileAds.Api;
using System;
using UnityEngine;

public class AdManager : MonoBehaviour
{
    public static AdManager Instance;
    private string adUnitId = "ca-app-pub-3940256099942544/5224354917";

    private RewardedAd rewardedAd;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        MobileAds.Initialize(initStatus =>
        {
            LoadRewardedAd();
        });
    }

    public void LoadRewardedAd()
    {
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }

        AdRequest adRequest = new AdRequest();
        RewardedAd.Load(adUnitId, adRequest, (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError("광고 로드 실패: " + error);
                return;
            }
            rewardedAd = ad;
            Debug.Log("광고 로드 성공");
        });
    }

    public void ShowRewardedAd(Action onRewardEarned)
    {
        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            rewardedAd.Show((Reward reward) =>
            {
                // 유저가 광고를 끝까지 다 봤을 때(보상 획득 조건 달성) 이 안쪽이 실행됩니다!
                Debug.Log("광고 시청 완료! 보상을 지급합니다.");

                onRewardEarned?.Invoke(); // GameManager의 OnReviveSuccess()를 실행시킴!

                LoadRewardedAd(); // 다음 번 죽었을 때를 위해 미리 다음 광고를 장전해 둡니다.
            });
        }
        else
        {
            Debug.LogWarning("광고가 아직 준비되지 않았거나 로드 실패했습니다.");
            // (선택) 광고가 없으면 그냥 무료로 부활시켜주거나, "광고 준비중입니다" 안내창 띄우기
            // onRewardEarned?.Invoke(); 
        }
    }
}
