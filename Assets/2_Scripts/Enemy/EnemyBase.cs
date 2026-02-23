using System.Collections;
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
    }

    public virtual void Die()
    {
        Destroy(gameObject);
    }
}
