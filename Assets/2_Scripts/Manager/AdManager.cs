using GoogleMobileAds.Api;
using System;
using UnityEngine;

public class AdManager : MonoBehaviour
{
    public static AdManager Instance;
    private string AdUnitId = "ca-app-pub-3940256099942544/5224354917";

    private RewardedAd RewardedAd;
    private Action OnRewardEarned;
    private bool bRewardEarned = false;
    public bool bIsAdReady => RewardedAd != null && RewardedAd.CanShowAd();

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
        if (RewardedAd != null)
        {
            RewardedAd.Destroy();
            RewardedAd = null;
        }

        AdRequest adRequest = new AdRequest();
        RewardedAd.Load(AdUnitId, adRequest, (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Invoke(nameof(LoadRewardedAd), 30f);
                return;
            }
            RewardedAd = ad;
            Debug.Log("광고 로드 성공");
        });
    }

    public void ShowRewardedAd(Action onRewardEarned)
    {
        if (RewardedAd == null || !RewardedAd.CanShowAd()) return;

        OnRewardEarned = onRewardEarned;
        bRewardEarned = false;

        RewardedAd.OnAdFullScreenContentClosed += OnAdClosed;
        RewardedAd.Show((Reward reward) =>
        {
            bRewardEarned = true;
            Debug.Log("[AdManager] 보상 획득 확인.");
        });
    }

    private void OnAdClosed()
    {
        RewardedAd.OnAdFullScreenContentClosed -= OnAdClosed;

        if (bRewardEarned)
        {
            OnRewardEarned?.Invoke();
            Debug.Log("[AdManager] 보상 지급 완료.");
        }

        bRewardEarned = false;
        OnRewardEarned = null;

        LoadRewardedAd();

    }
}
