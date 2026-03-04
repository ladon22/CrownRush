using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    [HideInInspector]
    public int CurrentScore = 0;
    [Header("Score Setting")]
    public TextMeshProUGUI ScoreText;
    public float ScorePopScale = 1.5f;

    [Header("Combo Settings")]
    public int CurrentCombo = 0;
    public float MaxComboTime = 5.0f;
    private float CurrentComboTimer = 0f;
    [Header("Combo Multiplier Settings")]
    public int Stage1Threshold = 2;
    public float Stage1Multiplier = 1.5f;

    public int Stage2Threshold = 10;
    public float Stage2Multiplier = 2.0f;

    [Header("Combo UI")]
    public TextMeshProUGUI ComboText;
    public Image ComboGaugeImage;
    public Vector2 ComboOffset = new Vector2(15f, 15f);
    public float ComboDuration = 0.1f;
    public Gradient ComboGaugeColorGradient;

    [Header("Item Spawn Settings")]
    public int ItemSpawnScoreThreshold = 200;
    private int CurrentItemScore = 0;

    private GameManager GameManager;
    private PlayerController Player;
    private bool bIsGaugeAnimating = false;
    private void Start()
    {
        GameManager = FindFirstObjectByType<GameManager>();
    }

    private void Update()
    {
        if (CurrentCombo > 0)
        {
            if(Player == null) Player = FindFirstObjectByType<PlayerController>();

            if (GameManager != null && GameManager.CurrentState == GameState.PlayerTurn)
            {
                if (Player != null && !Player.bIsMoving)
                {
                    CurrentComboTimer -= Time.deltaTime;
                    UpdateComboGaugeUI();
                    // ½Ã°£ÀÌ ´Ù µÇ¸é ÄÞº¸°¡ ²÷±é´Ï´Ù.
                    if (CurrentComboTimer <= 0)
                    {
                        ResetCombo();
                    }
                }
            }
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void ResetScore()
    {
        CurrentScore = 0;
        CurrentCombo = 0;
        CurrentComboTimer = 0f;
        CurrentItemScore = 0;

        if (ComboText != null)
        {
            ComboText.DOKill();
            ComboText.transform.DOKill();
            ComboText.gameObject.SetActive(false);
            ComboText.alpha = 1f;
        }

        if (ComboGaugeImage != null)
        {
            ComboGaugeImage.fillAmount = 0f;

        }
        UpdateScore();
        
    }

    public void AddScore(int ScoreToAdd)
    {
        CurrentCombo++;
        CurrentComboTimer = MaxComboTime;

        float Multiplier = 1.0f;

        if (CurrentCombo >= Stage2Threshold)
        {
            Multiplier = Stage2Multiplier;
        }
        else if (CurrentCombo >= Stage1Threshold)
        {
            Multiplier = Stage1Multiplier;
        }
        int FinalScore = Mathf.RoundToInt(ScoreToAdd * Multiplier);
        CurrentScore += FinalScore;

        CurrentItemScore += FinalScore;
        if (CurrentItemScore >= ItemSpawnScoreThreshold)
        {
            int SpawnCount = CurrentItemScore / ItemSpawnScoreThreshold;

            CurrentItemScore %= ItemSpawnScoreThreshold;
            ItemManager ItemManager = FindFirstObjectByType<ItemManager>();
            if (ItemManager != null)
            {
                for (int i = 0; i < SpawnCount; i++)
                {
                    ItemManager.SpawnRandomItem();
                }
            }
        }

        UpdateScore();
        PlayGaugeFillAnimation();
        UpdateComboUI();
        UpdateComboGaugeUI();

        
    }
    // Update is called once per frame

    private void UpdateScore()
    {
        if (ScoreText != null)
        { 
            ScoreText.text = $"{CurrentScore}";

            if (CurrentScore > 0)
            {
                ScoreText.transform.DOKill();
                ScoreText.transform.localScale = Vector3.one;

                Sequence PopSequence = DOTween.Sequence();
                PopSequence.Append(ScoreText.transform.DOScale(Vector3.one * ScorePopScale, 0.08f).SetEase(Ease.OutExpo))
                    .Append(ScoreText.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.InOutQuad));
            }
        }
    }

    private void ResetCombo()
    {
        CurrentCombo = 0;
        CurrentComboTimer = 0f;
        UpdateComboGaugeUI();
        UpdateComboUI();
    }

    private void UpdateComboUI()
    {
        if (ComboText != null)
        {
            if (CurrentCombo >= 1)
            {
                ComboText.gameObject.SetActive(true);
                ComboText.text = $"{CurrentCombo} COMBO!";
                ComboText.DOKill();
                ComboText.transform.DOKill();

                ComboText.transform.localScale = Vector3.one;
                ComboText.transform.rotation = Quaternion.identity;
                ComboText.alpha = 1f;

                Vector2 OringinPostion = ComboText.rectTransform.anchoredPosition;
                Sequence ComboSequence = DOTween.Sequence();
                ComboSequence.Append(ComboText.rectTransform.DOAnchorPos(OringinPostion, ComboDuration).From(OringinPostion + ComboOffset))
                    .Join(ComboText.transform.DOPunchScale(new Vector3(0.5f, 0.5f, 0f), ComboDuration * 2f, 5, 0.5f))
                    .Join(ComboText.transform.DOPunchRotation(new Vector3(0, 0, 15f), ComboDuration * 2f, 5, 0.5f));
            }
            else
            {
                ComboText.DOKill();
                ComboText.transform.DOKill();
                Vector2 BeforeDropPos = ComboText.rectTransform.anchoredPosition;

                Sequence BreakSequence = DOTween.Sequence();
                BreakSequence.Append(ComboText.rectTransform.DOAnchorPosY(ComboText.rectTransform.anchoredPosition.y - 30f, 0.3f).SetEase(Ease.InBack))
                    .Join(ComboText.DOFade(0f, 0.3f))
                    .OnComplete(() =>
                    {
                        ComboText.gameObject.SetActive(false);
                        ComboText.alpha = 1f;
                        ComboText.rectTransform.anchoredPosition = BeforeDropPos;
                    });
            }
        }
    }
    private void PlayGaugeFillAnimation()
    {
        if (ComboGaugeImage != null)
        {
            bIsGaugeAnimating = true;
            ComboGaugeImage.color = ComboGaugeColorGradient.Evaluate(1.0f);
            ComboGaugeImage.DOKill();
            ComboGaugeImage.DOFillAmount(1.0f, 0.2f)
                .SetEase(Ease.InOutExpo)
                .OnComplete(() =>
                {
                    bIsGaugeAnimating = false;
                });
        }
    }
    private void UpdateComboGaugeUI()
    {
        if (ComboGaugeImage != null && !bIsGaugeAnimating)
        {
            ComboGaugeImage.fillAmount = CurrentComboTimer / MaxComboTime;
            ComboGaugeImage.color = ComboGaugeColorGradient.Evaluate(ComboGaugeImage.fillAmount);
        }
    }
}
