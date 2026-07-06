using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour, IBoardOccupant
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

    public int CurrentX { get; set; } = 2;
    public int CurrentY { get; set; } = 0;
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

        // ��۱� ����
        AchievementManager.OnSkinEquipped += ApplyEquippedSkin;
        ApplyEquippedSkin(); // ������ �� �� �� �Ա�
    }

    private void OnDestroy()
    {
        if (InputController != null)
        {
            InputController.OnSwipeUpdate -= HandleSwipeUpdate;
            InputController.OnSwipeEnd -= HandleSwipeEnd;
            InputController.OnSwipeCanceled -= HidePreview;
        }

        // ��۱� ���� ����
        AchievementManager.OnSkinEquipped -= ApplyEquippedSkin;
    }

    private void HandleSwipeUpdate(Vector2Int Direction)
    {
        if (InputController != null && InputController.bIsInputBlocked) return;
        if (GameManager.Instance.CurrentState == GameState.EnemyTurn || HealthComponent == null || HealthComponent.CurrentHealth <= 0) return;

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
        if (InputController != null && InputController.bIsInputBlocked) return;
        if (GameManager.Instance.CurrentState == GameState.EnemyTurn || HealthComponent == null || HealthComponent.CurrentHealth <= 0) return;
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

        int PreviousX = CurrentX;
        int PreviousY = CurrentY;

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
            float T = Mathf.Clamp01(Elapsed / Duration); // 0.0 ���� 1.0 ������ ���൵

            // 1. �������� ���� �ٴ� ��ǥ
            Vector2 LinearPos = Vector2.Lerp(StartPosition, TargetPosition, T);

            // 2. ���� �׷����� �̿��� 0 -> ���� -> 0 ���� ���ϴ� ���� �� ���
            float HeightOffset = Mathf.Sin(T * Mathf.PI) * DynamicHopHeight;

            // 3. �ٴ� ��ǥ�� ���� ���� ���ؼ� ���� ��ġ ����
            transform.position = new Vector3(LinearPos.x, LinearPos.y + HeightOffset, transform.position.z);
            yield return null;
        }

        transform.position = TargetPosition;
        if (Sorter != null) Sorter.SortingOrderOffset -= 1000;

        CurrentX = TargetX;
        CurrentY = TargetY;
        GridManager.MoveOccupant(this, PreviousX, PreviousY, CurrentX, CurrentY);

        if (TargetEnemy != null)
        {
            EnemyManager.RemoveEnemy(TargetEnemy);
            TargetEnemy.Die();

            if (TutorialHintManager.Instance != null)
                TutorialHintManager.Instance.TryShowHint(TutorialHintID.Combo);

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
        int PreviousX = CurrentX;
        int PreviousY = CurrentY;

        int StartX, StartY;
        GridManager.GetPlayerStartIndex(out StartX, out StartY);
        CurrentX = StartX;
        CurrentY = StartY;
        GridManager.MoveOccupant(this, PreviousX, PreviousY, CurrentX, CurrentY);

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
                        // ���� Ÿ��(LightTile) �Ǵ� ��ο� Ÿ��(DarkTile) �� ���Ͻô� ����� �����ø� �˴ϴ�.
                        previewSR.sprite = skinData.LightTileSprite;
                    }
                }
                break;
            }
        }
    }
}
