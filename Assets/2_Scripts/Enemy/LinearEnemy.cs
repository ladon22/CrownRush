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
        PlayerController Player = FindFirstObjectByType<PlayerController>();

        if (Player != null)
        {
            // 1. 대각선 공격 확인: 플레이어가 나의 진행 방향 대각선(좌우 1칸)에 있는지 검사합니다.
            if (Player.CurrentY == CurrentY + MoveDirY && Mathf.Abs(Player.CurrentX - CurrentX) == 1)
            {
                // 공격 위치로 목표를 변경합니다! (대각선 돌진)
                CurrentX = Player.CurrentX;
                CurrentY = Player.CurrentY;
                return;
            }

            // 2. 정면 길막힘 확인: 플레이어가 정확히 나의 정면(진행 방향 1칸 앞)에 있는지 검사합니다.
            if (Player.CurrentX == CurrentX && Player.CurrentY == CurrentY + MoveDirY)
            {
                // 정면으로는 공격할 수 없으므로 제자리에서 대기합니다 (좌표를 바꾸지 않고 return).
                return;
            }
        }

        if (IsPositionOccupied(CurrentX, CurrentY + MoveDirY))
        {
            return; // 뚫고 가지 않고 제자리에 얌전히 멈춥니다.
        }

        // 3. 공격 대상도 없고, 정면이 플레이어로 막히지도 않았다면 무조건 한 칸 직진합니다.
        // (정면에 다른 몬스터나 아이템이 있어서 막힌 경우는 부모 클래스에서 알아서 처리해 줍니다!)
        CurrentY += MoveDirY;
    }

    public override IEnumerator MoveRoutine()
    {
        yield return StartCoroutine(base.MoveRoutine());

        if (this != null && gameObject.activeInHierarchy)
        {
            // 맵의 양쪽 끝 줄에 도달했는지 검사합니다.
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
            Debug.LogWarning("QueenPrefab이 할당되지 않았습니다.");
            yield break;
        }

        // 1. 퀸을 생성합니다. (EnemyBase의 Initialize 덕분에 자동으로 하늘에서 떨어집니다)
        Vector2 TargetPos = GridManager.GetWorldPosition(CurrentX, CurrentY);
        GameObject QueenObj = PoolManager.Instance.SpawnFromPool(QueenPrefab, TargetPos, Quaternion.identity);
        EnemyBase QueenEnemy = QueenObj.GetComponent<EnemyBase>();

        if (QueenEnemy != null)
        {
            QueenEnemy.Initialize(EnemyManager, GridManager, CurrentX, CurrentY);
            EnemyManager.ActiveEnemies.Add(QueenEnemy);
        }

        // 2. 퀸이 바닥에 닿을 때까지 정확히 대기합니다. (SpawnDropRoutine 낙하 시간이 0.35초입니다)
        yield return new WaitForSeconds(0.35f);

        // 3. 퀸이 착지하는 순간 기존 폰(자신)을 풀로 반납하여 사라지게 합니다.
        EnemyManager.RemoveEnemy(this);
        PoolManager.Instance.ReturnToPool(gameObject);
    }

    private void PromoteToQueen()
    {
        if (QueenPrefab == null)
        {
            Debug.LogWarning("퀸 프리팹이 비어있어 승급할 수 없습니다! 인스펙터를 확인해주세요.");
            return;
        }

        // 1. 현재 내 위치(월드 좌표)를 구합니다.
        Vector2 SpawnPosition = GridManager.GetWorldPosition(CurrentX, CurrentY);

        // 2. 오브젝트 풀을 이용해 그 자리에 퀸을 생성(꺼냄)합니다.
        GameObject QueenObj = PoolManager.Instance.SpawnFromPool(QueenPrefab, SpawnPosition, Quaternion.identity);

        EnemyBase QueenEnemy = QueenObj.GetComponent<EnemyBase>();
        if (QueenEnemy != null)
        {
            // 3. 퀸 초기화 및 적 매니저에 등록
            QueenEnemy.Initialize(EnemyManager, GridManager, CurrentX, CurrentY);
            EnemyManager.ActiveEnemies.Add(QueenEnemy);
        }

        // 4. 승급이 완료되었으므로 기존의 폰(자신)은 리스트에서 빼고 풀로 반납합니다!
        EnemyManager.RemoveEnemy(this);
        PoolManager.Instance.ReturnToPool(gameObject);
    }
}
