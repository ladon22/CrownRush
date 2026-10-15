using GoogleMobileAds.Api;
using GoogleMobileAds.Ump.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Serialization;

public class AdManager : MonoBehaviour
{
    public static AdManager Instance;
    //테스트용 ID: ca-app-pub-3940256099942544/5224354917
    private string AdUnitId = "ca-app-pub-1954417796107340/7852122167";

    private RewardedAd RewardedAd;
    private Action OnRewardEarned;
    private bool RewardEarned = false;
    public bool IsAdReady => RewardedAd != null && RewardedAd.CanShowAd();

    public static bool IsAdTransitioning { get; private set; } = false;
    private const float AdPauseCooldown = 1.0f;
    private bool AdsInitialized = false;

    private SynchronizationContext MainThreadContext;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
    [Header("동의 테스트")]
    [Tooltip("체크 시 EEA 지역으로 강제 처리")]
    [SerializeField] private bool DebugForceEEA = false;
    [Tooltip("테스트 기기 해시 ID")]
    [SerializeField] private List<string> DebugTestDeviceHashedIds = new List<string>();
    [Tooltip("체크 시 실행마다 동의 상태 초기화")]
    [SerializeField] private bool ResetConsentOnLaunch = false;
#endif

    private void Awake()
    {
        MainThreadContext = SynchronizationContext.Current;

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(transform.root.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        if (ResetConsentOnLaunch)
        {
            ConsentInformation.Reset();
        }
#endif
        if (ConsentInformation.CanRequestAds())
        {
            InitializeAds();
        }

        RequestConsent();
    }

    private void RequestConsent()
    {
        ConsentRequestParameters request = new ConsentRequestParameters
        {
            TagForUnderAgeOfConsent = false
        };

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        if (DebugForceEEA)
        {
            request.ConsentDebugSettings = new ConsentDebugSettings
            {
                DebugGeography = DebugGeography.EEA,
                TestDeviceHashedIds = DebugTestDeviceHashedIds
            };
        }
#endif

        ConsentInformation.Update(request, OnConsentInfoUpdated); //동의 캐싱
    }

    private void OnConsentInfoUpdated(FormError error)
    {
        MainThreadContext.Post(_ =>
        {
            if (error != null)
            {
                if (ConsentInformation.CanRequestAds())
                {
                    InitializeAds();
                }
                return;
            }

            ConsentForm.LoadAndShowConsentFormIfRequired(OnConsentFormDismissed);
        }, null);
    }

    private void OnConsentFormDismissed(FormError formError)
    {
        MainThreadContext.Post(_ =>
        {
            if (ConsentInformation.CanRequestAds())
                InitializeAds();
        }, null);
    }

    private void InitializeAds()
    {
        if (AdsInitialized) return;
        AdsInitialized = true;

        MobileAds.Initialize(initStatus =>
        {
            MainThreadContext.Post(_ =>
            {
                LoadRewardedAd();
            }, null);
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
            MainThreadContext.Post(_ =>
            {
                if (error != null || ad == null)
                {
                    Invoke(nameof(LoadRewardedAd), 30f);
                    return;
                }
                RewardedAd = ad;
            }, null);
        });
    }

    public void ShowRewardedAd(Action onRewardEarned)
    {
        if (RewardedAd == null || !RewardedAd.CanShowAd()) return;

        IsAdTransitioning = true;
        OnRewardEarned = onRewardEarned;
        RewardEarned = false;

        RewardedAd.OnAdFullScreenContentClosed += OnAdClosed;
        RewardedAd.Show((Reward reward) =>
        {
            RewardEarned = true;
        });
    }

    private void OnAdClosed()
    {
        MainThreadContext.Post(_ =>
        {
            RewardedAd.OnAdFullScreenContentClosed -= OnAdClosed;

            if (RewardEarned)
            {
                OnRewardEarned?.Invoke();
            }

            RewardEarned = false;
            OnRewardEarned = null;

            LoadRewardedAd();

            StartCoroutine(ClearAdTransitionFlagAfterDelay());
        }, null);
    }

    private IEnumerator ClearAdTransitionFlagAfterDelay()
    {
        yield return new WaitForSecondsRealtime(AdPauseCooldown);
        IsAdTransitioning = false;
    }
}
