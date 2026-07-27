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

    [Header("Floating Text")]
    public GameObject FloatingTextPrefab;

    public Action<int> OnScoreChanged;
    public Action<int> OnComboChanged;
    public Action OnComboReset;
    public Action<float, float> OnComboTimerUpdated;

    private PlayerController _player;
    private PlayerController Player => _player != null ? _player : _player = FindFirstObjectByType<PlayerController>();
    public int BestComboThisGame { get; private set; } = 0;

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
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged += HandleStateChanged;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(GameState state)
    {
        if (state == GameState.Initialize)
            ResetScore();
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.PlayerTurn) return;
        if (CurrentCombo <= 0) return;
        if (Player == null || Player.bIsMoving) return;

        CurrentComboTimer -= Time.deltaTime;
        OnComboTimerUpdated?.Invoke(CurrentComboTimer, MaxComboTime);

        if (CurrentComboTimer <= 0)
        {
            ResetCombo();
        }

    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void ResetScore()
    {
        CurrentScore = 0;
        BestComboThisGame = 0;
        ResetCombo();
        OnScoreChanged?.Invoke(CurrentScore);
    }

    public void AddScore(int ScoreToAdd, Vector3 spawnPosition = default)
    {
        CurrentCombo++;
        CurrentComboTimer = MaxComboTime;

        if (CurrentCombo > BestComboThisGame)
            BestComboThisGame = CurrentCombo;

        float multiplier = GetMultiplier(CurrentCombo);
        int finalScore = Mathf.RoundToInt(ScoreToAdd * multiplier);
        CurrentScore += finalScore;

        // 플로팅 텍스트 스폰 로직 추가
        if (FloatingTextPrefab != null && spawnPosition != default)
        {
            GameObject textObj = PoolManager.Instance.SpawnFromPool(FloatingTextPrefab, spawnPosition, Quaternion.identity);
            FloatingText floatingText = textObj.GetComponent<FloatingText>();
            floatingText?.Show(finalScore);
        }

        // 업적 매니저에게 현재 점수와 콤보 상태를 실시간으로 보고
        if (AchievementManager.Instance != null)
        {
            AchievementManager.Instance.ReportProgress(AchievementType.Score, CurrentScore);
            AchievementManager.Instance.ReportProgress(AchievementType.Combo, BestComboThisGame);
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

    private float GetMultiplier(int combo)
    {
        if (combo >= Stage4Threshold) return Stage4Multiplier;
        if (combo >= Stage3Threshold) return Stage3Multiplier;
        if (combo >= Stage2Threshold) return Stage2Multiplier;
        if (combo >= Stage1Threshold) return Stage1Multiplier;
        return 1.0f;
    }
}
