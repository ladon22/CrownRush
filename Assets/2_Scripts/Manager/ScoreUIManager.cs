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
    private bool bIsGaugeAnimating = false;

    private Sequence comboAnimSequence;

    private Tween shakeTween;
    private bool bIsShaking = false;
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
    }

    private void OnEnable()
    {
        //싱글톤 인스턴스에 바로 접근하여 구독
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged += PlayScoreAnimation;
            ScoreManager.Instance.OnComboChanged += PlayComboAnimation;
            ScoreManager.Instance.OnComboReset += PlayComboBreakAnimation;
        }
    }

    private void OnDisable()
    {
        shakeTween?.Kill();

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged -= PlayScoreAnimation;
            ScoreManager.Instance.OnComboChanged -= PlayComboAnimation;
            ScoreManager.Instance.OnComboReset -= PlayComboBreakAnimation;
        }
    }

    private void Update()
    {
        if (ScoreManager.Instance != null && ScoreManager.Instance.CurrentCombo > 0)
        {
            UpdateComboGaugeUI();
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
            bIsGaugeAnimating = true;
            if (ComboGaugeFillImage != null)
            {
                ComboGaugeFillImage.enabled = true;
                ComboGaugeFillImage.color = ComboGaugeColorGradient.Evaluate(1.0f);
            }
            ComboGaugeSlider.DOKill();
            ComboGaugeSlider.DOValue(1.0f, 0.2f).SetEase(Ease.InOutExpo).OnComplete(() => bIsGaugeAnimating = false);
        }

        if (ComboText != null && currentCombo >= 1)
        {
            ComboText.gameObject.SetActive(true);

            string MultiplierText = "";
            // 싱글톤을 통해 변수 접근
            if (currentCombo >= ScoreManager.Instance.Stage2Threshold) MultiplierText = " <color=#FF2A2A>[x2.0]</color>";
            else if (currentCombo >= ScoreManager.Instance.Stage1Threshold) MultiplierText = " <color=#FFB300>[x1.5]</color>";

            ComboText.text = $"{currentCombo} COMBO!{MultiplierText}";

            if (comboAnimSequence != null && comboAnimSequence.IsActive())
            {
                comboAnimSequence.Kill();
            }

            ComboText.DOKill();
            ComboText.transform.DOKill();
            ComboText.rectTransform.DOKill();

            ComboText.rectTransform.anchoredPosition = ComboTextOriginPos;
            ComboText.rectTransform.localScale = Vector3.one;
            ComboText.rectTransform.localRotation = Quaternion.identity; // 2D UI는 꼭 localRotation을 씁니다!
            ComboText.alpha = 1f;

            comboAnimSequence = DOTween.Sequence();
            comboAnimSequence.Append(ComboText.rectTransform.DOAnchorPos(ComboTextOriginPos, ComboDuration).From(ComboTextOriginPos + ComboOffset))
                .Join(ComboText.transform.DOPunchScale(new Vector3(0.5f, 0.5f, 0f), ComboDuration * 2f, 5, 0.5f))
                .Join(ComboText.transform.DOPunchRotation(new Vector3(0, 0, 15f), ComboDuration * 2f, 5, 0.5f));
        }
    }

    private void PlayComboBreakAnimation()
    {
        if (bIsShaking)
        {
            bIsShaking = false;
            shakeTween?.Kill();
        }

        if (ComboGaugeSlider != null)
        {
            ComboGaugeSlider.value = 0f;
            if (ComboGaugeFillImage != null) ComboGaugeFillImage.enabled = false;
            if (ComboGaugeRect != null) ComboGaugeRect.anchoredPosition = ComboGaugeOriginPos;
        }

        if (ComboText != null)
        {
            if (comboAnimSequence != null && comboAnimSequence.IsActive())
            {
                comboAnimSequence.Kill();
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

    private void UpdateComboGaugeUI()
    {
        if (ComboGaugeSlider != null && !bIsGaugeAnimating && ScoreManager.Instance != null)
        {
            float ratio = ScoreManager.Instance.CurrentComboTimer / ScoreManager.Instance.MaxComboTime;
            ComboGaugeSlider.value = ratio;

            if (ComboGaugeFillImage != null)
            {
                ComboGaugeFillImage.enabled = ratio > 0f;
                ComboGaugeFillImage.color = ComboGaugeColorGradient.Evaluate(ratio);
            }

            if (ComboGaugeRect != null)
            {
                // 30% 이하일 때 한 번만 DOTween 쉐이크 실행
                if (ratio <= ShakeThreshold && ratio > 0f)
                {
                    if (!bIsShaking)
                    {
                        bIsShaking = true;

                        // DOShakeAnchorPos(시간, 강도, 진동수, 랜덤성, 스내핑, 페이드아웃)
                        // fadeOut을 false로 꺼서 서서히 멈추지 않고 계속 동일한 강도로 떨리게 만듭니다.
                        shakeTween = ComboGaugeRect.DOShakeAnchorPos(0.3f, new Vector2(0f, ShakeStrength), (int)ShakeSpeed, 0f, false, false)
                            .SetLoops(-1); // 무한 반복
                    }
                }
                else
                {
                    // 30%를 다시 넘기거나 0이 되면 쉐이크 종료 후 원위치
                    if (bIsShaking)
                    {
                        bIsShaking = false;
                        shakeTween?.Kill();
                        ComboGaugeRect.anchoredPosition = ComboGaugeOriginPos;
                    }
                }
            }
        }
    }
}
