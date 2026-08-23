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

    private const string PrivacyPolicyUrlEn = "https://crownrush.net/privacy-policy.html";
    private const string PrivacyPolicyUrlKo = "https://crownrush.net/privacy-policy-ko.html";

    private bool ManageGameState = true;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void Show(bool manageGameState = true)
    {
        if (gameObject.activeInHierarchy) return;

        ManageGameState = manageGameState;

        if (Overlay != null)
            Overlay.SetActive(manageGameState);

        if (manageGameState && GameManager.Instance != null)
            GameManager.Instance.InputController.PushInputLock();

        gameObject.SetActive(true);
        if (AudioManager.Instance != null)
        {
            if (BGMToggle != null) BGMToggle.SetIsOnWithoutNotify(AudioManager.Instance.IsBGMOn);
            if (SFXToggle != null) SFXToggle.SetIsOnWithoutNotify(AudioManager.Instance.IsSFXOn);
        }

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
                if (ManageGameState)
                {
                    Time.timeScale = 1.0f;
                    if (GameManager.Instance != null)
                        GameManager.Instance.InputController.PopInputLock();
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

    public void OnPrivacyPolicyButtonClicked()
    {
        bool isKorean = LocalizationSettings.SelectedLocale != null
            && LocalizationSettings.SelectedLocale.Identifier.Code.StartsWith("ko");

        Application.OpenURL(isKorean ? PrivacyPolicyUrlKo : PrivacyPolicyUrlEn);
    }

    private IEnumerator SetLocaleRoutine(int localeID)
    {
        yield return LocalizationSettings.InitializationOperation;

        if (LocalizationSettings.AvailableLocales != null && localeID < LocalizationSettings.AvailableLocales.Locales.Count)
        {
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[localeID];
        }
    }
}
