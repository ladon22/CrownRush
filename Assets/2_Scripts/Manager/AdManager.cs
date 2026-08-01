using GoogleMobileAds.Api;
using GoogleMobileAds.Ump.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class AdManager : MonoBehaviour
{
    public static AdManager Instance;
    //테스트용 ID: ca-app-pub-3940256099942544/5224354917
    private string AdUnitId = "ca-app-pub-1954417796107340/7852122167";

    private RewardedAd RewardedAd;
    private Action OnRewardEarned;
    private bool bRewardEarned = false;
    public bool bIsAdReady => RewardedAd != null && RewardedAd.CanShowAd();

    public static bool bIsAdTransitioning { get; private set; } = false;
    private const float AdPauseCooldown = 1.0f;
    private bool bAdsInitialized = false;

    // UMP(동의) 콜백은 안드로이드 네이티브 SDK가 메인 스레드가 아닌
    // 스레드에서 호출할 수 있어, 그 안에서 바로 AdMob API(MobileAds.Initialize 등)를
    // 호출하면 "get_isPlaying can only be called from the main thread" 크래시가 납니다.
    // 반드시 이 컨텍스트를 통해 메인 스레드로 되돌린 뒤 호출해야 합니다.
    private SynchronizationContext MainThreadContext;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
    [Header("동의 테스트 (개발 빌드에서만 컴파일됨)")]
    [Tooltip("체크하면 이 기기를 EEA 지역으로 강제 처리해 동의창을 띄웁니다.")]
    [SerializeField] private bool bDebugForceEEA = false;
    [Tooltip("adb logcat에서 확인한 테스트 기기 해시 ID를 넣으세요.")]
    [SerializeField] private List<string> DebugTestDeviceHashedIds = new List<string>();
    [Tooltip("체크하면 실행 시마다 기기에 캐시된 동의 상태를 초기화해 동의창을 매번 다시 띄웁니다. 실기기에 반영하려면 다시 빌드해야 합니다.")]
    [SerializeField] private bool bResetConsentOnLaunch = false;
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
        if (bResetConsentOnLaunch)
        {
            // 테스트 중 동의창을 매번 다시 확인하기 위해 기기에 캐시된 동의 상태를 초기화합니다.
            ConsentInformation.Reset();
        }
#endif

        // 이전 세션에서 이미 동의 상태가 기기에 캐시되어 있다면, 이번 세션의
        // 동의 정보 갱신(네트워크 호출) 완료를 기다리지 않고 바로 초기화해
        // 광고 로딩 지연을 줄입니다.
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
        if (bDebugForceEEA)
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
        // 네이티브 UMP SDK가 메인 스레드가 아닌 스레드에서 호출할 수 있으므로
        // 이후 AdMob API 호출은 전부 메인 스레드로 되돌려서 실행합니다.
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
        if (bAdsInitialized) return;
        bAdsInitialized = true;

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
        });
    }

    public void ShowRewardedAd(Action onRewardEarned)
    {
        if (RewardedAd == null || !RewardedAd.CanShowAd()) return;

        bIsAdTransitioning = true;
        OnRewardEarned = onRewardEarned;
        bRewardEarned = false;

        RewardedAd.OnAdFullScreenContentClosed += OnAdClosed;
        RewardedAd.Show((Reward reward) =>
        {
            bRewardEarned = true;
        });
    }

    private void OnAdClosed()
    {
        RewardedAd.OnAdFullScreenContentClosed -= OnAdClosed;

        if (bRewardEarned)
        {
            OnRewardEarned?.Invoke();
        }

        bRewardEarned = false;
        OnRewardEarned = null;

        LoadRewardedAd();

        StartCoroutine(ClearAdTransitionFlagAfterDelay());
    }

    private IEnumerator ClearAdTransitionFlagAfterDelay()
    {
        // Time.timeScale�� �����ϰ� ���� �ð� �������� ���
        yield return new WaitForSecondsRealtime(AdPauseCooldown);
        bIsAdTransitioning = false;
    }
}
