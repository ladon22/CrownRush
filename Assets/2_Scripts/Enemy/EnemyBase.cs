using System.Collections;
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
    public virtual void Initialize(EnemyManager EnemyManager, GridManager GridManager, int StartX, int StartY)
    {
        this.EnemyManager = EnemyManager;
        this.GridManager = GridManager;
        CurrentX = StartX;
        CurrentY = StartY;
        transform.position = GridManager.GetWorldPosition(CurrentX, CurrentY);
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
        int PreviousX = CurrentX;
        int PreviousY = CurrentY;

        CalculateNextPosition();

        if (CurrentY < 0)
        {
            if (EnemyManager != null)
            {
                ApplyDamage();
                EnemyManager.RemoveEnemy(this);
            }
            Destroy(gameObject);
            yield break;
        }

        CurrentX = Mathf.Clamp(CurrentX, 0, GridManager.Columns - 1);

        yield return StartCoroutine(AnimateMove(PreviousX, PreviousY, CurrentX, CurrentY));

        if (CurrentY == 0)
        {
            PlayerController Player = FindFirstObjectByType<PlayerController>();
            if (Player != null && Player.CurrentX == CurrentX && Player.CurrentY == this.CurrentY)
            {
                if (EnemyManager != null)
                {
                    EnemyManager.RemoveEnemy(this);
                }
                Die();
            }
        }
    }

    protected virtual IEnumerator AnimateMove(int StartX, int StartY, int TargetX, int TargetY)
    {
        Vector2 TargetPosition = GridManager.GetWorldPosition(TargetX, TargetY);
        while ((Vector2)transform.position != TargetPosition)
        {
            transform.position = Vector2.MoveTowards(transform.position, TargetPosition, MoveSpeed * Time.deltaTime);
            yield return null;
        }
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
}
