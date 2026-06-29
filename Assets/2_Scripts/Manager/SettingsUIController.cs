using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;
using TMPro;
using System.Collections;

public class SettingsUIController : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private RectTransform PanelRect;
    [SerializeField] private GameObject Overlay;

    [Header("Audio")]
    [SerializeField] private Toggle BGMToggle;
    [SerializeField] private Toggle SFXToggle;

    [Header("Gameplay")]
    [SerializeField] private Toggle VibrationToggle;
    [SerializeField] private TMP_Dropdown FrameRateDropdown;

    [Header("Language")]
    [SerializeField] private TMP_Dropdown LanguageDropdown;

    [Header("Animation Settings")]
    public float PopDuration = 0.4f;
    public Ease PopEase = Ease.OutBack;

    private bool bManageGameState = true;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void Show(bool manageGameState = true)
    {
        if (gameObject.activeInHierarchy) return;

        bManageGameState = manageGameState;

        if (Overlay != null)
            Overlay.SetActive(manageGameState);

        if (GameManager.Instance != null)
            GameManager.Instance.InputController.PushUIBlock();

        gameObject.SetActive(true);
        // 1. 오디오 토글 UI 초기화 (AudioManager 연동)
        if (AudioManager.Instance != null)
        {
            if (BGMToggle != null) BGMToggle.SetIsOnWithoutNotify(AudioManager.Instance.IsBGMOn);
            if (SFXToggle != null) SFXToggle.SetIsOnWithoutNotify(AudioManager.Instance.IsSFXOn);
        }

        // 2. 화면 흔들림 토글 UI 초기화
        if (SettingsManager.Instance != null )
        {
            if(VibrationToggle != null)
                VibrationToggle.SetIsOnWithoutNotify(SettingsManager.Instance.IsVibrationEnabled);
            if(FrameRateDropdown != null)
            {
                int FpsIndex = SettingsManager.Instance.CurrentFrameRate == 60 ? 1 : 0;
                FrameRateDropdown.SetValueWithoutNotify(FpsIndex);
            }
        }

        // 3. 공식 Localization 언어 드롭다운 초기화
        if (LanguageDropdown != null && LocalizationSettings.AvailableLocales != null)
        {
            LanguageDropdown.SetValueWithoutNotify(LocalizationSettings.AvailableLocales.Locales.IndexOf(LocalizationSettings.SelectedLocale));
        }

        // 팝업 애니메이션 실행
        PanelRect.localScale = Vector3.zero;
        PanelRect.DOKill();
        PanelRect.DOScale(Vector3.one, PopDuration).SetEase(PopEase).SetUpdate(true);

        if (UIBackHandler.Instance != null)
            UIBackHandler.Instance.Push(Hide);
    }

    public void Hide()
    {
        PanelRect.DOKill();
        PanelRect.DOScale(Vector3.zero, PopDuration * 0.7f)
            .SetEase(Ease.InBack)
            .SetUpdate(true)
            .OnComplete(() => {
                gameObject.SetActive(false);
                if (bManageGameState)
                {
                    Time.timeScale = 1.0f;
                    if (GameManager.Instance != null)
                        GameManager.Instance.InputController.PopUIBlock();
                }

                if (UIBackHandler.Instance != null)
                    UIBackHandler.Instance.Pop();
            });

    }
    public void OnBGMToggleChanged(bool isOn)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.ToggleBGM(isOn);
    }

    public void OnSFXToggleChanged(bool isOn)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.ToggleSFX(isOn);
    }

    public void OnVibrationToggleChanged(bool isOn)
    {
        if (SettingsManager.Instance != null) SettingsManager.Instance.SetVibration(isOn);
    }

    public void OnLanguageDropdownChanged(int index)
    {
        StartCoroutine(SetLocaleRoutine(index));
    }

    public void OnFrameRateDropdownChanged(int index)
    {
        int TargetFPS = index == 0 ? 30 : 60;
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.SetFrameRate(TargetFPS);
        }
    }

    private IEnumerator SetLocaleRoutine(int localeID)
    {
        // 1. 다국어 시스템이 완벽하게 로딩될 때까지 안전하게 기다립니다.
        yield return LocalizationSettings.InitializationOperation;

        // 2. 시스템이 준비되면 언어를 교체합니다.
        if (LocalizationSettings.AvailableLocales != null && localeID < LocalizationSettings.AvailableLocales.Locales.Count)
        {
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[localeID];
        }
    }
}
