using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUIController : MonoBehaviour
{
    [Header("Panel")]
    public RectTransform PanelRect;

    [Header("Audio")]
    public Slider BGMSlider;
    public Slider SFXSlider;

    [Header("Language")]
    public Button KoreanButton;
    public Button EnglishButton;

    [Header("Gameplay")]
    public Toggle ScreenShakeToggle;

    [Header("Animation Settings")]
    public float PopDuration = 0.4f;
    public Ease PopEase = Ease.OutBack;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void Show()
    {
        // 현재 저장된 값으로 UI 초기화
        if (AudioManager.Instance != null)
        {
            if (BGMSlider != null) BGMSlider.value = AudioManager.Instance.BGMVolume;
            if (SFXSlider != null) SFXSlider.value = AudioManager.Instance.SFXVolume;
        }

        if (SettingsManager.Instance != null && ScreenShakeToggle != null)
            ScreenShakeToggle.isOn = SettingsManager.Instance.IsScreenShakeEnabled;

        UpdateLanguageButtons();

        gameObject.SetActive(true);
        PanelRect.localScale = Vector3.zero;
        PanelRect.DOKill();
        PanelRect.DOScale(Vector3.one, PopDuration).SetEase(PopEase);
    }

    public void Hide()
    {
        PanelRect.DOKill();
        PanelRect.DOScale(Vector3.zero, PopDuration * 0.7f)
            .SetEase(Ease.InBack)
            .OnComplete(() => gameObject.SetActive(false));
    }

    // BGM 슬라이더 OnValueChanged에 연결
    public void OnBGMChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetBGMVolume(value);
    }

    // SFX 슬라이더 OnValueChanged에 연결
    public void OnSFXChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSFXVolume(value);
    }

    // 한국어 버튼 OnClick에 연결
    public void OnKoreanButtonClicked()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.SetLanguage(Language.Korean);

        UpdateLanguageButtons();
    }

    // 영어 버튼 OnClick에 연결
    public void OnEnglishButtonClicked()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.SetLanguage(Language.English);

        UpdateLanguageButtons();
    }

    // 화면 흔들림 토글 OnValueChanged에 연결
    public void OnScreenShakeToggleChanged(bool value)
    {
        if (SettingsManager.Instance != null)
            SettingsManager.Instance.SetScreenShake(value);
    }

    // 현재 선택된 언어 버튼을 시각적으로 강조
    private void UpdateLanguageButtons()
    {
        if (LocalizationManager.Instance == null) return;

        bool isKorean = LocalizationManager.Instance.CurrentLanguage == Language.Korean;

        if (KoreanButton != null)
            KoreanButton.interactable = !isKorean;

        if (EnglishButton != null)
            EnglishButton.interactable = isKorean;
    }
}
