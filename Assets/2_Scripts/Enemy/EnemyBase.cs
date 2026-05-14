using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    [HideInInspector]
    public GridManager GridManager;
    [HideInInspector]
    public EnemyManager EnemyManager;
    [HideInInspector]
    public PlayerController Player;

    private ItemManager _itemManager;
    private ItemManager CachedItemManager => _itemManager != null ? _itemManager : _itemManager = FindFirstObjectByType<ItemManager>();

    public int CurrentX;
    public int CurrentY;
    public float MoveSpeed = 20f;
    public int ScoreValue = 1;

    [Header("UI")]
    public GameObject IntentIndicator;

    [Header("Skin Settings")]
    public EnemyPieceType PieceType; // 인스펙터에서 적 종류 선택!


    [Header("Movement Settings")]
    public int MoveInterval = 1;
    public float HopHeight = 0.5f;
    protected int CurrentWaitCount = 0;

    public virtual void Initialize(EnemyManager EnemyManager, GridManager GridManager, int StartX, int StartY)
    {
        this.EnemyManager = EnemyManager;
        this.GridManager = GridManager;
        CurrentX = StartX;
        CurrentY = StartY;

        CurrentWaitCount = 0;
        Player = FindFirstObjectByType<PlayerController>();
        SetIntent(false);

        ApplyEquippedSkin();

        Vector2 TargetPosition = GridManager.GetWorldPosition(CurrentX, CurrentY);
        StartCoroutine(SpawnDropRoutine(TargetPosition));
    }

    protected virtual void OnDestroy()
    {
        if (EnemyManager != null)
        {
            EnemyManager.RemoveEnemy(this);
        }
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
                EnemySkinData enemySkin = skinData.GetEnemySkin(PieceType);

                if (enemySkin.EnemySprite != null)
                {
                    sr.sprite = enemySkin.EnemySprite;
                }

                if (skinData.EnemyColor.a > 0)
                {
                    sr.color = skinData.EnemyColor;
                }
                break;
            }
        }
    }

    private IEnumerator SpawnDropRoutine(Vector2 TargetPosition)
    {
        float DropHeight = 5.0f;
        transform.position = new Vector3(TargetPosition.x, TargetPosition.y + DropHeight, 0f);

        // 떨어지는 시간
        float Duration = 0.35f;
        float Elapsed = 0f;
        SpriteDepthSorter Sorter = GetComponent<SpriteDepthSorter>();
        if (Sorter != null) Sorter.SortingOrderOffset += 2000;

        while (Elapsed < Duration)
        {
            Elapsed += Time.deltaTime;
            float T = Mathf.Clamp01(Elapsed / Duration);

            // T * T 를 곱해주면 처음엔 천천히 떨어지다 마지막에 확 떨어집니다.
            float EaseInT = T * T;

            transform.position = Vector3.Lerp(
                new Vector3(TargetPosition.x, TargetPosition.y + DropHeight, 0f),
                new Vector3(TargetPosition.x, TargetPosition.y, 0f),
                EaseInT
            );

            yield return null;
        }

        transform.position = TargetPosition;
        if (Sorter != null) Sorter.SortingOrderOffset -= 2000;
    }
    public abstract void CalculateNextPosition();

    public bool IsPositionOccupied(int TargetX, int TargetY)
    {
        if (EnemyManager != null)
        {
            EnemyBase OtherEnemy = EnemyManager.GetEnemyAtPosition(TargetX, TargetY);
            if (OtherEnemy != null && OtherEnemy != this)
            {
                return true;
            }
        }
        if (CachedItemManager != null)
        {
            ItemBase ItemOnTile = CachedItemManager.GetItemAtPosition(TargetX, TargetY);
            if (ItemOnTile != null)
            {
                return true;
            }
        }
        return false;
    }
    public virtual IEnumerator MoveRoutine()
    {
        if (!WillMoveThisTurn())
        {
            CurrentWaitCount--;
            yield break;
        }
        CurrentWaitCount = MoveInterval - 1;

        int PreviousX = CurrentX;
        int PreviousY = CurrentY;

        CalculateNextPosition();

        CurrentX = Mathf.Clamp(CurrentX, 0, GridManager.Columns - 1);
        CurrentY = Mathf.Clamp(CurrentY, 0, GridManager.Rows - 1);

        bool bIsAttacking = false;
        if (Player != null && Player.CurrentX == CurrentX && Player.CurrentY == CurrentY)
        {
            bIsAttacking = true;
        }
        else if (IsPositionOccupied(CurrentX, CurrentY))
        {
            CurrentX = PreviousX;
            CurrentY = PreviousY;
        }

        yield return StartCoroutine(AnimateMove(PreviousX, PreviousY, CurrentX, CurrentY));


        if (bIsAttacking)
        {
            ApplyDamage();

            if (EnemyManager != null)
            {
                EnemyManager.RemoveEnemy(this);
            }
            PoolManager.Instance.ReturnToPool(gameObject);
        }
    }

    protected virtual IEnumerator AnimateMove(int StartX, int StartY, int TargetX, int TargetY)
    {
        SpriteDepthSorter Sorter = GetComponent<SpriteDepthSorter>();
        if (Sorter != null) Sorter.SortingOrderOffset += 1000;

        Vector2 StartPosition = transform.position;
        Vector2 TargetPosition = GridManager.GetWorldPosition(TargetX, TargetY);
        float Distance = Vector2.Distance(StartPosition, TargetPosition);

        if (Distance > 0)
        {
            float Duration = Distance / MoveSpeed;
            float Elapsed = 0f;
            float DynamicHopHeight = HopHeight * (Distance * 0.5f + 0.5f);
            while (Elapsed < Duration)
            {
                Elapsed += Time.deltaTime;
                float T = Mathf.Clamp01(Elapsed / Duration);

                Vector2 LinearPos = Vector2.Lerp(StartPosition, TargetPosition, T);
                float HeightOffset = Mathf.Sin(T * Mathf.PI) * DynamicHopHeight;

                transform.position = new Vector3(LinearPos.x, LinearPos.y + HeightOffset, transform.position.z);
                yield return null;
            }
        }

        transform.position = TargetPosition;
        if (Sorter != null) Sorter.SortingOrderOffset -= 1000;
    }
    public virtual void Die()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(ScoreValue, transform.position);
        }
        PoolManager.Instance.ReturnToPool(gameObject);
    }

    public virtual void ApplyDamage()
    {
        if (Player != null && Player.HealthComponent != null)
        {
            Player.HealthComponent.TakeDamage(1);
        }
    }

    // 이동 턴 표시
    public virtual bool WillMoveThisTurn()
    {
        return CurrentWaitCount <= 0;
    }


    public void SetIntent(bool bIsAttacking)
    {
        if (IntentIndicator != null)
        {
            IntentIndicator.SetActive(bIsAttacking);
        }
    }
}
