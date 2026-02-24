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

    public abstract void CalculateNextPosition();

    public virtual IEnumerator MoveRoutine()
    {
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

        Vector2 TargetPosition = GridManager.GetWorldPosition(CurrentX, CurrentY);
        while((Vector2)transform.position != TargetPosition)
        {
            transform.position = Vector2.MoveTowards(transform.position, TargetPosition, MoveSpeed * Time.deltaTime);
            yield return null;
        }

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
