using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [HideInInspector]
    public GridManager GridManager;
    [HideInInspector]
    public TurnManager TurnManager;
    [HideInInspector]
    public EnemyManager EnemyManager;
    [HideInInspector]
    public ItemManager ItemManager;
    [HideInInspector]
    public InputController InputController;
    [HideInInspector]
    public HealthComponent HealthComponent;

    public GameObject PreviewIndicator;

    public int CurrentX = 2;
    public int CurrentY = 0;
    public float MoveSpeed = 15f;
    public bool bIsMoving = false;

    [Header("Movement Settings")]
    public float HopHeight = 0.5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HealthComponent = GetComponent<HealthComponent>();
        if (InputController != null)
        {
            InputController.OnSwipeUpdate += HandleSwipeUpdate;
            InputController.OnSwipeEnd += HandleSwipeEnd;
            InputController.OnSwipeCanceled += HidePreview;
        }

        // 방송국 구독
        AchievementManager.OnSkinEquipped += ApplyEquippedSkin;
        ApplyEquippedSkin(); // 시작할 때 한 번 입기
    }

    private void OnDestroy()
    {
        if (InputController != null)
        {
            InputController.OnSwipeUpdate -= HandleSwipeUpdate;
            InputController.OnSwipeEnd -= HandleSwipeEnd;
            InputController.OnSwipeCanceled -= HidePreview;
        }

        // 방송국 구독 해제
        AchievementManager.OnSkinEquipped -= ApplyEquippedSkin;
    }

    private void HandleSwipeUpdate(Vector2Int Direction)
    {
        if (TurnManager.GameManager.CurrentState == GameState.EnemyTurn || HealthComponent.CurrentHealth <= 0) return;

        if (bIsMoving) return;

        int TargetX = CurrentX + Direction.x;
        int TargetY = CurrentY + Direction.y;

        if (!GridManager.IsValidGridPosition(TargetX, TargetY))
        {
            HidePreview();
            return;
        }

        Vector2 PreviewPos = GridManager.GetWorldPosition(TargetX, TargetY);
        PreviewIndicator.transform.position = PreviewPos;
        PreviewIndicator.SetActive(true);
    }

    private void HandleSwipeEnd(Vector2Int Direction)
    {
        if (TurnManager.GameManager.CurrentState == GameState.EnemyTurn || HealthComponent.CurrentHealth <= 0) return;
        HidePreview();

        if (bIsMoving) return;

        int TargetX = CurrentX + Direction.x;
        int TargetY = CurrentY + Direction.y;

        if (!GridManager.IsValidGridPosition(TargetX, TargetY)) return;

        StartCoroutine(PerformAction(TargetX, TargetY));
    }

    private IEnumerator PerformAction(int TargetX, int TargetY)
    {
        bIsMoving = true;

        Vector2 StartPosition = transform.position;
        Vector2 TargetPosition = GridManager.GetWorldPosition(TargetX, TargetY);

        EnemyBase TargetEnemy = null;
        if (EnemyManager != null)
        {
            TargetEnemy = EnemyManager.GetEnemyAtPosition(TargetX, TargetY);
        }

        ItemBase TargetItem = null;
        if (ItemManager != null)
        {
            TargetItem = ItemManager.GetItemAtPosition(TargetX, TargetY);
        }

        SpriteDepthSorter Sorter = GetComponent<SpriteDepthSorter>();
        if (Sorter != null) Sorter.SortingOrderOffset += 1000;

        float Distance = Vector2.Distance(StartPosition, TargetPosition);
        float Duration = Distance / MoveSpeed;
        float DynamicHopHeight = HopHeight * (Distance * 0.5f + 0.5f);
        float Elapsed = 0f;

        while (Elapsed < Duration)
        {
            Elapsed += Time.deltaTime;
            float T = Mathf.Clamp01(Elapsed / Duration); // 0.0 에서 1.0 사이의 진행도

            // 1. 직선으로 가는 바닥 좌표
            Vector2 LinearPos = Vector2.Lerp(StartPosition, TargetPosition, T);

            // 2. 사인 그래프를 이용해 0 -> 정점 -> 0 으로 변하는 높이 값 계산
            float HeightOffset = Mathf.Sin(T * Mathf.PI) * DynamicHopHeight;

            // 3. 바닥 좌표에 높이 값을 더해서 최종 위치 적용
            transform.position = new Vector3(LinearPos.x, LinearPos.y + HeightOffset, transform.position.z);
            yield return null;
        }

        transform.position = TargetPosition;
        if (Sorter != null) Sorter.SortingOrderOffset -= 1000;

        CurrentX = TargetX;
        CurrentY = TargetY;

        if (TargetEnemy != null)
        {
            EnemyManager.RemoveEnemy(TargetEnemy);
            TargetEnemy.Die();
            if (CameraController.Instance != null)
            {
                CameraController.Instance.ShakeForAttack();
            }

            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.Vibrate(VibrateType.Light);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(SfxType.PlayerAttack, true);
            }
        }
        else if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(SfxType.PlayerMove, true);
        }

        if (TargetItem != null)
        {
            TargetItem.Consume(this);
        }


        bIsMoving = false;

        if (TurnManager != null) TurnManager.EndPlayerTurn();
    }
    private void HidePreview()
    {
        if (PreviewIndicator != null)
        {
            PreviewIndicator.SetActive (false);
        }
    }

    public void ResetPlayer()
    {
        GridManager.GetPlayerStartIndex(out CurrentX, out CurrentY);
        transform.position = GridManager.GetWorldPosition(CurrentX, CurrentY);

        if (HealthComponent != null)
        {
            HealthComponent.ResetHealth();
        }
        bIsMoving = false;
    }

    public void ApplyEquippedSkin()
    {
        string equippedSkinID = PlayerPrefs.GetString("Equipped_Skin", "skin_default");
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null || AchievementManager.Instance == null || AchievementManager.Instance.Database == null) return;

        foreach (var data in AchievementManager.Instance.Database.Achievements)
        {
            if (data.RewardItem is SkinData skinData && skinData.ItemID == equippedSkinID)
            {
                if (skinData.PlayerSprite != null)
                {
                    sr.sprite = skinData.PlayerSprite;
                }
                sr.color = skinData.PlayerColor;

                if (PreviewIndicator != null)
                {
                    SpriteRenderer previewSR = PreviewIndicator.GetComponent<SpriteRenderer>();
                    if (previewSR != null && skinData.LightTileSprite != null)
                    {
                        // 밝은 타일(LightTile) 또는 어두운 타일(DarkTile) 중 원하시는 모양을 넣으시면 됩니다.
                        previewSR.sprite = skinData.LightTileSprite;
                    }
                }
                break;
            }
        }
    }
}
