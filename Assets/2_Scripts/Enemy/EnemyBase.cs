using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    [HideInInspector]
    public GridManager GridManager;
    [HideInInspector]
    public EnemyManager EnemyManager;
    public int CurrentX;
    public int CurrentY;
    public float MoveSpeed = 15f;
    public int ScoreValue = 1;

    [Header("UI")]
    public GameObject IntentIndicator;

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
        transform.position = GridManager.GetWorldPosition(CurrentX, CurrentY);

        CurrentWaitCount = 0;
        SetIntent(false);
    }

    protected virtual void OnDestroy()
    {
        if (EnemyManager != null)
        {
            EnemyManager.RemoveEnemy(this);
        }
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

        PlayerController Player = FindFirstObjectByType<PlayerController>();
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
            Destroy(gameObject);
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
        ScoreManager ScoreManager = FindFirstObjectByType<ScoreManager>();
        if (ScoreManager != null)
        {
            ScoreManager.AddScore(ScoreValue);
        }
        Destroy(gameObject);
    }

    public virtual void ApplyDamage()
    {
        PlayerController Player = FindFirstObjectByType<PlayerController>();
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
