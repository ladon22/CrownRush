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
            // 1. �밢�� ���� Ȯ��: �÷��̾ ���� ���� ���� �밢��(�¿� 1ĭ)�� �ִ��� �˻��մϴ�.
            if (Player.CurrentY == CurrentY + MoveDirY && Mathf.Abs(Player.CurrentX - CurrentX) == 1)
            {
                // ���� ��ġ�� ��ǥ�� �����մϴ�! (�밢�� ����)
                CurrentX = Player.CurrentX;
                CurrentY = Player.CurrentY;
                return;
            }

            // 2. ���� �渷�� Ȯ��: �÷��̾ ��Ȯ�� ���� ����(���� ���� 1ĭ ��)�� �ִ��� �˻��մϴ�.
            if (Player.CurrentX == CurrentX && Player.CurrentY == CurrentY + MoveDirY)
            {
                // �������δ� ������ �� �����Ƿ� ���ڸ����� ����մϴ� (��ǥ�� �ٲ��� �ʰ� return).
                return;
            }
        }

        if (IsPositionOccupied(CurrentX, CurrentY + MoveDirY))
        {
            return; // �հ� ���� �ʰ� ���ڸ��� ������ ����ϴ�.
        }

        // 3. ���� ��� ����, ������ �÷��̾�� �������� �ʾҴٸ� ������ �� ĭ �����մϴ�.
        // (���鿡 �ٸ� ���ͳ� �������� �־ ���� ���� �θ� Ŭ�������� �˾Ƽ� ó���� �ݴϴ�!)
        CurrentY += MoveDirY;
    }

    public override IEnumerator MoveRoutine()
    {
        yield return StartCoroutine(base.MoveRoutine());

        if (this != null && gameObject.activeInHierarchy)
        {
            // ���� ���� �� �ٿ� �����ߴ��� �˻��մϴ�.
            if (CurrentY == 0 || CurrentY == GridManager.Rows - 1)
            {
                yield return StartCoroutine(PromotionRoutine());
            }
        }
    }

    private IEnumerator PromotionRoutine()
    {
        if (QueenPrefab == null)
        {
            Debug.LogWarning("QueenPrefab�� �Ҵ���� �ʾҽ��ϴ�.");
            yield break;
        }

        // 1. ���� �����մϴ�. (EnemyBase�� Initialize ���п� �ڵ����� �ϴÿ��� �������ϴ�)
        Vector2 TargetPos = GridManager.GetWorldPosition(CurrentX, CurrentY);
        GameObject QueenObj = PoolManager.Instance.SpawnFromPool(QueenPrefab, TargetPos, Quaternion.identity);
        EnemyBase QueenEnemy = QueenObj.GetComponent<EnemyBase>();

        if (QueenEnemy != null)
        {
            QueenEnemy.Initialize(EnemyManager, GridManager, CurrentX, CurrentY);
            EnemyManager.ActiveEnemies.Add(QueenEnemy);
        }

        // 2. ���� �ٴڿ� ���� ������ ��Ȯ�� ����մϴ�. (SpawnDropRoutine ���� �ð��� 0.35���Դϴ�)
        yield return new WaitForSeconds(0.35f);

        // 3. ���� �����ϴ� ���� ���� ��(�ڽ�)�� Ǯ�� �ݳ��Ͽ� ������� �մϴ�.
        EnemyManager.RemoveEnemy(this);
        PoolManager.Instance.ReturnToPool(gameObject);
    }

}
