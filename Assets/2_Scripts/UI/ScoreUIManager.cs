using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreUIManager : MonoBehaviour
{
    [Header("Score UI")]
    public TextMeshProUGUI ScoreText;
    public float ScorePopScale = 1.5f;

    [Header("Combo UI")]
    public TextMeshProUGUI ComboText;
    public Slider ComboGaugeSlider;
    public Image ComboGaugeFillImage;
    public Vector2 ComboOffset = new Vector2(15f, 15f);
    public float ComboDuration = 0.1f;
    public Gradient ComboGaugeColorGradient;

    [Header("Combo Gauge Shake Settings")]
    public float ShakeThreshold = 0.3f;
    public float ShakeStrength = 5f;
    public float ShakeSpeed = 20f;

    private RectTransform ComboGaugeRect;
    private Vector2 ComboGaugeOriginPos;
    private Vector2 ComboTextOriginPos;
    private bool IsGaugeAnimating = false;

    private Sequence ComboAnimSequence;

    private Tween ShakeTween;
    private bool IsShaking = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (ComboGaugeSlider != null)
        {
            ComboGaugeRect = ComboGaugeSlider.GetComponent<RectTransform>();
            if (ComboGaugeRect != null) ComboGaugeOriginPos = ComboGaugeRect.anchoredPosition;
        }

        if (ComboText != null)
        {
            ComboTextOriginPos = ComboText.rectTransform.anchoredPosition;
        }

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged += PlayScoreAnimation;
            ScoreManager.Instance.OnComboChanged += PlayComboAnimation;
            ScoreManager.Instance.OnComboReset += PlayComboBreakAnimation;
            ScoreManager.Instance.OnComboTimerUpdated += UpdateComboGaugeUI;
        }
    }

    private void OnDestroy()
    {
        ShakeTween?.Kill();

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged -= PlayScoreAnimation;
            ScoreManager.Instance.OnComboChanged -= PlayComboAnimation;
            ScoreManager.Instance.OnComboReset -= PlayComboBreakAnimation;
            ScoreManager.Instance.OnComboTimerUpdated -= UpdateComboGaugeUI;
        }
    }


    private void PlayScoreAnimation(int newScore)
    {
        if (ScoreText != null)
        {
            ScoreText.text = $"{newScore}";
            if (newScore > 0)
            {
                ScoreText.transform.DOKill();
                ScoreText.transform.localScale = Vector3.one;

                Sequence PopSequence = DOTween.Sequence();
                PopSequence.Append(ScoreText.transform.DOScale(Vector3.one * ScorePopScale, 0.08f).SetEase(Ease.OutExpo))
                    .Append(ScoreText.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.InOutQuad));
            }
        }
    }

    private void PlayComboAnimation(int currentCombo)
    {
        if (ComboGaugeSlider != null)
        {
            IsGaugeAnimating = true;
            if (ComboGaugeFillImage != null)
            {
                ComboGaugeFillImage.enabled = true;
                ComboGaugeFillImage.color = ComboGaugeColorGradient.Evaluate(1.0f);
            }
            ComboGaugeSlider.DOKill();
            ComboGaugeSlider.DOValue(1.0f, 0.2f).SetEase(Ease.InOutExpo).OnComplete(() => IsGaugeAnimating = false);
        }

        if (ComboText != null && currentCombo >= 1)
        {
            ComboText.gameObject.SetActive(true);

            string MultiplierText = "";
            if (currentCombo >= ScoreManager.Instance.Stage4Threshold)
                MultiplierText = $" <color=#D896FF>[x{ScoreManager.Instance.Stage4Multiplier:F1}]</color>";
            else if (currentCombo >= ScoreManager.Instance.Stage3Threshold)
                MultiplierText = $" <color=#00EAFF>[x{ScoreManager.Instance.Stage3Multiplier:F1}]</color>";
            else if (currentCombo >= ScoreManager.Instance.Stage2Threshold)
                MultiplierText = $" <color=#FF2A2A>[x{ScoreManager.Instance.Stage2Multiplier:F1}]</color>";
            else if (currentCombo >= ScoreManager.Instance.Stage1Threshold)
                MultiplierText = $" <color=#FFB300>[x{ScoreManager.Instance.Stage1Multiplier:F1}]</color>";

            ComboText.text = $"{currentCombo} COMBO!{MultiplierText}";

            if (ComboAnimSequence != null && ComboAnimSequence.IsActive())
            {
                ComboAnimSequence.Kill();
            }

            ComboText.DOKill();
            ComboText.transform.DOKill();
            ComboText.rectTransform.DOKill();

            ComboText.rectTransform.anchoredPosition = ComboTextOriginPos;
            ComboText.rectTransform.localScale = Vector3.one;
            ComboText.rectTransform.localRotation = Quaternion.identity;
            ComboText.alpha = 1f;

            ComboAnimSequence = DOTween.Sequence();
            ComboAnimSequence.Append(ComboText.rectTransform.DOAnchorPos(ComboTextOriginPos, ComboDuration).From(ComboTextOriginPos + ComboOffset))
                .Join(ComboText.transform.DOPunchScale(new Vector3(0.5f, 0.5f, 0f), ComboDuration * 2f, 5, 0.5f))
                .Join(ComboText.transform.DOPunchRotation(new Vector3(0, 0, 15f), ComboDuration * 2f, 5, 0.5f));
        }
    }

    private void PlayComboBreakAnimation()
    {
        if (IsShaking)
        {
            IsShaking = false;
            ShakeTween?.Kill();
        }

        if (ComboGaugeSlider != null)
        {
            ComboGaugeSlider.value = 0f;
            if (ComboGaugeFillImage != null) ComboGaugeFillImage.enabled = false;
            if (ComboGaugeRect != null) ComboGaugeRect.anchoredPosition = ComboGaugeOriginPos;
        }

        if (ComboText != null)
        {
            if (ComboAnimSequence != null && ComboAnimSequence.IsActive())
            {
                ComboAnimSequence.Kill();
            }

            ComboText.DOKill();
            ComboText.transform.DOKill();
            ComboText.rectTransform.DOKill();

            ComboText.transform.localScale = Vector3.one;
            ComboText.transform.rotation = Quaternion.identity;

            Sequence BreakSequence = DOTween.Sequence();
            BreakSequence.Append(ComboText.rectTransform.DOAnchorPosY(ComboTextOriginPos.y - 30f, 0.3f).SetEase(Ease.InBack))
                .Join(ComboText.DOFade(0f, 0.3f))
                .OnComplete(() =>
                {
                    ComboText.gameObject.SetActive(false);
                    ComboText.alpha = 1f;
                    ComboText.rectTransform.anchoredPosition = ComboTextOriginPos;
                });
        }
    }

    private void UpdateComboGaugeUI(float currentTime, float maxTime)
    {
        if (ComboGaugeSlider == null || IsGaugeAnimating) return;

        float ratio = currentTime / maxTime;
        ComboGaugeSlider.value = ratio;

        if (ComboGaugeFillImage != null)
        {
            ComboGaugeFillImage.enabled = ratio > 0f;
            ComboGaugeFillImage.color = ComboGaugeColorGradient.Evaluate(ratio);
        }

        if (ComboGaugeRect == null) return;

        if (ratio <= ShakeThreshold && ratio > 0f)
        {
            if (!IsShaking)
            {
                IsShaking = true;
                ShakeTween = ComboGaugeRect
                    .DOShakeAnchorPos(0.3f, new Vector2(0f, ShakeStrength),
                        (int)ShakeSpeed, 0f, false, false)
                    .SetLoops(-1);
            }
        }
        else
        {
            if (IsShaking)
            {
                IsShaking = false;
                ShakeTween?.Kill();
                ComboGaugeRect.anchoredPosition = ComboGaugeOriginPos;
            }
        }
    }
}