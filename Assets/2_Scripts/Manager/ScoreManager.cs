using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    public int CurrentScore { get; private set; }

    [Header("Combo Settings")]
    public int CurrentCombo = 0;
    public float MaxComboTime = 5.0f;
    public float CurrentComboTimer = 0f;

    [Header("Combo Multiplier Settings")]
    public int Stage1Threshold = 5;
    public float Stage1Multiplier = 1.5f;
    public int Stage2Threshold = 10;
    public float Stage2Multiplier = 2.0f;
    public int Stage3Threshold = 15;
    public float Stage3Multiplier = 3.0f;
    public int Stage4Threshold = 20;
    public float Stage4Multiplier = 5.0f; // 20콤보 달성 시 무려 5배!


    public Action<int> OnScoreChanged;
    public Action<int> OnComboChanged;
    public Action OnComboReset;

    private GameManager GameManager;
    private PlayerController _player;
    private PlayerController Player => _player != null ? _player : _player = FindFirstObjectByType<PlayerController>();

    private void Awake()
    {
        //씬 종속 싱글톤 초기화
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        GameManager = FindFirstObjectByType<GameManager>();
    }

    private void Update()
    {
        if (CurrentCombo > 0)
        {
            if (GameManager != null && GameManager.CurrentState == GameState.PlayerTurn)
            {
                if (Player != null && !Player.bIsMoving)
                {
                    CurrentComboTimer -= Time.deltaTime;

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
        OnScoreChanged?.Invoke(CurrentScore);
    }

    public void AddScore(int ScoreToAdd)
    {
        CurrentCombo++;
        CurrentComboTimer = MaxComboTime;

        float Multiplier = 1.0f;

        if (CurrentCombo >= Stage4Threshold)
        {
            Multiplier = Stage4Multiplier;
        }
        else if (CurrentCombo >= Stage3Threshold)
        {
            Multiplier = Stage3Multiplier;
        }
        else if (CurrentCombo >= Stage2Threshold)
        {
            Multiplier = Stage2Multiplier;
        }
        else if (CurrentCombo >= Stage1Threshold)
        {
            Multiplier = Stage1Multiplier;
        }

        int FinalScore = Mathf.RoundToInt(ScoreToAdd * Multiplier);
        CurrentScore += FinalScore;

        // 업적 매니저에게 현재 점수와 콤보 상태를 실시간으로 보고
        if (AchievementManager.Instance != null)
        {
            AchievementManager.Instance.ReportProgress(AchievementType.Score, CurrentScore);
            AchievementManager.Instance.ReportProgress(AchievementType.Combo, CurrentCombo);
        }

        OnScoreChanged?.Invoke(CurrentScore);
        OnComboChanged?.Invoke(CurrentCombo);
    }

    public void ResetCombo()
    {
        CurrentCombo = 0;
        CurrentComboTimer = 0f;

        // 콤보가 깨졌다고 방송합니다.
        OnComboReset?.Invoke();
    }
}
