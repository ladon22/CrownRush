using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinearEnemy : EnemyBase
{
    [Header("Promotion Settings")]
    public GameObject QueenPrefab;

    private int MoveDirY;
    public override void Initialize(EnemyManager EnemyManager, GridManager GridManager, int StartX, int StartY)
    {
        base.Initialize(EnemyManager, GridManager, StartX, StartY);
        if (StartY == 0) MoveDirY = 1;
        else MoveDirY = -1;
    }
    public override void CalculateNextPosition()
    {
        if (Player != null)
        {
            if (Player.CurrentY == CurrentY + MoveDirY && Mathf.Abs(Player.CurrentX - CurrentX) == 1)
            {
                CurrentX = Player.CurrentX;
                CurrentY = Player.CurrentY;
                return;
            }

            if (Player.CurrentX == CurrentX && Player.CurrentY == CurrentY + MoveDirY)
            {
                return;
            }
        }

        if (IsPositionOccupied(CurrentX, CurrentY + MoveDirY))
        {
            return;
        }
        CurrentY += MoveDirY;
    }

    public override IEnumerator MoveRoutine()
    {
        yield return StartCoroutine(base.MoveRoutine());

        if (this != null && gameObject.activeInHierarchy)
        {
            if ((MoveDirY == 1 && CurrentY == GridManager.Rows - 1) || (MoveDirY == -1 && CurrentY == 0))
            {
                yield return StartCoroutine(PromotionRoutine());
            }
        }
    }

    private IEnumerator PromotionRoutine()
    {
        if (QueenPrefab == null)
        {
            Debug.LogWarning("QueenPrefab이 없습니다.");
            yield break;
        }

        Vector2 TargetPos = GridManager.GetWorldPosition(CurrentX, CurrentY);
        GameObject QueenObj = PoolManager.Instance.SpawnFromPool(QueenPrefab, TargetPos, Quaternion.identity);
        EnemyBase QueenEnemy = QueenObj.GetComponent<EnemyBase>();

        if (QueenEnemy != null)
        {
            QueenEnemy.Initialize(EnemyManager, GridManager, CurrentX, CurrentY);
            EnemyManager.ActiveEnemies.Add(QueenEnemy);
        }

        yield return new WaitForSeconds(0.35f);

        if (TutorialHintManager.Instance != null)
            TutorialHintManager.Instance.TryShowHint(TutorialHintID.Queen);

        EnemyManager.RemoveEnemy(this);
        PoolManager.Instance.ReturnToPool(gameObject);
    }

}
