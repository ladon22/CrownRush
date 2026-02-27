using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public int CurrentScore = 0;
    public TextMeshProUGUI ScoreText;

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

    private GameManager GameManager;
    private PlayerController Player;

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
                    UpdateComboUI();

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
        ResetCombo();
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
        UpdateScore();
        UpdateComboUI();
    }
    // Update is called once per frame

    private void UpdateScore()
    {
        if (ScoreText != null)
        {
            ScoreText.text = $"{CurrentScore}";
        }
    }

    private void ResetCombo()
    {
        CurrentCombo = 0;
        CurrentComboTimer = 0f;
        UpdateComboUI();
    }

    private void UpdateComboUI()
    {
        if (ComboText != null)
        {
            if (CurrentCombo > 1)
            {
                ComboText.gameObject.SetActive(true);
                ComboText.text = $"{CurrentCombo} COMBO!";
            }
            else
            {
                ComboText.gameObject.SetActive(false);
            }
        }

        if (ComboGaugeImage != null)
        {
            ComboGaugeImage.fillAmount = CurrentComboTimer / MaxComboTime;
        }
    }
}
